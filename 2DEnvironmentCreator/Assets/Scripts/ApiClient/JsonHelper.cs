using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }

    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    public static List<T> ParseJsonArray<T>(string jsonArray)
    {
        string extendedJson = "{\"list\":" + jsonArray + "}";
        JsonList<T> parsedList = JsonUtility.FromJson<JsonList<T>>(extendedJson);
        return parsedList.list;
    }

    public static string ExtractToken(string data)
    {
        Token token = JsonUtility.FromJson<Token>(data);
        return token.accessToken;
    }
}

[Serializable]
public class JsonList<T>
{
    public List<T> list;
}
