using System.Linq;
using Alg;
using GameGUI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{
    enum State
    {
        Win,
        Fail,
        Game
    }
    public class EventWin
    {
    }

    public class EventFail
    {
    }

    public SimpleScoresDB ScoresDb;
    public MapController MapController;
    public SimpleGUI GUI;
    private State _curState;
    public bool AllowRestart { get; set; }


    protected override void Awake()
    {
        base.Awake();
        _curState = State.Game;
        MapController.Generate();

        // get random entity
        var entities = MapController.GetEntities();
        var rndIndex = Random.Range(0, entities.Length);
        var ent = entities[rndIndex];

        ent.Infect(-1);
    }

    void Update()
    {
        if (_curState == State.Fail && AllowRestart)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartProgression();
            }
        } 
        else if (_curState == State.Win)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextLevel();
            }
        }
        else if(_curState == State.Game)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExplodeAllInfected();
            }

            var ents = MapController.GetEntities();

            // check for win
            var isWin = ents.Length == 0;
            if (isWin)
            {
                _curState = State.Win;
                GlobalEventAggregator.EventAggregator.Publish(new EventWin());
                return;
            }

            // check for fail
            var hasInfected = ents.Any(x => x.IsInfected);
            var hasExploding = ents.Any(x => x.IsExpoding);
            var isFail = !hasInfected && !hasExploding;
            if (isFail)
            {
                _curState = State.Fail;
                DownloadHighScore();
                GUI.PushScreen("ScreenHighscore");
                GlobalEventAggregator.EventAggregator.Publish(new EventFail());
                return;
            }
        }
    }

    public void SendHighScore(string playerName, int score)
    {
        ScoresDb.UploadScore(playerName, score);
    }

    public void DownloadHighScore()
    {
        ScoresDb.DownloadScores();
    }

    public void RestartProgression()
    {
        SceneManager.LoadScene("Gameplay");
        Progression.Instance.ResetProgression();
    }

    public void NextLevel()
    {
        Progression.Instance.Progress();
        SceneManager.LoadScene("Gameplay");
    }

    private void ExplodeAllInfected()
    {
        var ents = MapController.GetEntities();
        foreach (var ent in ents)
        {
            if (ent.IsInfected)
                ent.Explode();
        }
    }
}

