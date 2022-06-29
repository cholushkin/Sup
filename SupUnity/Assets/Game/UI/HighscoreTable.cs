using UnityEngine;

public class HighscoreTable : MonoBehaviour
{
    public SlotController[] Slots;

    public void PlayLoadingAnimation()
    {
        foreach (var slotController in Slots)
        {
            slotController.PlayLoading();
        }
    }

    public void SetData(ScreenHighscore.ScoreTable tableData)
    {
        foreach (var slot in Slots)
        {
            slot.SetName("");
            slot.SetScore(0);
        }

        int i = 0;
        if (tableData != null)
            foreach (var row in tableData.Rows)
            {
                Slots[i].SetName(string.IsNullOrEmpty(row.Name) ? "No name" : row.Name);
                Slots[i].SetScore(row.Score);
                ++i;
            }
    }
}
