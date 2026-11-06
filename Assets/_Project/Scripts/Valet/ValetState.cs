using System;

namespace UncannyValet.Valet
{
    public enum ValetMode { OnFoot, Driving }

    /// <summary>Tiny state machine: the valet is on foot or driving. Only this module reads it.</summary>
    public sealed class ValetState
    {
        public ValetMode Mode { get; private set; } = ValetMode.OnFoot;

        public bool IsDriving => Mode == ValetMode.Driving;

        public event Action<ValetMode> Changed;

        /// <summary>OnFoot to Driving. Returns false if already driving.</summary>
        public bool TryEnterCar() => Switch(ValetMode.OnFoot, ValetMode.Driving);

        /// <summary>Driving to OnFoot. Returns false if already on foot.</summary>
        public bool TryExitCar() => Switch(ValetMode.Driving, ValetMode.OnFoot);

        private bool Switch(ValetMode from, ValetMode to)
        {
            if (Mode != from) return false;
            Mode = to;
            Changed?.Invoke(to);
            return true;
        }
    }
}
