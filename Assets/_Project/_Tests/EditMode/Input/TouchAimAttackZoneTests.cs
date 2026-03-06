using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Castlebound.Gameplay.Input;

namespace Castlebound.Tests.Input
{
    public class TouchAimAttackZoneTests
    {
        private GameObject _go;
        private TouchAimAttackZone _zone;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TouchAimAttackZone");
            _zone = _go.AddComponent<TouchAimAttackZone>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void FacingDirection_IsZero_BeforeTouchBegins()
        {
            Assert.AreEqual(Vector2.zero, _zone.FacingDirection,
                "FacingDirection should be zero before any touch input.");
        }

        [Test]
        public void IsFiring_IsFalse_BeforeTouchBegins()
        {
            Assert.IsFalse(_zone.IsFiring,
                "IsFiring should be false before any touch input.");
        }

        [Test]
        public void SimulateAimInput_BelowDeadzone_UpdatesFacingDirection()
        {
            _zone.AttackDeadzone = 50f;
            var smallDelta = new Vector2(10f, 0f);

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(smallDelta);

            Assert.AreNotEqual(Vector2.zero, _zone.FacingDirection,
                "FacingDirection should update even when drag is below deadzone.");
        }

        [Test]
        public void SimulateAimInput_BelowDeadzone_DoesNotFire()
        {
            _zone.AttackDeadzone = 50f;
            var smallDelta = new Vector2(10f, 0f);

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(smallDelta);

            Assert.IsFalse(_zone.IsFiring,
                "IsFiring should remain false when drag magnitude is below deadzone.");
        }

        [Test]
        public void SimulateAimInput_AboveDeadzone_UpdatesFacingDirection()
        {
            _zone.AttackDeadzone = 50f;
            var largeDelta = new Vector2(100f, 0f);

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(largeDelta);

            Assert.AreNotEqual(Vector2.zero, _zone.FacingDirection,
                "FacingDirection should update when drag is above deadzone.");
        }

        [Test]
        public void SimulateAimInput_AboveDeadzone_SetsFiringTrue()
        {
            _zone.AttackDeadzone = 50f;
            var largeDelta = new Vector2(100f, 0f);

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(largeDelta);

            Assert.IsTrue(_zone.IsFiring,
                "IsFiring should be true when drag magnitude exceeds deadzone.");
        }

        [Test]
        public void SimulateAimInput_AboveDeadzone_FacingDirectionMatchesDragDirection()
        {
            _zone.AttackDeadzone = 50f;

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(new Vector2(100f, 0f));

            Assert.Greater(_zone.FacingDirection.x, 0f,
                "Facing should point right when dragging right.");
            Assert.AreEqual(0f, _zone.FacingDirection.y, 0.001f,
                "Facing Y should be zero when dragging purely right.");
        }

        [Test]
        public void SimulatePointerUp_StopsFiring()
        {
            _zone.AttackDeadzone = 50f;

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(new Vector2(100f, 0f));
            Assert.IsTrue(_zone.IsFiring, "Precondition: should be firing.");

            _zone.SimulatePointerUp();

            Assert.IsFalse(_zone.IsFiring,
                "IsFiring should be false after pointer is released.");
        }

        [Test]
        public void SimulatePointerUp_ResetsFacingDirection()
        {
            _zone.AttackDeadzone = 50f;

            _zone.SimulatePointerDown(Vector2.zero);
            _zone.SimulateAimInput(new Vector2(100f, 0f));
            _zone.SimulatePointerUp();

            Assert.AreEqual(Vector2.zero, _zone.FacingDirection,
                "FacingDirection should reset to zero after pointer is released.");
        }

        [Test]
        public void AttackDeadzone_IsPubliclySettable()
        {
            _zone.AttackDeadzone = 75f;

            Assert.AreEqual(75f, _zone.AttackDeadzone,
                "AttackDeadzone should be readable and writable for inspector tuning.");
        }

        [Test]
        public void MousePointerEvents_AreIgnored_ByRuntimeHandlers()
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            var pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1, // Mouse pointer IDs are negative.
                position = new Vector2(300f, 300f)
            };

            _zone.OnPointerDown(pointerData);
            _zone.OnDrag(pointerData);

            Assert.AreEqual(Vector2.zero, _zone.FacingDirection,
                "Mouse pointer events should not drive touch aim direction.");
            Assert.IsFalse(_zone.IsFiring,
                "Mouse pointer events should not trigger touch-fire state.");

            Object.DestroyImmediate(eventSystemGo);
        }
    }
}
