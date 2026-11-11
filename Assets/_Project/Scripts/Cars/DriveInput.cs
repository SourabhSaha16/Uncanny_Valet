namespace UncannyValet.Cars
{
    /// <summary>One frame of driver input: throttle and steer in -1..1 and the handbrake.</summary>
    public readonly struct DriveInput
    {
        public readonly float Throttle;
        public readonly float Steer;
        public readonly bool Drift;

        public DriveInput(float throttle, float steer, bool drift)
        {
            Throttle = throttle;
            Steer = steer;
            Drift = drift;
        }
    }
}
