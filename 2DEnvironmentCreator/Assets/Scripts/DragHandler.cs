using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 mouseOffset;
    private bool hasCloned = false;


    private void OnMouseDown()
    {
        isDragging = true;

        mouseOffset = transform.position - GetMouseWorldPosition();

        if (!hasCloned)
        {
            CloneDice();
            hasCloned = true;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + mouseOffset;
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
        clone.GetComponent<DiceDragHandler>().hasCloned = false;
    }
}
