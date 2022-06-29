using System;
using System.Collections.Generic;
using Alg;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : Singleton<PlayFabManager>
{
    public abstract class EventGotResponse
    {
        public string Text;
    }

    public class EventGetLeaderboardSuccess : EventGotResponse
    {
        public List<PlayerLeaderboardEntry> Leaderboard;
    }

    public class EventLoginSuccess : EventGotResponse
    {

    }

    public class EventSendScoreSuccess : EventGotResponse
    {
    }

    public class EventRenameSuccess : EventGotResponse
    {
    }



    public abstract class EventResponseError
    {
        public PlayFabError Error;
    }

    public class EventLoginError : EventResponseError
    {
    }

    public class EventGetLeaderboardError : EventResponseError
    {
    }

    public class EventSendScoreError : EventResponseError
    {
    }

    public class EventChangeNameError : EventResponseError
    {
    }


    public bool AutoLogin;
    private const int MaxHighscoreRows = 10;
    private string _userName;
    private bool _isLoggedIn;

    protected override void Awake()
    {
        base.Awake();
        Login();
    }

    void Login()
    {
        if (_isLoggedIn)
        {
            Debug.Log("Attempt to call Login while _isLoggedIn==true");
            return;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = BuildIdentityString(),
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnErrorLoginWithCustomID);
    }



    public void GetLeaderboard()
    {
        if (!_isLoggedIn)
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventGetLeaderboardError());
            return;
        }
        var request = new GetLeaderboardRequest
        {
            StatisticName = "Highscore",
            StartPosition = 0,
            MaxResultsCount = MaxHighscoreRows
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGetSuccess, OnErrorGetLeaderboard);
    }



    public void SendScore(int score)
    {
        if (!_isLoggedIn)
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventSendScoreError());
            return;
        }
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>()
            {
                new StatisticUpdate
                {
                    StatisticName = "Highscore",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdateSuccess, OnErrorSendScore);
    }

    public void ChangeName(string newName)
    {
        if (!_isLoggedIn)
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventChangeNameError());
            return;
        }
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = newName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnNameUpdateSuccess, OnErrorChangeName);
    }

    public bool IsNameEmpty()
    {
        return string.IsNullOrEmpty(_userName);
    }

    public static bool HasNewHighscore(List<PlayerLeaderboardEntry> responseLeaderboard, int score)
    {
        foreach (var lbDataRow in responseLeaderboard)
            if (score > lbDataRow.StatValue)
                return true;
        return false;
    }

    private static string BuildIdentityString()
    {
        return $"{SystemInfo.deviceUniqueIdentifier}";
    }

    #region callbacks

    // Success callbacks
    // ---------------------------------------------------
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"Login successfully. Newly created {result.NewlyCreated}");

        if (result.InfoResultPayload.PlayerProfile != null)
        {
            _userName = result.InfoResultPayload.PlayerProfile.DisplayName;
            GlobalEventAggregator.EventAggregator.Publish(new EventLoginSuccess());
            Debug.Log($"displayName = {_userName}");
        }

        _isLoggedIn = true;
    }

    private void OnLeaderboardGetSuccess(GetLeaderboardResult result)
    {
        Debug.Log($"OnLeaderboardGet capacity|count {result.Leaderboard.Count}|{result.Leaderboard.Capacity}");
        Debug.Log("DisplayName|PlayFabId|Position|Profile|StatValue");

        GlobalEventAggregator.EventAggregator.Publish(
            new EventGetLeaderboardSuccess{ Leaderboard = result.Leaderboard }
        );

        foreach (var playerLeaderboardEntry in result.Leaderboard)
        {
            Debug.Log($"{playerLeaderboardEntry.DisplayName}|{playerLeaderboardEntry.PlayFabId}|{playerLeaderboardEntry.Position}|{playerLeaderboardEntry.Profile}|{playerLeaderboardEntry.StatValue}");
        }
    }

    private void OnNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Name changed");
        _userName = result.DisplayName;
        GlobalEventAggregator.EventAggregator.Publish(new EventRenameSuccess());
    }

    private void OnLeaderboardUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        GlobalEventAggregator.EventAggregator.Publish(new EventSendScoreSuccess());
        Debug.Log(result);
    }

    // Fail callbacks
    // ---------------------------------------------------
    private void PrintError(PlayFabError error)
    {
        Debug.LogError($"{error.GenerateErrorReport()}");
    }

    private void OnErrorLoginWithCustomID(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish(new EventLoginError());
        if (AutoLogin)
            Login();
    }

    private void OnErrorGetLeaderboard(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish( new EventGetLeaderboardError() );
    }

    private void OnErrorSendScore(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish( new EventSendScoreError() );
    }

    private void OnErrorChangeName(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish( new EventChangeNameError() );
    }
    #endregion


    [ContextMenu("Send score 10")]
    void DbgSendScore10()
    {
        SendScore(10);
    }

    [ContextMenu("Send score 20")]
    void DbgSendScore20()
    {
        SendScore(20);
    }
}
