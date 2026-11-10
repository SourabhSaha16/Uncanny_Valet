using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// The director's runtime numbers, refreshed in place from ITuning once per tick so the F2 panel steers it live.
    /// Pure classes hold a reference to one section and always see the latest values.
    /// </summary>
    public sealed class DirectorSettings
    {
        public WorkloadSettings Workload { get; } = new WorkloadSettings();
        public PacingSettings Pacing { get; } = new PacingSettings();
        public ReliefSettings Relief { get; } = new ReliefSettings();
        public FormSettings Form { get; } = new FormSettings();
        public DilemmaRuleSettings Rules { get; } = new DilemmaRuleSettings();
        public GateSettings Gate { get; } = new GateSettings();
        public PickerSettings Picker { get; } = new PickerSettings();

        /// <summary>False means the old fixed arrival timer runs and the director stays idle.</summary>
        public bool Enabled { get; private set; } = true;

        public float TickSeconds { get; private set; } = 0.5f;

        /// <summary>0 picks a fresh seed every shift.</summary>
        public int Seed { get; private set; }

        public void Refresh(ITuning t)
        {
            Enabled = TuningRead.Float(t, TuningKey.DirEnabled, Enabled ? 1f : 0f) >= 0.5f;
            TickSeconds = TuningRead.Float(t, TuningKey.DirTickSeconds, TickSeconds);
            Seed = (int)TuningRead.Float(t, TuningKey.DirSeed, Seed);
            Workload.Read(t);
            Pacing.Read(t);
            Relief.Read(t);
            Form.Read(t);
            Rules.Read(t);
            Gate.Read(t);
            Picker.Read(t);
        }
    }
}
