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

    public GameObject Platform;
    public GameObject PauseScreen;

    public bool GenerateRocks;

    public GameObject PlayerPrefab;
    private List<Player> _players = new List<Player>();
    private MenuManager _menu;
    private LevelManager _level;
    private Curriculum _curriculum;

    private bool _generatingLevel;

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
    void OnConnected(NetworkMessage netMsg)
    {
        if (_menu == null)
        {
            _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        }
        if (_level == null)
        {
            _level = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        }
        _menu.HideMenu();
        StartGame(NetworkServer.connections[NetworkServer.connections.Count - 1]);
    }

    [Server]
    void OnDisconnected(NetworkMessage netMsg)
    { 
        var player = _players.Find(p => p.ConnectionId == netMsg.conn.connectionId);

        // Remove player from the list
        _players.Remove(player);

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

        netMsg.conn.Disconnect();

        // Todo Assign New Roles
        ChangeRoles();
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


    public void StartGame(NetworkConnection conn)
    {
        var index = _players.Count;
        if (conn != null)
        {
            var playerObject = Instantiate(PlayerPrefab);
            var xMultiplier = index % 2 == 0 ? 1f : -1f;
            var zMultiplier = (index / 2) * 1.5f;
            playerObject.transform.position = new Vector3(4.5f * xMultiplier, -0.72f, zMultiplier);

            var player = playerObject.GetComponent<Player>();
            SetPlayerRole(index, player);

            player.ConnectionId = conn.connectionId;

            _players.Add(player);
            NetworkServer.AddPlayerForConnection(conn, player.gameObject, 0);
        }
#if !USE_PROSOCIAL_EVENTS
        StartGameTimer();
#endif


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
