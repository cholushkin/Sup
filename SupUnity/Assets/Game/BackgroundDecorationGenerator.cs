using UnityEngine;

public class BackgroundDecorationGenerator : MonoBehaviour
{
    public GameObject PrefabDecorationDot;

    public void Generate(Rect bounds)
    {
        // create dots
        for (int i = 0; i < Progression.Instance.FieldWidth / 2; ++i)
            CreateDot(new Vector3(Random.Range(bounds.left, bounds.right), Random.Range(bounds.top, bounds.bottom), 4), transform);
    }

    private void CreateDot(Vector3 pos, Transform parent)
    {
        var go = Instantiate(PrefabDecorationDot, pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
        go.transform.parent = parent;
    }
}
