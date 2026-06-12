using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyRootReceiver : MonoBehaviour
    {
        private float remainingRootSeconds;
        private Rigidbody2D body;
        private Vector2 lockPosition;
        private bool hasLockPosition;

        public bool IsRooted => remainingRootSeconds > 0f;
        public float RemainingRootSeconds => remainingRootSeconds;
        public Vector2 LockPosition => lockPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void RootForSeconds(float seconds)
        {
            remainingRootSeconds = Mathf.Max(remainingRootSeconds, seconds);
        }

        public void RootAt(Vector2 position, float seconds)
        {
            lockPosition = position;
            hasLockPosition = true;
            RootForSeconds(seconds);
            ApplyLockPosition();
        }

        public void Tick(float deltaTime)
        {
            if (remainingRootSeconds <= 0f || deltaTime <= 0f)
            {
                return;
            }

            ApplyLockPosition();
            remainingRootSeconds = Mathf.Max(0f, remainingRootSeconds - deltaTime);

            if (remainingRootSeconds <= 0f)
            {
                hasLockPosition = false;
            }
        }

        public void ClearRoot()
        {
            remainingRootSeconds = 0f;
            hasLockPosition = false;
        }

        private void ApplyLockPosition()
        {
            if (!hasLockPosition)
            {
                return;
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            if (body != null)
            {
                body.position = lockPosition;
            }

            transform.position = new Vector3(lockPosition.x, lockPosition.y, transform.position.z);
        }
    }
}
