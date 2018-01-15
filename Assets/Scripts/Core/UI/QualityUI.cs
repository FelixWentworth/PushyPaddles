using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> _buttonSelectedGameObjects;
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _inGameBackground;
    [SerializeField] private GameObject _quitGameObject;

    // Use this for initialization
    void Start ()
	{
	    _inGameBackground.SetActive(false);
	    _quitGameObject.SetActive(false);
	    _optionsPanel.SetActive(false);
	}

    public void ToggleMenu()
    {
	    var inGame = (SP_Manager.Instance.IsSinglePlayer() && SP_Manager.Instance.Get<SP_GameManager>().GameSetup()) ||
	                 (!SP_Manager.Instance.IsSinglePlayer() && GameObject.Find("GameManager").GetComponent<GameManager>()
		                  .GetLocalPlayer().IsReady);

        _optionsPanel.SetActive(!_optionsPanel.activeSelf);
        _inGameBackground.SetActive(inGame);
        _quitGameObject.SetActive(SP_Manager.Instance.Get<SP_GameManager>().GameSetup());
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
