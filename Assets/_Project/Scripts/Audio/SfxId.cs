namespace UncannyValet.Audio
{
    /// <summary>The sound effects the director can play.</summary>
    public enum SfxId
    {
        CarEnter,
        CarExit,
        CarParked,
        BlessingEarned,
        PickupRequested,
        CarVanished,
        CarArrives,
        NightEnded,
        ButtonClick,
        Charged,
        Washed,
        StationStart,
        PedestrianBump,
        CarBump,
        TyreChirp,
        TyreScreech,
        PerfectDing = 16,
        // 17 and 20 to 24 are retired (the speed boost and Park Smash sounds): their numbers are never reused or shifted.
        TimeStop = 18,
        TimeResume = 19,
        ThunderCrack = 25
    }
}
