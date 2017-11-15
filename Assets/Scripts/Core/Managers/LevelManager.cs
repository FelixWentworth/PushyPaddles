using System.Reflection;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LevelManager : NetworkBehaviour
{
    public Text RoundText;
    public Text TimeRemainingText;
    public Text PlayerText;
    public Text TargetText;
    public Text CurrentText;
        
    public GameTotal TotalUI;

    [SyncVar] private float _timeRemaining;
    [SyncVar] public int RoundNumber;
    [SyncVar] public int TotalRounds;

    [SyncVar] public bool RoundStarted;

    [SyncVar] public string Target = "";
    [SyncVar] public string Current = "";

    [SyncVar] public bool MathsVersion;

    public bool IsGameOver
    {
        get { return _timeRemaining <= 0f; }
    }

    public int SecondsTaken
    {
        get { return Mathf.RoundToInt(_timeLimit - _timeRemaining); }
    }

    private float _timeLimit
    {
        get
        {
#if USE_PROSOCIAL
            return PSL_LRSManager.Instance.TimeLimit;
#else
            return 10f * 60;
#endif
        }
    }

    public bool TimerPaused { get; private set; }

    private GameManager _gameManager;
    private Player _localPlayer;

    void Start()
    {
        TotalUI.gameObject.SetActive(false);
        if (isServer)
        {
            ResetRound();
            PlayerText.text = Localization.Get("UI_GAME_SERVER");
        }
        _localPlayer = null;
    }

    [ServerAccess]
    public void ResetRound()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        ResetAll();
        UpdateUI();
        RoundStarted = false;
    }

    [ServerAccess]
    public void StartRound()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        RoundStarted = true;
    }

    [ServerAccess]
    public void NextRound()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        RoundNumber += 1;
    }

    [ServerAccess]
    public void PauseTimer()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        TimerPaused = true;
    }

    [ServerAccess]
    public void ResumeTimer()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        TimerPaused = false;
    }


    void FixedUpdate()
    {
        if ((isServer || SP_Manager.Instance.IsSinglePlayer()) && !TimerPaused)
        {
            if (RoundStarted)
            {
                UpdateTimer(Time.deltaTime);
            }
        }
        if (!isServer)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            else if (_localPlayer == null)
            {
                _localPlayer = _gameManager.GetLocalPlayer();
            }
            else
            {
                PlayerText.text = _localPlayer.SyncNickName;
            }
        }
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        RoundText.text = string.Format(Localization.Get("FORMATTED_UI_GAME_ROUND"), RoundNumber);

        var totalTime = Mathf.RoundToInt(_timeRemaining);
        if (totalTime < 0)
        {
            // Round is over
            return;
        }
        var minute = totalTime / 60;
        var second = totalTime % 60;

        TimeRemainingText.text = minute + ":" + second.ToString("00");
        if (MathsVersion)
        {
            TargetText.text = Target != "" ? string.Format(Localization.Get("FORMATTED_UI_GAME_TARGET"), Target) : "";
            CurrentText.text = Current != ""
                ? string.Format(Localization.Get("FORMATTED_UI_GAME_CURRENT"), Current)
                : "";
        }
        else
        {
            TargetText.text = Localization.Get("UI_GAME_OBJECTIVE");
            CurrentText.text = "";
        }
    }

    [ServerAccess]
    private void UpdateTimer(float time)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        _timeRemaining -= time;
    }

    [ServerAccess]
    private void ResetAll()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        ResetRounds();
        ResetTimer();
    }

    [ServerAccess]
    private void ResetRounds()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        RoundNumber = 1;
    }

    [ServerAccess]
    private void ResetTimer()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        _timeRemaining = _timeLimit;
    }

    // From https://stackoverflow.com/questions/6052640/in-c-sharp-is-there-an-eval-function
    public double Evaluate(string expression)
    {
        expression = expression.Replace('x', '*');
        expression = expression.Replace('÷', '/');

        System.Data.DataTable table = new System.Data.DataTable();
        table.Columns.Add("expression", string.Empty.GetType(), expression);
        System.Data.DataRow row = table.NewRow();
        table.Rows.Add(row);
        return double.Parse((string)row["expression"]);
    }
}
