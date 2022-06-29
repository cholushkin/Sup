using System.Collections.Generic;
using Events;
using GameGUI;
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
    public TextMeshProUGUI TextSpaceToConinue;
    public HighscoreTable HighscoreTable;
    public TMP_InputField InputField;
    public ScoreTable Scores;

    private State _state;

    public void Initialize()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    void Update()
    {
        if (_state==State.WaitForRestart && Input.GetKeyDown(KeyCode.Space))
        {
            _state = State.None;
            GameController.Instance.RestartProgression();
        }
    }

    public override void StartAppearAnimation()
    {
        _state = State.FirstDownloadTable;
        ShowLoadingProgress();
        ShowYourScoreText();
        PlayFabManager.Instance.GetLeaderboard();
    }

    public void OnEnter()
    {
        _state = State.SendName;
        PlayFabManager.Instance.ChangeName(InputField.text);
        HighscoreTable.PlayLoadingAnimation();
        InputField.gameObject.SetActive(false);
    }

    public void Handle(PlayFabManager.EventGetLeaderboardSuccess message)
    {
        if (_state == State.FirstDownloadTable)
        {
            ShowTable(message.Leaderboard);
            if (PlayFabManager.HasNewHighscore(message.Leaderboard, Progression.Instance.Score))
            {
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
                    PlayFabManager.Instance.SendScore(Progression.Instance.Score);
                }
            }
            else
            {
                ShowPressSpaceToConinue();
                _state = State.WaitForRestart;
            }
        }
        else if (_state == State.SecondDownloadTable)
        {
            ShowTable(message.Leaderboard);
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
    }

    public void Handle(PlayFabManager.EventGetLeaderboardError message)
    {
        Debug.Log("Handle EventGetLeaderboardError");
        if (_state == State.FirstDownloadTable)
        {
            HighscoreTable.SetData(null);
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
        else if (_state == State.SecondDownloadTable)
        {
            HighscoreTable.SetData(null);
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
    }

    public void Handle(PlayFabManager.EventRenameSuccess message)
    {
        if (_state == State.SendName)
        {
            _state = State.SendScore;
            PlayFabManager.Instance.SendScore(Progression.Instance.Score);
        }
    }

    public void Handle(PlayFabManager.EventChangeNameError message)
    {
        Debug.Log("Handle EventChangeNameError");
        if (_state == State.SendName)
        {
            ShowPressSpaceToConinue();
            _state = State.WaitForRestart;
        }
    }
    
    public void Handle(PlayFabManager.EventSendScoreError message)
    {
        Debug.Log("Handle EventSendScoreError");
        _state = State.WaitForRestart;
        ShowPressSpaceToConinue();
    }

    public void Handle(PlayFabManager.EventSendScoreSuccess message)
    {
        _state = State.SecondDownloadTable;
        PlayFabManager.Instance.GetLeaderboard();
    }

    public void ShowLoadingProgress()
    {
        base.StartAppearAnimation();
        HighscoreTable.PlayLoadingAnimation();
        NewHighScoreText.gameObject.SetActive(false);
        InputField.gameObject.SetActive(false);
        TextSpaceToConinue.gameObject.SetActive(false);
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
}

