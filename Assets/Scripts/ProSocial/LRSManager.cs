using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LRSManager : NetworkBehaviour
{
#region Variables

    public static LRSManager Instance;

    // Our variables needed for sending data
    [SerializeField] private string _url;
    private object _body;

    // Our variables that identify the players and the match, used when sending data through LRS
    private string _matchId;
    private List<string> _playerIds = new List<string>();

    // Our tracked variables to be sent throught LRS
    private int _totalAttempts;
    private int _totalGoalReached;
    private int _totalRoundComplete;

    private List<int> _timeTakenPerRound;
    private double _averageTimeTaken {
        get { return _timeTakenPerRound.Average(); }
        set { }
    }   

    // Our variables defined by the current game
    private int _totalRounds;
    private int _timeLimit;

    #endregion

    void Awake()
    {
        Instance = this;
    }

    #region public methods - game tracking
    /// <summary>
    /// Save the match Id and player Id locally to use with LRS
    /// </summary>
    /// <param name="matchId">The current match id as defined in the orchestrator</param>
    /// <param name="playerId">The current player id as defined in the orchestrator</param>
    [Server]
    public void OnConnect(string matchId, string playerId)
    {
        _matchId = matchId;
        _playerIds.Add(playerId);
    }
    /// <summary>
    /// Setupd the game variables
    /// </summary>
    /// <param name="numRounds">Total number of rounds available</param>
    /// <param name="totalTime">Total time available</param>
    [Server]
    public void Setup(int numRounds, int totalTime)
    {
        _totalRounds = numRounds;
        _timeLimit = totalTime;
    }

    /// <summary>
    /// Players are attempting the level again
    /// </summary>
    [Server]
    public void NewAttempt()
    {
        _totalAttempts += 1;
    }

    /// <summary>
    /// Players have made it to the chest
    /// </summary>
    [Server]
    public void ChestReached()
    {
        _totalGoalReached += 1;
    }

    /// <summary>
    /// Players have made it to the new round
    /// </summary>
    [Server]
    public void NewRound(int timeTaken)
    {
        _timeTakenPerRound.Add(timeTaken);
        _totalRoundComplete += 1;
    }

    /// <summary>
    /// Game has been completed
    /// </summary>
    /// <param name="allChallengesComplete">Are all challenges in mode completed</param>
    [Server]
    public void GameCompleted(int timeTaken)
    {
        SendTrackedData(timeTaken);
    }

    #endregion

    #region private methods - data sending

    /// <summary>
    /// Send all the data to the LRS
    /// </summary>
    /// <param name="timeTaken">Time taken to complete all rounds</param>
    [Server]
    private void SendTrackedData(int timeTaken)
    {
        if (_url == "")
        {
            return;
        }

        var form = new WWWForm();

        form.AddField("Attempts", _totalAttempts);
        form.AddField("ChestsReached", _totalGoalReached);
        form.AddField("CalculationSuccessRate", Mathf.RoundToInt(((float)_totalRoundComplete / (float)_totalGoalReached) * 100f));
        form.AddField("RoundsComplete", _totalRoundComplete);
        form.AddField("TimeTaken", timeTaken);
        form.AddField("ProblemsComplete", (Mathf.RoundToInt(((float)_totalRoundComplete / (float)_totalRounds) * 100f)));

        form.AddField("MatchId", _matchId);

        foreach (var playerId in _playerIds)
        {
            StartCoroutine(SendPlayerData(form, playerId));
        }
    }

    [Server]
    private IEnumerator SendPlayerData(WWWForm data, string playerId)
    {
        data.AddField("PlayerId", playerId);

        var www = new WWW(_url, data);
        yield return www;
    }

#endregion

}
