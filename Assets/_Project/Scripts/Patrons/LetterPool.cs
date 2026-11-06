namespace UncannyValet.Patrons
{
    /// <summary>Hands out the lowest free car letter (A, B, C...) and takes it back when a ticket ends.</summary>
    public sealed class LetterPool
    {
        public const char First = 'A';
        public const int Count = 26;

        /// <summary>Shown when all letters are in use (not expected with the waiting cap).</summary>
        public const char Overflow = '?';

        private readonly bool[] _taken = new bool[Count];

        /// <summary>Takes the lowest free letter.</summary>
        public char Acquire()
        {
            for (var i = 0; i < Count; i++)
            {
                if (_taken[i]) continue;
                _taken[i] = true;
                return (char)(First + i);
            }
            return Overflow;
        }

        /// <summary>Frees every letter.</summary>
        public void Reset() => System.Array.Clear(_taken, 0, Count);

        /// <summary>Frees a letter so the next car can use it. Unknown characters are ignored.</summary>
        public void Release(char letter)
        {
            var i = letter - First;
            if (i >= 0 && i < Count) _taken[i] = false;
        }
    }
}
