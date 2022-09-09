using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchLib.Unity;
using TwitchLib.PubSub.Events;
using System;

[RequireComponent(typeof(TwitchChatManager), typeof(TwitchAPI))]
public class TwitchPubSub : MonoBehaviour
{
    private PubSub pubSub;
    private TwitchChatManager twitchChatManager;
    private TwitchAPI twitchAPI;

    private bool waitForChannelID = false;
    private float updateTimer = 0.0f;
    private float updateTimeMax = 2.0f;


    private void Start()
    {
        twitchChatManager = GetComponent<TwitchChatManager>();
        twitchAPI = GetComponent<TwitchAPI>();
    }

    public void ConnectToPubSub()
    {
        // Some PubSub events require channel id. Fetch before starting pubsub
        if(twitchChatManager.channel_ID == "")
        {
            waitForChannelID = true;
            twitchAPI.GetChannelID();
            return;
        }
        pubSub = new PubSub();


        // Events
        pubSub.OnWhisper += OnWhisper;
        pubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
        pubSub.OnBitsReceivedV2 += OnBitsReceived;
        pubSub.OnChannelPointsRewardRedeemed += OnChannelPointsRewardRedeemed;
        pubSub.OnChannelSubscription += OnChannelSubscription;
        pubSub.OnFollow += OnFollow;
        pubSub.OnRaidUpdateV2 += OnRaidUpdate;
        pubSub.OnRaidGo += OnRaidGo;

        pubSub.Connect();
    }

    private void Update()
    {
        if (waitForChannelID == true)
        {
            WaitForChannelIDFetch();
        }
    }

    private void WaitForChannelIDFetch()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateTimeMax)
        {
            updateTimer = 0.0f;
            if (twitchChatManager.channel_ID != "")
            {
                waitForChannelID = false;
                ConnectToPubSub();
            }
        }
    }

    #region PubSubEvents
    private void OnWhisper(object sender, OnWhisperArgs e)
    {
        Debug.Log("Whisper: " + e.Whisper.Data);
    }

    private void OnPubSubServiceConnected(object sender, EventArgs e)
    {
        pubSub.ListenToWhispers(twitchChatManager.channel_ID);
        pubSub.SendTopics(twitchChatManager.storedAuthToken);
        Debug.Log("PubSub Connected!");
    }

    private void OnBitsReceived(object sender, OnBitsReceivedV2Args e)
    {
        BitsEvent(e.BitsUsed, e.UserName, e.ChatMessage);
    }

    private void BitsEvent(int _bitsUsed, string _userName, string _chatMessage)
    {

    }
    private void OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
    {
        if (e.RewardRedeemed.Redemption.Reward.IsUserInputRequired == true)
        {
            ChannelPointEvent(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.User.DisplayName, e.RewardRedeemed.Redemption.UserInput);
        }
        else
        {
            ChannelPointEvent(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.User.DisplayName, "");
        }

    }

    private void ChannelPointEvent(string _title, string _username, string _userInput)
    {
        if (_userInput == "")
        {

            return;
        }
    }


    private void OnChannelSubscription(object sender, OnChannelSubscriptionArgs e)
    {
        SubscriptionEvent(e.Subscription.Username, e.Subscription.SubMessage.Message);
    }

    private void SubscriptionEvent(string _username, string _userInput)
    {

    }

    private void OnFollow(object sender, OnFollowArgs e)
    {
        FollowEvent(e.DisplayName);
    }

    private void FollowEvent(string _username)
    {

    }

    private void OnRaidUpdate(object sender, OnRaidUpdateV2Args e)
    {
        RaidUpdateEvent(e.TargetDisplayName, e.ViewerCount);
    }

    private void RaidUpdateEvent(string _targetUser, int _viewerCount)
    {

    }

    private void OnRaidGo(object sender, OnRaidGoArgs e)
    {
        RaidGoEvent(e.TargetDisplayName, e.ViewerCount);
    }

    private void RaidGoEvent(string _targetUser, int _viewerCount)
    {

    }
    #endregion


}
