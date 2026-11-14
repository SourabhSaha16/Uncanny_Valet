using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pops a floating gag line above a pedestrian when the valet (or the car he drives) passes close. Each pop is a <see cref="SpeechRequested"/>
    /// of priority <see cref="SpeechPriority.GagLine"/> anchored to the walker; the speech director still decides what shows. It only looks
    /// while a shift runs and while no bubble is on screen, so a line never talks over a watcher. Bootstrap calls <see cref="Configure"/> while
    /// the object is inactive, then activates it. Never allocates per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GagLineTrigger : MonoBehaviour
    {
        private const int Seed = 4711;

        private GagLineAsset _asset;
        private Transform _walkers;
        private IEventBus _bus;
        private IPlayerFocus _focus;
        private GagLineRule _rule;
        private GagLineDeck _deck;
        private Transform[] _transforms = new Transform[0];
        private Vector2[] _positions = new Vector2[0];
        private bool[] _active = new bool[0];
        private bool _shiftRunning;
        private float _busyUntil;

        /// <summary>Sets the lines and the object whose children are the walkers; both are required.</summary>
        public void Configure(GagLineAsset asset, Transform walkers)
        {
            _asset = asset;
            _walkers = walkers;
        }

        private void OnEnable()
        {
            if (_asset == null || _walkers == null)
            {
                Debug.LogError("GagLineTrigger on " + name + " has no gag lines or no pedestrians; check the lot definition.", this);
                enabled = false;
                return;
            }
            _bus = Services.Get<IEventBus>();
            _focus = Services.Get<IPlayerFocus>();
            _deck = new GagLineDeck(_asset.Lines, Seed);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<SpeechShown>(OnSpeechShown);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<SpeechShown>(OnSpeechShown);
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            _shiftRunning = true;
            _busyUntil = 0f;
            _rule?.Reset();
        }

        private void OnShiftEnded(ShiftEnded e) => _shiftRunning = false;

        private void OnSpeechShown(SpeechShown e) => _busyUntil = Mathf.Max(_busyUntil, Time.unscaledTime + e.Seconds);

        private void Update()
        {
            var now = Time.unscaledTime;
            if (!_shiftRunning || now < _busyUntil) return;
            if (_transforms.Length != _walkers.childCount) Rebuild();
            for (var i = 0; i < _transforms.Length; i++)
            {
                _active[i] = _transforms[i].gameObject.activeInHierarchy;
                _positions[i] = _transforms[i].position;
            }
            var pick = _rule.Pick(now, _focus.Position, _positions, _active, _transforms.Length);
            if (pick == GagLineRule.None) return;
            var line = _deck.Next();
            if (line != null)
                _bus.Publish(new SpeechRequested(new SpeechRequest(_asset.Speaker, line, _positions[pick], _transforms[pick], SpeechPriority.GagLine)));
        }

        // The crowd creates its walkers once, so this runs once (and again only if walkers are added).
        private void Rebuild()
        {
            var count = _walkers.childCount;
            _transforms = new Transform[count];
            _positions = new Vector2[count];
            _active = new bool[count];
            for (var i = 0; i < count; i++) _transforms[i] = _walkers.GetChild(i);
            _rule = new GagLineRule(_asset.TriggerDistance, _asset.GlobalLockoutSeconds, _asset.WalkerLockoutSeconds, count);
        }
    }
}
