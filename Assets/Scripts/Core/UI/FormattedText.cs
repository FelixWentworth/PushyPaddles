using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormattedText : MonoBehaviour
{
    public Text TextToFormat;

    public void SetText(string formattedText, string[] arguments)
    {
        TextToFormat.text = string.Format(formattedText, arguments);
    }
}
