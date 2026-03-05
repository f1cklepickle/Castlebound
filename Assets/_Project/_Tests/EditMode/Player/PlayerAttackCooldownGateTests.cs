using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackCooldownGateTests
    {
        [Test]
        public void HighRate_AllowsExpectedConsumeCadence()
        {
            var gate = new PlayerAttackCooldownGate();

            Assert.IsTrue(gate.TryConsume(0f, 4f));
            Assert.IsFalse(gate.TryConsume(0.10f, 4f));
            Assert.IsFalse(gate.TryConsume(0.20f, 4f));
            Assert.IsTrue(gate.TryConsume(0.25f, 4f));
            Assert.IsTrue(gate.TryConsume(0.50f, 4f));
        }

        [Test]
        public void TryConsume_BlocksUntilCooldownExpires()
        {
            var gate = new PlayerAttackCooldownGate();

            Assert.IsTrue(gate.TryConsume(0f, 2f));
            Assert.IsFalse(gate.TryConsume(0.25f, 2f));
            Assert.IsTrue(gate.TryConsume(0.5f, 2f));
        }

        [Test]
        public void Reset_ClearsCooldown()
        {
            var gate = new PlayerAttackCooldownGate();

            Assert.IsTrue(gate.TryConsume(0f, 1f));
            Assert.IsFalse(gate.TryConsume(0.25f, 1f));

            gate.Reset();

            Assert.IsTrue(gate.TryConsume(0.25f, 1f));
        }
    }
}
