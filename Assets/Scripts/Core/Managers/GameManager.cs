using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Newtonsoft.Json;
#if PSL_ENABLED
using PlayGen.Orchestrator.Common;
#endif
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using QualitySettings = UnityEngine.QualitySettings;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public bool DistributingRewards;
    [SyncVar]
    private bool _gamePlaying = false;
    [SyncVar]
    private bool _gameWon = false;
    [SyncVar]
    public bool AllPlayersReady;

    [SyncVar] public bool LessonSelectRequired;

    public GameObject Platform;
    public GameObject PauseScreen;

    private bool _generateRocks { get { return PSL_GameConfig.GameType == "Obstacle"; }
    }

    public GameObject PlayerPrefab;
    private List<Player> _players = new List<Player>();
    private MenuManager _menu;
    private LevelManager _level;
    private Curriculum _curriculum;
    private PlayerActionManager _playerActionManager;

    public GameObject PlayerOneSpawn;
    public GameObject PlayerTwoSpawn;
    public GameObject PlayerThreeSpawn;

    private bool _generatingLevel;

    private bool ready;
    private DateTime _startTime;

    public bool ControlledByOrchestrator;

    private bool _gameComplete;
    private int _postGameRounds;

    private float _noPlayerTimer = 0f;
    private const float _maxInactiveTime = 300f;

    void Start()
    {
        _startTime = DateTime.Now;

#if PSL_ENABLED
		Debug.Log("PSL ENABLED");
        Platform.SetActive(false);
#endif
#if !PSL_ENABLED
		Debug.Log("PSL DISABLED");
#endif
		PauseScreen.SetActive(false);

        if (isServer)
        {
            // Set the quality settings to lowest
            UnityEngine.QualitySettings.SetQualityLevel(0);
        }
        else
        {
            // Set the default quality setting
#if UNITY_ANDROID || UNITY_IPHONE
            UnityEngine.QualitySettings.SetQualityLevel(2);
#else
            UnityEngine.QualitySettings.SetQualityLevel(3);
#endif
            PauseScreen.SetActive(true);
        }
    }

    void Update()
    {
	    if (isServer || SP_Manager.Instance.IsSinglePlayer())
        {
            NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
            NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);

            if (_level == null)
            {
                _level = GameObject.Find("LevelManager").GetComponent<LevelManager>();
                if (_level != null)
                {
                    _level.MathsVersion = !_generateRocks;
                }
            }
            if (_menu == null)
            {
                _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
            }
            if (_curriculum == null)
            {
                _curriculum = GameObject.Find("CurriculumManager").GetComponent<Curriculum>();
            }
            if (_playerActionManager == null)
            {
                _playerActionManager = GameObject.Find("InteractionManager").GetComponent<PlayerActionManager>();
            }
            if (_level.RoundStarted && _level.IsGameOver)
            {
                PauseGame();
                GameOver(_gameComplete);
            }

            //// Check if the game should be started
            //if (!_level.RoundStarted)
            //{
            // Set the sync var variable
            AllPlayersReady = AreAllPlayersReady();
            if (SP_Manager.Instance != null && SP_Manager.Instance.IsSinglePlayer())
            {
                if (AllPlayersReady)
                {
                    StartGameTimer();
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
#if PSL_ENABLED
            if (!ControlledByOrchestrator || PlatformSelection.ConnectionType == ConnectionType.Testing)
            {
                if (AllPlayersReady)
                {
                    PlatformSelection.UpdateSeverState(GameState.Started);

					StartGameTimer();
                    ResumeGame();
                }
                else
                {
                    PlatformSelection.UpdateSeverState(GameState.WaitingForPlayers);
                    PauseGame();
                }
                //}
                //
                //AllPlayersReady = _players.Count >= 3;
                if (!AllPlayersReady)
                {
                    PauseGame();
                    PlatformSelection.UpdateSeverState(GameState.Paused);

                }
                else
                {
                    ResumeGame();
                    PlatformSelection.UpdateSeverState(GameState.Started);

                }
            }
            if (ControlledByOrchestrator && (PlatformSelection.GetGameState() == GameState.Started || PlatformSelection.GetGameState() == GameState.WaitingForPlayers) && !SP_Manager.Instance.IsSinglePlayer())
            {
                if (!AllPlayersReady)
                {
                    PauseGame();
                }
                else if (AllPlayersReady)
                {
                    StartGameTimer();
                    ResumeGame();
                }
            }
#endif

			// end game when no players after a certain time
			if (_players.Count == 0)
            {
                _noPlayerTimer += Time.deltaTime;
                if (_noPlayerTimer > _maxInactiveTime)
                {
                    Application.Quit();
                }
            }
            else
            {
                _noPlayerTimer = 0f;
            }
            LessonSelectRequired = PSL_GameConfig.LessonSelectionRequired;
        }



        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    ready = true;
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    ResetRound();
        //}
#if PSL_ENABLED
        PauseScreen.SetActive(!_gamePlaying);
#else

#endif

    }

    [ServerAccess]
    void RestartGame()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        _level.ResetRound();
        Restart();
    }

    [ServerAccess]
    public void OnConnected(NetworkMessage netMsg)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        OnConnected(netMsg.conn);
    }

    [ServerAccess]
    public void OnConnected(NetworkConnection netMsg)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        if (_menu == null)
        {
            _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        }
        if (_level == null)
        {
            _level = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        }

        var connection = netMsg;
        Debug.Log("Has connection " + NetworkServer.connections.Contains(connection));
        Debug.Log("(C) Current Players: " + _players.Count);
        //NetworkManager.singleton.OnClientConnect(connection);

        _menu.HideMenu();
        Debug.Log("Setting Client language to: " + Localization.SelectedLanguage.Name);
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ClientSetLanguage(Localization.SelectedLanguage.Name);
        }
        else
        {
            RpcSetLanguage(Localization.SelectedLanguage.Name);
        }
        StartCoroutine(JoinGame(netMsg));
    }

    [ServerAccess]
    public void OnDisconnected(NetworkMessage netMsg)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        OnDisconnected(netMsg.conn);
    }

    [ServerAccess]
    public void OnDisconnected(NetworkConnection netMsg)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        Debug.Log("Disconnected: " + netMsg.connectionId);
        var player = _players.Find(p => p.ConnectionId == netMsg.connectionId);
        if (player == null)
        {
            return;
        }
        // Remove player from the list
        _players.Remove(player);
        Debug.Log("(D} Current Players: " + _players.Count);


        if (player.HoldingPlatform)
        {
            var platform = GameObject.FindGameObjectWithTag("Platform");
            player.DropPlatform(platform);
        }
        if (player.OnPlatform)
        {
            GameObject.FindGameObjectWithTag("Platform").GetComponent<FloatingPlatform>().Respawn();

        }

        // Destroy player game object
        DestroyImmediate(player.gameObject);

        netMsg.Disconnect();

        ChangeRoles();

		// TODO check if needs a delay for player disconnection, may break otherwise
