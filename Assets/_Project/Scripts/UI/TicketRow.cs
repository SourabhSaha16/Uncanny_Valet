using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Plain data for one overlay line. Written by TicketBoard, read by TicketRowFormatter.</summary>
    public sealed class TicketRow
    {
        public const char NoLetter = '?';

        public int Car;
        public char Letter = NoLetter;
        public TicketRowState State = TicketRowState.AwaitingPark;
        public int Spot;
        public bool HasTimer;
        public TimerKind Timer;
        /// <summary>0 = just started, 1 = out of time.</summary>
        public float Elapsed01;
        public bool Expired;

        /// <summary>Charge shown when an EV's level is not known yet.</summary>
        public const float UnknownCharge = -1f;

        public bool IsEv;
        /// <summary>0 empty, 1 full, <see cref="UnknownCharge"/> until the first CarChargeChanged.</summary>
        public float Charge01 = UnknownCharge;
        public bool IsDirty;
        /// <summary>True once a dirty car has been washed.</summary>
        public bool Washed;

        public void StartTimer(TimerKind kind, float elapsed01)
        {
            HasTimer = true;
            Timer = kind;
            Elapsed01 = elapsed01;
        }
    }
}
