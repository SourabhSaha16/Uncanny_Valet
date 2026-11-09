using System.Text;

namespace UncannyValet.UI.Kit
{
    /// <summary>A prefix plus a number, rebuilt only when the number changes, so a per-frame draw allocates nothing.</summary>
    public sealed class CachedText
    {
        private readonly string _prefix;
        private readonly StringBuilder _text = new StringBuilder(16);
        private int _value = int.MinValue;

        public CachedText(string prefix)
        {
            _prefix = prefix ?? string.Empty;
        }

        public StringBuilder Text => _text;

        /// <summary>Sets the number; returns true when the text was rebuilt.</summary>
        public bool Set(int value)
        {
            if (value == _value) return false;
            _value = value;
            _text.Clear().Append(_prefix).Append(value);
            return true;
        }
    }
}
