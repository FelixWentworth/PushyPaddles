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

    private string _year;
    private string _lesson;

    public void SetModel(int model)
    {
        _playerModel = model;
        _modelSet = true;
    }

    public int GetModelNumber()
    {
        return _playerModel;
    }

    public void ForceActive()
    {
        // activate objects which normally rely on a network
        foreach (var obj in NetworkObjectsToActivate)
        {
            if (obj != null)
            {
                // chance for objects that dont destroy on load to be null
                obj.SetActive(true);
            }
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
        _year = "Year " + year;
        _lesson = lesson;
        _gameManager.SetSpLesson("Year " + year, lesson);
        _lessonSet = true;
    }

    public string GetYear()
    {
        return _year;
    }

    public string GetLesson()
    {
        return _lesson;
    }

    public void TogglePause()
    {
        _gamePaused = !_gamePaused;
    }

    public bool GameSetup()
    {
        return _modelSet && _lessonSet && !_gamePaused;
    }

    public List<Player> GetPlayers()
    {
        return _gameManager.GetAllPlayers();
    }

    public void NextRound()
    {
        // Get a player
        var player = _gameManager.GetAllPlayers()[0];
        // Call next round
        player.NextRound();
    }
    // Spawn players
    // Track time elapsed
    // Track goals reached
    // Track Player Model
}
