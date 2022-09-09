using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;

[RequireComponent(typeof(TwitchChatManager), typeof(TwitchApiCallHelper))]
public class TwitchOAuth : MonoBehaviour
{
    [SerializeField] private string twitchAuthUrl = "https://id.twitch.tv/oauth2/authorize";
    [SerializeField] private string twitchRedirectUrl = "http://localhost:8080/";
    private TwitchApiCallHelper twitchApiCallHelper = null;

    private string _twitchAuthStateVerify;
    private string _authToken;
    private string _refreshToken;

    TwitchChatManager twitchChatManager;

    private void Start()
    {
        twitchChatManager = GetComponent<TwitchChatManager>();
        twitchApiCallHelper = GetComponent<TwitchApiCallHelper>();

        _authToken = "";
    }

    public void InitiateTwitchAuth()
    {
        string[] scopes;
        string s;


        // List of scopes we want. Avaiable scopes: https://dev.twitch.tv/docs/authentication/scopes
        scopes = new[]
        {
            "bits:read",
            "channel:manage:polls",
            "channel:manage:predictions",
            "channel:manage:raids",
            "channel:manage:redemptions",
            "channel:read:goals",
            "channel:read:hype_train",
            "channel:read:polls",
            "channel:read:predictions",
            "channel:read:redemptions",
            "channel:read:subscriptions",
            "moderation:read",
            "user:read:follows",
            "user:read:subscriptions",
            "channel:moderate",
            "chat:edit",
            "chat:read"

        };

        // Generate a state that will echoed back
        _twitchAuthStateVerify = ((Int64) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();


        s = "client_id=" + Secrets.client_id + "&" +
            "redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl) + "&" +
            "state=" + _twitchAuthStateVerify + "&" +
            "response_type=code&" +
            "scope=" + String.Join("+", scopes);

        StartLocalWebserver();

        // Open browser and go to the Twitch auth URL which will send us back to our webserver after success
        Application.OpenURL(twitchAuthUrl + "?" + s);
    }


    private void StartLocalWebserver()
    {
        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add(twitchRedirectUrl);
        httpListener.Start();
        httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest), httpListener);
    }

    private void IncomingHttpRequest(IAsyncResult result)
    {
        string code;
        string state;
        HttpListener httpListener;
        HttpListenerContext httpContext;
        HttpListenerRequest httpRequest;
        HttpListenerResponse httpResponse;
        string responseString;

        httpListener = (HttpListener) result.AsyncState;

        httpContext = httpListener.EndGetContext(result);

        httpRequest = httpContext.Request;

        code = httpRequest.QueryString.Get("code");
        state = httpRequest.QueryString.Get("state");

        // Check if we got code back and that the state is the same as we generated
        if ((code.Length > 0) && (state == _twitchAuthStateVerify))
        {
            GetTokenFromCode(code);
        }

        httpResponse = httpContext.Response;
        responseString = "<html><body><b>DONE!</b><br>(You can close this tab/window now)</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        httpResponse.ContentLength64 = buffer.Length;
        System.IO.Stream output = httpResponse.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        httpListener.Stop();
    }

    private void GetTokenFromCode(string code)
    {
        string apiUrl;
        string apiResponseJson;
        ApiCodeTokenResponse apiResponseData;

        // construct full URL for API call
        apiUrl = "https://id.twitch.tv/oauth2/token" +
                 "?client_id=" + Secrets.client_id +
                 "&client_secret=" + Secrets.client_secret +
                 "&code=" + code +
                 "&grant_type=authorization_code" +
                 "&redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl);

        // make sure our API helper knows our client ID (it needed for the HTTP headers)
        twitchApiCallHelper.TwitchClientId = Secrets.client_id;

        apiResponseJson = twitchApiCallHelper.CallApi(apiUrl, "POST");

        // Convert json in to data
        apiResponseData = JsonUtility.FromJson<ApiCodeTokenResponse>(apiResponseJson);

        _authToken = apiResponseData.access_token;
        _refreshToken = apiResponseData.refresh_token;
        
        twitchChatManager.storedAuthToken = _authToken;
        twitchChatManager.storedRefreshToken = _refreshToken;
        Debug.Log("Got auth token: " + _authToken);
        Debug.Log("Got refresh token: " + _refreshToken);
       
    }


    // TODO: Implement updating a new auth token with the refresh token automatically once the auth token has expired



}
