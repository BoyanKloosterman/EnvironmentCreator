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
    private string apiUrl = "https://avansict2226638.azurewebsites.net/api/environment";

    void Start()
    {
        string token = PlayerPrefs.GetString("AuthToken", "");
        Debug.Log("Stored Auth Token: " + token); // Check if token is stored correctly
        createWorldButton.onClick.AddListener(() => SceneManager.LoadScene("WorldCreateScene"));
        StartCoroutine(GetWorlds());
    }

    IEnumerator GetWorlds()
    {
        string token = PlayerPrefs.GetString("AuthToken", "").Trim();
        Debug.Log($"Sending Auth Token: Bearer {token}");

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token is missing! Redirecting to login.");
            SceneManager.LoadScene("LoginScene");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json"); // Ensure this is set

        yield return request.SendWebRequest();

        Debug.Log($"Response Code: {request.responseCode}"); // Log response code

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                List<Environment2D> worlds = JsonConvert.DeserializeObject<List<Environment2D>>(request.downloadHandler.text);
                if (worlds == null || worlds.Count == 0) yield break;

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
            Debug.LogError("Error fetching worlds: " + request.error + " | Response: " + request.downloadHandler.text);
            if (request.responseCode == 401)
            {
                Debug.LogError("Unauthorized! Clearing token and redirecting to login.");
                PlayerPrefs.DeleteKey("AuthToken");
                SceneManager.LoadScene("LoginScene"); // Redirect user to login
            }
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


}
