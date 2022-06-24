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

    public void SetData(SimpleScoresDB.ScoreTable tableData)
    {
        for (int i = 0; i < Slots.Length; ++i)
        {
            if (i < tableData.Rows.Length)
            {
                Slots[i].SetName(tableData.Rows[i].Name);
                Slots[i].SetScore(tableData.Rows[i].Score);
            }
            else
            {
                Slots[i].SetName("");
                Slots[i].SetScore(0);
            }
        }
    }
}
