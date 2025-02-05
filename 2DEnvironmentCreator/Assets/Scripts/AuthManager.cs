using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;

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
        string username = usernameField.text;
        string password = passwordField.text;

        if (!IsPasswordValid(password, out string errorMessage))
        {
            feedbackText.text = errorMessage;
            return;
        }

        StartCoroutine(RegisterUser(username, password));
    }

    public void Login()
    {
        string username = usernameField.text;
        string password = passwordField.text;

        if (!IsPasswordValid(password, out string errorMessage))
        {
            feedbackText.text = errorMessage;
            return;
        }

        StartCoroutine(LoginUser(username, password));
    }

    public bool IsPasswordValid(string password, out string errorMessage)
    {
        errorMessage = "";

        if (password.Length < 10)
        {
            errorMessage = "Wachtwoord moet minimaal 10 karakters lang zijn.";
            return false;
        }

        bool hasLowercase = false, hasUppercase = false, hasDigit = false, hasSpecialChar = false;

        foreach (char c in password)
        {
            if (char.IsLower(c)) hasLowercase = true;
            if (char.IsUpper(c)) hasUppercase = true;
            if (char.IsDigit(c)) hasDigit = true;
            if (!char.IsLetterOrDigit(c)) hasSpecialChar = true;
        }

        if (!hasLowercase)
        {
            errorMessage = "Wachtwoord moet minstens ��n kleine letter bevatten.";
            return false;
        }
        if (!hasUppercase)
        {
            errorMessage = "Wachtwoord moet minstens ��n hoofdletter bevatten.";
            return false;
        }
        if (!hasDigit)
        {
            errorMessage = "Wachtwoord moet minstens ��n cijfer bevatten.";
            return false;
        }
        if (!hasSpecialChar)
        {
            errorMessage = "Wachtwoord moet minstens ��n speciaal teken bevatten.";
            return false;
        }

        return true;
    }

    IEnumerator RegisterUser(string username, string password)
    {
        UserData userData = new UserData(username, password);
        var jsonData = JsonUtility.ToJson(userData);

        Debug.Log("Register JSON Data: " + jsonData);

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
                string serverResponse = www.downloadHandler.text;
                feedbackText.text = "Registratie mislukt: " + serverResponse;
                Debug.LogError("Registration Error: " + serverResponse);
            }
        }
    }

    IEnumerator LoginUser(string username, string password)
    {
        UserData userData = new UserData(username, password);
        var jsonData = JsonUtility.ToJson(userData);

        Debug.Log("Login JSON Data: " + jsonData);

        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Assuming the response body contains just the token as a string.
                string token = www.downloadHandler.text;
                PlayerPrefs.SetString("AuthToken", token);  // Store the token in PlayerPrefs

                PlayerPrefs.Save();  // Ensure it's saved

                Debug.Log("Login successful, token saved: " + token);
                SceneManager.LoadScene("WorldScene");  // Load the next scene
            }
            else
            {
                string serverResponse = www.downloadHandler.text;
                feedbackText.text = "Inloggen mislukt: " + serverResponse;
                Debug.LogError("Login Error: " + serverResponse);
            }
        }
    }

}
