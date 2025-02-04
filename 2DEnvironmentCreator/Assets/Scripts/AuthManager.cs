using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using TMPro;

public class AuthManager : MonoBehaviour
{
    public InputField usernameField;
    public InputField passwordField;
    public TextMeshProUGUI feedbackText;

    private string apiUrl = "http://localhost:5067/api/auth";

    // Define a class to handle the user data for both Register and Login
    [System.Serializable]
    public class UserData
    {
        public string Username;
        public string Password;

        public UserData(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public void Register()
    {
        StartCoroutine(RegisterUser(usernameField.text, passwordField.text));
    }

    public void Login()
    {
        StartCoroutine(LoginUser(usernameField.text, passwordField.text));
    }

    IEnumerator RegisterUser(string username, string password)
    {
        // Create UserData object
        UserData userData = new UserData(username, password);
        var jsonData = JsonUtility.ToJson(userData);

        Debug.Log("Register JSON Data: " + jsonData); // Debug the JSON content

        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                feedbackText.text = "Registratie succesvol!";
            }
            else
            {
                feedbackText.text = "Registratie mislukt: " + www.error;
                Debug.LogError("Registration Error: " + www.error); // Log the error
            }
        }
    }

    IEnumerator LoginUser(string username, string password)
    {
        // Create UserData object
        UserData userData = new UserData(username, password);
        var jsonData = JsonUtility.ToJson(userData);

        Debug.Log("Login JSON Data: " + jsonData); // Debug the JSON content

        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                PlayerPrefs.SetString("AuthToken", www.downloadHandler.text);
                SceneManager.LoadScene("WorldScene");
            }
            else
            {
                feedbackText.text = "Inloggen mislukt: " + www.error;
                Debug.LogError("Login Error: " + www.error); // Log the error
            }
        }
    }
}
