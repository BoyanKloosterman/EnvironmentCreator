using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Newtonsoft.Json;

public class WorldCreate : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField lengthInput;
    public TMP_InputField widthInput;
    public Button createButton;
    public TextMeshProUGUI feedbackText;

    private string apiUrl = "http://localhost:5067/api/worlds";

    void Start()
    {
        createButton.onClick.AddListener(CreateWorld);
    }

    void CreateWorld()
    {
        string name = nameInput.text.Trim(); // Ensure no leading/trailing spaces
        int length = int.Parse(lengthInput.text);
        int width = int.Parse(widthInput.text);
        int userId = PlayerPrefs.GetInt("UserId"); // Ensure this is set during login

        if (string.IsNullOrEmpty(name) || length <= 0 || width <= 0)
        {
            feedbackText.text = "Vul alle velden correct in.";
            return;
        }

        StartCoroutine(PostWorld(name, length, width, userId));
    }

    IEnumerator PostWorld(string name, int length, int width, int userId)
    {
        var worldData = new
        {
            Name = name,
            MaxLength = length,
            MaxHeight = width,
            UserId = userId
        };

        string jsonData = JsonConvert.SerializeObject(worldData);  // Use Newtonsoft.Json for serialization
        Debug.Log("JsonData: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("AuthToken"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            feedbackText.text = "Wereld succesvol aangemaakt!";
            SceneManager.LoadScene("WorldScene");
        }
        else
        {
            Debug.LogError("Server Response: " + request.downloadHandler.text);
            feedbackText.text = "Fout bij het aanmaken van de wereld: " + request.downloadHandler.text;
        }
    }


}

