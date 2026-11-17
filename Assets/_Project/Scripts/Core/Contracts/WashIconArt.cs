using UncannyValet.Core.Modifiers;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The symbol on a wash station: the mud splat of the Muddy modifier, the very bitmap the HUD shows for a muddy car beside its plate, so a
    /// muddy car and the station that cleans it show one picture. The bitmap and its colours live only in <see cref="MudIconArt"/>.
    /// </summary>
    public static class WashIconArt
    {
        /// <summary>The bitmap rows, top row first (the same array the muddy car's HUD icon draws).</summary>
        public static string[] Rows => MudIconArt.Rows;

        /// <summary>The colour of a bitmap key; false for the clear key '.'.</summary>
        public static bool TryColor(char key, out Color32 color) => MudIconArt.TryColor(key, out color);
    }
}
