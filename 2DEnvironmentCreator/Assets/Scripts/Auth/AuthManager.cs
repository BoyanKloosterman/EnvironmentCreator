using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks; // Add this to work with async/await

public class AuthManager : MonoBehaviour
{
    public InputField usernameField;
    public InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public UserApiClient userApiClient;

    public void Register()
    {
        string username = usernameField.text;
        string password = passwordField.text;

        if (!IsPasswordValid(password, out string errorMessage))
        {
            feedbackText.text = errorMessage;
            return;
        }

        // Use async method for registration
        RegisterUser(username, password);
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

        // Use async method for login
        LoginUser(username, password);
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

    private async void RegisterUser(string username, string password)
    {
        User userData = new User(username, password);
        var response = await userApiClient.Register(userData);

        // Process registration response
        if (response is WebRequestData<string> data && data.Data == "Succes")
        {
            feedbackText.text = "Registratie succesvol!";
        }
        else
        {
            feedbackText.text = "Registratie mislukt: " + response.ToString();
            Debug.LogError("Registration Error: " + response.ToString());
        }
    }

    private async void LoginUser(string username, string password)
    {
        User userData = new User(username, password);
        var response = await userApiClient.Login(userData);

        if (response is WebRequestData<string> data && data.Data == "Succes")
        {
            string token = PlayerPrefs.GetString("AuthToken", string.Empty);
            string userId = PlayerPrefs.GetInt("UserId", 0).ToString();

            // Proceed with the scene change or other actions
            SceneManager.LoadScene("EnvironmentSelectScene");
        }
        else
        {
            feedbackText.text = "Inloggen mislukt: " + response.ToString();
            Debug.LogError("Login Error: " + response.ToString());
        }
    }
}
