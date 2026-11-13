using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Lot
{
    /// <summary>The live Perfect Park and Perfect drop-off thresholds: seeded into the tuning store and kept in step with the F2 panel.</summary>
    public sealed class PerfectTuning
    {
        public float DropOffAngle { get; private set; }
        public float DropOffTimerShare { get; private set; }
        public float ParkAngle { get; private set; }
        public float ParkSideOffset { get; private set; }
        public float ParkLengthOffset { get; private set; }
        public float ParkCooldown { get; private set; }
        public float ParkMinTravel { get; private set; }

        public PerfectTuning(PerfectSettings s)
        {
            DropOffAngle = s.DropOffAngleDegrees;
            DropOffTimerShare = s.DropOffTimerShare;
            ParkAngle = s.ParkAngleDegrees;
            ParkSideOffset = s.ParkSideOffset;
            ParkLengthOffset = s.ParkLengthOffset;
            ParkCooldown = s.ParkCooldownSeconds;
            ParkMinTravel = s.ParkMinTravel;
        }

        /// <summary>Writes the values into the tuning store as seeds (Bootstrap calls this).</summary>
        public void Seed(ITuning t)
        {
            t.Seed(TuningKey.PerfectDropOffAngle, DropOffAngle);
            t.Seed(TuningKey.PerfectDropOffTimerShare, DropOffTimerShare);
            t.Seed(TuningKey.PerfectParkAngle, ParkAngle);
            t.Seed(TuningKey.PerfectParkSideOffset, ParkSideOffset);
            t.Seed(TuningKey.PerfectParkLengthOffset, ParkLengthOffset);
            t.Seed(TuningKey.PerfectParkCooldown, ParkCooldown);
            t.Seed(TuningKey.PerfectParkMinTravel, ParkMinTravel);
        }

        /// <summary>Takes the stored value of every key that has been seeded.</summary>
        public void Apply(ITuning t)
        {
            foreach (var row in PerfectRows.Rows)
                if (t.TryGet(row.Key, out var value)) TrySet(row.Key, value);
        }

        /// <summary>Applies one change; false when the key is not a Perfect key.</summary>
        public bool TrySet(TuningKey key, float value)
        {
            switch (key)
            {
                case TuningKey.PerfectDropOffAngle: DropOffAngle = value; return true;
                case TuningKey.PerfectDropOffTimerShare: DropOffTimerShare = value; return true;
                case TuningKey.PerfectParkAngle: ParkAngle = value; return true;
                case TuningKey.PerfectParkSideOffset: ParkSideOffset = value; return true;
                case TuningKey.PerfectParkLengthOffset: ParkLengthOffset = value; return true;
                case TuningKey.PerfectParkCooldown: ParkCooldown = value; return true;
                case TuningKey.PerfectParkMinTravel: ParkMinTravel = value; return true;
                default: return false;
            }
        }
    }
}
