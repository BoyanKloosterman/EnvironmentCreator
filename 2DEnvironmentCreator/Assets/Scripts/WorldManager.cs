using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;

public class WorldManager : MonoBehaviour
{
    public Button backButton;
    private string apiUrl = "http://localhost:5067/api/objects"; // Endpoint for saving objects

    // This should be set when loading the world scene
    private int environmentId;

    // Track objects to update their state
    private GameObject currentObject;
    private int currentPrefabId;

    void Start()
    {
        Debug.Log("WorldManager started.");

        backButton.onClick.AddListener(() => SceneManager.LoadScene("WorldSelectScene"));
        environmentId = PlayerPrefs.GetInt("SelectedEnvironmentId", 0); // Get selected world ID from PlayerPrefs

        if (environmentId == 0)
        {
            Debug.LogWarning("Environment ID not set.");
        }
        else
        {
            Debug.Log("Selected environment ID: " + environmentId);
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
