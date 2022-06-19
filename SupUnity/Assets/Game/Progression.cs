using Alg;
using UnityEngine;

public class Progression : Singleton<Progression>
{
    public int Level;
    public int Score;
    public int FieldHeight = 50 / 3;
    public int FieldWidth = 90 / 3;

    private const float FieldWidthMax = 90f;
    private const float FieldHeightMax = 50f;
    private const float FieldScaleInc = 1.15f;

    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
        DontDestroyOnLoad(transform.gameObject);
    }

    public void Progress()
    {
        ++Level;
        FieldWidth = (int)(FieldWidth * FieldScaleInc);
        FieldHeight = (int)(FieldHeight * FieldScaleInc);

        FieldWidth = (int)Mathf.Min(FieldWidth, FieldWidthMax);
        FieldHeight = (int)Mathf.Min(FieldHeight, FieldHeightMax);
    }

    public void ResetProgression()
    {
        Level = 0;
        Score = 0;
        FieldHeight = 50 / 3;
        FieldWidth = 90 / 3;
    }
}
