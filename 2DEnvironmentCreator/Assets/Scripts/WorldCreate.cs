using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

public class WorldCreate : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField heightInput;
    public TMP_InputField widthInput;
    public Button createButton;
    public Button returnButton;
    public TextMeshProUGUI feedbackText;
    public Environment2DApiClient environmentApiClient; // Reference to the Environment2DApiClient

    void Start()
    {
        createButton.onClick.AddListener(CreateWorld);
        returnButton.onClick.AddListener(() => SceneManager.LoadScene("WorldSelectScene"));

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

    void CreateWorld()
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

        int userId = PlayerPrefs.GetInt("UserId");
        Environment2D newEnvironment = new Environment2D
        {
            name = name,
            userId = userId
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

        if (task.IsCompletedSuccessfully)
        {
            feedbackText.text = "World created successfully!";
            SceneManager.LoadScene("WorldSelectScene");
        }
        else
        {
            feedbackText.text = "Error creating world.";
            Debug.LogError("Error creating world: " + task.Exception);
        }
    }

    private async Task CreateWorldAsync(Environment2D environment)
    {
        var response = await environmentApiClient.CreateEnvironment(environment);

        if (response is WebRequestData<Environment2D> data)
        {
            Debug.Log("World created successfully: " + data.Data.name);
        }
        else
        {
            Debug.LogError("Error creating world: " + response.ToString());
        }
    }
}
