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

    private string apiUrl = "http://localhost:5067/api/user";

    private void Start()
    {
        //AutoLoginAsAdmin();
    }

    //private void AutoLoginAsAdmin()
    //{
    //    string username = "MiepMap123!";
    //    string password = "MiepMap123!";


    //    feedbackText.text = "Logged in as Admin!";
    //    StartCoroutine(LoginUser(username, password));
    //}
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

    private bool IsPasswordValid(string password, out string errorMessage)
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

        if (!hasLowercase) errorMessage = "Wachtwoord moet minstens één kleine letter bevatten.";
        else if (!hasUppercase) errorMessage = "Wachtwoord moet minstens één hoofdletter bevatten.";
        else if (!hasDigit) errorMessage = "Wachtwoord moet minstens één cijfer bevatten.";
        else if (!hasSpecialChar) errorMessage = "Wachtwoord moet minstens één speciaal teken bevatten.";

        return string.IsNullOrEmpty(errorMessage);
    }

    IEnumerator RegisterUser(string username, string password)
    {
        UserData userData = new UserData(username, password);
        var jsonData = JsonUtility.ToJson(userData);

        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            feedbackText.text = www.result == UnityWebRequest.Result.Success ?
                "Registratie succesvol!" : $"Registratie mislukt: {www.downloadHandler.text}";

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Registration Error: " + www.downloadHandler.text);
        }
    }

    IEnumerator LoginUser(string username, string password)
    {
        UserData userData = new UserData(username, password);
        var jsonData = JsonUtility.ToJson(userData);

        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text;
                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);

                PlayerPrefs.SetString("AuthToken", loginResponse.token);
                PlayerPrefs.SetInt("UserId", loginResponse.userId);
                PlayerPrefs.Save();

                SceneManager.LoadScene("WorldSelectScene");
            }
            else
            {
                feedbackText.text = "Inloggen mislukt: " + www.downloadHandler.text;
                Debug.LogError("Login Error: " + www.downloadHandler.text);
            }
        }
    }

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

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
        public int userId;
    }
}
