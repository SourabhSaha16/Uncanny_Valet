namespace UncannyValet.Core.Events
{
    /// <summary>The final numbers of one shift, shown on the results screen.</summary>
    public readonly struct ShiftResult
    {
        public readonly int Score;
        public readonly int Stars;
        public readonly int Complaints;
        public readonly int OneStarScore;
        public readonly int TwoStarScore;
        public readonly int ThreeStarScore;

        public ShiftResult(int score, int stars, int complaints, int oneStarScore, int twoStarScore, int threeStarScore)
        {
            Score = score; Stars = stars; Complaints = complaints;
            OneStarScore = oneStarScore; TwoStarScore = twoStarScore; ThreeStarScore = threeStarScore;
        }
    }
}
