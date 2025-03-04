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
        string route = "/api/Objects/" + object2D.id;
        string data = JsonUtility.ToJson(object2D);

        return await webClient.SendPutRequest(route, data);
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
                Debug.Log("Response data raw: " + data.Data);
                List<Object2D> objects = JsonHelper.ParseJsonArray<Object2D>(data.Data);
                WebRequestData<List<Object2D>> parsedData = new WebRequestData<List<Object2D>>(objects);
                return parsedData;
            default:
                return webRequestResponse;
        }
    }
}
