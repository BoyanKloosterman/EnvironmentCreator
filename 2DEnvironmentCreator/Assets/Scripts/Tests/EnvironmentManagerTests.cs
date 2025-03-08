using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[TestFixture]
public class EnvironmentManagerTests
{
    private EnvironmentManager environmentManager;
    private GameObject environmentManagerGameObject;

    [SetUp]
    public void SetUp()
    {
        environmentManagerGameObject = new GameObject();
        environmentManager = environmentManagerGameObject.AddComponent<EnvironmentManager>();

        environmentManager.backButton = new GameObject().AddComponent<Button>();
        environmentManager.saveButton = new GameObject().AddComponent<Button>();
        environmentManager.object2DApiClient = new GameObject().AddComponent<Object2DApiClient>();
        environmentManager.prefab1 = new GameObject();
        environmentManager.prefab2 = new GameObject();
        environmentManager.prefab3 = new GameObject();
        environmentManager.prefab4 = new GameObject();
        environmentManager.prefab5 = new GameObject();
        environmentManager.prefab6 = new GameObject();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(environmentManagerGameObject);
    }

    [Test]
    public void SelectObject_ValidObject_SetsLastSelectedObject()
    {
        GameObject obj = new GameObject();
        int prefabId = 1;

        environmentManager.SelectObject(obj, prefabId);

        Assert.AreEqual(obj, environmentManager.lastSelectedObject);
    }

    [Test]
    public void GetPrefabById_ValidId_ReturnsPrefab()
    {
        GameObject prefab = environmentManager.GetPrefabById(1);

        Assert.AreEqual(environmentManager.prefab1, prefab);
    }

    [Test]
    public void GetPrefabById_InvalidId_ReturnsNull()
    {
        GameObject prefab = environmentManager.GetPrefabById(999);

        Assert.IsNull(prefab);
    }
}
