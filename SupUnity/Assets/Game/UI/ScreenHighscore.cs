using Events;
using GameGUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class ScreenHighscore : GUIScreenBase, SimpleGUI.IInitialize, IHandle<SimpleScoresDB.EventResponseReceived>
{
    enum State
    {
        FirstDownload,
        Send,
        SecondDownload
    }
    public TextMeshProUGUI YourScoreText;
    public TextMeshProUGUI NewHighScoreText;
    public TextMeshProUGUI TextSpaceToConinue;
    public HighscoreTable HighscoreTable;
    public TMP_InputField InputField;

    private State _state;

    public void Initialize()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    public override void StartAppearAnimation()
    {
        _state = State.FirstDownload;
        base.StartAppearAnimation();
        HighscoreTable.PlayLoadingAnimation();
        NewHighScoreText.gameObject.SetActive(false);
        InputField.gameObject.SetActive(false);
        TextSpaceToConinue.gameObject.SetActive(false);
        ShowYourScoreText();
    }

    public void ShowYourScoreText()
    {
        YourScoreText.text = $"Your score is <color=#ff0000>{Progression.Instance.Score}<color=#005500>";
    }

    public void OnEnter()
    {
        _state = State.Send;
        GameController.Instance.SendHighScore(InputField.text, Progression.Instance.Score);
        HighscoreTable.PlayLoadingAnimation();
        InputField.gameObject.SetActive(false);
        TextSpaceToConinue.gameObject.SetActive(false);
        NewHighScoreText.gameObject.SetActive(false);
    }

    public void Handle(SimpleScoresDB.EventResponseReceived message)
    {
        if (message.Response.State == SimpleScoresDB.State.DownloadScore)
        {
            if (message.Response.RequestResult == UnityWebRequest.Result.ConnectionError)
            {
                GameController.Instance.AllowRestart = true;
                TextSpaceToConinue.gameObject.SetActive(true);
                return;
            }
            var tableData = SimpleScoresDB.ConvertResponseToTable(message.Response);
            Assert.IsNotNull(tableData);

            HighscoreTable.SetData(tableData);

            if (_state == State.FirstDownload)
            {
                // has new highscore?
                var isNewHighscore = false;

                if (Progression.Instance.Score > 0)
                    foreach (var tableDataRow in tableData.Rows)
                    {
                        if (tableDataRow.Score <= Progression.Instance.Score)
                        {
                            isNewHighscore = true;
                            break;
                        }
                    }

                if (isNewHighscore)
                {
                    InputField.gameObject.SetActive(true);
                    InputField.Select();
                    InputField.ActivateInputField();
                    NewHighScoreText.gameObject.SetActive(true);
                    NewHighScoreText.text = "You have a new High Score! Please enter your name:";
                }
                else
                {
                    GameController.Instance.AllowRestart = true;
                    TextSpaceToConinue.gameObject.SetActive(true);
                }
            }
            else // second download
            {
                GameController.Instance.AllowRestart = true;
                TextSpaceToConinue.gameObject.SetActive(true);
            }

            
        }
        else if (message.Response.State == SimpleScoresDB.State.UploadScore)
        {
            Debug.Log($"response on sent scores:{message.Response.Text}");
            _state = State.SecondDownload;
            GameController.Instance.DownloadHighScore();
        }
    }

}

