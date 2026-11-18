namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// The sprites of the Valhalla Midgard lot art (sliced into Assets/_Project/Art/ValhallaMidgard/Lot by tools/slice-valhalla.py). The name of a value is the
    /// file name in PascalCase (AsphaltPlain is asphalt_plain.png, Bay1 is bay_1.png). Stored as integers in <see cref="ValhallaArt"/>: only ever append.
    /// </summary>
    public enum ValhallaPiece
    {
        AsphaltPlain, AsphaltSkid, CentreLineH, CrosswalkH, CrosswalkV, SidewalkPlain, HedgeBlock,
        RoofFloor, RoofFrameTop, RoofFrameBottom, RoofFrameLeft,
        WallWindow1, WallWindow2, WallWindow3, WallWindow4, WallDoorDouble, WallDoorSingle, WallPlain, WallPilasterLeft, WallPilasterRight,
        AwningMidA, AwningMidB, FlowerboxLarge, FlowerboxMedium, FlowerboxSmall,
        DinerSign, GadgetHvacA, GadgetHvacB, GadgetDish, GadgetDome,
        PadDropOff, PadValetPickupLeft, PadValetPickupRight, BayDashedA,
        Bay1, Bay2, Bay3, Bay4, Bay5, Bay6, Bay7, Bay8,
        TreeLarge, TreeSmall, Bush, PlanterFlowers, HedgePlanter, Bench, LampPost, Bin, Hydrant, Mailbox, NewspaperBox, SignCoffee, Meter,
    }
}
