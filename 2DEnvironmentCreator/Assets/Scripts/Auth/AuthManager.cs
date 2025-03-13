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
