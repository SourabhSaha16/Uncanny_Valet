namespace UncannyValet.Lot.Handmade
{
    /// <summary>A hand-placed bay: its marker name, its number among bays of the same kind (1 up), the kind and the painted rectangle.</summary>
    public readonly struct BayData
    {
        public readonly string Name;
        public readonly int Number;
        public readonly BayKind Kind;
        public readonly OrientedRect Rect;

        public BayData(string name, int number, BayKind kind, OrientedRect rect)
        {
            Name = name;
            Number = number;
            Kind = kind;
            Rect = rect;
        }
    }
}
