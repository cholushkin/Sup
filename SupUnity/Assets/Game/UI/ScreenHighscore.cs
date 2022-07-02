using System.Collections.Generic;
using Events;
using GameGUI;
using GameLib.Log;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;


public class ScreenHighscore :
    GUIScreenBase,
    SimpleGUI.IInitialize,
    IHandle<PlayFabManager.EventGetLeaderboardSuccess>,
    IHandle<PlayFabManager.EventGetLeaderboardError>,
    IHandle<PlayFabManager.EventRenameSuccess>,
    IHandle<PlayFabManager.EventChangeNameError>,
    IHandle<PlayFabManager.EventSendScoreError>,
    IHandle<PlayFabManager.EventSendScoreSuccess>
{
    enum State
    {
        None,
        FirstDownloadTable,
        SendName,
        SendScore,
        SecondDownloadTable,
        WaitForRestart
    }

    public class ScoreTable
    {
        public class TableRow
        {
            public TableRow()
            {
                Name = "";
                Score = 0;
            }
            public string Name;
            public int Score;
        }

        public List<TableRow> Rows = new();
    }

    public TextMeshProUGUI YourScoreText;
    public TextMeshProUGUI NewHighScoreText;
    public TextMeshProUGUI TextError;
    public TextMeshProUGUI TextSpaceToConinue;
    public HighscoreTable HighscoreTable;
    public TMP_InputField InputField;
    public ScoreTable Scores;
    public LogChecker Log;

    private State _state;

    public void Initialize()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    void Update()
    {
        if (_state == State.WaitForRestart && Input.GetKeyDown(KeyCode.Space))
        {
            _state = State.None;
            GameController.Instance.RestartProgression();
        }
    }

    public override void StartAppearAnimation()
    {
        if (Log.Normal())
            Debug.Log("StartAppearAnimation: Show table loading");

        base.StartAppearAnimation();
        _state = State.FirstDownloadTable;

        NewHighScoreText.gameObject.SetActive(false);
        InputField.gameObject.SetActive(false);
        TextSpaceToConinue.gameObject.SetActive(false);

        ShowLoadingProgress();
        ShowYourScoreText();
        PlayFabManager.Instance.GetLeaderboard();
    }

    public void OnEnter()
    {
        if (Log.Normal())
            Debug.Log("OnEnter: Enter name");
        _state = State.SendName;
        PlayFabManager.Instance.ChangeName(InputField.text);
        HighscoreTable.PlayLoadingAnimation();
        InputField.gameObject.SetActive(false);
    }

    public void Handle(PlayFabManager.EventGetLeaderboardSuccess message)
    {
        if (Log.Normal())
            Debug.Log($"On EventGetLeaderboardSuccess {_state}");
        if (_state == State.FirstDownloadTable)
        {
            ShowTable(message.Leaderboard);
            if (PlayFabManager.HasNewHighscore(message.Leaderboard, Progression.Instance.Score))
            {
                if (Log.Normal())
                    Debug.Log("new high score");
                ShowHasNewHighscore();
                if (PlayFabManager.Instance.IsNameEmpty())
                {
                    // enter name state
                    ShowInputField();
                }
                else
                {
                    // send score
                    _state = State.SendScore;
                    ShowLoadingProgress();
                    if (Log.Normal())
                        Debug.Log("send score");
                    PlayFabManager.Instance.SendScore(Progression.Instance.Score);
                }
            }
            else
            {
                if (Log.Normal())
                    Debug.Log("no new high score");
                ShowPressSpaceToConinue();
                _state = State.WaitForRestart;
            }
        }
        else if (_state == State.SecondDownloadTable)
        {
            Debug.Log($"user index in table: {GetIndexOfUserInTable(message.Leaderboard, PlayFabManager.Instance.GetPlayerID())}");
            ShowTable(message.Leaderboard);
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
    }

    public void Handle(PlayFabManager.EventGetLeaderboardError message)
    {
        if (Log.Important())
            Debug.Log($"Handle EventGetLeaderboardError {_state}");
        if (_state == State.FirstDownloadTable)
        {
            HighscoreTable.SetData(null);
            ShowError("Can't load leader board.");
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
        else if (_state == State.SecondDownloadTable)
        {
            HighscoreTable.SetData(null);
            ShowError("Can't load leader board.");
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
    }

    public void Handle(PlayFabManager.EventRenameSuccess message)
    {
        if (Log.Normal())
            Debug.Log("Handle EventRenameSuccess");
        if (_state == State.SendName)
        {
            _state = State.SendScore;
            ShowLoadingProgress();
            PlayFabManager.Instance.SendScore(Progression.Instance.Score);
        }
    }

    public void Handle(PlayFabManager.EventChangeNameError message)
    {
        if (Log.Important())
            Debug.Log("Handle EventChangeNameError");
        if (_state == State.SendName)
        {
            ShowError("Can't change name.");
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
    }

    public void Handle(PlayFabManager.EventSendScoreError message)
    {
        if (Log.Important())
            Debug.Log("Handle EventSendScoreError");
        _state = State.WaitForRestart;
        ShowError("Can't send score.");
        ShowPressSpaceToConinue();
    }

    public void Handle(PlayFabManager.EventSendScoreSuccess message)
    {
        if (Log.Normal())
            Debug.Log("Handle EventSendScoreSuccess");
        _state = State.SecondDownloadTable;
        PlayFabManager.Instance.GetLeaderboard();
    }

    public void ShowLoadingProgress()
    {
        HighscoreTable.PlayLoadingAnimation();
    }

    private void ShowTable(List<PlayerLeaderboardEntry> leaderboard)
    {
        var tableData = ConvertResponseToTable(leaderboard);
        Assert.IsNotNull(tableData);
        HighscoreTable.SetData(tableData);
    }

    public void ShowYourScoreText()
    {
        YourScoreText.text = $"Your score is <color=#ff0000>{Progression.Instance.Score}<color=#005500>";
    }
    private void ShowPressSpaceToConinue()
    {
        TextSpaceToConinue.gameObject.SetActive(true);
    }
    public void ShowHasNewHighscore()
    {
        NewHighScoreText.gameObject.SetActive(true);
    }
    public void ShowInputField()
    {
        InputField.gameObject.SetActive(true);
        InputField.Select();
        InputField.ActivateInputField();
    }

    private void ShowError(string errorText)
    {
        TextError.text = errorText;
    }

    public static ScoreTable ConvertResponseToTable(List<PlayerLeaderboardEntry> responseLeaderboard)
    {
        var scoreTable = new ScoreTable();
        foreach (var entry in responseLeaderboard)
        {
            var row = new ScoreTable.TableRow();
            row.Score = entry.StatValue;
            row.Name = entry.DisplayName;
            scoreTable.Rows.Add(row);
        }
        return scoreTable;
    }

    public static int GetIndexOfUserInTable(List<PlayerLeaderboardEntry> responseLeaderboard, string playerId)
    {
        int index = 0;
        foreach (var entry in responseLeaderboard)
        {
            if (entry.Profile.PlayerId == playerId)
                return index;
            index++;
        }
        return -1;
    }
}

