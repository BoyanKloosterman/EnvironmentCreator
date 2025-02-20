using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class WorldManager : MonoBehaviour
{
    public Button backButton;
    private string apiUrl = "https://avansict2226638.azurewebsites.net/api/objects";
    private int environmentId;
    private GameObject currentObject;
    private int currentPrefabId;

    public float gridSize = 1f; // Define grid size for snapping

    public GameObject prefab1, prefab2, prefab3, prefab4, prefab5, prefab6;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("WorldSelectScene"));
        environmentId = PlayerPrefs.GetInt("SelectedEnvironmentId", 0);

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
        string jsonData = JsonUtility.ToJson(object2D);
        string authToken = PlayerPrefs.GetString("AuthToken", "");

        if (string.IsNullOrEmpty(authToken)) yield break;

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Object saved successfully.");
        }
        else
        {
            Debug.LogError("Error saving object: " + request.error);
        }
    }

    public void LoadObjectsFromEnvironment()
    {
        string url = $"{apiUrl}/environment/{environmentId}";
        StartCoroutine(GetObjectsFromServer(url));
    }

    IEnumerator GetObjectsFromServer(string url)
    {
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        if (string.IsNullOrEmpty(authToken)) yield break;

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                Object2D[] objects = JsonHelper.FromJson<Object2D>(request.downloadHandler.text);

                if (objects != null)
                {
                    foreach (var objectData in objects)
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
        }
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
