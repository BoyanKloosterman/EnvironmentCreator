using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
    public Button backButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("WorldSelectScene"));
    }

    // Update is called once per frame

}
