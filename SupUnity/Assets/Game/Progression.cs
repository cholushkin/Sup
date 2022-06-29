using Alg;
using Assets.Plugins.Alg;
using UnityEngine;

public class Progression : Singleton<Progression>
{
    public int Level;
    public int Score { get; private set; }
    public int BestScore;
    public int FieldHeight = 50 / 3;
    public int FieldWidth = 90 / 3;

    private const float FieldWidthMax = 90f;
    private const float FieldHeightMax = 50f;
    private const float FieldScaleInc = 1.15f;
    public GameObject PrefabLevCube;

    private void BuildLevelVisualization()
    {
        var c = Instantiate(PrefabLevCube);
        c.transform.SetParent(transform);
        c.transform.localPosition = Vector3.right * Level;
    }

    public void Progress()
    {
        ++Level;
        FieldWidth = (int)(FieldWidth * FieldScaleInc);
        FieldHeight = (int)(FieldHeight * FieldScaleInc);

        FieldWidth = (int)Mathf.Min(FieldWidth, FieldWidthMax);
        FieldHeight = (int)Mathf.Min(FieldHeight, FieldHeightMax);
        BuildLevelVisualization();
    }

    public void ResetProgression()
    {
        Level = 0;
        Score = 0;
        FieldHeight = 50 / 3;
        FieldWidth = 90 / 3;
        transform.DestroyChildren();
        BuildLevelVisualization();
    }

    public void IncScore()
    {
        ++Score;
        if (Score > BestScore)
            BestScore = Score;
    }
}
