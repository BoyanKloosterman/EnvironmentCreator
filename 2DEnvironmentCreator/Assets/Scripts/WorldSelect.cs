using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class WorldSelect : MonoBehaviour
{
    public GameObject worldPrefab;
    public Transform worldsPanel;
    public Button createWorldButton;
    public Environment2DApiClient environmentApiClient; // Reference to the Environment2DApiClient

    void Start()
    {
        string token = PlayerPrefs.GetString("AuthToken", "").Trim();
        Debug.Log("Stored Auth Token: " + token); // Check if token is stored correctly

        createWorldButton.onClick.AddListener(() => SceneManager.LoadScene("WorldCreateScene"));

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token is missing! Redirecting to login.");
            SceneManager.LoadScene("LoginScene");
        }
        else
        {
            environmentApiClient.webClient.SetToken(token); // Set the token in the WebClient instance
            LoadWorlds();
        }
    }

    private async void LoadWorlds()
    {
        var response = await environmentApiClient.ReadEnvironment2Ds();

        if (response is WebRequestData<List<Environment2D>> data)
        {
            List<Environment2D> worlds = data.Data;
            if (worlds == null || worlds.Count == 0) return;

            PlayerPrefs.SetInt("UserId", worlds[0].userId);
            PlayerPrefs.Save();

            for (int i = 0; i < Mathf.Min(worlds.Count, 5); i++)
            {
                AddWorldToUI(worlds[i]);
            }
        }
        else
        {
            Debug.LogError("Failed to load worlds.");
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
        string token = PlayerPrefs.GetString("AuthToken", "");

        // Start the async task within the coroutine
        var task = DeleteWorldAsync(environmentId, token, worldObject);
        while (!task.IsCompleted)
        {
            yield return null; // Wait until the task is completed
        }
    }

    private async Task DeleteWorldAsync(int environmentId, string token, GameObject worldObject)
    {
        var response = await environmentApiClient.DeleteEnvironment(environmentId.ToString());

        if (response is WebRequestData<string> data && data.Data == "Succes")
        {
            Destroy(worldObject);
        }
        else
        {
            Debug.LogError("Error deleting world: " + response.ToString());
        }
    }
}