#if PSL_ENABLED
        PlatformSelection.UpdatePlayers(_players.Select(p => p.PlayerID).ToList());
#endif
	}

	[ClientRpc]
    public void RpcSetLanguage(string language)
    {
        ClientSetLanguage(language);
    }

    [ClientAccess]
    public void ClientSetLanguage(string language)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        Localization.UpdateLanguage(language);
        Debug.Log("Set Language from server to: " + language);
    }

    private bool AreAllPlayersReady()
    {
        if (SP_Manager.Instance.IsSinglePlayer() && SP_Manager.Instance.Get<SP_GameManager>().GameSetup())
        {
            return true;
        }
        if (ready)
            return true;

		if (_players.Count != 3)
        {
            return false;
        }
        foreach (var player in _players)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    public bool GamePlaying()
    {
        return _gamePlaying;
    }

    public Player GetLocalPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            var _p = player.GetComponent<Player>();
            if (_p.isLocalPlayer)
            {
                return _p;
            }
        }

        return null;
    }

    private void RemovePlayer(Player player)
    {
        Debug.Log("Remove Player");
    }

    public Player GetPlayer(string id)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            var _p = player.GetComponent<Player>();
            if (_p.PlayerID == id)
            {
                return _p;
            }
        }

        return null;
    }

    public List<Player> GetAllPlayers()
    {
        return _players;
    }
    public List<string> GetPlayerIds()
    {
        if (_players.Count == 0)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                _players.Add(player.GetComponent<Player>());
            }
        }

        return _players.Select(p => p.PlayerID).ToList();
    }

    public int GetPlayerCount()
    {
        if (_players.Count == 0)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                _players.Add(player.GetComponent<Player>());
            }
        }

        return _players.Count;
    }

    public IEnumerator JoinGame(NetworkConnection conn)
    {
        var index = _players.Count;
        if (conn != null)
        {
            var playerObject = Instantiate(PlayerPrefab);

            playerObject.transform.position = GetPlayerRespawn(index);

            var player = playerObject.GetComponent<Player>();
            SetPlayerRole(index, player);
            player.PlayerNum = index;

            player.ConnectionId = conn.connectionId;

            _players.Add(player);

            NetworkServer.AddPlayerForConnection(conn, player.gameObject, 0);

            while (!conn.isReady)
            {
                Debug.Log("Waiting for connection to be ready");
                yield return null;
            }
        }
    }

    public void CreatePlayer(int id)
    {
        var index = _players.Count;
        var playerObject = Instantiate(PlayerPrefab);

        playerObject.transform.position = GetPlayerRespawn(index);

        var player = playerObject.GetComponent<Player>();
        SetPlayerRole(index, player);
        player.PlayerNum = index;
        player.ConnectionId = id;

        _players.Add(player);
    }

    public Vector3 GetPlayerRespawn(int index)
    {
        switch (index)
        {
            case 0:
                return PlayerOneSpawn.transform.position;
            case 1:
                return PlayerTwoSpawn.transform.position;
            case 2:
                return PlayerThreeSpawn.transform.position;
            default:
                var xMultiplier = index % 2 == 0 ? 1f : -1f;
                var zMultiplier = (index / 2) * 1.5f;
                return new Vector3(xMultiplier * 4.5f, -0.72f, zMultiplier);
        }
    }

    /// <summary>
    /// Called by ProSocial event listener to start the actual game
    /// </summary>

    [ServerAccess]
    public void StartGameTimer()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        if (_level.RoundStarted)
        {
            return;
        }


        // Generate Obstacles
        if (_generateRocks)
        {
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<ObstacleGeneration>().Setup(3, "");
        }
        else if (!PSL_GameConfig.LessonSelectionRequired)
        {
            var challenge = Curriculum.GetNewChallenge(PSL_GameConfig.Level, PSL_GameConfig.LessonNumber);
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }
        else if (SP_Manager.Instance.IsSinglePlayer())
        {
            var manager = SP_Manager.Instance.Get<SP_GameManager>();
            var challenge = Curriculum.GetNewChallenge(manager.GetYear(), manager.GetLesson());
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }

        // Start the game timer
        StartTimer();

        // Place the platform in the scene
        
        Platform.SetActive(true);
        if (!SP_Manager.Instance.IsSinglePlayer())
        {
            NetworkServer.Spawn(Platform);
        }
    }

    [ServerAccess]
    public void SetLesson(string year, string lesson)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        var challenge = Curriculum.GetChallengesForYear(year).FirstOrDefault(c => c.Lesson == lesson);

        if (challenge != null)
        {
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }
    }

    public void SetSpLesson(string year, string lesson)
    {
        var challenge = Curriculum.GetChallengesForYear(year).FirstOrDefault(c => c.Lesson == lesson);

        if (challenge != null)
        {
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }
    }

    [ServerAccess]
    private void SetPlayerRole(int playerIndex, Player player)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        // Single player does not need nick names
        if (!SP_Manager.Instance.IsSinglePlayer())
        {
            player.SyncNickName = Localization.Get("UI_GAME_PLAYER") + (playerIndex + 1);
        }
        else
        {
            player.SyncNickName = "";
        }
        //only the 1st player should be able to ride the platform
        player.SetRole(playerIndex == 0 ? Player.Role.Floater : Player.Role.Paddler);
    }
    
    [ServerAccess]
    public void StartTimer()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        _gamePlaying = true;

        _level.ResetRound();
        _level.StartRound();
    }

    [ServerAccess]
    public void PauseGame()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        _level.PauseTimer();
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ClientShowPause(true);
        }
        else
        {
            RpcShowPause(true);
        }
        _gamePlaying = false;
    }

    [ServerAccess]
    public void ResumeGame()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        _level.ResumeTimer();

        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ClientShowPause(false);
        }
        else
        {
            RpcShowPause(false);
        }
        _gamePlaying = true;
    }

    [ServerAccess]
    public void StopGame()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        // End the game
        _gamePlaying = false;
        _menu.ShowGameOver(_gameWon, _level.SecondsTaken, ControlledByOrchestrator);
