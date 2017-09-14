using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualitySettings : MonoBehaviour {

	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(0);
	    }
	    if (Input.GetKeyDown(KeyCode.Alpha2))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(1);
	    }
	    if (Input.GetKeyDown(KeyCode.Alpha3))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(2);
	    }
	    if (Input.GetKeyDown(KeyCode.Alpha4))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(3);
	    }
	    if (Input.GetKeyDown(KeyCode.Alpha5))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(4);
	    }
	    if (Input.GetKeyDown(KeyCode.Alpha6))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(5);
	    }
    }
}
