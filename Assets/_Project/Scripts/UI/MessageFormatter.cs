using System.Text;

namespace UncannyValet.UI
{
    /// <summary>Text for the boss panel and the run-lost card. Pure, so it is unit-tested.</summary>
    public static class MessageFormatter
    {
        public const string Header = "BOSS";
        public const string LostTitle = "The boss is finished with you";
        public const string LostButton = "New run";

        /// <summary>The order text, e.g. "Take car B to spot 7."</summary>
        public static string Order(char letter, int spot) => "Take car " + letter + " to spot " + spot + ".";

        public static string Badge(MessageStatus status)
        {
            switch (status)
            {
                case MessageStatus.Done: return "DONE";
                case MessageStatus.Failed: return "FAILED";
                case MessageStatus.Cancelled: return "CANCELLED";
                default: return "OPEN";
            }
        }

        /// <summary>Writes "[X] [X] [ ]": one mark per allowed strike, X for each given.</summary>
        public static void AppendStrikes(StringBuilder text, int given, int max)
        {
            for (var i = 0; i < max; i++)
            {
                if (i > 0) text.Append(' ');
                text.Append(i < given ? "[X]" : "[ ]");
            }
        }

        /// <summary>Writes "Strikes: 3 of 3".</summary>
        public static void AppendLostDetail(StringBuilder text, int given, int max)
        {
            text.Append("Strikes: ").Append(given).Append(" of ").Append(max);
        }
    }
}
