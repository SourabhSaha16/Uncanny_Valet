using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    /// <summary>The wash station decal (the mud splat) only uses colours of the HUD pixel palette.</summary>
    public sealed class SharedMudSymbolTests
    {
        [Test]
        public void EveryKeyOfTheStationDecal_HasTheSameColourAsThePixelPalette()
        {
            foreach (var row in WashIconArt.Rows)
                foreach (var key in row)
                {
                    var onStation = WashIconArt.TryColor(key, out var stationColour);
                    var index = PixelIconData.Keys.IndexOf(key);
                    Assert.AreEqual(onStation, index >= 0, "key " + key);
                    if (onStation) Assert.AreEqual(PixelIconData.Colors[index], stationColour, "key " + key);
                }
        }
    }
}
