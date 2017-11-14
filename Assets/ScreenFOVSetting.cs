using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFOVSetting : MonoBehaviour
{

    public float LandscapeFOV;
    public float PortraitFOV;

    void Update()
    {
        if (Screen.width > Screen.height)
        {
            Camera.main.fieldOfView = LandscapeFOV;
        }
        else
        {
            Camera.main.fieldOfView = PortraitFOV;
        }
    }
}
