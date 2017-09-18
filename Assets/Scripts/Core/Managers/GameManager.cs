using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using PlayGen.Orchestrator.Common;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public bool DistributingRewards;
    [SyncVar]
    private bool _gamePlaying = false;
    [SyncVar]
    public bool AllPlayersReady;

    public GameObject Platform;
    public GameObject PauseScreen;

    private bool _generateRocks { get { return PSL_GameConfig.Instance.GameType == "Obstacle"; }
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

    void Start()
    {
        _startTime = DateTime.Now;

#if USE_PROSOCIAL_EVENTS
        Platform.SetActive(false);
#endif
        PauseScreen.SetActive(false);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Restart(true);
        }

        if (isServer)
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
            if (_level.IsGameOver)
            {
                GameOver(_gameComplete);
            }

            //// Check if the game should be started
            //if (!_level.RoundStarted)
            //{
            // Set the sync var variable
            AllPlayersReady = AreAllPlayersReady();

            if (AllPlayersReady)
            {
                // TODO change to waiting for start and wait for Orchestrator to start game
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
            if (!ControlledByOrchestrator)
            {
                AllPlayersReady = _players.Count >= 3;
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
            
        }



        if (Input.GetKeyDown(KeyCode.F1))
        {
            ready = true;
        }
#if USE_PROSOCIAL_EVENTS
        PauseScreen.SetActive(!_gamePlaying);
#else

#endif

    }

    [Server]
    void RestartGame()
    {
        _level.ResetRound();
        Restart();
    }

    [Server]
    public void OnConnected(NetworkMessage netMsg)
    {
        OnConnected(netMsg.conn); ;
    }

    [Server]
    public void OnConnected(NetworkConnection netMsg)
    {
        if (_menu == null)
        {
            _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        }
        if (_level == null)
        {
            _level = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        }

        var connection = netMsg;
        Debug.Log("Has connectionL " + NetworkServer.connections.Contains(connection));
        Debug.Log("(C) Current Players: " + _players.Count);
        //NetworkManager.singleton.OnClientConnect(connection);

        _menu.HideMenu();
        StartCoroutine(JoinGame(netMsg));
    }

    [Server]
    public void OnDisconnected(NetworkMessage netMsg)
    {
        OnDisconnected(netMsg.conn);
    }

    [Server]
    public void OnDisconnected(NetworkConnection netMsg)
    {
        Debug.Log("Disconnected: " + netMsg.connectionId);
        var player = _players.Find(p => p.ConnectionId == netMsg.connectionId);
        if (player == null)
        {
            return;
        }
        // Remove player from the list
        _players.Remove(player);
        Debug.Log("(D) Current Players: " + _players.Count);


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
        PlatformSelection.UpdatePlayers(_players.Select(p => p.PlayerID).ToList());
    }

    private bool AreAllPlayersReady()
    {
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

    [Server]
    public void StartGameTimer()
    {
        if (_level.RoundStarted)
        {
            return;
        }


        // Generate Obstacles
        if (_generateRocks)
        {
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<ObstacleGeneration>().Setup(3, "");
        }
        else if (!PSL_GameConfig.Instance.LessonSelectionRequired)
        {
            var challenge = _curriculum.GetNewChallenge(PSL_GameConfig.Instance.Level, PSL_GameConfig.Instance.LessonNumber);
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }

        // Start the game timer
        StartTimer();

        // Place the platform in the scene
        Platform.SetActive(true);
        NetworkServer.Spawn(Platform);
    }

    [Server]
    public void SetLesson(string year, string lesson)
    {
        var challenge = _curriculum.GetChallengesForYear(year).FirstOrDefault(c => c.Lesson == lesson);

        Debug.Log(challenge);

        if (challenge != null)
        {
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }
    }

    [Server]
    private void SetPlayerRole(int playerIndex, Player player)
    {
        //only the 1st player should be able to ride the platform
        player.SyncNickName = Localization.Get("UI_GAME_PLAYER") + (playerIndex + 1);
        player.SetRole(playerIndex == 0 ? Player.Role.Floater : Player.Role.Paddler);
    }



    [Server]
    public void StartTimer()
    {
        _gamePlaying = true;

        _level.ResetRound();
        _level.StartRound();
    }

    [Server]
    public void PauseGame()
    {
        _level.PauseTimer();
        RpcShowPause(true);
        _gamePlaying = false;
    }

    [Server]
    public void ResumeGame()
    {
        _level.ResumeTimer();

        RpcShowPause(false);

        _gamePlaying = true;
    }

    [Server]
    public void StopGame()
    {
        // End the game
        _gamePlaying = false;

        RpcStopGame();

        ClearPlayerObjects();
    }

    [ClientRpc]
    private void RpcStopGame()
    {
        NetworkManager.singleton.StopClient();

    }

    [ClientRpc]
    private void RpcShowPause(bool showing)
    {
        PauseScreen.SetActive(showing);
    }

    /// <summary>
    /// Actions completed as a group gets credited to each player
    /// </summary>
    /// <param name="action">the action that was completed</param>
    [Server]
    public void GroupAction(PlayerAction action)
    {
        foreach (var player in  _players)
        {
            PlayerAction(action, player.PlayerID);
        }
    }

    [Server]
    public void PlayerAction(PlayerAction action, string playerId)
    {
        _playerActionManager.PerformedAction(action, playerId, action.GetAlwaysTracked(), action.GetCancelAction());
    }

    [Server]
    private void ClearPlayerObjects()
    {
        foreach (var player in _players)
        {
            Destroy(player.gameObject);
        }
        _players.Clear();
    }

    [Server]
    public void NextRound()
    {
        _level.NextRound();
        // output the values from this round
        PSL_LRSManager.Instance.NewRound((DateTime.Now - _startTime).Seconds);
        Restart(newRound: true);
        _startTime = DateTime.Now;

    }

    [Server]
    public void Restart(bool newRound = false)
    {
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
                    challenge = _curriculum.GetNextChallenge(PSL_GameConfig.Instance.Level, PSL_GameConfig.Instance.LessonNumber);
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

            ChangeRoles();

            // Reset Player Posititions
            var players = GameObject.FindGameObjectsWithTag("Player");

            // Reset Platform Positions
            var platforms = GameObject.FindGameObjectsWithTag("Platform");

            foreach (var player in players)
            {
                player.GetComponent<Player>().Respawn();
            }
            foreach (var platform in platforms)
            {
                platform.GetComponent<FloatingPlatform>().Respawn();
            }
            _generatingLevel = false;
        }
    }

    [Server]
    public void ChangeRoles()
    {
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

    [Server]
    public void SetNewPlayerNum(int floater)
    {
        for (var i = 0; i < _players.Count; i++)
        {
            _players[floater].PlayerNum = i;
            floater = floater == _players.Count - 1 ? 0 : floater += 1;
        }        
    }

    [Server]
    public void RespawnPlayers()
    {
        foreach (var player in _players)
        {
            player.SyncRespawn(player.transform.eulerAngles);
        }
    }

    [Server]
    private void GameOver(bool victory)
    {
        _menu.ShowGameOver(victory, _level.SecondsTaken);
        PSL_LRSManager.Instance.GameCompleted(_level.SecondsTaken);
        PlatformSelection.UpdateSeverState(GameState.Stopped);
        RestartGame();
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
