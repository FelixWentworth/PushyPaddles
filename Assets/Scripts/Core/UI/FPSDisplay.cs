using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSDisplay : MonoBehaviour {

    void OnGUI()
    {
        GUI.Label(new Rect(20, 0, 100, 20), Mathf.RoundToInt(1 / Time.deltaTime).ToString());
    }
}
