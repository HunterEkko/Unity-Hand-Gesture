using UnityEngine;
using TMPro;

public class ChangeText : MonoBehaviour
{
    public TMP_Text text;
    public void ShowText(string s)
    {
        text.text = s;
    }
}
