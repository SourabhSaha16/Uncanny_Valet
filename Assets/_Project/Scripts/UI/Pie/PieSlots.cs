namespace UncannyValet.UI.Pie
{
    /// <summary>Pure layout of the two pies a car can show: alone a pie sits at the base offset, together they sit side by side, pickup on the left.</summary>
    public static class PieSlots
    {
        /// <summary>True when both the pickup pie and the wash pie of the row are showing.</summary>
        public static bool Both(TicketRow row, int steps) => PickupPieRule.IsShown(row, steps) && WashPieRule.IsShown(row, steps);

        /// <summary>Sideways shift (world units) of the pickup pie from the base offset: half the gap to the left when both show.</summary>
        public static float PickupShift(TicketRow row, int steps, float gap) => Both(row, steps) ? -gap * 0.5f : 0f;

        /// <summary>Sideways shift (world units) of the wash pie from the base offset: half the gap to the right when both show.</summary>
        public static float WashShift(TicketRow row, int steps, float gap) => Both(row, steps) ? gap * 0.5f : 0f;
    }
}
