using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Input;

namespace Castlebound.Tests.Input
{
    public class TouchMovementZoneTests
    {
        private GameObject _go;
        private TouchMovementZone _zone;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TouchMovementZone");
            _zone = _go.AddComponent<TouchMovementZone>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void MoveVector_IsZero_BeforeTouchBegins()
        {
            Assert.AreEqual(Vector2.zero, _zone.MoveVector,
                "MoveVector should be zero before any touch input.");
        }

        [Test]
        public void SimulatePointerDown_RecordsAnchorPosition()
        {
            var touchPos = new Vector2(100f, 200f);

            _zone.SimulatePointerDown(touchPos);

            Assert.AreEqual(touchPos, _zone.AnchorPosition,
                "Anchor should be set to the initial touch position.");
        }

        [Test]
        public void SimulateDrag_ProducesNonZeroMoveVector()
        {
            _zone.SimulatePointerDown(new Vector2(100f, 100f));
            _zone.SimulateDrag(new Vector2(150f, 100f));

            Assert.AreNotEqual(Vector2.zero, _zone.MoveVector,
                "Dragging right should produce a non-zero MoveVector.");
        }

        [Test]
        public void SimulateDrag_MoveVector_DirectionMatchesDragDirection()
        {
            _zone.SimulatePointerDown(new Vector2(100f, 100f));
            _zone.SimulateDrag(new Vector2(200f, 100f));

            Assert.Greater(_zone.MoveVector.x, 0f,
                "Dragging right of anchor should produce positive X move.");
            Assert.AreEqual(0f, _zone.MoveVector.y, 0.001f,
                "Dragging purely right should produce zero Y move.");
        }

        [Test]
        public void SimulatePointerUp_ResetsMoveVectorToZero()
        {
            _zone.SimulatePointerDown(new Vector2(100f, 100f));
            _zone.SimulateDrag(new Vector2(200f, 100f));
            _zone.SimulatePointerUp();

            Assert.AreEqual(Vector2.zero, _zone.MoveVector,
                "MoveVector should reset to zero when touch is released.");
        }

        [Test]
        public void SimulatePointerUp_ResetsAnchorPosition()
        {
            _zone.SimulatePointerDown(new Vector2(100f, 100f));
            _zone.SimulatePointerUp();

            Assert.AreEqual(Vector2.zero, _zone.AnchorPosition,
                "AnchorPosition should reset to zero when touch is released.");
        }

        [Test]
        public void MoveVector_IsClamped_ToMagnitudeOfOne()
        {
            _zone.SimulatePointerDown(new Vector2(100f, 100f));
            _zone.SimulateDrag(new Vector2(10000f, 10000f));

            Assert.LessOrEqual(_zone.MoveVector.magnitude, 1.001f,
                "MoveVector magnitude should never exceed 1.");
        }
    }
}
