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
    private string apiUrl = "http://localhost:5067/api/environment";

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
                List<Environment2D> worlds = JsonConvert.DeserializeObject<List<Environment2D>>(request.downloadHandler.text);

                if (worlds == null || worlds.Count == 0)
                    yield break;

                PlayerPrefs.SetInt("UserId", worlds[0].userId);
                PlayerPrefs.Save();

                for (int i = 0; i < Mathf.Min(worlds.Count, 5); i++)
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
        }
    }

    void AddWorldToUI(Environment2D world)
    {
        GameObject obj = Instantiate(worldPrefab, worldsPanel);

        TextMeshProUGUI nameText = obj.transform.Find("WorldName")?.GetComponent<TextMeshProUGUI>();
        Button worldButton = obj.GetComponent<Button>();
        Button deleteButton = obj.transform.Find("DeleteButton")?.GetComponent<Button>();

        if (nameText != null)
        {
            nameText.text = world.name;
        }

        if (worldButton != null)
        {
            worldButton.onClick.AddListener(() => OnWorldButtonClicked(world.environmentId));
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => StartCoroutine(DeleteWorld(world.environmentId, obj)));
        }
    }

    void OnWorldButtonClicked(int environmentId)
    {
        PlayerPrefs.SetInt("SelectedEnvironmentId", environmentId);
        PlayerPrefs.Save();
        SceneManager.LoadScene("WorldScene");
    }

    IEnumerator DeleteWorld(int environmentId, GameObject worldObject)
    {
        string token = PlayerPrefs.GetString("AuthToken");
        UnityWebRequest request = UnityWebRequest.Delete(apiUrl + "/" + environmentId);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Destroy(worldObject);
        }
        else
        {
            Debug.LogError("Error deleting world: " + request.error);
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
