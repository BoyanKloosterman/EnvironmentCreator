using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class WorldCreate : MonoBehaviour
{
    public InputField nameInput;
    public InputField maxHeightInput;
    public InputField maxLengthInput;

    private string apiUrl = "http://localhost:5067/api/worlds";

    public void OnCreateWorldButtonPressed()
    {
        // Validate inputs
        if (string.IsNullOrEmpty(nameInput.text) ||
            string.IsNullOrEmpty(maxHeightInput.text) ||
            string.IsNullOrEmpty(maxLengthInput.text))
        {
            Debug.Log("All fields are required.");
            return;
        }

        // Create the Environment2D object
        Environment2D newEnvironment = new Environment2D
        {
            name = nameInput.text,
            maxHeight = int.Parse(maxHeightInput.text),
            maxLength = int.Parse(maxLengthInput.text)
        };

        // Send to API
        StartCoroutine(CreateWorld(newEnvironment));
    }

    private IEnumerator CreateWorld(Environment2D environment)
    {
        string json = JsonUtility.ToJson(environment);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(apiUrl, json))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("World Created Successfully!");
                // Load the WorldScene
                SceneManager.LoadScene("WorldScene");
            }
            else
            {
                Debug.LogError("Error creating world: " + www.error);
            }
        }
    }
}
