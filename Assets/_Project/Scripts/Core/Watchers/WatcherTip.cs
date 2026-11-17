using System;
using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>One tip: the line and the situation that makes the watcher say it.</summary>
    [Serializable]
    public sealed class WatcherTip
    {
        [SerializeField] private TipSituation _situation;
        [SerializeField, Tooltip("The line. Uppercase, up to 40 characters, only A-Z 0-9 and . , : ! ? - + / $ % ' # ( ) *.")]
        private string _text;

        public WatcherTip(TipSituation situation, string text) { _situation = situation; _text = text; }

        public TipSituation Situation => _situation;
        public string Text => _text;
    }
}
