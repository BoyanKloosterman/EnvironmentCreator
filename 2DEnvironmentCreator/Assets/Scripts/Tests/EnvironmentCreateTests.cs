using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[TestFixture]
public class EnvironmentCreateTests
{
    private EnvironmentCreate environmentCreate;
    private GameObject environmentCreateGameObject;

    [SetUp]
    public void SetUp()
    {
        environmentCreateGameObject = new GameObject();
        environmentCreate = environmentCreateGameObject.AddComponent<EnvironmentCreate>();

        environmentCreate.nameInput = new GameObject().AddComponent<TMP_InputField>();
        environmentCreate.heightInput = new GameObject().AddComponent<TMP_InputField>();
        environmentCreate.widthInput = new GameObject().AddComponent<TMP_InputField>();
        environmentCreate.createButton = new GameObject().AddComponent<Button>();
        environmentCreate.returnButton = new GameObject().AddComponent<Button>();
        environmentCreate.feedbackText = new GameObject().AddComponent<TextMeshProUGUI>();
        environmentCreate.environmentApiClient = new GameObject().AddComponent<Environment2DApiClient>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(environmentCreateGameObject);
    }

    [Test]
    public void CreateWorld_InvalidInput_ShowsErrorMessage()
    {
        environmentCreate.nameInput.text = "";
        environmentCreate.heightInput.text = "invalid";
        environmentCreate.widthInput.text = "invalid";

        environmentCreate.CreateWorld();

        Assert.IsNotEmpty(environmentCreate.feedbackText.text);
    }

    [Test]
    public void CreateWorld_ValidInput_CreatesWorld()
    {
        environmentCreate.nameInput.text = "TestWorld";
        environmentCreate.heightInput.text = "50";
        environmentCreate.widthInput.text = "50";

        environmentCreate.CreateWorld();

        Assert.IsEmpty(environmentCreate.feedbackText.text);
    }
}
