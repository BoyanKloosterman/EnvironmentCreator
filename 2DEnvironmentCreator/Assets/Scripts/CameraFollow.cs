using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Assign the player GameObject in the Inspector
    public float smoothSpeed = 5.0f; // Adjust for smoother movement
    public Vector3 offset = new Vector3(0, 0, -10); // Ensure the camera stays behind

    private void LateUpdate()
    {
        if (player != null)
        {
            // Smoothly follow the player's position
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
