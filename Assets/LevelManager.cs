using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LevelManager : NetworkBehaviour
{
    public Text RoundText;
    public Text TimeRemainingText;
    public Text PlayerText;

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
        Reset();
        if (isServer)
        {
            PlayerText.text = "Server";
        }
        else
        {
            PlayerText.text = "Player " +
                              (GameObject.Find("GameManager").GetComponent<GameManager>().GetLocalPlayer().PlayerID + 1); 
        }
    }

    [Server]
    public void Reset()
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
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        RoundText.text = "ROUND: " + _roundNumber;

        var totalTime = Mathf.RoundToInt(_timeRemaining);
        if (totalTime < 0)
        {
            // Round is over
            return;
        }
        var minute = totalTime / 60;
        var second = totalTime % 60;

        TimeRemainingText.text = minute + ":" + second.ToString("00");
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
