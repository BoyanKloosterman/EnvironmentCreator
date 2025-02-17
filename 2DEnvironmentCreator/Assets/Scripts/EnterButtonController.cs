using UnityEngine;
using UnityEngine.UI;

public class EnterButtonController : MonoBehaviour
{
    public CameraFollow cameraFollow; // Reference to CameraFollow
    public Button enterButton; // Reference to the Enter Button

    private void Start()
    {
        // Add listener to the button's onClick event
        enterButton.onClick.AddListener(OnEnterButtonPressed);
    }

    private void OnEnterButtonPressed()
    {

        // Hide the button
        enterButton.gameObject.SetActive(false);
    }
}
