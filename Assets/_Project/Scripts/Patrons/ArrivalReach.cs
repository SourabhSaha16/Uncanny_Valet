using System.Collections.Generic;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule for the queue column of lot v2: slot 0 is nearest the curb and later slots lie closer to the road, so a
    /// car can only drive in to a slot when no later slot holds a car in its way.
    /// </summary>
    public static class ArrivalReach
    {
        /// <summary>Marks every slot behind a blocked later slot as blocked too (in place).</summary>
        public static void BlockBehind(List<bool> blocked)
        {
            var roadSideBlocked = false;
            for (var i = blocked.Count - 1; i >= 0; i--)
            {
                if (blocked[i]) roadSideBlocked = true;
                else if (roadSideBlocked) blocked[i] = true;
            }
        }
    }
}
