using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public class EventWin
    {
    }

    public class EventFail
    {
    }


    public MapController MapController;
    private bool _isFail;
    private bool _isWin;

    void Awake()
    {
        MapController.Generate();

        // get random entity
        var entities = MapController.GetEntities();
        var rndIndex = Random.Range(0, entities.Length);
        var ent = entities[rndIndex];

        ent.Infect(-1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isFail)
                RestartProgression();
            else if (_isWin)
                NextLevel();
            else
                ExplodeAllInfected();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartProgression();
            return;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            NextLevel();
            return;
        }


        if (_isFail || _isWin)
            return;

        var ents = MapController.GetEntities();

        // check for win
        var isWin = ents.Length == 0;
        if (isWin)
        {
            _isWin = true;
            GlobalEventAggregator.EventAggregator.Publish(new EventWin());
            return;
        }

        // check for fail
        var hasInfected = ents.Any(x => x.IsInfected);
        var hasExploding = ents.Any(x => x.IsExpoding);
        var isFail = !hasInfected && !hasExploding;
        if (isFail)
        {
            _isFail = true;
            GlobalEventAggregator.EventAggregator.Publish(new EventFail());
            return;
        }
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

