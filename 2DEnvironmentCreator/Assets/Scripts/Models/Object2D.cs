using UnityEngine;

[System.Serializable]
public class Object2D
{
    public int id;
    public int environmentId;
    public int prefabId;
    public float positionX;
    public float positionY;
    public float scaleX;
    public float scaleY;
    public float rotationZ;
    public int sortingLayer;

    public Object2D(int environmentId, int prefabId, float positionX, float positionY, float scaleX, float scaleY, float rotationZ, int sortingLayer)
    {
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
