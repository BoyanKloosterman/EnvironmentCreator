using UnityEngine;

public class WebRequestError : IWebRequestReponse
{
    public int StatusCode { get; private set; }
    public string Message { get; private set; }
    public string Details { get; private set; }

    public WebRequestError(string message, int statusCode = 0, string details = "")
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }

    public override string ToString()
    {
        return "WebRequestError";
    }
}
