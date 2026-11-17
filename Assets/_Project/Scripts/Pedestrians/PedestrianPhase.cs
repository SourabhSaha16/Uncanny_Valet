namespace UncannyValet.Pedestrians
{
    /// <summary>What a pooled person is doing. Ambient people use Roaming, Arriving and Inside; patrons use ToDoor to Waiting.</summary>
    public enum PedestrianPhase
    {
        /// <summary>Not on the lot (in the pool, or an ambient person waiting to appear).</summary>
        Idle,
        /// <summary>An ambient person walking from where they are to the diner door.</summary>
        Arriving,
        /// <summary>A patron walking from their car, via the crosswalks, to the diner door.</summary>
        ToDoor,
        /// <summary>Inside the diner, out of sight.</summary>
        Inside,
        /// <summary>A patron walking from the diner door, via the crosswalks, to the curb.</summary>
        ToCurb,
        /// <summary>A patron standing at the curb until the car is delivered or the ticket ends.</summary>
        Waiting,
        /// <summary>A delivered patron walking to their car to board it.</summary>
        Leaving,
        /// <summary>An ambient person wandering the sidewalks and crosswalks until they decide to visit the diner.</summary>
        Roaming
    }
}
