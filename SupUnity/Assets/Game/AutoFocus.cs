using TMPro;
using UnityEngine;

public class AutoFocus : MonoBehaviour
{
    void Update()
    {
        var f = GetComponent<TMP_InputField>();
        f.Select();
        f.ActivateInputField();
    }
}
