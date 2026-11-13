using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI
{
    /// <summary>One service modifier of a ticket's car as the board shows it. Plain data written by <see cref="TicketNeedsTracker"/>.</summary>
    public sealed class RowModifier
    {
        public ModifierId Id;
        /// <summary>The kind of station that clears it.</summary>
        public StationKind ClearedAt;
        /// <summary>The small picture drawn next to the plate while it is open.</summary>
        public PixelIcon Icon;
        /// <summary>Status word while its station works on it (for example Washing).</summary>
        public string ActiveWord = string.Empty;
        /// <summary>Status word once it is cleared (for example Washed).</summary>
        public string DoneWord = string.Empty;
        /// <summary>0 untouched, 1 cleared.</summary>
        public float Progress01;
        public bool IsCleared;
        /// <summary>Set when it clears, dropped by the next CarParked: the status shows the done word only until the car is parked again.</summary>
        public bool ShowDone;

        /// <summary>The modifier is still open.</summary>
        public bool IsOpen => !IsCleared;
    }
}
