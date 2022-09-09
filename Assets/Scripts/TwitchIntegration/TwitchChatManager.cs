using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchLib.Unity;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System;

[RequireComponent(typeof(TwitchApiCallHelper))]
[RequireComponent(typeof(TwitchOAuth), typeof(TwitchAPI), typeof(TwitchPubSub))]
public class TwitchChatManager : MonoBehaviour
{

    public Client client;

    [HideInInspector]
    public string channel_name = "";
    [HideInInspector]
    public string channel_ID = "";
    [HideInInspector]
    public string storedAuthToken = "";
    [HideInInspector]
    public string storedRefreshToken = "";

    private bool hasKey = false;

    private TwitchOAuth twitchOAuth;
    private TwitchAPI twitchAPI;
    private TwitchPubSub twitchPubSub;


    private bool waitSuccess = false;
    private float waitTimeSuccess = 0.0f;
    private float waitTimeSuccessMax = 2.0f;


    private float connectingTimer = 0.0f;
    private float connectingTimerMax = 10.0f;


    // Project specific variable. Change flow auth depending on project.
    public MenuManager menuManager;

    public enum GameState
    {
        Starting,
        FirstTime,
        NoKey,
        GettingKey,
        GotKey,
        NotConnected,
        Connecting,
        Connected
    }

    [HideInInspector]
    public GameState gameState = GameState.Starting;


    void Start()
    {
        Application.runInBackground = true;
        twitchOAuth = GetComponent<TwitchOAuth>();
        twitchAPI = GetComponent<TwitchAPI>();
        twitchPubSub = GetComponent<TwitchPubSub>();


        hasKey = PlayerPrefs.HasKey("authtoken");
        if(hasKey == false)
        {
            gameState = GameState.FirstTime;
            menuManager.ShowStartMenu();
        } else
        {
            gameState = GameState.GotKey;
            LoadData();
            menuManager.CloseStartMenu();
            StartBot();

        }

    }

    public void InitAuth()
    {
        if(channel_name == "")
        {
            menuManager.noChannelName.SetActive(true);
            return;
        }
        menuManager.noChannelName.SetActive(false);
        menuManager.AuthState(1);
        gameState = GameState.GettingKey;
        twitchOAuth.InitiateTwitchAuth();
        
    }

    public void AuthSuccess()
    {
        menuManager.AuthState(2);
        SaveData();
        waitSuccess = true;
    }

    public void GotID()
    {
        SaveData();
    }


    public void StartBot()
    {
        channel_name = channel_name.ToLower();

        ConnectionCredentials credentials = new ConnectionCredentials("cyborgspicy", storedAuthToken);
        client = new Client();
        client.Initialize(credentials, channel_name);


        // Start Listening To Events
        client.OnMessageReceived += OnMessageReceived;
        client.OnNewSubscriber += OnNewSubscriber;
        client.OnRaidNotification += OnRaidNotification;
        
        
        // Connect
        client.Connect();
        gameState = GameState.Connecting;
    }

    private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        Debug.Log("Chat message: " + e.ChatMessage.Message);
    }

    private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        Debug.Log("Subscriber: " + e.Subscriber + " to " + e.Channel);
    }

    private void OnRaidNotification(object sender, OnRaidNotificationArgs e)
    {
        Debug.Log("Raided by: " + e.Channel);
    }

    public void SendBotMessage(string _message)
    {
        //TODO: CHECK IF CONNECTED ELSE RETURN
        client.SendMessage(client.JoinedChannels[0], _message);
    }


    void Update()
    {

        if(waitSuccess == true)
        {
            waitTimeSuccess += Time.deltaTime;
            if(waitTimeSuccess >= waitTimeSuccessMax)
            {
                menuManager.CloseStartMenu();
                waitSuccess = false;
                waitTimeSuccess = 0.0f;
                StartBot();
            }
        }

        if(gameState == GameState.GettingKey)
        {
            if(storedAuthToken != "")
            {
                AuthSuccess();
                gameState = GameState.GotKey;
            }
        }


        if(gameState == GameState.Connecting)
        {
            if(client.JoinedChannels.Count > 0)
            {
                Debug.Log("Is connected!");
                gameState = GameState.Connected;
                ClientConnectSuccess();

            } else
            {
                connectingTimer += Time.deltaTime;
                if(connectingTimer >= connectingTimerMax)
                {
                    // TODO: Show error can't connect
                    Debug.Log("Connecting for too long!");
                }
            }
        }


        // Debug inputs
        if (Input.GetKeyDown(KeyCode.End))
        {
            ResetData();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SendBotMessage("Test bot message");
        }
    }

    public void ClientConnectSuccess()
    {
        twitchAPI.StartAPI();
        twitchPubSub.ConnectToPubSub();
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("authtoken", storedAuthToken);
        PlayerPrefs.SetString("refreshtoken", storedRefreshToken);
        PlayerPrefs.SetString("channel_name", channel_name);
        PlayerPrefs.SetString("channel_id", channel_ID);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        storedAuthToken = PlayerPrefs.GetString("authtoken");
        storedRefreshToken = PlayerPrefs.GetString("refreshtoken");
        channel_name = PlayerPrefs.GetString("channel_name");
        channel_ID = PlayerPrefs.GetString("channel_id");
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public void SetChannelName(string _value)
    {
        channel_name = _value.ToLower();
    }
    
}
