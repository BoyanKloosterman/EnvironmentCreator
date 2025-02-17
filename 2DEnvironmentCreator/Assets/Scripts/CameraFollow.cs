using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Assign the player in the inspector
    public float smoothing = 5f;

    private Vector3 offset;

    private void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}
