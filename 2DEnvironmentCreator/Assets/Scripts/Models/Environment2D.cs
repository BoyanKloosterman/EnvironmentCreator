using Newtonsoft.Json;

[System.Serializable]
public class Environment2D
{
    public int environmentId;
    public string name;
    public string userId;

    [JsonProperty("MaxHeight")]
    public int height;

    [JsonProperty("MaxWidth")]
    public int width;
}

