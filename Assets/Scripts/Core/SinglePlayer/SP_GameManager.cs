using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SP_GameManager : MonoBehaviour {

    public int _playerModel { get; private set; }

    public List<GameObject> NetworkObjectsToActivate;

    private GameManager _gameManager;

    private bool _modelSet;
    private bool _lessonSet;
    private bool _gamePaused;

    public void SetModel(int model)
    {
        _playerModel = model;
        _modelSet = true;
    }

    public void ForceActive()
    {
        // activate objects which normally rely on a network
        foreach (var obj in NetworkObjectsToActivate)
        {
            obj.SetActive(true);
        }
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void CreatePlayers()
    {
        for (int i = 0; i < 3; i++)
        {
            _gameManager.CreatePlayer(i+1);
        }
    }

    public void SetLesson(string year, string lesson)
    {
        _gameManager.SetSpLesson("Year " + year, lesson);
        _lessonSet = true;
    }

    public void TogglePause()
    {
        _gamePaused = !_gamePaused;
    }

    public bool GameSetup()
    {
        return _modelSet && _lessonSet && !_gamePaused;
    }

    // Spawn players
    // Track time elapsed
    // Track goals reached
    // Track Player Model
}
