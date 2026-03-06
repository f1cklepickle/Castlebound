using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimInputResolver : MonoBehaviour
{
    [SerializeField] private float stickDeadZone = 0.0001f;
    [SerializeField] private float stickMagnitudeMax = 1.1f;

    public Vector2 Resolve(Vector2 playerPosition, Vector2 stickAimInput)
    {
        if (stickAimInput.sqrMagnitude > stickDeadZone &&
            stickAimInput.sqrMagnitude <= stickMagnitudeMax * stickMagnitudeMax)
        {
            return stickAimInput;
        }

        var mouse = Mouse.current;
        var camera = Camera.main;
        if (mouse != null && camera != null)
        {
            var mouseScreenPosition = Mouse.current.position.ReadValue();
            var mouseWorldPosition = camera.ScreenToWorldPoint(
                new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
            var mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            var mouseAim = mouseWorldPosition2D - playerPosition;
            if (mouseAim.sqrMagnitude > stickDeadZone)
                return mouseAim.normalized;
        }

        return stickAimInput;
    }
}
