using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class UserApiClient : MonoBehaviour
{
    public WebClient webClient;

    public async Awaitable<IWebRequestReponse> Register(User user)
    {
        string route = "/account/register";
        string data = JsonUtility.ToJson(user);

        return await webClient.SendPostRequest(route, data);
    }

    public async Awaitable<IWebRequestReponse> Login(User user)
    {
        string route = "/account/login";
        string data = JsonUtility.ToJson(user);

        IWebRequestReponse response = await webClient.SendPostRequest(route, data);
        return ProcessLoginResponse(response);
    }

    private IWebRequestReponse ProcessLoginResponse(IWebRequestReponse webRequestResponse)
    {
        switch (webRequestResponse)
        {
            case WebRequestData<string> data:
                Debug.Log("Response data raw: " + data.Data);
                // Assuming the token is in the response JSON under the "token" key
                string token = ExtractTokenFromJson(data.Data);
                webClient.SetToken(token);
                PlayerPrefs.SetString("AuthToken", token);  // Save the token locally
                return new WebRequestData<string>("Succes");
            default:
                return webRequestResponse;
        }
    }

    private string ExtractTokenFromJson(string json)
    {
        // Extract the token value from the JSON response
        var tokenStart = json.IndexOf("\"token\":\"") + 9;
        var tokenEnd = json.IndexOf("\"", tokenStart);
        return json.Substring(tokenStart, tokenEnd - tokenStart);
    }


}

