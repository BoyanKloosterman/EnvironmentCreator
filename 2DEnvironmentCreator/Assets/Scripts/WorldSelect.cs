using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WorldSelect : MonoBehaviour
{
    public GameObject worldPrefab;
    public Transform worldsPanel;
    public Button createWorldButton;
    private string apiUrl = "http://localhost:5067/api/worlds";

    void Start()
    {
        createWorldButton.onClick.AddListener(() => SceneManager.LoadScene("WorldCreateScene"));
        StartCoroutine(GetWorlds());
    }

    IEnumerator GetWorlds()
    {
        string token = PlayerPrefs.GetString("AuthToken");

        if (string.IsNullOrEmpty(token))
        {
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                Debug.Log("Response body: " + request.downloadHandler.text);

                List<Environment2D> worlds = JsonConvert.DeserializeObject<List<Environment2D>>(request.downloadHandler.text);

                if (worlds == null)
                {
                    throw new System.Exception("Worlds list is null.");
                }

                if (worlds.Count > 0)
                {
                    int userId = worlds[0].userId;
                    PlayerPrefs.SetInt("UserId", userId);
                    PlayerPrefs.Save();
                    Debug.Log("UserId saved: " + userId);
                }

                int count = Mathf.Min(worlds.Count, 5);
                for (int i = 0; i < count; i++)
                {
                    AddWorldToUI(worlds[i]);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing world data: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Error fetching worlds: " + request.error);
            Debug.LogError("Response body: " + request.downloadHandler.text);
        }
    }

    void AddWorldToUI(Environment2D world)
    {
        GameObject obj = Instantiate(worldPrefab, worldsPanel);

        // Find the components
        TextMeshProUGUI nameText = obj.transform.Find("WorldName")?.GetComponent<TextMeshProUGUI>();
        Button worldButton = obj.GetComponent<Button>(); // Assuming the entire prefab is a button
        Button deleteButton = obj.transform.Find("DeleteButton")?.GetComponent<Button>();

        // Set the name text
        if (nameText != null)
        {
            nameText.text = world.name;
        }
        else
        {
            Debug.LogError("WorldName TextMeshProUGUI not found in the prefab.");
        }

        // Add a listener to the world button to load the environment scene
        if (worldButton != null)
        {
            worldButton.onClick.AddListener(() => OnWorldButtonClicked(world.environmentId));
        }
        else
        {
            Debug.LogError("World Button not found in the prefab.");
        }

        // Set up the delete button
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => StartCoroutine(DeleteWorld(world.environmentId, obj)));
        }
        else
        {
            Debug.LogError("DeleteButton not found in the prefab.");
        }
    }

    // Method to handle when the world button is clicked
    void OnWorldButtonClicked(int environmentId)
    {
        // Assuming you want to load a scene based on the environmentId, you can load a scene like this
        // For example, use the environmentId to load the scene dynamically or pass it through the scene manager
        PlayerPrefs.SetInt("SelectedEnvironmentId", environmentId); // Save the selected environmentId to PlayerPrefs
        SceneManager.LoadScene("WorldScene"); // Load the scene where the environmentId will be used
    }


    IEnumerator DeleteWorld(int environmentId, GameObject worldObject)
    {
        string token = PlayerPrefs.GetString("AuthToken");
        UnityWebRequest request = UnityWebRequest.Delete(apiUrl + "/" + environmentId);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("World deleted successfully.");
            Destroy(worldObject);
        }
        else
        {
            Debug.LogError("Error deleting world: " + request.error);
            Debug.LogError("Response body: " + request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class Environment2D
    {
        public int environmentId;
        public string name;
        public int userId;
    }
}