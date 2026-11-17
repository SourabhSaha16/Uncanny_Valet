namespace UncannyValet.Audio
{
    /// <summary>One looping level sound (music, an ambience loop or the escalation layer) that can be silenced for good.</summary>
    public interface IAudioLayer
    {
        /// <summary>True once <see cref="Silence"/> ran: the layer is stopped, at volume 0 and has released its clip.</summary>
        bool IsSilenced { get; }

        /// <summary>Stops the layer, sets its volume to 0 and releases its clip. Later volume changes are ignored.</summary>
        void Silence();
    }
}
