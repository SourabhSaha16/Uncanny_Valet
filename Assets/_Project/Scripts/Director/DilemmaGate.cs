using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>
    /// Pure dilemma budget (spec section 4): one director-made dilemma open at a time, a cooldown after it resolves,
    /// no dilemma during relief or right after an angry patron, and only with headroom or a rising curve.
    /// </summary>
    public sealed class DilemmaGate
    {
        private readonly GateSettings _settings;
        private float _openedAt;
        private float _cooldownUntil;

        public DilemmaGate(GateSettings settings) { _settings = settings; }

        /// <summary>A dilemma is open: made, and neither its station freed nor its car parked.</summary>
        public bool IsOpen { get; private set; }

        public DilemmaKind OpenKind { get; private set; }

        public CarId OpenCar { get; private set; }

        /// <summary>Seconds until the cooldown ends; 0 when it is over.</summary>
        public float CooldownLeft(float now) => now < _cooldownUntil ? _cooldownUntil - now : 0f;

        /// <summary>Forgets everything for a new shift.</summary>
        public void Reset()
        {
            IsOpen = false;
            OpenCar = CarId.None;
            _openedAt = 0f;
            _cooldownUntil = 0f;
        }

        /// <summary>
        /// May a rule boost apply to the next draw? <paramref name="gap"/> is workload minus target;
        /// <paramref name="curveRising"/> is true when the intensity curve is climbing.
        /// </summary>
        public bool CanFire(in LotSnapshot lot, float gap, bool curveRising, bool relief)
        {
            if (IsOpen || relief) return false;
            if (CooldownLeft(lot.NowSeconds) > 0f) return false;
            if (lot.CountAngry(_settings.BlackoutSeconds) > 0) return false;
            return gap <= _settings.HeadroomGap || curveRising;
        }

        /// <summary>A boosted car of this rule arrived: the dilemma is now open.</summary>
        public void Open(DilemmaKind kind, CarId car, float now)
        {
            IsOpen = true;
            OpenKind = kind;
            OpenCar = car;
            _openedAt = now;
        }

        /// <summary>Closes the open dilemma, starts the cooldown and returns what resolved. False when none was open.</summary>
        public bool TryResolve(float now, out DilemmaKind kind, out float seconds)
        {
            kind = OpenKind;
            seconds = IsOpen ? now - _openedAt : 0f;
            if (!IsOpen) return false;
            IsOpen = false;
            OpenCar = CarId.None;
            _cooldownUntil = now + _settings.CooldownSeconds;
            return true;
        }
    }
}
