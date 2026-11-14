using System;
using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>The story lines of one lot's intro card: a few lines of flavour under the lot's name.</summary>
    [Serializable]
    public struct LotStory
    {
        [SerializeField] private LotId _lot;
        [SerializeField, Tooltip("One to three lines under the lot name. Uppercase, up to 40 characters, only A-Z 0-9 and . , : ! ? - + / $ % ' # ( ) *.")]
        private List<string> _introLines;

        public LotStory(LotId lot, List<string> introLines) { _lot = lot; _introLines = introLines; }

        public LotId Lot => _lot;
        public IReadOnlyList<string> IntroLines => _introLines ?? (IReadOnlyList<string>)Array.Empty<string>();
    }
}
