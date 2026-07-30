using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);

    public Transform Target => target;
    public Vector3 Offset => offset;

    void LateUpdate()
    {
        Tick();
    }

    public void Tick()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
    }
}
