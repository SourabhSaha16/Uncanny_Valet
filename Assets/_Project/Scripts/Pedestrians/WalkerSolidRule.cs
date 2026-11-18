namespace UncannyValet.Pedestrians
{
    /// <summary>Pure rule: a person who is not solid (passing through a blocker) only turns solid again once they stand clear of every car, so the collider never switches on inside one.</summary>
    public static class WalkerSolidRule
    {
        /// <summary>True when the collider may be on: it is wanted and the person is not inside a car.</summary>
        public static bool Resolve(bool wantSolid, bool insideCar) => wantSolid && !insideCar;
    }
}
