using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// The first draft of the results screen's verdict lines (issue 85): one speaker and three lines (failed, cleared, cleared well) for every
    /// god's lot and the finale, and a failed and cleared trainer line per world. Only the starting content of the story text asset: the owner
    /// edits the asset, and setup adds an entry only when it is missing. Lines are uppercase, up to 90 characters, pixel-font characters only.
    /// </summary>
    public static class VerdictTextDrafts
    {
        public static List<VerdictStory> Verdicts() => new List<VerdictStory>
        {
            Crazy(World.Midgard, "MRS. OKAFOR", "OH, DEAR. THE FAIR DESERVED BETTER. GO HOME AND REST.", "WELL PARKED. YOU ARE MORE THAN YOU LOOK, YOUNG MAN.", "MAGNIFICENT! I HAVE NOT SEEN PARKING LIKE THAT IN AGES."),
            Crazy(World.Vanaheim, "FREYJA", "THE HARVEST ROTS ON THE VINE. GO AGAIN, VALET.", "THE HARVEST IS YOURS, VALET.", "A FEAST FOR THE AGES! MY BARNS ARE OVERFLOWING."),
            Crazy(World.Niflheim, "SKADI", "WEAK. THE COLD HAS TAKEN YOUR NERVE.", "YOU DID NOT FREEZE. THAT IS ENOUGH.", "SHARP AS ICE. I HUNT THE WORTHY, AND YOU ARE ONE."),
            Crazy(World.Svartalfheim, "BROKKR AND EITRI", "SCRAP! THE FORGE DEMANDS BETTER THAN THIS.", "SOUND WORK. WE WILL SMITH SOMETHING OF IT.", "FLAWLESS! EITRI, GET THE GOOD HAMMER."),
            Crazy(World.Alfheim, "FREYR", "THE LIGHT FADES. TRY AGAIN WHEN THE SUN RETURNS.", "THE LIGHT HOLDS. YOU MAY PASS.", "RADIANT! THE ELVES SING OF YOUR PARKING."),
            Crazy(World.Muspelheim, "SURTR", "ASH! YOU BURN OUT LIKE A CANDLE.", "THE FLAME TOLERATES YOU. FOR NOW.", "YOU WALK THROUGH FIRE. I WILL REMEMBER YOUR NAME."),
            Crazy(World.Jotunheim, "UTGARD-LOKI", "HA! YOU SAW WHAT I WANTED YOU TO SEE.", "NOT BAD. ALMOST AS IF YOU SAW THROUGH ME.", "IMPOSSIBLE! NO TRICK OF MINE FOOLED YOU."),
            Crazy(World.Helheim, "HEL", "ANOTHER SOUL TURNED AWAY. HOW DULL.", "YOU MAY LEAVE. MOST DO NOT.", "THE DEAD APPLAUD. THAT IS RARE."),
            Verdict(new LotId(World.Asgard, LotKind.Finale), "ODIN", "NOT YET, HEIR OF THUNDER. THE DOORS STAY SHUT.", "THE ALLFATHER HAS WATCHED. THE DOORS OPEN.", "GLORIOUS. THUNDER ITSELF WOULD BE PROUD."),
        };

        public static List<TrainerStory> Trainers() => new List<TrainerStory>
        {
            new TrainerStory(World.Midgard, "NOT YET. EVEN A GOD WITHOUT A HAMMER CAN DO BETTER.", "NOT BAD FOR A GOD WITHOUT A HAMMER."),
            new TrainerStory(World.Vanaheim, "THE FIELDS ARE NOT READY. TRY THE LOT AGAIN.", "THE BARN APPROVES. THE MUD STAYS OUTSIDE."),
            new TrainerStory(World.Niflheim, "TOO SLOW. THE ICE DOES NOT WAIT.", "STEADY HANDS ON THE FROZEN ROAD. GOOD."),
            new TrainerStory(World.Svartalfheim, "TOO LOOSE. A GOOD FIT TAKES PRACTICE.", "A GOOD FIT. THE DWARVES NOD."),
            new TrainerStory(World.Alfheim, "THE LIGHT WAS WITH YOU, BUT NOT ENOUGH.", "BRIGHT WORK. KEEP THAT SHINE."),
            new TrainerStory(World.Muspelheim, "TOO COOL. THE HEAT WANTS MORE FROM YOU.", "YOU HELD THE LINE IN THE HEAT. WELL DONE."),
            new TrainerStory(World.Jotunheim, "THE ILLUSIONS WON THIS ROUND. LOOK CLOSER.", "YOU LOOKED TWICE. SMART."),
            new TrainerStory(World.Helheim, "THE DEAD ARE UNIMPRESSED. TRY AGAIN.", "THE DEAD ARE UNIMPRESSED, BUT YOU PASSED."),
        };

        private static VerdictStory Crazy(World world, string speaker, string failed, string cleared, string well) =>
            Verdict(new LotId(world, LotKind.Crazy), speaker, failed, cleared, well);

        private static VerdictStory Verdict(LotId lot, string speaker, string failed, string cleared, string well) =>
            new VerdictStory(lot, speaker, failed, cleared, well);
    }
}
