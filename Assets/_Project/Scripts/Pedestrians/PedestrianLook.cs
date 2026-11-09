namespace UncannyValet.Pedestrians
{
    /// <summary>Which skin row, hair row and outfit a pedestrian wears.</summary>
    public readonly struct PedestrianLook
    {
        public readonly int Skin;
        public readonly int Hair;
        public readonly int Outfit;

        public PedestrianLook(int skin, int hair, int outfit) { Skin = skin; Hair = hair; Outfit = outfit; }
    }
}
