using System.Reflection;
using Castlebound.Gameplay.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Castlebound.Tests.Input
{
    public class TouchDefenseAimButtonTests
    {
        private GameObject buttonObject;
        private TouchDefenseAimButton button;

        [SetUp]
        public void SetUp()
        {
            buttonObject = new GameObject("TouchDefenseAimButton", typeof(RectTransform));
            button = buttonObject.AddComponent<TouchDefenseAimButton>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(buttonObject);
        }

        [Test]
        public void PointerBeginningOnButton_CapturesDefenseUntilMatchingRelease()
        {
            Assert.IsTrue(button.SimulatePointerDown(7, new Vector2(100f, 100f)));
            button.SimulateDrag(7, new Vector2(140f, 100f));

            Assert.IsTrue(button.IsDefending);
            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.right));
            Assert.IsFalse(button.SimulatePointerUp(8));
            Assert.IsTrue(button.IsDefending);

            Assert.IsTrue(button.SimulatePointerUp(7));
            Assert.IsFalse(button.IsDefending);
            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void SecondPointer_CannotTakeOverCapturedDefenseGesture()
        {
            Assert.IsTrue(button.SimulatePointerDown(3, Vector2.zero));

            Assert.IsFalse(button.SimulatePointerDown(4, new Vector2(20f, 20f)));
            Assert.IsFalse(button.SimulateDrag(4, Vector2.up * 50f));
            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.zero));

            Assert.IsTrue(button.SimulateDrag(3, Vector2.up * 50f));
            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void InputSystemTouch_UsesStableTouchIdForRelease()
        {
            var eventSystemObject = new GameObject("EventSystem");
            var eventSystem = eventSystemObject.AddComponent<EventSystem>();
            try
            {
                var pointerDown = CreateExtendedPointer(
                    eventSystem,
                    UIPointerType.Touch,
                    touchId: 17,
                    pointerId: 100,
                    position: Vector2.zero);
                var pointerUp = CreateExtendedPointer(
                    eventSystem,
                    UIPointerType.Touch,
                    touchId: 17,
                    pointerId: 200,
                    position: Vector2.zero);

                button.OnPointerDown(pointerDown);
                Assert.IsTrue(button.IsDefending);

                button.OnPointerUp(pointerUp);
                Assert.IsFalse(button.IsDefending);
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        [Test]
        public void InputSystemMouse_IsNotCapturedAsTouch()
        {
            var eventSystemObject = new GameObject("EventSystem");
            var eventSystem = eventSystemObject.AddComponent<EventSystem>();
            try
            {
                var pointerDown = CreateExtendedPointer(
                    eventSystem,
                    UIPointerType.MouseOrPen,
                    touchId: 0,
                    pointerId: 5,
                    position: Vector2.zero);

                button.OnPointerDown(pointerDown);

                Assert.IsFalse(button.IsDefending);
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        [Test]
        public void MissingPointerUp_ResetsWhenCapturedTouchIsNoLongerActive()
        {
            var eventSystemObject = new GameObject("EventSystem");
            var eventSystem = eventSystemObject.AddComponent<EventSystem>();
            try
            {
                button.OnPointerDown(CreateExtendedPointer(
                    eventSystem,
                    UIPointerType.Touch,
                    touchId: 23,
                    pointerId: 123,
                    position: Vector2.zero));
                Assert.IsTrue(button.IsDefending);
                button.SimulateDrag(23, Vector2.right * 200f);

                InvokeLifecycle(button, "LateUpdate");

                Assert.IsFalse(button.IsDefending);
                Assert.That(button.FacingDirection, Is.EqualTo(Vector2.zero));
                Assert.That(buttonObject.GetComponent<RectTransform>().anchoredPosition,
                    Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        [Test]
        public void GenericPointer_MissingPointerUpResetsWhenNoPointerIsPressed()
        {
            var eventSystemObject = new GameObject("EventSystem");
            var eventSystem = eventSystemObject.AddComponent<EventSystem>();
            try
            {
                button.OnPointerDown(new PointerEventData(eventSystem)
                {
                    pointerId = 12,
                    position = Vector2.zero
                });
                Assert.IsTrue(button.IsDefending);
                button.SimulateDrag(12, Vector2.up * 200f);

                InvokeLifecycle(button, "LateUpdate");

                Assert.IsFalse(button.IsDefending);
                Assert.That(button.FacingDirection, Is.EqualTo(Vector2.zero));
                Assert.That(buttonObject.GetComponent<RectTransform>().anchoredPosition,
                    Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        [Test]
        public void DisableAndFocusLoss_ClearCapturedDefense()
        {
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            button.SimulatePointerDown(4, Vector2.zero);
            button.SimulateDrag(4, Vector2.right * 200f);
            InvokeLifecycle(button, "OnDisable");
            Assert.IsFalse(button.IsDefending);
            Assert.That(buttonRect.anchoredPosition, Is.EqualTo(Vector2.zero));

            button.SimulatePointerDown(5, Vector2.zero);
            button.SimulateDrag(5, Vector2.up * 200f);
            InvokeLifecycle(button, "OnApplicationFocus", false);
            Assert.IsFalse(button.IsDefending);
            Assert.That(buttonRect.anchoredPosition, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ApplicationPause_ClearsCaptureAndSnapsButtonHome()
        {
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            button.SoftAnchorRadius = 75f;
            button.SimulatePointerDown(4, Vector2.zero);
            button.SimulateDrag(4, Vector2.right * 200f);
            Assert.That(buttonRect.anchoredPosition, Is.Not.EqualTo(Vector2.zero));

            InvokeLifecycle(button, "OnApplicationPause", true);

            Assert.IsFalse(button.IsDefending);
            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.zero));
            Assert.That(buttonRect.anchoredPosition, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void DragWithinSoftRadius_AimsWithoutMovingButton()
        {
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            button.SoftAnchorRadius = 75f;
            button.MaxAnchorDrift = 100f;
            button.SimulatePointerDown(4, Vector2.zero);

            button.SimulateDrag(4, Vector2.right * 50f);

            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.right));
            Assert.That(button.CurrentAnchorScreenPosition, Is.EqualTo(Vector2.zero));
            Assert.That(buttonRect.anchoredPosition, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void PointerDownAwayFromVisualCenter_BecomesLogicalAimAnchor()
        {
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            Vector2 pointerDownPosition = new Vector2(100f, 100f);
            button.SimulatePointerDown(4, pointerDownPosition);

            button.SimulateDrag(4, pointerDownPosition + Vector2.right * 50f);

            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.right));
            Assert.That(button.CurrentAnchorScreenPosition, Is.EqualTo(pointerDownPosition));
            Assert.That(buttonRect.anchoredPosition, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void DragBeyondSoftRadius_MovesButtonByOnlyExcessDistance()
        {
            button.SoftAnchorRadius = 75f;
            button.MaxAnchorDrift = 100f;
            button.SimulatePointerDown(4, Vector2.zero);

            button.SimulateDrag(4, Vector2.right * 125f);

            Assert.That(button.CurrentAnchorScreenPosition, Is.EqualTo(Vector2.right * 50f));
            Assert.That(buttonObject.GetComponent<RectTransform>().anchoredPosition,
                Is.EqualTo(Vector2.right * 50f));
        }

        [Test]
        public void PointerReturningInsideRadius_KeepsAnchorAtItsNewPosition()
        {
            button.SoftAnchorRadius = 75f;
            button.MaxAnchorDrift = 100f;
            button.SimulatePointerDown(4, Vector2.zero);
            button.SimulateDrag(4, Vector2.right * 125f);
            Assert.That(button.CurrentAnchorScreenPosition, Is.EqualTo(Vector2.right * 50f));

            button.SimulateDrag(4, Vector2.right * 80f);

            Assert.That(button.CurrentAnchorScreenPosition, Is.EqualTo(Vector2.right * 50f));
            Assert.That(buttonObject.GetComponent<RectTransform>().anchoredPosition,
                Is.EqualTo(Vector2.right * 50f));
            Assert.That(button.FacingDirection, Is.EqualTo(Vector2.right));
        }

        [Test]
        public void ExtendedDrag_ClampsButtonDriftFromOriginalPosition()
        {
            button.SoftAnchorRadius = 75f;
            button.MaxAnchorDrift = 100f;
            button.SimulatePointerDown(4, Vector2.zero);

            Vector2 farDiagonalPointer = new Vector2(-400f, 400f);
            button.SimulateDrag(4, farDiagonalPointer);

            Assert.That(button.CurrentAnchorScreenPosition.magnitude, Is.EqualTo(100f).Within(0.0001f));
            Assert.That(button.FacingDirection, Is.EqualTo(farDiagonalPointer.normalized));
        }

        [Test]
        public void ReleaseAfterSoftAnchorDrift_SnapsButtonBackHome()
        {
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            button.SoftAnchorRadius = 75f;
            button.MaxAnchorDrift = 100f;
            button.SimulatePointerDown(4, Vector2.zero);
            button.SimulateDrag(4, Vector2.up * 200f);
            Assert.That(buttonRect.anchoredPosition, Is.Not.EqualTo(Vector2.zero));

            button.SimulatePointerUp(4);

            Assert.That(buttonRect.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(button.CurrentAnchorScreenPosition, Is.EqualTo(Vector2.zero));
        }

        private static void InvokeLifecycle(
            TouchDefenseAimButton target,
            string methodName,
            params object[] arguments)
        {
            target.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(target, arguments);
        }

        private static ExtendedPointerEventData CreateExtendedPointer(
            EventSystem eventSystem,
            UIPointerType pointerType,
            int touchId,
            int pointerId,
            Vector2 position)
        {
            return new ExtendedPointerEventData(eventSystem)
            {
                pointerType = pointerType,
                touchId = touchId,
                pointerId = pointerId,
                position = position
            };
        }
    }
}
