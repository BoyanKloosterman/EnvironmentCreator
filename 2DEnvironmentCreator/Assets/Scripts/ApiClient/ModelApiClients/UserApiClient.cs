using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;

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
                // Deserialize the response to LoginResponse
                LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(data.Data);
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.accessToken))
                {
                    webClient.SetToken(loginResponse.accessToken);
                    PlayerPrefs.SetString("AuthToken", loginResponse.accessToken);  // Save the token locally
                    PlayerPrefs.SetString("TokenType", loginResponse.tokenType);
                    PlayerPrefs.SetString("RefreshToken", loginResponse.refreshToken);
                    PlayerPrefs.Save();
                    return new WebRequestData<string>("Succes");
                }
                return new WebRequestData<string>("Failed");
            default:
                return webRequestResponse;
        }
    }

}

