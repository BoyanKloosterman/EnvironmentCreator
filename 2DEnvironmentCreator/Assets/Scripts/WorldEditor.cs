using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WorldEditor : MonoBehaviour
{
    public GameObject[] objectsToPlace;
    private GameObject selectedObject;

    public void SelectObject(int index)
    {
        selectedObject = objectsToPlace[index];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && selectedObject != null)
        {
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Instantiate(selectedObject, position, Quaternion.identity);
            StartCoroutine(SaveObject(position, selectedObject.name));
        }
    }

    IEnumerator SaveObject(Vector2 position, string objectType)
    {
        string token = PlayerPrefs.GetString("AuthToken");
        var jsonData = JsonUtility.ToJson(new { x = position.x, y = position.y, type = objectType });

        using (UnityWebRequest www = UnityWebRequest.Put("https://jouw-backend-url/api/objects", jsonData))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Authorization", "Bearer " + token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Fout bij opslaan: " + www.error);
        }
    }
}
