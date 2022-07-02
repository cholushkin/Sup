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


    private const int MaxHighscoreRows = 10;
    private string _userName;
    private PlayerProfileModel _playerProfile;
    private bool _isLoggedIn;

    public void Login()
    {
        if (_isLoggedIn)
        {
            if (LogChecker.Normal())
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
        if (LogChecker.Normal())
            Debug.Log("GetLeaderboard");
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
        if (LogChecker.Normal())
            Debug.Log("SendScore");
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
        if (LogChecker.Normal())
            Debug.Log("ChangeName");
        if (!_isLoggedIn)
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventChangeNameError());
            return;
        }
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = newName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnNameUpdateSuccess, OnErrorChangeName);
    }

    public string GetPlayerID()
    {
        return _playerProfile.PlayerId;
    }

    public bool IsNameEmpty()
    {
        return string.IsNullOrEmpty(_userName);
    }

    public static bool HasNewHighscore(List<PlayerLeaderboardEntry> responseLeaderboard, int score)
    {
        if (responseLeaderboard.Count == 0 && score > 0)
            return true;
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
        if (LogChecker.Normal())
            Debug.Log($"Login successfully. Newly created {result.NewlyCreated}");

        if (result.InfoResultPayload.PlayerProfile != null)
        {
            _userName = result.InfoResultPayload.PlayerProfile.DisplayName;
            _playerProfile = result.InfoResultPayload.PlayerProfile;
            GlobalEventAggregator.EventAggregator.Publish(new EventLoginSuccess());
        }

        _isLoggedIn = true;
    }

    private void OnLeaderboardGetSuccess(GetLeaderboardResult result)
    {
        if (LogChecker.Normal())
        {
            Debug.Log($"OnLeaderboardGet capacity|count {result.Leaderboard.Count}");
            Debug.Log("DisplayName|PlayFabId|Position|Profile|StatValue");
        }

        GlobalEventAggregator.EventAggregator.Publish(
            new EventGetLeaderboardSuccess { Leaderboard = result.Leaderboard }
        );

        if (LogChecker.Normal())
            foreach (var playerLeaderboardEntry in result.Leaderboard)
            {
                Debug.Log($"{playerLeaderboardEntry.DisplayName}|{playerLeaderboardEntry.PlayFabId}|{playerLeaderboardEntry.Position}|{playerLeaderboardEntry.Profile}|{playerLeaderboardEntry.StatValue}");
            }
    }

    private void OnNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
    {
        if (LogChecker.Normal())
            Debug.Log("OnNameUpdateSuccess");
        _userName = result.DisplayName;
        GlobalEventAggregator.EventAggregator.Publish(new EventRenameSuccess());
    }

    private void OnLeaderboardUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        if (LogChecker.Normal())
            Debug.Log("OnLeaderboardUpdateSuccess");
        GlobalEventAggregator.EventAggregator.Publish(new EventSendScoreSuccess());
        Debug.Log(result);
    }

    // Fail callbacks
    // ---------------------------------------------------
    private void PrintError(PlayFabError error)
    {
        if (LogChecker.Normal())
            Debug.LogError($"{error.GenerateErrorReport()}");
    }

    private void OnErrorLoginWithCustomID(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish(new EventLoginError());
    }

    private void OnErrorGetLeaderboard(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish(new EventGetLeaderboardError());
    }

    private void OnErrorSendScore(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish(new EventSendScoreError());
    }

    private void OnErrorChangeName(PlayFabError error)
    {
        PrintError(error);
        GlobalEventAggregator.EventAggregator.Publish(new EventChangeNameError());
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