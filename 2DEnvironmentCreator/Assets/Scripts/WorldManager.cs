using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using System;

public class WorldManager : MonoBehaviour
{
    public Button backButton;
    private string apiUrl = "http://localhost:5067/api/objects"; // Endpoint for saving objects

    // This should be set when loading the world scene
    private int environmentId;

    // Track objects to update their state
    private GameObject currentObject;
    private int currentPrefabId;

    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;
    public GameObject prefab5;
    public GameObject prefab6;

    void Start()
    {
        Debug.Log("WorldManager started.");

        backButton.onClick.AddListener(() => SceneManager.LoadScene("WorldSelectScene"));
        environmentId = PlayerPrefs.GetInt("SelectedEnvironmentId", 0); // Get selected world ID from PlayerPrefs

        Debug.Log("Selected Environment ID: " + environmentId);

        if (environmentId == 0)
        {
            Debug.LogWarning("Environment ID not set.");
        }
        else
        {
            Debug.Log("Loading objects for environment ID: " + environmentId);
            LoadObjectsFromEnvironment();
        }
    }

    // Call this method when you select or spawn an object
    public void SelectObject(GameObject obj, int prefabId)
    {
        Debug.Log("Object selected: " + obj.name + ", Prefab ID: " + prefabId);
        currentObject = obj;
        currentPrefabId = prefabId;

        // Optionally start updating its state
        StartCoroutine(UpdateObjectState());
    }

    IEnumerator UpdateObjectState()
    {
        if (currentObject == null)
        {
            Debug.LogWarning("No object selected for tracking.");
            yield break; // Exit if no object is selected
        }

        Vector3 lastPosition = currentObject.transform.position;
        Vector3 lastScale = currentObject.transform.localScale;
        Quaternion lastRotation = currentObject.transform.rotation;

        Debug.Log("Start tracking object: " + currentObject.name);

        while (currentObject != null)
        {
            // If position, scale, or rotation changed, save the object
            if (lastPosition != currentObject.transform.position ||
                lastScale != currentObject.transform.localScale ||
                lastRotation != currentObject.transform.rotation)
            {
                Debug.Log("Object state changed. Saving...");

                lastPosition = currentObject.transform.position;
                lastScale = currentObject.transform.localScale;
                lastRotation = currentObject.transform.rotation;

                SaveObjectToEnvironment(currentObject, currentPrefabId);
            }

            yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds to reduce the frequency of requests
        }
    }

    public void SaveObjectToEnvironment(GameObject obj, int prefabId)
    {
        if (environmentId == 0)
        {
            Debug.LogError("Environment ID is not set.");
            return; // Make sure the environment is set
        }

        Object2D object2D = new Object2D
        {
            EnvironmentId = environmentId,
            PrefabId = prefabId,
            PositionX = obj.transform.position.x,
            PositionY = obj.transform.position.y,
            ScaleX = obj.transform.localScale.x,
            ScaleY = obj.transform.localScale.y,
            RotationZ = obj.transform.rotation.eulerAngles.z,
            SortingLayer = obj.GetComponent<Renderer>().sortingOrder
        };

        Debug.Log("Saving object data: " + JsonUtility.ToJson(object2D));

        StartCoroutine(PostObjectToEnvironment(object2D));
    }

    IEnumerator PostObjectToEnvironment(Object2D object2D)
    {
        if (object2D == null)
        {
            Debug.LogError("Object2D is null! Aborting request.");
            yield break;
        }

        string jsonData = JsonUtility.ToJson(object2D);
        Debug.Log("Sending object data to server: " + jsonData);

        if (string.IsNullOrEmpty(apiUrl))
        {
            Debug.LogError("API URL is null or empty!");
            yield break;
        }

        string authToken = PlayerPrefs.GetString("AuthToken", "");
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Auth token is missing! Make sure the user is logged in.");
            yield break;
        }

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Object saved successfully.");
        }
        else
        {
            string errorMessage = "Error saving object: " + (request.downloadHandler != null ? request.downloadHandler.text : "Unknown error");
            Debug.LogError(errorMessage);
            Debug.LogError("Server Response: " + (request.downloadHandler != null ? request.downloadHandler.text : "No response"));
        }
    }

    public void LoadObjectsFromEnvironment()
    {
        if (environmentId == 0)
        {
            Debug.LogError("Environment ID is not set.");
            return; // Make sure the environment is set
        }

        // Assuming API URL provides a list of objects
        string url = $"{apiUrl}/environment/{environmentId}"; // Adjust according to your API

        Debug.Log("Requesting objects for environment ID: " + environmentId);

        StartCoroutine(GetObjectsFromServer(url));
    }

    IEnumerator GetObjectsFromServer(string url)
    {
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Auth token is missing! Make sure the user is logged in.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Debug.Log("Sending request to server: " + url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Objects loaded successfully.");
            string responseJson = request.downloadHandler.text;

            // Debug the raw JSON response
            Debug.Log("Response JSON: " + responseJson);

            try
            {
                // Deserialize the response to get objects
                Object2DListWrapper objectWrapper = JsonUtility.FromJson<Object2DListWrapper>(responseJson);

                if (objectWrapper != null && objectWrapper.objects != null)
                {
                    Debug.Log("Restoring objects...");
                    foreach (var objectData in objectWrapper.objects)
                    {
                        RestoreObject(objectData);
                    }
                }
                else
                {
                    Debug.LogError("Failed to parse objects from the server.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error parsing JSON response: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Error loading objects: " + request.error);
            Debug.LogError("Server Response: " + request.downloadHandler.text); // Log server response for more insight
        }
    }




    public void RestoreObject(Object2D objectData)
    {
        Debug.Log("Restoring object with PrefabId: " + objectData.PrefabId);

        // Instantiate the object based on prefab ID or whatever method you're using to create objects
        GameObject prefab = GetPrefabById(objectData.PrefabId);
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, new Vector3(objectData.PositionX, objectData.PositionY, 0), Quaternion.Euler(0, 0, objectData.RotationZ));
            obj.transform.localScale = new Vector3(objectData.ScaleX, objectData.ScaleY, 1);

            // Optionally set the sorting layer if needed
            obj.GetComponent<Renderer>().sortingOrder = objectData.SortingLayer;
            Debug.Log("Object restored: " + obj.name);
        }
        else
        {
            Debug.LogWarning("Prefab not found for ID: " + objectData.PrefabId);
        }
    }

    public GameObject GetPrefabById(int prefabId)
    {
        Debug.Log("Getting prefab with ID: " + prefabId);

        switch (prefabId)
        {
            case 1:
                return prefab1; // Dice 1 prefab
            case 2:
                return prefab2; // Dice 2 prefab
            case 3:
                return prefab3; // Dice 3 prefab
            case 4:
                return prefab4; // Dice 4 prefab
            case 5:
                return prefab5; // Dice 5 prefab
            case 6:
                return prefab6; // Dice 6 prefab
            default:
                Debug.LogError("PrefabId not recognized");
                return null;
        }
    }



    [System.Serializable]
    public class Object2DListWrapper
    {
        public List<Object2D> objects;
    }

    [System.Serializable]
    public class Object2D
    {
        public int EnvironmentId;
        public int PrefabId;
        public float PositionX;
        public float PositionY;
        public float ScaleX;
        public float ScaleY;
        public float RotationZ;
        public int SortingLayer;

    }
}
