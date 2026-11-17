namespace UncannyValet.Core.Watchers
{
    /// <summary>How important a speech bubble is; when several coincide only the highest shows. Tip over comment over gag line.</summary>
    public enum SpeechPriority
    {
        /// <summary>A floating pedestrian gag line.</summary>
        GagLine = 0,

        /// <summary>A comment on something that just happened.</summary>
        Comment = 1,

        /// <summary>A tip about the lot's mechanics.</summary>
        Tip = 2
    }
}
