using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class Environment2DApiClient : MonoBehaviour
{
    public WebClient webClient;

    public async Task<IWebRequestReponse> ReadEnvironment2Ds()
    {
        string route = "/api/environment";
        IWebRequestReponse response = await webClient.SendGetRequest(route);
        return ParseEnvironment2DListResponse(response);
    }

    public async Task<IWebRequestReponse> CreateEnvironment(Environment2D environment)
    {
        string route = "/api/environment";
        string data = JsonConvert.SerializeObject(environment);
        IWebRequestReponse response = await webClient.SendPostRequest(route, data);
        return ParseEnvironment2DResponse(response);
    }

    public async Task<IWebRequestReponse> DeleteEnvironment(string environmentId)
    {
        string route = $"/api/environment/{environmentId}";
        IWebRequestReponse response = await webClient.SendDeleteRequest(route);
        return ParseEnvironment2DResponse(response);
    }

    private IWebRequestReponse ParseEnvironment2DResponse(IWebRequestReponse webRequestResponse)
    {
        if (webRequestResponse is WebRequestData<string> data)
        {
            Environment2D environment = JsonConvert.DeserializeObject<Environment2D>(data.Data);
            return new WebRequestData<Environment2D>(environment);
        }
        return webRequestResponse;
    }

    private IWebRequestReponse ParseEnvironment2DListResponse(IWebRequestReponse webRequestResponse)
    {
        if (webRequestResponse is WebRequestData<string> data)
        {
            List<Environment2D> environments = JsonConvert.DeserializeObject<List<Environment2D>>(data.Data);
            return new WebRequestData<List<Environment2D>>(environments);
        }
        return webRequestResponse;
    }
}
