using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EnvironmentManager : MonoBehaviour
{
    public Button backButton;
    public Object2DApiClient object2DApiClient; // Reference to the Object2DApiClient
    private int environmentId;
    private GameObject currentObject;
    private int currentPrefabId;

    public float gridSize = 1f; // Define grid size for snapping

    public GameObject prefab1, prefab2, prefab3, prefab4, prefab5, prefab6;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("EnvironmentSelectScene"));
        environmentId = PlayerPrefs.GetInt("SelectedEnvironmentId", 0);

        string token = PlayerPrefs.GetString("AuthToken", "").Trim();
        if (!string.IsNullOrEmpty(token))
        {
            object2DApiClient.webClient.SetToken(token);
        }
        else
        {
            Debug.LogError("Token is missing! Redirecting to login.");
            SceneManager.LoadScene("LoginScene");
            return;
        }

        if (environmentId != 0)
        {
            LoadObjectsFromEnvironment();
        }
        else
        {
            Debug.LogWarning("Environment ID not set.");
        }
    }

    public void SelectObject(GameObject obj, int prefabId)
    {
        currentObject = obj;
        currentPrefabId = prefabId;
        StartCoroutine(UpdateObjectState());
    }

    IEnumerator UpdateObjectState()
    {
        if (currentObject == null) yield break;

        Vector3 lastPosition = currentObject.transform.position;
        Vector3 lastScale = currentObject.transform.localScale;
        Quaternion lastRotation = currentObject.transform.rotation;

        while (currentObject != null)
        {
            Vector3 snappedPosition = SnapToGrid(currentObject.transform.position);
            if (lastPosition != snappedPosition ||
                lastScale != currentObject.transform.localScale ||
                lastRotation != currentObject.transform.rotation)
            {
                currentObject.transform.position = snappedPosition;
                SaveObjectToEnvironment(currentObject, currentPrefabId);

                lastPosition = snappedPosition;
                lastScale = currentObject.transform.localScale;
                lastRotation = currentObject.transform.rotation;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SaveObjectToEnvironment(GameObject obj, int prefabId)
    {
        if (environmentId == 0)
        {
            Debug.LogError("Environment ID is not set.");
            return;
        }

        Object2D object2D = new Object2D(
            environmentId,
            prefabId,
            obj.transform.position.x,
            obj.transform.position.y,
            obj.transform.localScale.x,
            obj.transform.localScale.y,
            obj.transform.rotation.eulerAngles.z,
            obj.GetComponent<Renderer>().sortingOrder
        );

        StartCoroutine(PostObjectToEnvironment(object2D));
    }

    IEnumerator PostObjectToEnvironment(Object2D object2D)
    {
        var task = CreateObjectAsync(object2D);
        while (!task.IsCompleted)
        {
            yield return null; // Wait until the task is completed
        }

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("Object saved successfully.");
        }
        else
        {
            Debug.LogError("Error saving object: " + task.Exception);
        }
    }

    private async Task CreateObjectAsync(Object2D object2D)
    {
        var response = await object2DApiClient.CreateObject2D(object2D);

        if (response is WebRequestData<Object2D> data)
        {
            Debug.Log("Object created successfully: " + data.Data.id);
        }
        else
        {
            Debug.LogError("Error creating object: " + response.ToString());
        }
    }

    public void LoadObjectsFromEnvironment()
    {
        StartCoroutine(GetObjectsFromEnvironment());
    }

    IEnumerator GetObjectsFromEnvironment()
    {
        var task = ReadObjectsAsync(environmentId.ToString());
        while (!task.IsCompleted)
        {
            yield return null; // Wait until the task is completed
        }

        if (task.IsCompletedSuccessfully)
        {
            var response = task.Result;
            if (response is WebRequestData<List<Object2D>> data)
            {
                foreach (var objectData in data.Data)
                {
                    RestoreObject(objectData);
                }
            }
            else
            {
                Debug.LogError("Failed to load objects.");
            }
        }
        else
        {
            Debug.LogError("Error loading objects: " + task.Exception);
        }
    }

    private async Task<IWebRequestReponse> ReadObjectsAsync(string environmentId)
    {
        return await object2DApiClient.ReadObject2Ds(environmentId);
    }

    public void RestoreObject(Object2D objectData)
    {
        GameObject prefab = GetPrefabById(objectData.prefabId);
        if (prefab != null)
        {
            Vector3 snappedPosition = SnapToGrid(new Vector3(objectData.positionX, objectData.positionY, 0));
            GameObject obj = Instantiate(prefab, snappedPosition, Quaternion.Euler(0, 0, objectData.rotationZ));
            obj.transform.localScale = new Vector3(objectData.scaleX, objectData.scaleY, 1);
            obj.GetComponent<Renderer>().sortingOrder = objectData.sortingLayer;
        }
        else
        {
            Debug.LogWarning("Prefab not found for ID: " + objectData.prefabId);
        }
    }

    public GameObject GetPrefabById(int prefabId)
    {
        switch (prefabId)
        {
            case 1: return prefab1;
            case 2: return prefab2;
            case 3: return prefab3;
            case 4: return prefab4;
            case 5: return prefab5;
            case 6: return prefab6;
            default: return null;
        }
    }

    // Function to snap object positions to the grid
    private Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, 0);
    }
}

