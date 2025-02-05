using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class WorldSelect : MonoBehaviour
{
    public GameObject worldPrefab;
    public Transform worldsPanel;
    public Button createWorldButton;
    private string apiUrl = "http://localhost:5067/api/worlds";

    void Start()
    {
        createWorldButton.onClick.AddListener(() => SceneManager.LoadScene("CreateWorldScene"));
        StartCoroutine(GetWorlds());
    }

    IEnumerator GetWorlds()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("token"));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            List<Environment2D> worlds = JsonUtility.FromJson<WorldList>("{\"worlds\":" + request.downloadHandler.text + "}").worlds;

            int count = Mathf.Min(worlds.Count, 5);
            for (int i = 0; i < count; i++)
            {
                AddWorldToUI(worlds[i]);
            }
        }
        else
        {
            Debug.LogError("Fout bij ophalen werelden: " + request.error);
        }
    }

    void AddWorldToUI(Environment2D world)
    {
        GameObject obj = Instantiate(worldPrefab, worldsPanel);
        obj.transform.Find("NameText").GetComponent<Text>().text = world.name;

        Button deleteButton = obj.transform.Find("DeleteButton").GetComponent<Button>();
        deleteButton.onClick.AddListener(() => StartCoroutine(DeleteWorld(world.environmentId, obj)));
    }

    IEnumerator DeleteWorld(int id, GameObject worldItem)
    {
        UnityWebRequest request = UnityWebRequest.Delete(apiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("token"));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Destroy(worldItem);
        }
        else
        {
            Debug.LogError("Verwijderen mislukt: " + request.error);
        }
    }

    [System.Serializable]
    public class WorldList
    {
        public List<Environment2D> worlds;
    }
}
