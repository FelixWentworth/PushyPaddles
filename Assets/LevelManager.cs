using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LevelManager : NetworkBehaviour
{
    public Text RoundText;
    public Text TimeRemainingText;

    [SyncVar] private float _timeRemaining;
    [SyncVar] private int _roundNumber;

    [SyncVar] private bool _roundStarted;

    public bool IsGameOver
    {
        get { return _timeRemaining <= 0f; }
    }


    private float _timeLimit = 120f;

    void Start()
    {
        Setup();
    }

    [Server]
    public void Setup()
    {
        ResetAll();
        UpdateUI();
    }

    [Server]
    public void StartRound()
    {
        _roundStarted = true;
    }

    [Server]
    public void NextRound()
    {
        _roundNumber += 1;
        ResetTimer();
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            if (_roundStarted)
            {
                UpdateTimer(Time.deltaTime);
            }
        }
        UpdateUI();
    }

    [Server]
    private void UpdateUI()
    {
        RoundText.text = "ROUND: " + _roundNumber;
        TimeRemainingText.text = Mathf.RoundToInt(_timeRemaining) + "s";
    }

    [Server]
    private void UpdateTimer(float time)
    {
        _timeRemaining -= time;
    }

    [Server]
    private void ResetAll()
    {
        ResetRounds();
        ResetTimer();
    }

    [Server]
    private void ResetRounds()
    {
        _roundNumber = 1;
    }

    [Server]
    private void ResetTimer()
    {
        _timeRemaining = _timeLimit;
    }
}
