using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventory;
    public Transform slotsParent; // Assign the parent object containing all slots
    public GameObject slotPrefab; // Assign the slot prefab (if slots are dynamic)

    public List<GameObject> slots = new List<GameObject>();

    public GameObject dicePrefab;

    private void Awake()
    {
        inventory.SetActive(false);

        // Initialize slots
        foreach (Transform slot in slotsParent)
        {
            slots.Add(slot.gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        inventory.SetActive(!inventory.activeSelf);
    }

    public void AddItem(GameObject itemPrefab)
    {
        foreach (GameObject slot in slots)
        {
            if (slot.transform.childCount == 0) // Find an empty slot
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.transform.localScale = Vector3.one; // Ensure correct scale
                return;
            }
        }

        Debug.Log("Inventory Full!");
    }

    // Test adding items
    private void Start()
    {
        AddItem(dicePrefab);
    }
}
