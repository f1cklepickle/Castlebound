using UnityEngine;

namespace Castlebound.Gameplay.Projectile
{
    public class ProjectileSpin : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 360f;

        public float DegreesPerSecond
        {
            get => degreesPerSecond;
            set => degreesPerSecond = value;
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        public void Advance(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            transform.Rotate(0f, 0f, degreesPerSecond * safeDeltaTime, Space.Self);
        }
    }
}
