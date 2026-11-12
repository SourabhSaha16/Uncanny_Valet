using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    public class DamageTagTests
    {
        [TestCase(0f, 0)]
        [TestCase(0.004f, 1)]
        [TestCase(0.3f, 30)]
        [TestCase(1f, 100)]
        [TestCase(2f, 100)]
        public void Percent_IsWholeAndAtLeastOneWhileDamaged(float damage, int percent)
            => Assert.AreEqual(percent, DamageTag.Percent(damage));

        [Test]
        public void TheTagShowsOnlyOnceTheTicketRowHasDamage()
        {
            var bus = new EventBus();
            var board = new TicketBoard(bus);
            bus.Publish(new TicketOpened(new TicketId(1), new CarId(7), 10f));
            var rows = new System.Collections.Generic.List<TicketRow>();
            board.CollectRows(rows);
            Assert.IsFalse(DamageTag.IsShown(rows[0]));
            bus.Publish(new CarDamaged(new CarId(7), 0.3f, 0.3f));
            Assert.IsTrue(DamageTag.IsShown(rows[0]));
            Assert.AreEqual(30, DamageTag.Percent(rows[0].Damage01));
            board.Dispose();
        }
    }
}
