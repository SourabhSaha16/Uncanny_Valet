using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    /// <summary>A muddy car and its wash station show one symbol: the HUD icon of the Muddy modifier and the station decal share a single bitmap.</summary>
    public sealed class SharedMudSymbolTests
    {
        [Test]
        public void TheMuddyCarIcon_AndTheWashStationDecal_ResolveToTheSameBitmap()
        {
            var carIcon = PixelIconData.Rows(ModifierIcons.ToPixel(ModifierIcon.Mud));
            Assert.AreSame(carIcon, WashIconArt.Rows, "one bitmap, not two copies");
            Assert.AreSame(MudIconArt.Rows, WashIconArt.Rows);
        }

        [Test]
        public void EveryKeyOfTheSymbol_HasTheSameColour_OnTheCarIconAndTheStationDecal()
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
