using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebClient : MonoBehaviour
{
    public string baseUrl;
    private string token;

    public void SetToken(string token)
    {
        this.token = token;
        Debug.Log("Token set: " + token);
    }

    public async Task<IWebRequestReponse> SendGetRequest(string route)
    {
        UnityWebRequest webRequest = CreateWebRequest("GET", route, "");
        return await SendWebRequest(webRequest);
    }

    public async Task<IWebRequestReponse> SendPostRequest(string route, string data)
    {
        UnityWebRequest webRequest = CreateWebRequest("POST", route, data);
        return await SendWebRequest(webRequest);
    }

    public async Task<IWebRequestReponse> SendPutRequest(string route, string data)
    {
        UnityWebRequest webRequest = CreateWebRequest("PUT", route, data);
        return await SendWebRequest(webRequest);
    }

    public async Task<IWebRequestReponse> SendDeleteRequest(string route)
    {
        UnityWebRequest webRequest = CreateWebRequest("DELETE", route, "");
        return await SendWebRequest(webRequest);
    }

    private UnityWebRequest CreateWebRequest(string type, string route, string data)
    {
        string url = baseUrl + route;
        Debug.Log("Creating " + type + " request to " + url + " with data: " + data);

        data = RemoveIdFromJson(data); // Backend throws error if it receiving empty strings as a GUID value.
        var webRequest = new UnityWebRequest(url, type);
        byte[] dataInBytes = new UTF8Encoding().GetBytes(data);
        webRequest.uploadHandler = new UploadHandlerRaw(dataInBytes);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(token))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + token);
            Debug.Log("Authorization header set: Bearer " + token);
        }
        else
        {
            Debug.LogWarning("Authorization header not set: Token is null or empty");
        }

        return webRequest;
    }

    private async Task<IWebRequestReponse> SendWebRequest(UnityWebRequest webRequest)
    {
        await webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string responseData = webRequest.downloadHandler.text;
            return new WebRequestData<string>(responseData);
        }
        else
        {
            Debug.LogError($"Request failed: {webRequest.responseCode} - {webRequest.error}");
            Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            return new WebRequestError(webRequest.error);
        }
    }

    private string RemoveIdFromJson(string json)
    {
        return json.Replace("\"id\":\"\",", "");
    }
}

[Serializable]
public class Token
{
    public string tokenType;
    public string accessToken;
}
