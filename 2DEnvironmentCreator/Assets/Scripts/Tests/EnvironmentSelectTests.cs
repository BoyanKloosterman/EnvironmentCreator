using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[TestFixture]
public class EnvironmentSelectTests
{
    private EnvironmentSelect environmentSelect;
    private GameObject environmentSelectGameObject;

    [SetUp]
    public void SetUp()
    {
        environmentSelectGameObject = new GameObject();
        environmentSelect = environmentSelectGameObject.AddComponent<EnvironmentSelect>();

        environmentSelect.worldPrefab = new GameObject();
        environmentSelect.worldsPanel = new GameObject().transform;
        environmentSelect.createWorldButton = new GameObject().AddComponent<Button>();
        environmentSelect.logoutButton = new GameObject().AddComponent<Button>();
        environmentSelect.environmentApiClient = new GameObject().AddComponent<Environment2DApiClient>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(environmentSelectGameObject);
    }

    [Test]
    public void Logout_ClearsAuthData()
    {
        PlayerPrefs.SetString("AuthToken", "testToken");
        PlayerPrefs.SetString("UserId", "testUserId");
        PlayerPrefs.SetString("SelectedEnvironmentId", "testEnvironmentId");

        environmentSelect.Logout();

        Assert.IsEmpty(PlayerPrefs.GetString("AuthToken"));
        Assert.IsEmpty(PlayerPrefs.GetString("UserId"));
        Assert.IsEmpty(PlayerPrefs.GetString("SelectedEnvironmentId"));
    }

    [Test]
    public void AddWorldToUI_ValidWorld_AddsWorldToUI()
    {
        Environment2D world = new Environment2D
        {
            environmentId = 1,
            name = "TestWorld",
            userId = "testUserId",
            height = 50,
            width = 50
        };

        environmentSelect.AddWorldToUI(world);

        Assert.AreEqual(1, environmentSelect.worldsPanel.childCount);
    }
}