#if PSL_ENABLED
        PSL_LRSManager.Instance.GameCompleted(_level.SecondsTaken);
#endif
		if (!ControlledByOrchestrator)
        {
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                ClientStopGame();
            }
            else
            {
                RpcStopGame();
            }
            ClearPlayerObjects();
        }
    }

    [ClientRpc]
    private void RpcStopGame()
    {
        NetworkManager.singleton.StopClient();
    }

    [ClientAccess]
    private void ClientStopGame()
    {
        Application.Quit();
    }

    [ClientRpc]
    private void RpcShowPause(bool showing)
    {
        ClientShowPause(showing);
    }

    [ClientAccess]
    private void ClientShowPause(bool showing)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        PauseScreen.SetActive(showing);
    }

    /// <summary>
    /// Actions completed as a group gets credited to each player
    /// </summary>
    /// <param name="action">the action that was completed</param>
    [ServerAccess]
    public void GroupAction(PlayerAction action)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        foreach (var player in  _players)
        {
            PlayerAction(action, player.PlayerID);
        }
    }

    [ServerAccess]
    public void PlayerAction(PlayerAction action, string playerId)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
#if PSL_ENABLED
        _playerActionManager.PerformedAction(action, playerId, action.GetAlwaysTracked(), action.GetCancelAction());
