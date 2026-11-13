using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>Pure: turns a lot snapshot and the cost table into workload W and pressure P (spec section 2).</summary>
    public static class WorkloadEstimator
    {
        public static WorkloadReading Estimate(in LotSnapshot lot, WorkloadSettings s)
        {
            var seconds = 0f;
            var washDemand = 0;
            var chargeDemand = 0;
            var leastPatience = 1f;
            var anyTimer = false;
            for (var i = 0; i < lot.Cars.Count; i++)
            {
                var c = lot.Cars[i];
                seconds += TaskSeconds(c, s);
                seconds += ServiceSeconds(lot, c, s, ref washDemand, ref chargeDemand);
                if (c.State != CarLotState.PickupRequested) continue;
                anyTimer = true;
                if (c.Patience01 < leastPatience) leastPatience = c.Patience01;
            }
            seconds += EvictSeconds(lot, StationKind.Wash, washDemand, s);
            seconds += EvictSeconds(lot, StationKind.Charging, chargeDemand, s);
            return new WorkloadReading(seconds, seconds / s.HorizonSeconds, anyTimer ? 1f - leastPatience : 0f);
        }

        private static float TaskSeconds(in CarSnapshot c, WorkloadSettings s)
        {
            if (c.State == CarLotState.PickupRequested) return s.PickupCost;
            if (c.AwaitingPark) return s.ParkCost;
            var soon = c.State != CarLotState.Arriving && c.SecondsToPickup <= s.PickupSoonSeconds;
            return soon ? s.PickupSoonCost : 0f;
        }

        // Wash: the trip (unless already in the slot) plus the wash left. Charge: the trip to a bay (charging runs alone).
        private static float ServiceSeconds(in LotSnapshot lot, in CarSnapshot c, WorkloadSettings s,
            ref int washDemand, ref int chargeDemand)
        {
            var seconds = 0f;
            if (c.NeedsAt(StationKind.Wash))
            {
                var inWash = Holds(lot, StationKind.Wash, c.Car);
                if (!inWash) { washDemand++; seconds += s.WashTripCost; }
                seconds += s.WashSecondsFull * c.LeftAt(StationKind.Wash);
            }
            if (c.NeedsAt(StationKind.Charging) && !Holds(lot, StationKind.Charging, c.Car))
            {
                chargeDemand++;
                seconds += s.ChargeTripCost;
            }
            return seconds;
        }

        // A finished or squatting car only costs an eviction when a car that needs the station cannot get a slot.
        private static float EvictSeconds(in LotSnapshot lot, StationKind kind, int demand, WorkloadSettings s)
        {
            var stuck = lot.CountSlots(kind, StationSlotState.Blocked) + lot.CountSlots(kind, StationSlotState.Squatted);
            var open = lot.CountSlots(kind, StationSlotState.Free) + lot.CountSlots(kind, StationSlotState.Reserved);
            var missing = demand - open;
            var evictions = missing < stuck ? missing : stuck;
            return evictions > 0 ? evictions * s.EvictCost : 0f;
        }

        private static bool Holds(in LotSnapshot lot, StationKind kind, CarId car)
        {
            for (var i = 0; i < lot.Slots.Count; i++)
                if (lot.Slots[i].Kind == kind && lot.Slots[i].Car == car) return true;
            return false;
        }
    }
}
