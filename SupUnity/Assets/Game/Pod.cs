using UnityEngine;

public class Pod : MonoBehaviour
{
    public int OwnerId;

    void Awake()
    {
        OwnerId = transform.parent.gameObject.GetInstanceID();
    }
}
