using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

public class EnvironmentSelect : MonoBehaviour
{
    public GameObject worldPrefab;
    public Transform worldsPanel;
    public Button createWorldButton;
    public Button logoutButton;
    public Environment2DApiClient environmentApiClient;

    internal void Start()
    {
        string token = PlayerPrefs.GetString("AuthToken", "").Trim();
        Debug.Log("Stored Auth Token: " + token);

        createWorldButton.onClick.AddListener(() => SceneManager.LoadScene("EnvironmentCreateScene"));

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }
        else
        {
            Debug.LogWarning("Logout button reference not set. Please assign it in the Inspector.");
        }

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

    public void Logout()
    {
        // Clear authentication data
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("SelectedEnvironmentId");
        PlayerPrefs.Save();

        Debug.Log("User logged out successfully");

        SceneManager.LoadScene("LoginScene");
    }

    private async void LoadWorlds()
    {
        var response = await environmentApiClient.ReadEnvironment2Ds();

        if (response is WebRequestData<List<Environment2D>> data)
        {
            List<Environment2D> worlds = data.Data;
            if (worlds == null || worlds.Count == 0) return;

            PlayerPrefs.SetString("userId", worlds[0].userId); // Changed from SetInt to SetString
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

    internal void AddWorldToUI(Environment2D world)
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
        SceneManager.LoadScene("EnvironmentScene");
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
        // Send the delete request
        var response = await environmentApiClient.DeleteEnvironment(environmentId.ToString());

        // Check if the response is of the expected type (204 No Content or empty response)
        if (response is WebRequestData<Environment2D> data && data.Data == null)
        {
            // Successfully deleted, no content in response
            Destroy(worldObject);
            Debug.Log("World deleted successfully.");
        }
        else
        {
            // Unexpected response type or error
            Debug.LogError("Unexpected response or error deleting world. Response: " + response?.GetType());
        }
    }


}
