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

                // Save the userId from the first world in the list (if available)
                if (worlds.Count > 0)
                {
                    int userId = worlds[0].userId;  // Assuming userId is the same for all worlds
                    PlayerPrefs.SetInt("UserId", userId);  // Save userId in PlayerPrefs
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
            Debug.LogError("Fout bij ophalen werelden: " + request.error);
            Debug.LogError("Response body: " + request.downloadHandler.text);
        }
    }

    void AddWorldToUI(Environment2D world)
    {
        GameObject obj = Instantiate(worldPrefab, worldsPanel);

        // Update to find "WorldName" instead of "NameText"
        TextMeshProUGUI nameText = obj.transform.Find("WorldName")?.GetComponent<TextMeshProUGUI>();

        if (nameText != null)
        {
            nameText.text = world.name;  // Display the world name
        }
        else
        {
            Debug.LogError("WorldName TextMeshProUGUI not found in the prefab.");
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
