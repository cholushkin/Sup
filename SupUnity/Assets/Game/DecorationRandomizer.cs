using UnityEngine;

public class DecorationRandomizer : MonoBehaviour
{
    public Transform[] chunks;
    
    void Awake()
    {
        foreach (var chunk in chunks)
        {
            chunk.gameObject.SetActive(Random.value > 0.3f);
        }
    }
}
