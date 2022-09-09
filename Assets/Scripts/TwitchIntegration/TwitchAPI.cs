using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchLib.Unity;
using TwitchLib.Api.Core.Models.Undocumented.Chatters;
using System;
using TwitchLib.Api.V5.Models.Users;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Helix.Models.HypeTrain;

[RequireComponent(typeof(TwitchChatManager))]
public class TwitchAPI : MonoBehaviour
{

    private TwitchChatManager twitchChatManager;

    public Api api;

    void Start()
    {
        twitchChatManager = GetComponent<TwitchChatManager>();
        Application.runInBackground = true;

    }

    public void StartAPI()
    {
        api = new Api();
        api.Settings.AccessToken = twitchChatManager.storedAuthToken;
        api.Settings.ClientId = Secrets.client_id;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            api.Invoke(api.Undocumented.GetChattersAsync(twitchChatManager.client.JoinedChannels[0].Channel), GetChatterListCallback);

            if(!(twitchChatManager.channel_ID == "") || !(twitchChatManager.channel_ID == null))
            {
                // TODO: Learn how to implement hype train features
                //api.Invoke(api.Helix.HypeTrain.GetHypeTrainEventsAsync(twitchChatManager.channel_ID), GetHypeTrainEvents);
            }
            
        }
    }

    // TODO: Learn how to implement hype train features
    /*
    private void GetHypeTrainEvents(GetHypeTrainResponse obj)
    {
        //obj.HypeTrain[0].EventData.Level
    }
    */

    public void GetChannelID()
    {

        List<string> myList = new List<string>();
        myList.Add(twitchChatManager.channel_name);
        api.Invoke(api.Helix.Users.GetUsersAsync(logins: new List<string> { twitchChatManager.channel_name }), GetChannelIDCallbackNew);
        
    }

    private void GetChannelIDCallbackNew(GetUsersResponse obj)
    {
        if(obj.Users.Length == 0) 
        {
            // TODO: IMPLEMENT USER NOT FOUND
            return;
        }
        twitchChatManager.channel_ID = obj.Users[0].Id;
        twitchChatManager.GotID();
    }


    private void GetChatterListCallback(List<ChatterFormatted> listOfChatters)
    {
        // TODO: Implement userID database with data containing usernames
        Debug.Log("Viewers: ");
        for(int i = 0; i < listOfChatters.Count; i++)
        {
            Debug.Log(listOfChatters[i].Username);
        }
        
    }



}
