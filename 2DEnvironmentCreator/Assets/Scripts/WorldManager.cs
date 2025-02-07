using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class WorldManager : MonoBehaviour
{
    public Button backButton;
    private string apiUrl = "http://localhost:5067/api/objects";

    private int environmentId;

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
        environmentId = PlayerPrefs.GetInt("SelectedEnvironmentId", 0);

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

    public void SelectObject(GameObject obj, int prefabId)
    {
        Debug.Log("Object selected: " + obj.name + ", Prefab ID: " + prefabId);
        currentObject = obj;
        currentPrefabId = prefabId;

        StartCoroutine(UpdateObjectState());
    }

    IEnumerator UpdateObjectState()
    {
        if (currentObject == null)
        {
            Debug.LogWarning("No object selected for tracking.");
            yield break;
        }

        Vector3 lastPosition = currentObject.transform.position;
        Vector3 lastScale = currentObject.transform.localScale;
        Quaternion lastRotation = currentObject.transform.rotation;

        Debug.Log("Start tracking object: " + currentObject.name);

        while (currentObject != null)
        {
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
            return;
        }

        string url = $"{apiUrl}/environment/{environmentId}";

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

            Debug.Log("Response JSON: " + responseJson);
            try
            {
                Object2D[] objects = JsonHelper.FromJson<Object2D>(responseJson);

                if (objects != null)
                {
                    Debug.Log("Restoring objects...");

                    foreach (var objectData in objects)
                    {
                        Debug.Log($"Restoring object with prefabId: {objectData.prefabId}");
                        // Directly pass the deserialized objectData to the restore method
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
            Debug.LogError("Server Response: " + request.downloadHandler.text);
        }
    }

    public void RestoreObject(Object2D objectData)
    {
        Debug.Log("Restoring object with prefabId: " + objectData.prefabId);

        GameObject prefab = GetPrefabById(objectData.prefabId);
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, new Vector3(objectData.positionX, objectData.positionY, 0), Quaternion.Euler(0, 0, objectData.rotationZ));
            obj.transform.localScale = new Vector3(objectData.scaleX, objectData.scaleY, 1);

            obj.GetComponent<Renderer>().sortingOrder = objectData.sortingLayer;
            Debug.Log("Object restored: " + obj.name);
        }
        else
        {
            Debug.LogWarning("Prefab not found for ID: " + objectData.prefabId);
        }
    }

    public GameObject GetPrefabById(int prefabId)
    {
        Debug.Log("Getting prefab with ID: " + prefabId);

        switch (prefabId)
        {
            case 1:
                return prefab1;
            case 2:
                return prefab2;
            case 3:
                return prefab3;
            case 4:
                return prefab4;
            case 5:
                return prefab5;
            case 6:
                return prefab6;
            default:
                Debug.LogError("PrefabId not recognized");
                return null;
        }
    }

    public static class JsonHelper
    {
        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }

        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
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
        public int environmentId;
        public int prefabId;
        public float positionX;
        public float positionY;
        public float scaleX;
        public float scaleY;
        public float rotationZ;
        public int sortingLayer;

        public Object2D(int environmentId, int prefabId, double positionX, double positionY, double scaleX, double scaleY, double rotationZ, int sortingLayer)
        {
            this.environmentId = environmentId;
            this.prefabId = prefabId;
            this.positionX = (float)positionX;
            this.positionY = (float)positionY;
            this.scaleX = (float)scaleX;
            this.scaleY = (float)scaleY;
            this.rotationZ = (float)rotationZ;
            this.sortingLayer = sortingLayer;
        }
    }
}
