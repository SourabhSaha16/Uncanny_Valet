namespace UncannyValet.Economy
{
    /// <summary>Turns a score into 0 to 3 stars against three thresholds. Pure.</summary>
    public static class StarRating
    {
        public static int Stars(int score, int oneStar, int twoStar, int threeStar)
        {
            if (score >= threeStar) return 3;
            if (score >= twoStar) return 2;
            return score >= oneStar ? 1 : 0;
        }
    }
}
