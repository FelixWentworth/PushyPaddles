using System;
using System.Collections;
using System.Collections.Generic;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;

public class CombinedLocalizations : MonoBehaviour
{

    public List<string> Keys;
    public bool SeparateKeys;

    void OnEnable()
    {
        var text = GetComponent<Text>();
        var str = "";
        for (var i = 0; i < Keys.Count; i++)
        {
            str += Localization.Get(Keys[i]);
            if (i + 1 < Keys.Count && SeparateKeys)
            {
                str += Environment.NewLine + Environment.NewLine;
            }
        }
        text.text = str;
    }
}
