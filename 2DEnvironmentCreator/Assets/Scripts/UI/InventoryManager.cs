using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventory; // Reference to the inventory Canvas

    void Awake()
    {
        // Hide the inventory in the editor (before playing)
#if UNITY_EDITOR
        inventory.SetActive(false);
#endif
    }

    void Start()
    {
        // Hide inventory at the start of the game when the game is playing
        if (Application.isPlaying)
        {
            inventory.SetActive(false);
        }
    }

    void Update()
    {
        // Check if the Tab key is pressed during play mode
        if (Application.isPlaying && Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    // Toggle the inventory visibility
    void ToggleInventory()
    {
        // Toggle the active state of the inventory
        inventory.SetActive(!inventory.activeSelf);
    }
}
