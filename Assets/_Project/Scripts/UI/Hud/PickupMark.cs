
namespace UncannyValet.UI.Hud
{
    /// <summary>Pure rule for the pickup marks (patron mood bubble): which rows are wanted back.</summary>
    public static class PickupMark
    {
        /// <summary>True when the row's car is wanted back, so the mark shows.</summary>
        public static bool IsRequested(TicketRow row) => row.State == TicketRowState.PickupRequested;
    }
}
