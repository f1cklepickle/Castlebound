using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player to follow
    public Vector3 offset = new Vector3(0, 0, -10); // A default offset to position the camera

    // LateUpdate is called once per frame, after all Update functions have been called.
    // This ensures the camera moves after the player has moved.
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

        // Set the camera's position to the target's position plus the offset
        transform.position = target.position + offset;
    }
}
