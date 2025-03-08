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

    private async Task<IWebRequestReponse> SendWebRequest(UnityWebRequest request)
    {
        try
        {
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Request failed: " + request.responseCode + " - " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);

                return new WebRequestError(
                    statusCode: (int)request.responseCode,
                    message: request.error,
                    details: request.downloadHandler.text
                );
            }

            return new WebRequestData<string>(request.downloadHandler.text);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception in web request: " + e.Message);
            return new WebRequestError(message: e.Message);
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
