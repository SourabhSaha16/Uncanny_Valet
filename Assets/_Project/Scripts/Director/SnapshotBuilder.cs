using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>Turns the car book into a <see cref="LotSnapshot"/>, reusing its buffers so a capture allocates nothing.</summary>
    internal sealed class SnapshotBuilder
    {
        private readonly List<CarSnapshot> _cars = new List<CarSnapshot>();
        private readonly List<StationSlot> _slots = new List<StationSlot>();

        /// <param name="secondsPerMinute">Real seconds in one game minute.</param>
        public LotSnapshot Build(CarBook book, IReadOnlyList<float> angryTimes, float nowSeconds, float nowMinutes,
            float secondsPerMinute)
        {
            _cars.Clear();
            var nextPickup = float.PositiveInfinity;
            var occupiedRegular = 0;
            foreach (var r in book.Records)
            {
                var toPickup = SecondsToPickup(r, nowMinutes, secondsPerMinute);
                if (toPickup < nextPickup) nextPickup = toPickup;
                if (r.SpotId != CarRecord.NoSpot && r.SpotId <= book.RegularSpots) occupiedRegular++;
                _cars.Add(new CarSnapshot(r.Car, r.State, r.IsDirty, r.IsEv, r.WashDone, r.ChargeDone, r.AwaitingPark,
                    r.IsDirty && !r.WashDone ? 1f - r.Clean01 : 0f, r.Patience01, toPickup,
                    r.StayMinutes * secondsPerMinute, r.ShiftedMinutes * secondsPerMinute));
            }
            BuildSlots(book);
            var total = TotalSpots(book);
            var free = total - occupiedRegular;
            return new LotSnapshot(_cars, _slots, free < 0 ? 0 : free, total, nextPickup, nowSeconds, angryTimes);
        }

        private static int TotalSpots(CarBook book)
            => book.ActiveSpots > 0 && book.ActiveSpots < book.RegularSpots ? book.ActiveSpots : book.RegularSpots;

        // Seconds until the patron wants the car back (wherever it is); infinity once requested or while the stay has not started.
        private static float SecondsToPickup(CarRecord r, float nowMinutes, float secondsPerMinute)
        {
            if (r.PickupRequested || float.IsNaN(r.StayStartedAtMinute)) return float.PositiveInfinity;
            var minutes = r.StayStartedAtMinute +r.StayMinutes + r.ShiftedMinutes - nowMinutes;
            return (minutes < 0f ? 0f : minutes) * secondsPerMinute;
        }

        private void BuildSlots(CarBook book)
        {
            _slots.Clear();
            if (book.WashOpen) AddSlots(book.WashOccupants, 1, StationKind.Wash, book);
            if (book.ChargingOpen) AddSlots(book.BayOccupants, book.ChargingBays, StationKind.Charging, book);
        }

        private void AddSlots(IReadOnlyList<CarRecord> occupants, int count, StationKind kind, CarBook book)
        {
            var drivenNeeding = DrivenNeeding(book, kind);
            var empty = 0;
            for (var i = 0; i < count; i++)
            {
                if (i < occupants.Count)
                {
                    var o = occupants[i];
                    var state = SlotStateRule.Occupied(kind, o.IsDirty, o.WashDone, o.IsEv, o.ChargeDone);
                    _slots.Add(new StationSlot(kind, state, o.Car));
                    continue;
                }
                _slots.Add(new StationSlot(kind, SlotStateRule.Empty(empty, drivenNeeding), CarId.None));
                empty++;
            }
        }

        private static int DrivenNeeding(CarBook book, StationKind kind)
        {
            var n = 0;
            foreach (var r in book.Records)
                if (r.Driven && (kind == StationKind.Wash ? r.NeedsWash : r.NeedsCharge)) n++;
            return n;
        }
    }
}
