using UnityEngine;

[System.Serializable]
public class Object2D
{
    [SerializeField] private int objectId;
    public int id => objectId;

    public int environmentId;
    public int prefabId;
    public float positionX;
    public float positionY;
    public float scaleX;
    public float scaleY;
    public float rotationZ;
    public int sortingLayer;

    public Object2D()
    {
        objectId = -1;
        environmentId = 0;
        prefabId = 0;
        positionX = 0f;
        positionY = 0f;
        scaleX = 1f;
        scaleY = 1f;
        rotationZ = 0f;
        sortingLayer = 0;
    }

    public Object2D(int environmentId, int prefabId, float positionX, float positionY, float scaleX, float scaleY, float rotationZ, int sortingLayer)
    {
        this.objectId = -1;
        this.environmentId = environmentId;
        this.prefabId = prefabId;
        this.positionX = positionX;
        this.positionY = positionY;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.rotationZ = rotationZ;
        this.sortingLayer = sortingLayer;
    }
}