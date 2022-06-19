using DG.Tweening;
using Events;
using GameGUI;
using TMPro;
using UnityEngine;

public class ScreenGame : GUIScreenBase, SimpleGUI.IInitialize,
    IHandle<GameController.EventFail>,
    IHandle<GameController.EventWin>,
    IHandle<EntTamplier.EventScore>
{
    public Transform StartMessage;
    public Transform HintMessage;
    public TextMeshProUGUI TextLevel;
    public TextMeshProUGUI TextScore;

    public TextMeshProUGUI TextGameScore;

    public Transform PressAnyKeyToRestart;
    public Transform PressAnyKeyToNextLevel;

    public void Initialize()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    public void Handle(GameController.EventFail message)
    {
        ShowPressAnyKeyToRestart();
    }

    public void Handle(EntTamplier.EventScore message)
    {
        TextGameScore.text = Progression.Instance.Score.ToString();
    }

    public void Handle(GameController.EventWin message)
    {
        ShowPressAnyKeyForNextLevel();
    }


    public override void StartAppearAnimation()
    {
        base.StartAppearAnimation();
        TextGameScore.text = Progression.Instance.Score.ToString();
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

    private void ShowPressAnyKeyToRestart()
    {
        PressAnyKeyToRestart.gameObject.SetActive(true);
    }
}
