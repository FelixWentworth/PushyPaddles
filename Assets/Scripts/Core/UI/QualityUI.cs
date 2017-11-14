using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> _buttonSelectedGameObjects;
    private GameObject _optionsPanel;

	// Use this for initialization
	void Start ()
	{
	    _optionsPanel = transform.Find("Options").gameObject;
	    _optionsPanel.SetActive(false);
	}

    public void ToggleMenu()
    {
        _optionsPanel.SetActive(!_optionsPanel.activeSelf);
    }
	
	// Update is called once per frame
	void Update ()
	{
	    if (_optionsPanel.activeSelf)
	    {
	        var quality = UnityEngine.QualitySettings.GetQualityLevel();
	        for (var i = 0; i < _buttonSelectedGameObjects.Count; i++)
	        {
	            _buttonSelectedGameObjects[i].SetActive(i == quality);
	        }
	    }
	}
}
