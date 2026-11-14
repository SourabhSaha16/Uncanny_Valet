using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// The first draft of every story card text, written from the design doc and issues 106 and 107. They are only the starting content of the
    /// story text asset: the owner edits the asset, and setup never overwrites it. Lines are uppercase, up to 40 characters, pixel-font characters only.
    /// </summary>
    public static class StoryTextDrafts
    {
        public static List<WorldStory> Worlds() => new List<WorldStory>
        {
            new WorldStory(World.Midgard, "MIDGARD", new Color32(0xE3, 0xB9, 0x7A, 255), new Color32(0x6F, 0xB3, 0x6B, 255)),
            new WorldStory(World.Vanaheim, "VANAHEIM", new Color32(0xFF, 0xC8, 0x3A, 255), new Color32(0xB5, 0x40, 0x2E, 255)),
        };

        public static List<LotStory> Lots() => new List<LotStory>
        {
            Lot(World.Midgard, LotKind.Training, "A SUNNY DAY AT THE LOCAL DINER.", "PARK THE CARS, THEN BRING THEM BACK.", "EVERYONE LAUGHS AT THE NAME THOR."),
            Lot(World.Midgard, LotKind.Crazy, "THE TOWN FAIR LIGHTS UP THE NIGHT.", "THE WHOLE TOWN WANTS A PARKING SPOT.", "SHE KNOWS MORE THAN SHE SAYS."),
            Lot(World.Vanaheim, LotKind.Training, "A RED BARN AMONG GOLDEN WHEAT.", "MUDDY CARS NEED A WASH AT THE TROUGH.", "THE WATERWHEEL TURNS SLOWLY."),
            Lot(World.Vanaheim, LotKind.Crazy, "THE STORM IS OVER. THE FEAST IS ON.", "MUD EVERYWHERE, TROUGHS FAR AWAY.", "SOMETHING SMELLS LIKE THUNDER."),
        };

        public static List<PowerStory> Powers() => new List<PowerStory>
        {
            new PowerStory(StoryPower.ThunderStride, "DASH IN A FLASH OF LIGHTNING, OR BOOST YOUR CAR.",
                "WELL PARKED, DEAR. THE LIGHTNING IS YOURS NOW. MIND THE FAIR ON YOUR WAY OUT."),
            new PowerStory(StoryPower.StormWind, "BLOW A GUST THAT CLEARS A LANE OF CARS AND PEOPLE.",
                "THE STORM TOLD ME YOU WOULD COME. TAKE ITS WIND AND LEAVE MY BARN STANDING."),
            new PowerStory(StoryPower.FreezeTime, "STOP THE WORLD FOR A FEW SECONDS WHILE YOU MOVE.",
                "TIME IS A RIVER. FOR A MOMENT YOU MAY HOLD IT STILL."),
            new PowerStory(StoryPower.MjolnirRecall, "CALL THE HAMMER BACK TO YOUR HAND.",
                "THE HAMMER HAS MISSED YOU, HEIR OF THUNDER."),
        };

        private static LotStory Lot(World world, LotKind kind, params string[] lines) =>
            new LotStory(new LotId(world, kind), new List<string>(lines));
    }
}
