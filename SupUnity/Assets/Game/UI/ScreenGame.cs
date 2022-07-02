using DG.Tweening;
using Events;
using GameGUI;
using TMPro;
using UnityEngine;

public class ScreenGame : GUIScreenBase, SimpleGUI.IInitialize,
    IHandle<GameController.EventWin>,
    IHandle<Progression.EventScore>
{
    public Transform StartMessage;
    public Transform HintMessage;
    public TextMeshProUGUI TextLevel;
    public TextMeshProUGUI TextScore;

    public TextMeshProUGUI TextGameScore;
    public TextMeshProUGUI TextBestScore;

    public Transform PressAnyKeyToNextLevel;

    public void Initialize()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }
    public void Handle(Progression.EventScore message)
    {
        TextGameScore.text = $"Score: {Progression.Instance.Score}";
        TextBestScore.text = $"Your best score: {Progression.Instance.BestScore}";
    }

    public void Handle(GameController.EventWin message)
    {
        ShowPressAnyKeyForNextLevel();
    }

    public override void StartAppearAnimation()
    {
        base.StartAppearAnimation();
        ShowStartMessage();
        if (Progression.Instance.Level == 0)
            ShowControlHintMessage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HintMessage.gameObject.SetActive(false);
        }
    }

    private void ShowControlHintMessage()
    {
        HintMessage.gameObject.SetActive(true);
    }

    private void ShowStartMessage()
    {
        StartMessage.gameObject.SetActive(true);
        TextLevel.text = $"Level {Progression.Instance.Level + 1}";
        TextScore.text = $"Score {Progression.Instance.Score}";
        DOVirtual.DelayedCall(2f, () => StartMessage.gameObject.SetActive(false));
    }

    private void ShowPressAnyKeyForNextLevel()
    {
        PressAnyKeyToNextLevel.gameObject.SetActive(true);
    }
}
