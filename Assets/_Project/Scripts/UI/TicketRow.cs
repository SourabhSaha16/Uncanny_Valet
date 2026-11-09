using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Plain data for one open ticket. Written by TicketBoard, read by the painters.</summary>
    public sealed class TicketRow
    {
        /// <summary>Tag index used until the car's key-tag colour is known.</summary>
        public const int NoTag = -1;

        public int Car;
        /// <summary>The key-tag colour index (see TagPalette) of this ticket.</summary>
        public int Tag = NoTag;
        public TicketRowState State = TicketRowState.AwaitingPark;
        public int Spot;
        public bool HasTimer;
        public TimerKind Timer;
        /// <summary>0 = just started, 1 = out of time.</summary>
        public float Elapsed01;
        public bool Expired;

        /// <summary>True from the moment the car has finished driving in until it leaves.</summary>
        public bool Arrived;

        /// <summary>Charge shown when an EV's level is not known yet.</summary>
        public const float UnknownCharge = -1f;

        public bool IsEv;
        /// <summary>0 empty, 1 full, <see cref="UnknownCharge"/> until the first CarChargeChanged.</summary>
        public float Charge01 = UnknownCharge;
        public bool IsDirty;
        /// <summary>0 filthy, 1 spotless; stays 0 until the wash starts.</summary>
        public float Clean01;
        /// <summary>True once a dirty car has been washed.</summary>
        public bool Washed;

        /// <summary>Charge at or above this counts as full.</summary>
        public const float FullCharge = 0.999f;

        /// <summary>The patron wants a wash that has not happened yet.</summary>
        public bool NeedsWash => IsDirty && !Washed;

        /// <summary>An EV that is not fully charged yet (an unknown level counts as not charged).</summary>
        public bool NeedsCharge => IsEv && Charge01 < FullCharge;

        /// <summary>How much dirt is left: 1 filthy, 0 clean.</summary>
        public float Dirt01 => Washed ? 0f : 1f - Clean01;

        /// <summary>The charge level to draw (an unknown level reads as empty).</summary>
        public float ChargeShown => Charge01 < 0f ? 0f : Charge01;

        /// <summary>The floating info box lists the dirt meter while the car still needs a wash.</summary>
        public bool ShowsDirtMeter => NeedsWash;

        /// <summary>The floating info box lists the charge meter for every EV (amber once full).</summary>
        public bool ShowsChargeMeter => IsEv;

        /// <summary>The floating info box appears only after arrival and only when a meter applies.</summary>
        public bool ShowsInfoBox => Arrived && (ShowsDirtMeter || ShowsChargeMeter);

        public void StartTimer(TimerKind kind, float elapsed01)
        {
            HasTimer = true;
            Timer = kind;
            Elapsed01 = elapsed01;
        }
    }
}
