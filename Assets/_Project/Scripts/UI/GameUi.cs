using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Story;
using UncannyValet.UI.Flow;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Screens;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>
    /// The whole game UI on one 480x270 pixel canvas (IMGUI, generated art only): title, how-to-play (first time per page load), the story
    /// cards (lot intro, reward, thrown back, end), the HUD with floating car tickets and the results screen. Which screen shows is the
    /// <see cref="FlowMachine"/>'s state, which outlives the lot scenes; Bootstrap hands it over with <see cref="UseFlow"/> before enabling.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameUi : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Seconds before keys work on the results screen, so a held key cannot skip it.")]
        private float _resultsKeyDelaySeconds = 0.8f;
        [SerializeField, Min(0f), Tooltip("Seconds before a key dismisses the title, how-to-play or a story card.")]
        private float _titleKeyDelaySeconds = 0.3f;

        [SerializeField, Tooltip("Emotion thresholds, flash speeds and colours of the pickup mark. Set by Setup/Build Everything.")]
        private HudEmotionConfig _emotions;
        [SerializeField, Tooltip("The words of the story cards. Set by Setup/Build Everything.")]
        private StoryTextAsset _story;

        private EmotionTuningLink _emotionLink;
        private Func<string> _prompt = NoPrompt;
        private IEventBus _bus;
        private ShiftStats _stats;
        private PlayingHud _playing;
        private PixelKit _kit;
        private PixelCanvas _canvas;
        private ResultsPainter _resultsPainter;
        private TitlePainter _titlePainter;
        private HowToPlayPainter _howToPlayPainter;
        private StoryCardPainter _cardPainter;
        private TitlePicker _picker;
        private FlowMachine _machine;
        private IFlowActions _actions;
        private IStoryFacts _facts;
        private FlowController _flow;
        private FlowEventLink _flowLink;
        private StoryCardCache _cards;
        private ResultsContent _results;
        private Camera _camera;

        /// <summary>True while a screen other than the HUD and the results shows (title, how-to-play, a card): the valet must not be controllable meanwhile.</summary>
        public bool IsFlowScreenShowing => _machine == null || (_machine.State != FlowState.Playing && _machine.State != FlowState.Results);

        /// <summary>Sets where the interact prompt text comes from.</summary>
        public void Configure(Func<string> prompt)
        {
            _prompt = prompt ?? NoPrompt;
        }

        /// <summary>Hands over the flow state (which outlives the scene), what the flow asks of the game and the lot and power facts for the cards. Call before enabling.</summary>
        public void UseFlow(FlowMachine machine, IFlowActions actions, IStoryFacts facts)
        {
            _machine = machine;
            _actions = actions;
            _facts = facts;
        }

        /// <summary>The pickup emotion config (levels, flash speeds, colours); Bootstrap seeds the F2 rows from it.</summary>
        public HudEmotionConfig Emotions => EmotionConfig();

        private static string NoPrompt() => string.Empty;

        private void OnEnable()
        {
            if (_machine == null || _actions == null || _facts == null || _story == null)
            {
                Debug.LogError("GameUi needs the flow, its actions, the story facts and the story text; rebuild with Setup/Build Everything.", this);
                enabled = false;
                return;
            }
            if (!Services.TryGet<ILotGoal>(out var goal))
            {
                Debug.LogError("GameUi found no ILotGoal: the lot must register its goal before the UI starts.", this);
                enabled = false;
                return;
            }
            _bus = Services.Get<IEventBus>();
            _stats = new ShiftStats(_bus);
            _kit = new PixelKit();
            _canvas = new PixelCanvas(_kit);
            var style = new EmotionStyle(EmotionConfig());
            _emotionLink = new EmotionTuningLink(_bus, Services.Get<ITuning>(), style.Rule, EmotionConfig().AngryFrom, EmotionConfig().WillNotBlessFrom);
            _playing = new PlayingHud(_canvas, _bus, Services.Get<IGameClock>(), Services.Get<IShiftBlessingsView>(),
                goal, Services.Get<ICarFactory>(), style, EmotionConfig().BlessingPop, EmotionConfig().PowerBar);
            _resultsPainter = new ResultsPainter(_canvas);
            _titlePainter = new TitlePainter(_canvas);
            _howToPlayPainter = new HowToPlayPainter(_canvas);
            _cardPainter = new StoryCardPainter(_canvas);
            _cards = new StoryCardCache(_machine, _story, _facts, Services.Get<IRunProgress>());
            _flow = new FlowController(_machine, _actions, _titleKeyDelaySeconds, _resultsKeyDelaySeconds);
            _flowLink = new FlowEventLink(_bus, _machine);
            _picker = Services.TryGet<ILotPicker>(out var lots) ? new TitlePicker(new DevLotPicker(lots, _flow)) : null;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftResultReady>(OnResult);
            _machine.SceneLoaded(goal.Lot);
        }

        private HudEmotionConfig EmotionConfig()
        {
            if (_emotions != null) return _emotions;
            Debug.LogError("GameUi has no HudEmotionConfig; using defaults. Rebuild with Setup/Build Everything.", this);
            _emotions = ScriptableObject.CreateInstance<HudEmotionConfig>();
            return _emotions;
        }

        private void OnDisable()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
                _bus.Unsubscribe<ShiftResultReady>(OnResult);
            }
            _flowLink?.Dispose();
            _emotionLink?.Dispose();
            _playing?.Dispose();
            _stats?.Dispose();
            _kit?.Dispose();
        }

        private void OnShiftStarted(ShiftStarted e) => _results = null;

        private void OnResult(ShiftResultReady e) => _results = ResultsFormatter.Build(e.Result, _stats.CarsParked, _stats.PerfectParks);

        // Used by the PlayMode tests (by reflection) to skip every card and start the shift.
        private void StartPlaying() => _flow.SkipToPlaying();

        private void Update()
        {
            var devKey = _machine.State == FlowState.Title && UiInput.DevKeyHeld();
            if (devKey && _flow.SecondsOnScreen(Time.unscaledTime) >= _titleKeyDelaySeconds && _picker != null && _picker.TryPick()) return;
            _flow.Update(Time.unscaledTime, Time.frameCount, !devKey && UiInput.AnyPressed(), UiInput.AnyKeyPressed(), UiInput.RetryPressed());
        }

        private void OnGUI()
        {
            if (_canvas == null) return;
            GUI.depth = 10; // behind the tuning panel
            var current = Event.current;
            if (current.type == EventType.MouseDown && _machine.State == FlowState.Results && ClickResultsButton(current.mousePosition))
            {
                current.Use();
                return;
            }
            if (current.type != EventType.Repaint) return;
            _canvas.Begin(Screen.width, Screen.height);
            DrawScreen();
            _canvas.End();
        }

        private bool ClickResultsButton(Vector2 mouse)
        {
            var grid = new PixelGrid(Screen.width, Screen.height);
            var x = grid.VirtualX(mouse.x);
            var y = grid.VirtualY(mouse.y);
            if (ResultsPainter.RetryButton.Contains(x, y)) _flow.Retry();
            else if (ResultsPainter.NextButton.Contains(x, y)) _flow.Continue();
            else return false;
            return true;
        }

        private void DrawScreen()
        {
            var time = Time.unscaledTime;
            switch (_machine.State)
            {
                case FlowState.Title: _titlePainter.Draw(time, UiInput.DevKeyHeld() ? _picker : null); return;
                case FlowState.HowToPlay: _howToPlayPainter.Draw(time); return;
                case FlowState.Playing:
                case FlowState.Results:
                    _playing.Draw(ResolveCamera(), time, _prompt());
                    if (_machine.State == FlowState.Results && _results != null) _resultsPainter.Draw(_results);
                    return;
                default:
                    var card = _cards.Current;
                    if (card != null) _cardPainter.Draw(card, time);
                    return;
            }
        }

        // Camera.main is looked up only until it is found, then cached.
        private Camera ResolveCamera()
        {
            if (_camera == null) _camera = Camera.main;
            return _camera;
        }
    }
}
