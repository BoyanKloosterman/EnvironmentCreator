using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
public class EnvironmentCreate : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField heightInput;
    public TMP_InputField widthInput;
    public Button createButton;
    public Button returnButton;
    public TextMeshProUGUI feedbackText;
    public Environment2DApiClient environmentApiClient;
    void Start()
    {
        createButton.onClick.AddListener(CreateWorld);
        returnButton.onClick.AddListener(() => SceneManager.LoadScene("EnvironmentSelectScene"));
        string token = PlayerPrefs.GetString("AuthToken", "").Trim();
        if (!string.IsNullOrEmpty(token))
        {
            environmentApiClient.webClient.SetToken(token);
        }
        else
        {
            Debug.LogError("Token is missing! Redirecting to login.");
            SceneManager.LoadScene("LoginScene");
        }
    }
    internal void CreateWorld()
    {
        string name = nameInput.text.Trim();
        if (!int.TryParse(heightInput.text, out int height) || !int.TryParse(widthInput.text, out int width))
        {
            feedbackText.text = "Please enter valid numbers for height and width.";
            return;
        }
        if (string.IsNullOrEmpty(name) || height <= 0 || width <= 0)
        {
            feedbackText.text = "Please fill in all fields correctly.";
            return;
        }
        if (width < 20 || width > 200)
        {
            feedbackText.text = "Width must be between 20 and 200.";
            return;
        }
        if (height < 10 || height > 100)
        {
            feedbackText.text = "Height must be between 10 and 100.";
            return;
        }
        string userId = PlayerPrefs.GetString("userId", "");
        Debug.Log("Retrieved UserId from PlayerPrefs: " + userId);
        Environment2D newEnvironment = new Environment2D
        {
            name = name,
            userId = userId, // Changed from int to string
            height = height,
            width = width
        };
        StartCoroutine(PostWorld(newEnvironment));
    }
    IEnumerator PostWorld(Environment2D environment)
    {
        var task = CreateWorldAsync(environment);
        while (!task.IsCompleted)
        {
            yield return null; // Wait until the task is completed
        }

        if (task.Exception != null)
        {
            feedbackText.text = "Error creating world. Please try again later.";
            Debug.LogError("Exception when creating world: " + task.Exception);
            yield break;
        }

        var result = task.Result;
        if (result is WebRequestData<Environment2D>)
        {
            feedbackText.text = "World created successfully!";
            SceneManager.LoadScene("EnvironmentSelectScene");
        }
        else if (result is WebRequestError error)
        {
            // Extract the error message from the WebRequestError
            string errorMessage = error.Message;

            // Check for 5 world limit error
            if (error.StatusCode == 400)
            {
                feedbackText.text = "You can have a maximum of 5 worlds.";
            }
            else
            {
                feedbackText.text = errorMessage;
            }

            Debug.LogError($"Error creating world: {error.Message} (Status: {error.StatusCode})");
            // Do not redirect to EnvironmentSelectScene
        }
        else
        {
            feedbackText.text = "Unknown error occurred.";
            Debug.LogError("Unknown response type: " + result.GetType().Name);
        }
    }
    private async Task<IWebRequestReponse> CreateWorldAsync(Environment2D environment)
    {
        return await environmentApiClient.CreateEnvironment(environment);
    }
}