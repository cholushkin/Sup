using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    public TextMeshProUGUI TextPlayerName;
    public TextMeshProUGUI TextPlayerScores;
    public Image ImageLoadingIcon1;
    public Image ImageLoadingIcon2;

    public void PlayLoading()
    {
        TextPlayerName.gameObject.SetActive(false);
        TextPlayerScores.gameObject.SetActive(false);
        ImageLoadingIcon1.gameObject.SetActive(true);
        ImageLoadingIcon2.gameObject.SetActive(true);
        ImageLoadingIcon1.DOFade(1f, 0.5f);
        ImageLoadingIcon2.DOFade(1f, 0.5f);
    }

    public void SetScore(int score)
    {
        ImageLoadingIcon2.DOFade(0f, 1f);
        TextPlayerScores.gameObject.SetActive(true);
        TextPlayerScores.text = score > 0 ? score.ToString() : "";
    }

    public void SetName(string name)
    {
        ImageLoadingIcon1.DOFade(0f, 1f);
        TextPlayerName.gameObject.SetActive(true);
        TextPlayerName.text = name;
    }
}
