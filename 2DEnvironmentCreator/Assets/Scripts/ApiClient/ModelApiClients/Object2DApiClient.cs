using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Object2DApiClient : MonoBehaviour
{
    public WebClient webClient;

    public async Awaitable<IWebRequestReponse> ReadObject2Ds(string environmentId)
    {
        string route = "/api/Objects/environment/" + environmentId;

        IWebRequestReponse webRequestResponse = await webClient.SendGetRequest(route);
        return ParseObject2DListResponse(webRequestResponse);
    }

    public async Awaitable<IWebRequestReponse> CreateObject2D(Object2D object2D)
    {
        string route = "/api/Objects";
        string data = JsonUtility.ToJson(object2D);

        IWebRequestReponse webRequestResponse = await webClient.SendPostRequest(route, data);
        return ParseObject2DResponse(webRequestResponse);
    }

    public async Awaitable<IWebRequestReponse> UpdateObject2D(Object2D object2D)
    {
        if (object2D == null)
        {
            Debug.LogError("Attempted to update null Object2D");
            return null;
        }

        string route = "/api/Objects/" + object2D.id;
        string data = JsonUtility.ToJson(object2D);

        Debug.Log($"Updating object - Route: {route}, Data: {data}");

        try
        {
            IWebRequestReponse webRequestResponse = await webClient.SendPutRequest(route, data);

            // Log the raw response for debugging
            if (webRequestResponse is WebRequestData<string> stringResponse)
            {
                Debug.Log($"Raw response: {stringResponse.Data}");
            }

            // Parse the response similar to CreateObject2D method
            return ParseObject2DResponse(webRequestResponse);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during object update: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Awaitable<IWebRequestReponse> DeleteObject2D(Object2D object2D)
    {
        string route = "/api/Objects/" + object2D.id;

        IWebRequestReponse webRequestResponse = await webClient.SendDeleteRequest(route);
        return webRequestResponse;
    }


    private IWebRequestReponse ParseObject2DResponse(IWebRequestReponse webRequestResponse)
    {
        switch (webRequestResponse)
        {
            case WebRequestData<string> data:
                Debug.Log("Response data raw: " + data.Data);
                Object2D object2D = JsonUtility.FromJson<Object2D>(data.Data);
                WebRequestData<Object2D> parsedWebRequestData = new WebRequestData<Object2D>(object2D);
                return parsedWebRequestData;
            default:
                return webRequestResponse;
        }
    }

    private IWebRequestReponse ParseObject2DListResponse(IWebRequestReponse webRequestResponse)
    {
        switch (webRequestResponse)
        {
            case WebRequestData<string> data:
                try
                {
                    // Directly parse the JSON array
                    List<Object2D> objects = JsonHelper.ParseJsonArray<Object2D>(data.Data);

                    // Log parsed objects for verification
                    foreach (var obj in objects)
                    {
                        Debug.Log($"Parsed Object - ID: {obj.id}, PrefabId: {obj.prefabId}, Position: ({obj.positionX}, {obj.positionY})");
                    }

                    WebRequestData<List<Object2D>> parsedData = new WebRequestData<List<Object2D>>(objects);
                    return parsedData;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    Debug.LogError($"Problematic JSON: {data.Data}");
                    return webRequestResponse;
                }
            default:
                return webRequestResponse;
        }
    }

    // Custom JSON parsing method to handle potential ID parsing issues
    private List<Object2D> ParseCustomJsonArray(string json)
    {
        // If using JsonUtility, it might not parse nested objects correctly
        // You might need to modify this based on your exact JSON structure
        if (json.Trim().StartsWith("{"))
        {
            // Likely a single object or wrapped response
            json = ExtractObjectArrayFromResponse(json);
        }

        // Fallback parsing method
        List<Object2D> objects = JsonHelper.ParseJsonArray<Object2D>(json);

        // Try to fix zero IDs if possible
        foreach (var obj in objects)
        {
            // Log original object details for debugging
            Debug.Log($"Original Parsed Object - ID: {obj.id}, PrefabId: {obj.prefabId}");
        }

        return objects;
    }

    // Helper method to extract array from potential wrapper
    private string ExtractObjectArrayFromResponse(string json)
    {
        // This method attempts to extract the array from potential wrapper JSON
        // Modify based on your specific API response structure
        try
        {
            // Common patterns to extract the array
            if (json.Contains("\"data\""))
            {
                int startIndex = json.IndexOf("\"data\"") + 7;
                int endIndex = json.LastIndexOf("}");
                return json.Substring(startIndex, endIndex - startIndex).Trim('[', ']');
            }

            // Add more extraction patterns as needed
            return json;
        }
        catch
        {
            Debug.LogError("Failed to extract array from JSON response");
            return json;
        }
    }
}
