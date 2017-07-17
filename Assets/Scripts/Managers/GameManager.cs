using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class GameManager : NetworkBehaviour
{
    [SyncVar] public bool DistributingRewards;
    [SyncVar] private bool _gamePlaying = false;
    [SyncVar] public bool AllPlayersReady;

    public GameObject Platform;
    public GameObject PauseScreen;

    public bool GenerateRocks;

    public GameObject PlayerPrefab;
    private List<Player> _players = new List<Player>();
    private MenuManager _menu;
    private LevelManager _level;
    private Curriculum _curriculum;

    public GameObject PlayerOneSpawn;
    public GameObject PlayerTwoSpawn;
    public GameObject PlayerThreeSpawn;

    private bool _generatingLevel;

    private bool ready;

    void Start()
    {
#if USE_PROSOCIAL_EVENTS
        Platform.SetActive(false);
#endif
        PauseScreen.SetActive(false);
    }

    void Update()
    {
        if (isServer)
        {
            NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
            NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);

            if (_level == null)
            {
                _level = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            }
            if (_menu == null)
            {
                _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
            }
            if (_curriculum == null)
            {
                _curriculum = GameObject.Find("CurriculumManager").GetComponent<Curriculum>();
            }
            if (_level.IsGameOver)
            {
                _menu.ShowGameOver();
                RestartGame();
            }


            // Check if the game should be started
            if (!_level.RoundStarted)
            {
                // Set the sync var variable
                AllPlayersReady = AreAllPlayersReady();
                if (AllPlayersReady)
                {
                    StartGameTimer();
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
        OnConnected(netMsg.conn);
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

        PauseGame();

        // Destroy player game object
        DestroyImmediate(player.gameObject);

        netMsg.Disconnect();

        ChangeRoles();

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

    public Player GetPlayer(int id)
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
        Debug.LogError("Connection Ready: " + conn.isReady + ", " + conn.connectionId);
        
        var index = _players.Count;
        if (conn != null)
        {
            Debug.Log("Gladiator ready");

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
        if (GenerateRocks)
        {        
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<ObstacleGeneration>().Setup(3, "");
        }
        else
        {
            var challenge = _curriculum.GetNewChallenge(1);
            GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
        }

        // Start the game timer
        StartTimer();

        // Place the platform in the scene
        Platform.SetActive(true);
        NetworkServer.Spawn(Platform);
    }

    [Server]
    private void SetPlayerRole(int playerIndex, Player player)
    {
        //only the 1st player should be able to ride the platform
        player.PlayerID = playerIndex;
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

        _gamePlaying = false;
    }

    [Server]
    public void ResumeGame()
    {
        _level.ResumeTimer();

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
        Restart();
    }

    [Server]
    public void Restart()
    {
        if (!_generatingLevel)
        {
            _generatingLevel = true;
            // Reset the obstacles

            if (GenerateRocks)
            {
                GameObject.Find("LevelColliders/SpawnedObjects")
                        .GetComponent<ObstacleGeneration>().GenerateNewLevel(_level.RoundNumber * 3);
            }
            else
            {
                var challenge = _curriculum.GetNewChallenge(1);
                GameObject.Find("LevelColliders/SpawnedObjects").GetComponent<CollectibleGeneration>().Setup(0, challenge);
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
