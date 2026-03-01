using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.Input;

namespace Castlebound.Tests.Input
{
    public class TouchRepairButtonTests
    {
        private GameObject _buttonGo;
        private TouchRepairButton _button;
        private GameObject _barrierGo;

        [SetUp]
        public void SetUp()
        {
            _buttonGo = new GameObject("TouchRepairButton");
            _button = _buttonGo.AddComponent<TouchRepairButton>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_barrierGo != null)
                Object.DestroyImmediate(_barrierGo);

            Object.DestroyImmediate(_buttonGo);
        }

        [Test]
        public void IsVisible_IsFalse_WhenNoDamagedBarriersExist()
        {
            _button.SetBarrierSource(new List<BarrierHealth>());
            _button.UpdateProximity(Vector2.zero, checkRadius: 5f);

            Assert.IsFalse(_button.IsVisible,
                "Button should be hidden when no damaged barriers exist.");
        }

        [Test]
        public void IsVisible_IsFalse_WhenDamagedBarrierIsOutOfRange()
        {
            var health = CreateDamagedBarrierAt(new Vector2(100f, 100f), out _barrierGo);
            _button.SetBarrierSource(new List<BarrierHealth> { health });

            _button.UpdateProximity(Vector2.zero, checkRadius: 5f);

            Assert.IsFalse(_button.IsVisible,
                "Button should be hidden when damaged barrier is outside check radius.");
        }

        [Test]
        public void IsVisible_IsTrue_WhenDamagedBarrierIsInRange()
        {
            var health = CreateDamagedBarrierAt(new Vector2(1f, 0f), out _barrierGo);
            _button.SetBarrierSource(new List<BarrierHealth> { health });

            _button.UpdateProximity(Vector2.zero, checkRadius: 5f);

            Assert.IsTrue(_button.IsVisible,
                "Button should be visible when a damaged barrier is within check radius.");
        }

        [Test]
        public void IsVisible_ReturnsFalse_WhenBarrierInRangeButNotBroken()
        {
            _barrierGo = new GameObject("Barrier");
            _barrierGo.transform.position = new Vector3(1f, 0f, 0f);
            var health = _barrierGo.AddComponent<BarrierHealth>();
            _button.SetBarrierSource(new List<BarrierHealth> { health });

            _button.UpdateProximity(Vector2.zero, checkRadius: 5f);

            Assert.IsFalse(_button.IsVisible,
                "Button should be hidden when nearby barrier is not broken.");
        }

        [Test]
        public void IsVisible_BecomesHidden_AfterBarrierMovesOutOfRange()
        {
            var health = CreateDamagedBarrierAt(new Vector2(1f, 0f), out _barrierGo);
            _button.SetBarrierSource(new List<BarrierHealth> { health });

            _button.UpdateProximity(Vector2.zero, checkRadius: 5f);
            Assert.IsTrue(_button.IsVisible, "Precondition: barrier in range.");

            _barrierGo.transform.position = new Vector3(100f, 100f, 0f);
            _button.UpdateProximity(Vector2.zero, checkRadius: 5f);

            Assert.IsFalse(_button.IsVisible,
                "Button should hide after barrier moves out of range.");
        }

        [Test]
        public void FireRepair_InvokesOnRepairRequested()
        {
            var repairCalled = false;
            _button.OnRepairRequested += () => repairCalled = true;

            _button.FireRepair();

            Assert.IsTrue(repairCalled,
                "FireRepair should invoke the OnRepairRequested event.");
        }

        private static BarrierHealth CreateDamagedBarrierAt(Vector2 position, out GameObject go)
        {
            go = new GameObject("Barrier");
            go.transform.position = new Vector3(position.x, position.y, 0f);
            var health = go.AddComponent<BarrierHealth>();
            health.TakeDamage(999);
            return health;
        }
    }
}
