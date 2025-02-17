using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float moveSpeed = 5.0f; // Speed for manual camera movement
    private float horizontalInput, verticalInput;

    private void Update()
    {
        // Handle manual camera movement (with Arrow keys, WASD, or Joystick)
        HandleManualMovement();
    }

    private void HandleManualMovement()
    {
        // Get input for movement (Horizontal and Vertical)
        horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right
        verticalInput = Input.GetAxis("Vertical"); // W/S or Up/Down

        // Move the camera based on input
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0) * moveSpeed * Time.deltaTime;
        transform.position += moveDirection;
    }
}
