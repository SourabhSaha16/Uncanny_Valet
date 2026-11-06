namespace UncannyValet.Patrons
{
    /// <summary>Pure rule: a new patron car may arrive only if few are waiting and the arrival point is clear.</summary>
    public static class ArrivalGate
    {
        public static bool CanArrive(int waitingTickets, int maxWaiting, bool arrivalPointBlocked)
        {
            return waitingTickets < maxWaiting && !arrivalPointBlocked;
        }
    }
}
