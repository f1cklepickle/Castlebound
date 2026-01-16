using UnityEngine;

namespace Castlebound.Gameplay.Loot
{
    public sealed class LootSpillMotion : MonoBehaviour
    {
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float duration;
        private float elapsed;
        private bool isActive;

        public void Initialize(Vector3 target, float moveDuration)
        {
            startPosition = transform.position;
            targetPosition = target;
            duration = Mathf.Max(0f, moveDuration);
            elapsed = 0f;
            isActive = duration > 0f;

            if (!isActive)
            {
                transform.position = targetPosition;
            }
        }

        public void Step(float deltaTime)
        {
            if (!isActive)
            {
                return;
            }

            elapsed += Mathf.Max(0f, deltaTime);
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float eased = easeCurve != null ? easeCurve.Evaluate(t) : t;
            transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
            if (t >= 1f)
            {
                isActive = false;
            }
        }

        private void Update()
        {
            Step(Time.deltaTime);
        }
    }
}
