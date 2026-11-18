namespace UncannyValet.UI.Sandbox
{
    /// <summary>What the sandbox picker drives: the list of worlds, the current one, and the car reset. Bootstrap implements and registers it.</summary>
    public interface ISandboxControl
    {
        /// <summary>Number of worlds the sandbox can show.</summary>
        int WorldCount { get; }

        /// <summary>Index of the world on screen.</summary>
        int CurrentWorld { get; }

        /// <summary>Display name of a world.</summary>
        string WorldName(int index);

        /// <summary>Shows another world and puts a fresh car in it. False when the index is unknown, the world is already shown or a change is under way.</summary>
        bool TrySelectWorld(int index);

        /// <summary>Replaces the car with a fresh one at the start point. Ignored while a change is under way.</summary>
        void ResetCar();
    }
}
