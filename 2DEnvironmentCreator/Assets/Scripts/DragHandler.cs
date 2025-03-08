using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

public class DiceDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 mouseOffset;
    public EnvironmentManager environmentManager;
    public int prefabId = 1;

    private bool hasServerRecord = false;
    private Object2D existingObjectData;
    private Vector3 lastSavedPosition;
    private Vector3 originalScale;

    // Animation parameters
    public float clickGrowFactor = 1.2f;
    public float clickGrowDuration = 0.3f;
    public float wiggleDuration = 0.5f;
    public float wiggleStrength = 5f;
    private bool isAnimating = false;

    public bool hasBeenMoved { get; private set; } = false;

    private void Start()
    {
        environmentManager = FindFirstObjectByType<EnvironmentManager>();
        lastSavedPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        mouseOffset = transform.position - GetMouseWorldPosition();

        if (environmentManager != null)
        {
            environmentManager.lastSelectedObject = gameObject;
        }

        // Start grow animation when clicked
        StartCoroutine(GrowOnClick());
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (environmentManager != null && environmentManager.lastSelectedObject == gameObject)
        {
            bool positionChanged = Vector3.Distance(transform.position, lastSavedPosition) > 0.01f;

            // Check if position has actually changed
            if (!hasServerRecord)
            {
                // First-time object, always save and wiggle
                SaveFirstTimeObject();
                StartCoroutine(WiggleOnPlace());
            }
            else if (positionChanged)
            {
                // Update existing object if position changed significantly
                hasBeenMoved = true;
                StartCoroutine(WiggleOnPlace());
            }
        }
    }

    private IEnumerator GrowOnClick()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        Vector3 targetScale = originalScale * clickGrowFactor;
        float elapsedTime = 0f;

        // Grow
        while (elapsedTime < clickGrowDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (clickGrowDuration / 2);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Shrink back
        elapsedTime = 0f;
        while (elapsedTime < clickGrowDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (clickGrowDuration / 2);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }

    private IEnumerator WiggleOnPlace()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        float elapsedTime = 0f;
        Quaternion originalRotation = transform.rotation;

        while (elapsedTime < wiggleDuration)
        {
            elapsedTime += Time.deltaTime;
            float wiggleAmount = Mathf.Sin(elapsedTime * 20f) * wiggleStrength * (1f - elapsedTime / wiggleDuration);
            transform.rotation = originalRotation * Quaternion.Euler(0, 0, wiggleAmount);
            yield return null;
        }

        transform.rotation = originalRotation;
        isAnimating = false;
    }

    private void SaveFirstTimeObject()
    {
        // Create a new Object2D for first-time save
        Object2D newObject = new Object2D(
            environmentManager.environmentId,
            prefabId,
            transform.position.x,
            transform.position.y,
            transform.localScale.x,
            transform.localScale.y,
            transform.rotation.eulerAngles.z,
            GetComponent<Renderer>().sortingOrder
        );

        // Use the API client to create the object
        CreateFirstTimeObject(newObject);
    }

    private async void CreateFirstTimeObject(Object2D newObject)
    {
        try
        {
            var response = await environmentManager.object2DApiClient.CreateObject2D(newObject);

            if (response is WebRequestData<Object2D> successResponse)
            {
                Debug.Log($"First-time object created successfully: {successResponse.Data.id}");

                // Set the existing object data and mark as having server record
                existingObjectData = successResponse.Data;
                hasServerRecord = true;
                hasBeenMoved = false;
                lastSavedPosition = transform.position;
            }
            else
            {
                Debug.LogError("Failed to create first-time object");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during first-time object creation: {ex.Message}");
        }
    }

    public async void UpdateExistingObject()
    {
        if (!hasBeenMoved || existingObjectData == null)
        {
            Debug.Log("No need to update object");
            return;
        }

        try
        {
            // Update the existing object data with current transform
            existingObjectData.positionX = transform.position.x;
            existingObjectData.positionY = transform.position.y;
            existingObjectData.scaleX = transform.localScale.x;
            existingObjectData.scaleY = transform.localScale.y;
            existingObjectData.rotationZ = transform.rotation.eulerAngles.z;

            var response = await environmentManager.object2DApiClient.UpdateObject2D(existingObjectData);

            switch (response)
            {
                case WebRequestData<Object2D> successResponse:
                    Debug.Log($"Object updated successfully: {successResponse.Data.id}");
                    hasBeenMoved = false;
                    lastSavedPosition = transform.position;
                    break;
                case WebRequestData<string> stringResponse:
                    Debug.Log($"Object updated with string response: {stringResponse.Data}");
                    hasBeenMoved = false;
                    lastSavedPosition = transform.position;
                    break;
                default:
                    Debug.LogError($"Unhandled response type: {response?.GetType()}");
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during object update: {ex.Message}");
        }
    }

    // Method to reset the moved state after saving
    public void ResetMovedState()
    {
        hasBeenMoved = false;
        lastSavedPosition = transform.position;
    }

    // Method to set the existing object data when loading from server
    public void SetExistingObjectData(Object2D objectData)
    {
        if (objectData != null && objectData.id > 0)
        {
            existingObjectData = objectData;
            hasServerRecord = true;
            lastSavedPosition = new Vector3(objectData.positionX, objectData.positionY, 0);
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

    private void ScaleObject(float scaleChange)
    {
        Vector3 newScale = transform.localScale + new Vector3(scaleChange, scaleChange, 0);
        transform.localScale = new Vector3(Mathf.Max(10f, newScale.x), Mathf.Max(10f, newScale.y), 1);
    }
}