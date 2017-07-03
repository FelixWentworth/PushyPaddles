using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LevelManager : NetworkBehaviour
{
    public Text RoundText;
    public Text TimeRemainingText;
    public Text PlayerText;
    public Text TargetText;
        
    public GameTotal TotalUI;

    [SyncVar] private float _timeRemaining;
    [SyncVar] public int RoundNumber;

    [SyncVar] public bool RoundStarted;

    [SyncVar] public string Target = "";

    public bool IsGameOver
    {
        get { return _timeRemaining <= 0f; }
    }

    private float _timeLimit = 600;

    public bool TimerPaused { get; private set; }

    void Start()
    {
        TotalUI.gameObject.SetActive(false);
        ResetRound();
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
    public void ResetRound()
    {
        ResetAll();
        UpdateUI();
        RoundStarted = false;
    }

    [Server]
    public void StartRound()
    {
        RoundStarted = true;
    }

    [Server]
    public void NextRound()
    {
        RoundNumber += 1;
    }

    [Server]
    public void PauseTimer()
    {
        TimerPaused = true;
    }

    [Server]
    public void ResumeTimer()
    {
        TimerPaused = false;
    }


    void FixedUpdate()
    {
        if (isServer && !TimerPaused)
        {
            if (RoundStarted)
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
        RoundText.text = "ROUND: " + RoundNumber;

        var totalTime = Mathf.RoundToInt(_timeRemaining);
        if (totalTime < 0)
        {
            // Round is over
            return;
        }
        var minute = totalTime / 60;
        var second = totalTime % 60;

        TimeRemainingText.text = minute + ":" + second.ToString("00");

        TargetText.text = Target != "" ? "Target: " + Target : "";
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
        RoundNumber = 1;
    }

    [Server]
    private void ResetTimer()
    {
        _timeRemaining = _timeLimit;
    }

    // From https://stackoverflow.com/questions/6052640/in-c-sharp-is-there-an-eval-function
    public double Evaluate(string expression)
    {
        System.Data.DataTable table = new System.Data.DataTable();
        table.Columns.Add("expression", string.Empty.GetType(), expression);
        System.Data.DataRow row = table.NewRow();
        table.Rows.Add(row);
        return double.Parse((string)row["expression"]);
    }
}
