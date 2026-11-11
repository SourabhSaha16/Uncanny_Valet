namespace UncannyValet.Cars
{
    /// <summary>Edits the driver's input before the handling model reads it (future faults such as swapped steering plug in here).</summary>
    public interface IDriveModifier
    {
        /// <summary>Lower orders run first.</summary>
        int Order { get; }

        DriveInput Modify(DriveInput input);
    }
}
