using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;

public class PSL_LRSManager : NetworkBehaviour
{
#region Variables

    public static PSL_LRSManager Instance;

    private PSL_SkillTracking _tracking;
    
    // Our variables needed for sending data
    [SerializeField] private string _url;

    [SerializeField] private string _logFileName;
    private object _body;

    // Our variables that identify the players and the match, used when sending data through LRS
    private string _matchId;
    private List<string> _playerIds = new List<string>();

    // Our tracked variables to be sent throught LRS
    private int _totalAttempts;
    private int _totalGoalReached;
    private int _totalRoundComplete;

    private List<int> _timeTakenPerRound = new List<int>();
    private double _averageTimeTaken {
        get { return _timeTakenPerRound.Average(); }
        set { }
    }   

    // Our variables defined by the current game
    private int _totalRounds;
    public int TimeLimit { get; private set; }

    #endregion

    void Awake()
    {
        TimeLimit = 600;
        Instance = this;

        _tracking = GetComponent<PSL_SkillTracking>();
    }

    #region public methods - game tracking
    /// <summary>
    /// Save the match Id and player Id locally to use with LRS
    /// </summary>
    /// <param name="matchId">The current match id as defined in the orchestrator</param>
    /// <param name="playerId">The current player id as defined in the orchestrator</param>
    [Server]
    public void JoinedGame(string matchId, string playerId)
    {
        _matchId = matchId;

        // Make sure the player has not rejoined without being removed properly
        if (!_playerIds.Contains(playerId))
        {
            _playerIds.Add(playerId);
        }
    }
    /// <summary>
    /// Setupd the game variables
    /// </summary>
    /// <param name="totalTime">Total time available</param>
    [Server]
    public void SetTotalTime(int totalTime)
    {
        TimeLimit = totalTime;
    }

    /// <summary>
    /// Setupd the game variables
    /// </summary>
    /// <param name="numRounds">Total number of rounds available</param>
    [Server]
    public void SetNumRounds(int numRounds)
    {
        _totalRounds = numRounds;
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

        SendSkillData(false, false, timeTaken);
    }

    /// <summary>
    /// Game has been completed
    /// </summary>
    /// <param name="allChallengesComplete">Are all challenges in mode completed</param>
    [Server]
    public void GameCompleted(int timeTaken)
    {
        SendSkillData(false, true, timeTaken);
    }

    [Server]
    public void PlayerShowedSkill(string playerId, PSL_Verbs verb, int increment)
    {
        _tracking.AddSkill(playerId, verb, increment);
    }

    [Server]
    public void SendSkillData(bool usePLS, bool finalResult, int timeTaken)
    {
        if (usePLS)
        {
            _tracking.SendSkillData();
            SendTrackedData(timeTaken);
        }
        else
        {
            var individualData = _tracking.OutputSkillData();
            OutputTrackedData(individualData, finalResult, timeTaken);
        }
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

    /// <summary>
    /// Output the tracked data to log
    /// </summary>
    /// <param name="timeTaken"></param>
    [Server]
    private void OutputTrackedData(string individualData, bool finalResult, int timeTaken)
    {
        if (!finalResult)
        {
            StartCoroutine(WriteToFile(individualData +
                                       "\nAttempts: " + _totalAttempts +
                                       "\nChests Reached: " + _totalGoalReached +
                                       "\nCalculation Success Rate: " +
                                       Mathf.RoundToInt(
                                           ((float) _totalRoundComplete / (float) _totalGoalReached) * 100f) + "%" +
                                       "\nRounds Complete: " + _totalRoundComplete +
                                       "\nTime Taken: " + timeTaken +
                                       //"\nProblems Complete: " + (Mathf.RoundToInt(((float)_totalRoundComplete / (float)_totalRounds) * 100f)) +
                                       "\n\n"));
        }
        else
        {

            StartCoroutine(WriteToFile("\nFinal Result for game\n" + individualData +
                                       "\nTotal Attempts: " + _totalAttempts +
                                       "\nChests Reached: " + _totalGoalReached +
                                       "\nCalculation Success Rate: " +
                                       Mathf.RoundToInt(
                                           ((float) _totalRoundComplete / (float) _totalGoalReached) * 100f) + "%" +
                                       "\nRounds Complete: " + _totalRoundComplete +
                                       "\nTotal Time Taken: " + timeTaken +
                                       //"\nProblems Complete: " + (Mathf.RoundToInt(((float)_totalRoundComplete / (float)_totalRounds) * 100f)) +
                                       "\n-----------------------------------------------------\n"));
        }
    }
    

    [Server]
    private IEnumerator SendPlayerData(WWWForm data, string playerId)
    {
        data.AddField("PlayerId", playerId);

        var www = new WWW(_url, data);
        yield return www;
    }

    [Server]
    private IEnumerator WriteToFile(string message)
    {
        var path = Application.streamingAssetsPath + "/" + _logFileName;

        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WSAPlayerX86 ||
            Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            path = "file:///" + path;
        }

        var www = new WWW(path);

        yield return www;
        if (www.text != null)
        {
            var newtext = www.text + message;
            using (var sw = new StreamWriter(Application.streamingAssetsPath + "/" + _logFileName))
            {
                sw.Write(newtext);
            }
        }
    }

#endregion

}
