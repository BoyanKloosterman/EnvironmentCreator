using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

[TestFixture]
public class AuthManagerTests
{
    private AuthManager authManager;
    private GameObject authManagerGameObject;

    [SetUp]
    public void SetUp()
    {
        authManagerGameObject = new GameObject();
        authManager = authManagerGameObject.AddComponent<AuthManager>();

        authManager.emailField = new GameObject().AddComponent<InputField>();
        authManager.passwordField = new GameObject().AddComponent<InputField>();
        authManager.feedbackText = new GameObject().AddComponent<TextMeshProUGUI>();
        authManager.userApiClient = new GameObject().AddComponent<UserApiClient>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(authManagerGameObject);
    }

    [Test]
    public void IsPasswordValid_ValidPassword_ReturnsTrue()
    {
        string errorMessage;
        bool result = authManager.IsPasswordValid("ValidPassword1!", out errorMessage);

        Assert.IsTrue(result);
        Assert.IsEmpty(errorMessage);
    }

    [Test]
    public void IsPasswordValid_InvalidPassword_ReturnsFalse()
    {
        string errorMessage;
        bool result = authManager.IsPasswordValid("short", out errorMessage);

        Assert.IsFalse(result);
        Assert.IsNotEmpty(errorMessage);
    }

    [Test]
    public void Register_InvalidPassword_ShowsErrorMessage()
    {
        authManager.emailField.text = "test@example.com";
        authManager.passwordField.text = "short";

        authManager.Register();

        Assert.IsNotEmpty(authManager.feedbackText.text);
    }

    [Test]
    public void Login_InvalidPassword_ShowsErrorMessage()
    {
        authManager.emailField.text = "test@example.com";
        authManager.passwordField.text = "short";

        authManager.Login();

        Assert.IsNotEmpty(authManager.feedbackText.text);
    }
}
