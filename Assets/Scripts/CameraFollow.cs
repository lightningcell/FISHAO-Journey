using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Target transform for the camera to follow.")]
    public Transform target;

    [Tooltip("Offset from the target position.")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("Smoothing factor for camera movement. 0 = no smoothing, 1 = instant.")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}