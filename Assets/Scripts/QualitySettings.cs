using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualitySettings : MonoBehaviour {

    [Serializable]
    public struct QualityDependentObjects
    {
        public GameObject GameObject;
        public int MinimumQualityLevel;
        public int MaximumQualityLevel;
    }

    // Populated through inspector
    [SerializeField] private List<QualityDependentObjects> _qualityDependentObjects = new List<QualityDependentObjects>();

    private bool _qualityChanged = false;

	// Update is called once per frame
    void Start()
    {
        SetObjectsForQuality(UnityEngine.QualitySettings.GetQualityLevel());
    }
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(0);
	        _qualityChanged = true;
	    }
	    if (Input.GetKeyDown(KeyCode.Alpha2))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(1);
	        _qualityChanged = true;
	    }
        if (Input.GetKeyDown(KeyCode.Alpha3))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(2);
	        _qualityChanged = true;
	    }
        if (Input.GetKeyDown(KeyCode.Alpha4))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(3);
	        _qualityChanged = true;
	    }
        if (Input.GetKeyDown(KeyCode.Alpha5))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(4);
	        _qualityChanged = true;
	    }
        if (Input.GetKeyDown(KeyCode.Alpha6))
	    {
	        UnityEngine.QualitySettings.SetQualityLevel(5);
	        _qualityChanged = true;
        }

	    if (_qualityChanged)
	    {
	        SetObjectsForQuality(UnityEngine.QualitySettings.GetQualityLevel());
	        _qualityChanged = false;
	    }
    }

    private void SetObjectsForQuality(int quality)
    {
        foreach (var obj in _qualityDependentObjects)
        {
            obj.GameObject.SetActive(quality >= obj.MinimumQualityLevel && quality < obj.MaximumQualityLevel);
        }
    }

    public void SetQuality(int quality)
    {
        UnityEngine.QualitySettings.SetQualityLevel(quality);
        _qualityChanged = true;
    }
}
