using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnvironmentManager : MonoBehaviour
{
    public Button backButton;
    public Button saveButton;
    public Object2DApiClient object2DApiClient;
    public int environmentId;
    private List<GameObject> draggedObjects = new List<GameObject>();  // Store dragged objects temporarily
    private int currentPrefabId;
    public GameObject lastSelectedObject;
    public float gridSize = 1f;
    private List<GameObject> objectsToSave = new List<GameObject>();
    public GameObject prefab1, prefab2, prefab3, prefab4, prefab5, prefab6;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("EnvironmentSelectScene"));
        saveButton.onClick.AddListener(SaveObjects);  // Save all dragged objects when Save button is pressed

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
        currentPrefabId = prefabId;
        lastSelectedObject = obj;
        // Store the dragged object temporarily
        draggedObjects.Add(obj);
    }

    public void SaveObjects()
    {
        // Find all objects with DiceDragHandler component
        DiceDragHandler[] dragHandlers = FindObjectsOfType<DiceDragHandler>();

        // Filter only moved objects
        var movedObjects = new List<DiceDragHandler>();
        foreach (DiceDragHandler dragHandler in dragHandlers)
        {
            if (dragHandler.hasBeenMoved)
            {
                movedObjects.Add(dragHandler);
            }
        }

        if (movedObjects.Count == 0)
        {
            Debug.Log("No objects have been moved to save.");
            return;
        }

        Debug.Log($"Saving {movedObjects.Count} moved objects");

        foreach (DiceDragHandler dragHandler in movedObjects)
        {
            if (dragHandler.gameObject != null)
            {
                // Use the existing method from DiceDragHandler to update or save
                dragHandler.UpdateExistingObject();
            }
        }

        Debug.Log("Save objects process completed");
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


    public IEnumerator PostObjectToEnvironment(Object2D object2D)
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

    public IEnumerator GetObjectsFromEnvironment()
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
                // Log the raw data for diagnostic purposes
                Debug.Log($"Total objects loaded: {data.Data.Count}");
                foreach (var objectData in data.Data)
                {
                    // Add detailed logging
                    Debug.Log($"Loaded Object - ID: {objectData.id}, PrefabId: {objectData.prefabId}, Position: ({objectData.positionX}, {objectData.positionY})");

                    // Only restore objects with valid IDs
                    if (objectData.id > 0)
                    {
                        RestoreObject(objectData);
                    }
                    else
                    {
                        Debug.LogWarning($"Skipping object with invalid ID: {objectData.id}");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load objects or invalid response type.");
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
        // Validate input
        if (objectData == null)
        {
            Debug.LogError("Attempted to restore null object data");
            return;
        }

        GameObject prefab = GetPrefabById(objectData.prefabId);
        if (prefab != null)
        {
            Vector3 snappedPosition = SnapToGrid(new Vector3(objectData.positionX, objectData.positionY, 0));
            GameObject obj = Instantiate(prefab, snappedPosition, Quaternion.Euler(0, 0, objectData.rotationZ));
            obj.transform.localScale = new Vector3(objectData.scaleX, objectData.scaleY, 1);
            obj.GetComponent<Renderer>().sortingOrder = objectData.sortingLayer;

            // Set the existing object data for the drag handler
            DiceDragHandler dragHandler = obj.GetComponent<DiceDragHandler>();
            if (dragHandler != null)
            {
                // Always set the object data, but with a more permissive validation
                dragHandler.SetExistingObjectData(objectData);
            }
        }
        else
        {
            Debug.LogWarning($"Prefab not found for ID: {objectData.prefabId}");
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

    private Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, 0);
    }
}
