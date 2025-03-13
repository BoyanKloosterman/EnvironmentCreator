using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

public class AuthManager : MonoBehaviour
{
    public InputField emailField;
    public InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public UserApiClient userApiClient;

    public void Register()
    {
        string email = emailField.text;
        string password = passwordField.text;

        if (!IsPasswordValid(password, out string errorMessage))
        {
            feedbackText.text = errorMessage;
            return;
        }

        // Use async method for registration
        RegisterUser(email, password);
    }

    public void Login()
    {
        string email = emailField.text;
        string password = passwordField.text;

        if (!IsPasswordValid(password, out string errorMessage))
        {
            feedbackText.text = errorMessage;
            return;
        }

        // Use async method for login
        LoginUser(email, password);
    }

    internal bool IsPasswordValid(string password, out string errorMessage)
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

    private async void RegisterUser(string email, string password)
    {
        User userData = new User(email, password);
        Debug.Log("Attempting to register user: " + email);
        var response = await userApiClient.Register(userData);
        Debug.Log("Raw response type: " + response.GetType().FullName);

        if (response is WebRequestData<string> data)
        {
            Debug.Log("Response data: " + data.Data);

            if (data.Data == "Succes" || string.IsNullOrEmpty(data.Data))
            {
                feedbackText.text = "Registratie succesvol!";
                await Task.Delay(1000);
                LoginUser(email, password);
            }
            else
            {
                feedbackText.text = "Registratie mislukt: " + data.Data;
                Debug.LogError("Registration Error: " + data.Data);
            }
        }
        else if (response is WebRequestError error)
        {
            Debug.LogError("Registration Error: Status=" + error.StatusCode + ", Message=" + error.Message);

            if (error.Message.Contains("DuplicateUserName") ||
                error.Message.Contains("already taken") ||
                error.StatusCode == 409 ||
                error.StatusCode == 400)
            {
                feedbackText.text = "Email is al geregistreerd.";
            }
            else
            {
                feedbackText.text = "Registratie mislukt: " + error.Message;
            }
        }
        else
        {
            Debug.LogError("Registration Error: Unknown response type: " + response?.ToString() ?? "null");
            feedbackText.text = "Registratie mislukt: Onbekende fout";
        }
    }


    private async void LoginUser(string email, string password)
    {
        User userData = new User(email, password);
        var response = await userApiClient.Login(userData);

        if (response is WebRequestData<string> data && data.Data == "Succes")
        {
            feedbackText.text = "Login succesvol!";
            SceneManager.LoadScene("EnvironmentSelectScene");
        }
        else
        {
            if (response.ToString() == "WebRequestError" &&
                (response is WebRequestError error && error.StatusCode == 401))
            {
                feedbackText.text = "Email of wachtwoord is onjuist.";
            }
            else
            {
                feedbackText.text = "Login mislukt: " + response.ToString();
            }

            Debug.LogError("Login Error: " + response.ToString());
        }
    }
}
