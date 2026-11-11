using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class DriveModifierStackTests
    {
        private sealed class Fn : IDriveModifier
        {
            private readonly System.Func<DriveInput, DriveInput> _f;
            public Fn(int order, System.Func<DriveInput, DriveInput> f) { Order = order; _f = f; }
            public int Order { get; }
            public DriveInput Modify(DriveInput input) => _f(input);
        }

        [Test]
        public void EmptyStack_ReturnsInputUnchanged()
        {
            var result = new DriveModifierStack().Apply(new DriveInput(1f, -0.5f, true));
            Assert.AreEqual(1f, result.Throttle);
            Assert.AreEqual(-0.5f, result.Steer);
            Assert.IsTrue(result.Drift);
        }

        [Test]
        public void SwappedSteering_FlipsSteerBeforeTheModelReadsIt()
        {
            var stack = new DriveModifierStack();
            stack.Add(new Fn(0, i => new DriveInput(i.Throttle, -i.Steer, i.Drift)));
            Assert.AreEqual(-0.7f, stack.Apply(new DriveInput(0f, 0.7f, false)).Steer, 1e-5f);
        }

        [Test]
        public void ModifiersRunInOrderRegardlessOfAddOrder()
        {
            var stack = new DriveModifierStack();
            stack.Add(new Fn(2, i => new DriveInput(i.Throttle * 2f, i.Steer, i.Drift)));
            stack.Add(new Fn(1, i => new DriveInput(i.Throttle + 1f, i.Steer, i.Drift)));
            Assert.AreEqual(4f, stack.Apply(new DriveInput(1f, 0f, false)).Throttle, 1e-5f); // (1 + 1) * 2
        }

        [Test]
        public void EqualOrdersKeepInsertionOrder()
        {
            var stack = new DriveModifierStack();
            stack.Add(new Fn(1, i => new DriveInput(i.Throttle + 1f, i.Steer, i.Drift)));
            stack.Add(new Fn(1, i => new DriveInput(i.Throttle * 3f, i.Steer, i.Drift)));
            Assert.AreEqual(6f, stack.Apply(new DriveInput(1f, 0f, false)).Throttle, 1e-5f);
        }

        [Test]
        public void RemovedModifierNoLongerApplies()
        {
            var stack = new DriveModifierStack();
            var m = new Fn(0, i => new DriveInput(0f, 0f, false));
            stack.Add(m);
            Assert.IsTrue(stack.Remove(m));
            Assert.AreEqual(0, stack.Count);
            Assert.AreEqual(1f, stack.Apply(new DriveInput(1f, 0f, false)).Throttle);
            Assert.IsFalse(stack.Remove(m));
        }
    }
}
