namespace UncannyValet.UI.Kit
{
    /// <summary>Cached strings for small non-negative numbers, so drawing a spot number allocates nothing.</summary>
    public static class SmallNumbers
    {
        public const int Max = 99;

        private static readonly string[] Cache = Build();

        /// <summary>The number as text. Numbers above <see cref="Max"/> allocate; they are not expected on the HUD.</summary>
        public static string Get(int value)
        {
            if (value < 0) value = 0;
            return value <= Max ? Cache[value] : value.ToString();
        }

        private static string[] Build()
        {
            var all = new string[Max + 1];
            for (var i = 0; i <= Max; i++) all[i] = i.ToString();
            return all;
        }
    }
}
