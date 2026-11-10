using System;
using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Screens;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.UI
{
    /// <summary>
    /// The whole game UI on one 480x270 pixel canvas (IMGUI, generated art only): title screen, HUD with floating
    /// car tickets, and the results screen. The title waits for any key before <see cref="TitleDismissed"/> fires;
    /// the shift clock must not start until then. Retry publishes RetryShiftRequested.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameUi : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Seconds before keys work on the results screen, so a held key cannot skip it.")]
        private float _resultsKeyDelaySeconds = 0.8f;
        [SerializeField, Min(0f), Tooltip("Seconds before a key dismisses the title screen.")]
        private float _titleKeyDelaySeconds = 0.3f;

        private readonly List<TicketRow> _rows = new List<TicketRow>(8);
        private readonly List<TicketRow> _keyRows = new List<TicketRow>(8);
        private Func<string> _prompt = NoPrompt;
        private UiPhase _phase = UiPhase.Title;
        private float _phaseStartedAt;
        private IEventBus _bus;
        private TicketBoard _board;
        private ShiftStats _stats;
        private PixelKit _kit;
        private PixelCanvas _canvas;
        private HudPainter _hud;
        private KeyBoardPainter _keyBoard;
        private NoticePainter _notices;
        private WorldTicketPainter _world;
        private CurbHintTracker _curbHints;
        private CurbHintPainter _hintPainter;
        private OccupiedHintTracker _occupiedHints;
        private OccupiedHintPainter _occupiedPainter;
        private ResultsPainter _resultsPainter;
        private TitlePainter _titlePainter;
        private ResultsContent _results;
        private Camera _camera;

        /// <summary>Raised once, when the player dismisses the title screen. Bootstrap starts the first shift then.</summary>
        public event Action TitleDismissed;

        /// <summary>True until the title screen is dismissed; the valet must not be controllable meanwhile.</summary>
        public bool IsTitleShowing => _phase == UiPhase.Title;

        /// <summary>Sets where the interact prompt text comes from.</summary>
        public void Configure(Func<string> prompt)
        {
            _prompt = prompt ?? NoPrompt;
        }

        private static string NoPrompt() => string.Empty;

        private static float UnscaledTime() => Time.unscaledTime;

        private void OnEnable()
        {
            _bus = Services.Get<IEventBus>();
            _board = new TicketBoard(_bus);
            _stats = new ShiftStats(_bus);
            _kit = new PixelKit();
            _canvas = new PixelCanvas(_kit);
            _hud = new HudPainter(_canvas, Services.Get<IGameClock>(), Services.Get<IShiftScoreView>());
            _keyBoard = new KeyBoardPainter(_canvas);
            _notices = new NoticePainter(_canvas);
            _world = new WorldTicketPainter(_canvas, Services.Get<ICarFactory>());
            _curbHints = new CurbHintTracker(_bus);
            _hintPainter = new CurbHintPainter(_canvas, Services.Get<ICarFactory>(), _curbHints);
            _occupiedHints = new OccupiedHintTracker(_bus, UnscaledTime);
            _occupiedPainter = new OccupiedHintPainter(_canvas, _occupiedHints);
            _resultsPainter = new ResultsPainter(_canvas);
            _titlePainter = new TitlePainter(_canvas);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftResultReady>(OnResult);
            SetPhase(UiPhase.Title);
        }

        private void OnDisable()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
                _bus.Unsubscribe<ShiftResultReady>(OnResult);
            }
            _board?.Dispose();
            _stats?.Dispose();
            _curbHints?.Dispose();
            _occupiedHints?.Dispose();
            _kit?.Dispose();
        }

        private void SetPhase(UiPhase phase)
        {
            _phase = phase;
            _phaseStartedAt = Time.unscaledTime;
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            _results = null;
            SetPhase(UiPhase.Playing);
        }

        private void OnResult(ShiftResultReady e)
        {
            _results = ResultsFormatter.Build(e.Result, _stats.CarsParked, _stats.BestCombo);
            SetPhase(UiPhase.Results);
        }

        private void Update()
        {
            var waited = Time.unscaledTime - _phaseStartedAt;
            if (_phase == UiPhase.Title && waited >= _titleKeyDelaySeconds && AnyInputPressed()) DismissTitle();
            else if (_phase == UiPhase.Results && waited >= _resultsKeyDelaySeconds && RetryKeyPressed()) Retry();
        }

        private static bool AnyInputPressed()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return keyboard != null && keyboard.anyKey.wasPressedThisFrame
                || mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private static bool RetryKeyPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
        }

        private void DismissTitle()
        {
            SetPhase(UiPhase.Playing);
            TitleDismissed?.Invoke();
        }

        private void Retry()
        {
            _results = null;
            SetPhase(UiPhase.Playing);
            _bus.Publish(new RetryShiftRequested());
        }

        private void OnGUI()
        {
            if (_canvas == null) return;
            GUI.depth = 10; // behind the tuning panel
            var current = Event.current;
            if (current.type == EventType.MouseDown && _phase == UiPhase.Results && ClickedRetry(current.mousePosition))
            {
                current.Use();
                Retry();
                return;
            }
            if (current.type != EventType.Repaint) return;
            _canvas.Begin(Screen.width, Screen.height);
            DrawPhase();
            _canvas.End();
        }

        private bool ClickedRetry(Vector2 mouse)
        {
            var grid = new PixelGrid(Screen.width, Screen.height);
            return ResultsPainter.RetryButton.Contains(grid.VirtualX(mouse.x), grid.VirtualY(mouse.y));
        }

        private void DrawPhase()
        {
            var time = Time.unscaledTime;
            if (_phase == UiPhase.Title) { _titlePainter.Draw(time); return; }
            _board.CollectRows(_rows);
            _world.Draw(_rows, ResolveCamera(), time);
            _hintPainter.Draw(ResolveCamera());
            _occupiedPainter.Draw(ResolveCamera());
            _hud.Draw(time);
            _board.CollectKeyBoardRows(_keyRows);
            _keyBoard.Draw(_keyRows, time);
            _notices.Draw(_prompt());
            if (_phase == UiPhase.Results && _results != null) _resultsPainter.Draw(_results, time - _phaseStartedAt);
        }

        // Camera.main is looked up only until it is found, then cached.
        private Camera ResolveCamera()
        {
            if (_camera == null) _camera = Camera.main;
            return _camera;
        }
    }
}
