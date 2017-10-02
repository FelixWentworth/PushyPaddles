using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PlayGen.Orchestrator.Common;
using PlayGen.Orchestrator.Contracts;
using PlayGen.Orchestrator.PSL.Common.LRS;
using PlayGen.Orchestrator.PSL.Unity.Server;
using PlayGen.Orchestrator.Unity.Common.Model;
using PlayGen.Orchestratror.Unity.Client;
using PlayGen.Orchestratror.Unity.Server;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;

public class PlatformSelection : MonoBehaviour
{
    [SerializeField] private ConnectionType _connectionType;
    [SerializeField] private PlatformManager[] _platformManagers;

    private static PlatformSelection _instance;
    public static ConnectionType ConnectionType { get; private set; }

    public static Action<GameState> ServerStateChanged;

    private PSLOrchestratedGameServer _orchestratedServer;
    private OrchestrationClient _orchestrationClient;

    public struct PSLPlayerData
    {
        public string PlayerId;
        public string MatchId;
        public string NickName;
    }

    public PSLPlayerData PlayerData { get; private set; }

    void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this);
        ConnectionType = _connectionType;

        var manager = _platformManagers.FirstOrDefault(p => p.ConnectionType == _connectionType);
        if (manager != null && manager.NetworkManagerObj != null)
        {
            Instantiate(manager.NetworkManagerObj);
        }

        switch (_connectionType)
        {
            case ConnectionType.Server:
                _orchestratedServer = FindObjectOfType<PSLOrchestratedGameServer>();
                _orchestratedServer.StateChanged += ServerStateChange;
                _orchestratedServer.ConfigValidated += ConfigValidated;
                _orchestratedServer.RegisteredWithOrchestrator += RegisteredWithOrchestrator;
                break;
            case ConnectionType.Client:
                _orchestrationClient = FindObjectOfType<OrchestrationClient>();
                _orchestrationClient.PlayerIdentified += PlayerIdentified;
                _orchestrationClient.EndpointLocated += StartClient;
                break;
            case ConnectionType.Testing:
                break;
        }
    }

    public static void EnsureServerState()
    {
        if (_instance._orchestratedServer.State < GameState.Started)
        {
            _instance._orchestratedServer.SetState(GameState.Started, true);
        }
    }

    private void ServerStateChange(GameState state)
    {
        ServerStateChanged(state);
    }

    public static void UpdateSeverState(GameState state)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().ControlledByOrchestrator = true;
        if (_instance._orchestratedServer != null)
        {
            _instance._orchestratedServer.SetState(state, true);
        }
    }

    private void PlayerIdentified(SessionIdentifier obj)
    {
        PlayerData = new PSLPlayerData()
        {
            MatchId = obj.gameInstanceID,
            PlayerId = obj.playerID,
            NickName = obj.playerNickName
        };
    }

    private void ConfigValidated()
    {
        _orchestratedServer.StartListening();
    }

    private void RegisteredWithOrchestrator(GameRegistrationResponse obj)
    {
        if (obj.scenario != "Default" || obj.scenario != "Custom")
        { 
            PSL_GameConfig.Instance.SetGameConfig(obj.scenario, GetLessonFromDifficulty(obj.scenario, obj.difficulty), "Maths", "All");
        }
        PSL_LRSManager.Instance.SetTotalTime(Convert.ToInt16(obj.maxTime));

        Localization.UpdateLanguage("en");

        // Set language
        var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
        if (allCultures.Any(c => c.Name.Equals(obj.language, StringComparison.OrdinalIgnoreCase)))
        {
            Localization.UpdateLanguage(obj.language);
        }
        else if (allCultures.Any(c => c.EnglishName.Equals(obj.language, StringComparison.OrdinalIgnoreCase)))
        {
            Localization.UpdateLanguage(allCultures.First(c => c.EnglishName.Equals(obj.language, StringComparison.OrdinalIgnoreCase)));
        }
    }

    private string GetLessonFromDifficulty(string year, int difficulty)
    {
        year = year.Replace("year", "");
        year = year.Replace(" ", "");
        var availableLessons = PSL_GameConfig.Instance.GetLessonCountForScenario(year);

        switch (difficulty)
        {
            case 1:
                return GetRandomInRange(0f, .33f, availableLessons);
            case 2:
                return GetRandomInRange(.33f, .66f, availableLessons);
            case 3:
                return GetRandomInRange(.66f, 1f, availableLessons);
            default:
                return "1";
        }
    }

    private string GetRandomInRange(float min, float max, int range)
    {
        var low = Convert.ToInt16(range * min);
        var high = Convert.ToInt16(range * max);

        return UnityEngine.Random.Range(low, high + 1).ToString();

    }

    private void StartClient(Endpoint endpoint)
    {
        _orchestrationClient.ConnectNetworkManager();
    }

    public static void UpdatePlayers(List<string> playerIDs)
    {
        if (_instance._orchestratedServer)
        {
            _instance._orchestratedServer.UpdateConnectedPlayers(playerIDs);
        }
    }

    public static void AddSkill(string playerId, LRSSkillVerb verb, int increment)
    {
        if (_instance._orchestratedServer)
        {
            _instance._orchestratedServer.AddSkill(playerId, verb, increment);
        }
    }

    public static void SendSkillData()
    {
        if (_instance._orchestratedServer)
        {
            _instance._orchestratedServer.SendStoredLRSData();
        }
    }

    public static string OutputSkillData()
    {
        if (_instance._orchestratedServer)
        {
            return _instance._orchestratedServer.OutputSkillData();
        }
        return string.Empty;
    }
}
