using UnityEngine;

public class DiceDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 mouseOffset;
    private bool hasCloned = false;
    private EnvironmentManager environmentManager;
    public int prefabId = 1;

    private void Start()
    {
        environmentManager = FindFirstObjectByType<EnvironmentManager>();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        mouseOffset = transform.position - GetMouseWorldPosition();

        if (environmentManager != null)
        {
            environmentManager.lastSelectedObject = gameObject;
        }

        // If this dice has never been instantiated (original prefab), clone it
        if (!hasCloned)
        {
            CloneDice(); // Instantiate new dice
            hasCloned = true;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (environmentManager != null && environmentManager.lastSelectedObject == gameObject)
        {
            // Mark the dragged object as selected, but don't save yet
            environmentManager.SelectObject(gameObject, prefabId);
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + mouseOffset;
        }

        if (environmentManager.lastSelectedObject == gameObject)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateObject(15f);
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                ScaleObject(-10f);
            }

            if (Input.GetKeyDown(KeyCode.Equals))
            {
                ScaleObject(10f);
            }
        }
    }
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void CloneDice()
    {
        GameObject clone = Instantiate(gameObject, transform.position, transform.rotation);
        DiceDragHandler cloneHandler = clone.GetComponent<DiceDragHandler>();
        cloneHandler.hasCloned = false;
    }

    private float lastRotationZ = 0f;
    private const float rotationThreshold = 1f;

    private void RotateObject(float angle)
    {
        float newRotationZ = transform.rotation.eulerAngles.z + angle;

        newRotationZ = (newRotationZ + 360f) % 360f;

        if (Mathf.Abs(newRotationZ - lastRotationZ) > rotationThreshold)
        {
            transform.Rotate(0, 0, angle);
            lastRotationZ = newRotationZ;
        }
    }

    // Method to scale the object
    private void ScaleObject(float scaleChange)
    {
        Vector3 newScale = transform.localScale + new Vector3(scaleChange, scaleChange, 0);
        transform.localScale = new Vector3(Mathf.Max(10f, newScale.x), Mathf.Max(10f, newScale.y), 1);
    }

}
