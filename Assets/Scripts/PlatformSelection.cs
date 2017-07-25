using System;
using System.Collections;
using System.Linq;
using PlayGen.Orchestrator.Common;
using PlayGen.Orchestrator.Contracts;
using PlayGen.Orchestrator.Unity.Common.Model;
using PlayGen.Orchestratror.Unity.Client;
using PlayGen.Orchestratror.Unity.Server;
using PlayGen.Unity.AsyncUtilities;

using UnityEngine;
using UnityEngine.Networking;

public class PlatformSelection : MonoBehaviour
{
    [SerializeField] private ConnectionType _connectionType;
    [SerializeField] private PlatformManager[] _platformManagers;

    private static PlatformSelection _instance;
    public static ConnectionType ConnectionType { get; private set; }

    public static Action<GameState> ServerStateChanged;

    private OrchestratedGameServer _orchestratedServer;
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
                _orchestratedServer = FindObjectOfType<OrchestratedGameServer>();
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

    private void ServerStateChange(GameState state)
    {
        ServerStateChanged(state);
    }

    public static void UpdateSeverState(GameState state)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().ControlledByOrchestrator = true;
        _instance._orchestratedServer.SetState(state, true);
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
        Debug.Log(obj.language);
        Debug.Log(obj.scenario);
        Debug.Log(obj.maxPlayers);
        Debug.Log(obj.difficulty);
    }

    private void StartClient(Endpoint endpoint)
    {
        _orchestrationClient.ConnectNetworkManager();
    }
}