#endif
	}

	[ServerAccess]
    private void ClearPlayerObjects()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        foreach (var player in _players)
        {
            Destroy(player.gameObject);
        }
        _players.Clear();
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
        _level.NextRound();
		// output the values from this round
#if PSL_ENABLED
        PSL_LRSManager.Instance.NewRound((DateTime.Now - _startTime).Seconds);
#endif
		Restart(newRound: true);
        _startTime = DateTime.Now;

    }

    [ServerAccess]
    public void Restart(bool newRound = false)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        if (!_generatingLevel)
        {
            _generatingLevel = true;
            // Reset the obstacles

            if (_generateRocks)
            {
                GameObject.Find("LevelColliders/SpawnedObjects")
                        .GetComponent<ObstacleGeneration>().GenerateNewLevel(_level.RoundNumber * 3);
            }
            else if (_gameComplete)
            {
                _postGameRounds += 1;
                GameObject.Find("LevelColliders/SpawnedObjects")
                        .GetComponent<ObstacleGeneration>().GenerateNewLevel(_postGameRounds * 3);
            }
            else
            {
                CurriculumChallenge challenge = null;
                if (newRound)
                {
                    if (SP_Manager.Instance.IsSinglePlayer())
                    {
                        challenge = _curriculum.GetNextChallenge(SP_Manager.Instance.Get<SP_GameManager>().GetYear(), SP_Manager.Instance.Get<SP_GameManager>().GetLesson());
                    }
                    else
                    {
                        challenge = _curriculum.GetNextChallenge(PSL_GameConfig.Level, PSL_GameConfig.LessonNumber);
                    }
                }
                if (challenge == null)
                {
                    // Reached the end of the game
                    // Dont show game over until the time has run out
                    _generatingLevel = false;
                    _gameComplete = true;
                    _level.RoundNumber -= 1;
                    _level.MathsVersion = false;
                    Restart(newRound: true);
                }
                else
                {
                    GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>()
                        .Setup(0, challenge);
                }
            }

            if (!SP_Manager.Instance.IsSinglePlayer())
            {
                // No need to switch roles here, makes no difference to the player
                ChangeRoles();
            }
            // Reset Player Posititions
            var players = GameObject.FindGameObjectsWithTag("Player");

            // Reset Platform Positions
            var platforms = GameObject.FindGameObjectsWithTag("Platform");

            foreach (var player in players)
            {
                player.GetComponent<Player>().Respawn();
                player.GetComponent<Player>().OnPlatform = false;
            }
            foreach (var platform in platforms)
            {
                platform.GetComponent<FloatingPlatform>().Respawn();
            }
            _generatingLevel = false;
        }
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
        if (!_generatingLevel)
        {
            _generatingLevel = true;
            
            // Reset the obstacles
            if (_generateRocks)
            {
                GameObject.Find("LevelColliders/SpawnedObjects")
                    .GetComponent<ObstacleGeneration>().GenerateNewLevel(_level.RoundNumber * 3);
            }
            else
            {
                var currentChallenge = _curriculum.GetCurrentChallenge();
                GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().NewSetup(0, currentChallenge);
            }

            _generatingLevel = false;
        }
    }

    [ServerAccess]
    public void ChangeRoles()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        var players = GameObject.FindGameObjectsWithTag("Player");

        var _players = new List<Player>();

        foreach (var player in players)
        {
            _players.Add(player.GetComponent<Player>());
        }

        if (_players.Count == 0)
        {
            return;
        }

        var floaterIndex = 0;
        for (var i = 0; i < _players.Count; i++)
        {
            if (_players[i].PlayerRole == Player.Role.Floater)
            {
                floaterIndex = i;
            }
            _players[i].SetRole(Player.Role.Paddler);
            _players[i].RpcShowSwitchingRoles();
        }
        // increment to next player
        floaterIndex = floaterIndex >= _players.Count - 1 ? 0 : floaterIndex + 1;

        _players[floaterIndex].SetRole(Player.Role.Floater);

        SetNewPlayerNum(floaterIndex);

        RespawnPlayers();
    }

    [ServerAccess]
    public void SetNewPlayerNum(int floater)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        for (var i = 0; i < _players.Count; i++)
        {
            _players[floater].PlayerNum = i;
            floater = floater == _players.Count - 1 ? 0 : floater += 1;
        }        
    }

    [ServerAccess]
    public void RespawnPlayers()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        foreach (var player in _players)
        {
            player.SyncRespawn(player.transform.eulerAngles);
        }
    }

    [ServerAccess]
    private void GameOver(bool victory)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        //PSL_LRSManager.Instance.GameCompleted(_level.SecondsTaken);
        _gameWon = victory;
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            SP_Manager.Instance.Get<SP_Menus>().ShowGameOver(victory, _level.SecondsTaken);
            _curriculum.ResetLevel();
            RestartGame();
        }
        else
        {
			_menu.ShowGameOver(victory, _level.SecondsTaken, false);
			RestartGame();
		}
        if (ControlledByOrchestrator)
        {
#if PSL_ENABLED
            PlatformSelection.UpdateSeverState(GameState.Stopped);
#endif
		}
	}
    


    public void HideRewards()
    {
        if (_menu == null)
        {
            _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        }
        _menu.HideRewards();
    }


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), Mathf.RoundToInt(1 / Time.deltaTime).ToString());
    }
}

