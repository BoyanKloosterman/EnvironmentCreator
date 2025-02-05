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
    public TextMeshProUGUI feedbackText;  // Add feedback text for error messages
    private string apiUrl = "http://localhost:5067/api/worlds";

    void Start()
    {
        createWorldButton.onClick.AddListener(() => SceneManager.LoadScene("CreateWorldScene"));
        StartCoroutine(GetWorlds());
    }

    IEnumerator GetWorlds()
    {
        string token = PlayerPrefs.GetString("AuthToken");

        if (string.IsNullOrEmpty(token))
        {
            feedbackText.text = "Token ontbreekt. Log opnieuw in.";
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // Log the raw response body for debugging
                Debug.Log("Response body: " + request.downloadHandler.text);

                // Use Newtonsoft.Json to directly deserialize the JSON array
                List<Environment2D> worlds = JsonConvert.DeserializeObject<List<Environment2D>>(request.downloadHandler.text);

                if (worlds == null)
                {
                    throw new System.Exception("Worlds list is null.");
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
                feedbackText.text = "Fout bij het ophalen van werelden.";
            }
        }
        else
        {
            Debug.LogError("Fout bij ophalen werelden: " + request.error);
            feedbackText.text = "Fout bij het ophalen van werelden: " + request.error;
            Debug.LogError("Response body: " + request.downloadHandler.text);  // Log the response body for debugging
        }
    }






    void AddWorldToUI(Environment2D world)
    {
        GameObject obj = Instantiate(worldPrefab, worldsPanel);

        // Use TextMeshProUGUI for the TMP component instead of the older Text component
        TextMeshProUGUI nameText = obj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameText.text = world.name;  // Set the world name from the database

        Button deleteButton = obj.transform.Find("DeleteButton").GetComponent<Button>();
        deleteButton.onClick.AddListener(() => StartCoroutine(DeleteWorld(world.environmentId, obj)));
    }


    IEnumerator DeleteWorld(int id, GameObject worldItem)
    {
        string token = PlayerPrefs.GetString("AuthToken");

        if (string.IsNullOrEmpty(token))
        {
            feedbackText.text = "Token ontbreekt. Log opnieuw in.";
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Delete(apiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Destroy(worldItem);
            feedbackText.text = "Wereld verwijderd!";
        }
        else
        {
            Debug.LogError("Verwijderen mislukt: " + request.error);
            feedbackText.text = "Verwijderen mislukt: " + request.error;
        }
    }

    [System.Serializable]
    public class Environment2DList
    {
        public List<Environment2D> worlds;
    }
}
