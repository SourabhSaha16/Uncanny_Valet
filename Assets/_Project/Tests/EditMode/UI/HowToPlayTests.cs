using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Screens;

namespace UncannyValet.Tests.UI
{
    public class HowToPlayTests
    {
        [Test]
        public void Gate_ShowsOnTheFirstStartOnly()
        {
            var gate = new HowToPlayGate();
            Assert.IsFalse(gate.WasShown);
            Assert.IsTrue(gate.TryShow());
            Assert.IsTrue(gate.WasShown);
            Assert.IsFalse(gate.TryShow());
            Assert.IsFalse(gate.TryShow());
        }

        [Test]
        public void Gate_NewInstanceShowsAgain_LikeANewPageLoad()
        {
            new HowToPlayGate().TryShow();
            Assert.IsTrue(new HowToPlayGate().TryShow());
        }

        [Test]
        public void Wrap_BreaksAtSpacesAndNeverExceedsTheLimit()
        {
            const string text = "Customer cars will sometimes have SPECIAL REQUESTs like car wash or EV charging";
            var spans = new List<WordWrap.Span>();
            WordWrap.Wrap(text, 30, spans);
            Assert.Greater(spans.Count, 1);
            foreach (var span in spans)
            {
                Assert.LessOrEqual(span.Length, 30);
                Assert.AreNotEqual(' ', text[span.Start]);
            }
        }

        [Test]
        public void Wrap_ShortTextIsOneSpan_AndLongWordIsSplit()
        {
            var spans = new List<WordWrap.Span>();
            WordWrap.Wrap("Short", 30, spans);
            Assert.AreEqual(1, spans.Count);
            spans.Clear();
            WordWrap.Wrap("ABCDEFGHIJ", 4, spans);
            Assert.AreEqual(3, spans.Count);
        }

        [Test]
        public void Mask_MarksEveryOccurrenceOfEveryPhrase()
        {
            const string text = "paid TIPS $$ for PICK UPs and NOT for DROP OFFs";
            var mask = HighlightMask.Build(text, HowToPlayText.Highlights);
            Assert.IsTrue(mask[text.IndexOf("TIPS", System.StringComparison.Ordinal)]);
            Assert.IsTrue(mask[text.IndexOf("$$", System.StringComparison.Ordinal) + 1]);
            Assert.IsTrue(mask[text.IndexOf("PICK UP", System.StringComparison.Ordinal) + 6]);
            Assert.IsTrue(mask[text.IndexOf("DROP OFF", System.StringComparison.Ordinal)]);
            Assert.IsFalse(mask[0]);
            Assert.IsFalse(mask[text.IndexOf("NOT", System.StringComparison.Ordinal)]);
        }

        [Test]
        public void Text_FitsTheCanvasAndUsesOnlyGlyphsTheFontHas()
        {
            var maxChars = (PixelGrid.Width - 62) / PixelTextLayout.Advance;
            foreach (var group in new[] { HowToPlayText.Rules, HowToPlayText.Controls })
                foreach (var line in group)
                {
                    var spans = new List<WordWrap.Span>();
                    WordWrap.Wrap(line, maxChars, spans);
                    Assert.LessOrEqual(spans.Count, 3, line);
                    foreach (var c in line)
                        Assert.IsTrue(PixelGlyphs.Chars.Contains(char.ToUpperInvariant(c).ToString()), "glyph for " + c);
                }
            Assert.AreEqual(5, HowToPlayText.Rules.Length);
            Assert.AreEqual(3, HowToPlayText.Controls.Length);
        }
    }
}
