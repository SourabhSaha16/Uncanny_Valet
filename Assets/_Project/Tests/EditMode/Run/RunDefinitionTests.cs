using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UncannyValet.UI.Screens;

namespace UncannyValet.Tests.Run
{
    public sealed class RunDefinitionTests
    {
        private RunFixtures _fx;

        [SetUp] public void SetUp() { _fx = new RunFixtures(); }
        [TearDown] public void TearDown() { _fx.Dispose(); }

        [Test]
        public void Lots_KeepPlayOrder_AndIndexOfFindsThem()
        {
            var run = _fx.MidgardRun();
            Assert.AreEqual(2, run.Lots.Count);
            Assert.AreEqual(0, run.IndexOf(new LotId(World.Midgard, LotKind.Training)));
            Assert.AreEqual(1, run.IndexOf(new LotId(World.Midgard, LotKind.Crazy)));
            Assert.AreEqual(-1, run.IndexOf(new LotId(World.Vanaheim, LotKind.Training)));
        }

        [Test]
        public void TryGetNext_ReturnsTheFollowingLot_ThenFalseAtTheEnd()
        {
            var run = _fx.MidgardRun();
            Assert.IsTrue(run.TryGetNext(new LotId(World.Midgard, LotKind.Training), out var next));
            Assert.AreEqual(LotKind.Crazy, next.Id.Kind);
            Assert.IsFalse(run.TryGetNext(new LotId(World.Midgard, LotKind.Crazy), out _));
            Assert.IsFalse(run.TryGetNext(new LotId(World.Helheim, LotKind.Crazy), out _));
        }

        [Test]
        public void TryGet_AndTryGetFirst()
        {
            var run = _fx.MidgardRun();
            Assert.IsTrue(run.TryGet(new LotId(World.Midgard, LotKind.Crazy), out var crazy));
            Assert.AreEqual("Fair Night at Grandma's", crazy.DisplayName);
            Assert.IsTrue(run.TryGetFirst(out var first));
            Assert.AreEqual(LotKind.Training, first.Id.Kind);
            Assert.IsFalse(_fx.Run().TryGetFirst(out _));
        }

        [Test]
        public void TryValidate_AcceptsTheMidgardRun()
        {
            Assert.IsTrue(_fx.MidgardRun().TryValidate(out var problem), problem);
        }

        [Test]
        public void TryValidate_RejectsDuplicateLotId()
        {
            var run = _fx.Run(_fx.Lot(World.Midgard, LotKind.Training, "A"), _fx.Lot(World.Midgard, LotKind.Training, "B"));
            Assert.IsFalse(run.TryValidate(out var problem));
            StringAssert.Contains("twice", problem);
        }

        [Test]
        public void TryValidate_RejectsDuplicateSceneAndEmptyRun()
        {
            var run = _fx.Run(_fx.Lot(World.Midgard, LotKind.Training, "Same"), _fx.Lot(World.Midgard, LotKind.Crazy, "Same"));
            Assert.IsFalse(run.TryValidate(out var problem));
            StringAssert.Contains("scene", problem);
            Assert.IsFalse(_fx.Run().TryValidate(out _));
        }

        [Test]
        public void TryValidate_RejectsAnInvalidLot()
        {
            var run = _fx.Run(_fx.Lot(World.Asgard, LotKind.Training));
            Assert.IsFalse(run.TryValidate(out _));
        }

        [Test]
        public void LotChoices_ListEveryLotInOrder()
        {
            var choices = LotChoices.From(_fx.MidgardRun());
            Assert.AreEqual(2, choices.Count);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Training), choices[0].Id);
            Assert.AreEqual("Grandma's Diner", choices[0].Name);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Crazy), choices[1].Id);
            Assert.AreEqual(0, LotChoices.From(null).Count);
        }

        [Test]
        public void TitlePicker_FormatsNumberedLines_AndChoosesByIndex()
        {
            var fake = new FakePicker(LotChoices.From(_fx.MidgardRun()));
            var picker = new TitlePicker(fake);
            Assert.AreEqual(2, picker.Count);
            Assert.AreEqual("1 GRANDMA'S DINER (TRAINING)", picker.Line(0));
            Assert.AreEqual("2 FAIR NIGHT AT GRANDMA'S (CRAZY)", picker.Line(1));
            Assert.IsTrue(fake.TryChoose(1));
            Assert.AreEqual(1, fake.Chosen);
            Assert.IsFalse(fake.TryChoose(2));
        }

        [Test]
        public void TitlePicker_TenthLineUsesKeyZero_AndTheListIsCapped()
        {
            Assert.AreEqual("0 X (CRAZY)", TitlePicker.Format(9, new LotChoice(new LotId(World.Midgard, LotKind.Crazy), "x")));
            var many = new System.Collections.Generic.List<LotChoice>();
            for (var i = 0; i < 17; i++) many.Add(new LotChoice(new LotId(World.Midgard, LotKind.Training), "L" + i));
            Assert.AreEqual(TitlePicker.MaxEntries, new TitlePicker(new FakePicker(many)).Count);
        }

        private sealed class FakePicker : ILotPicker
        {
            public FakePicker(System.Collections.Generic.IReadOnlyList<LotChoice> choices) { Choices = choices; }
            public System.Collections.Generic.IReadOnlyList<LotChoice> Choices { get; }
            public int Chosen = -1;
            public bool TryChoose(int index)
            {
                if (index < 0 || index >= Choices.Count) return false;
                Chosen = index;
                return true;
            }
        }
    }
}
