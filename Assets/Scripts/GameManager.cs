using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    [SyncVar] private bool _gameStarted = false;

    public GameObject PlayerPrefab;
    private List<Player> _players = new List<Player>();
    private MenuManager _menu;
    private LevelManager _level;

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
            if (_level.IsGameOver)
            {
                Debug.LogError("GameLost");
                _level.Reset();
                Restart();
            }
        }
        //Debug.Log(_gameStarted);
        //if (!_gameStarted && !isServer)
        //{
        //    if (_menu == null)
        //    {
        //        _menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        //    }
        //    if (Input.GetKey(KeyCode.Space))
        //    {
        //       // Start game

        //        _menu.CmdHideMenu();
        //        _gameStarted = true;
        //        CmdStartGame();
        //        _menu.CmdShowCharacterSelect();
        //    }
        //}
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
        _menu.ShowCharacterSelect();
    }

    [Server]
    void OnDisconnected(NetworkMessage netMsg)
    {
        Debug.Log("Player Disconnected");

        // Todo Assign New Roles
            
    }

    public bool GameStarted()
    {
        return _gameStarted;
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
        _gameStarted = true;
        var index = _players.Count;
        if (conn != null)
        {
            var playerObject = Instantiate(PlayerPrefab);
            var xMultiplier = index % 2 == 0 ? 1f : -1f;
            var zMultiplier = (index / 2) * 1.5f;
            playerObject.transform.position = new Vector3(4.5f * xMultiplier, -0.72f, zMultiplier);
            var player = playerObject.GetComponent<Player>();
            SetPlayerRole(index, player);
            _players.Add(player);
            NetworkServer.AddPlayerForConnection(conn, player.gameObject, 0);
        }
        StartTimer();
    }

    [Server]
    private void SetPlayerRole(int playerIndex, Player player)
    {
        //only the 1st player should be able to ride the platform
        player.PlayerID = playerIndex;
        player.SetRole(playerIndex == 0 ? Player.Role.Floater : Player.Role.Paddler);
    }

    [Command]
    public void CmdAssignSpeedBoost(int playerNumber, float increment)
    {
        if (playerNumber >= _players.Count)
        {
            Debug.LogError(string.Format("Player number {0}, exceeds player count {1}", playerNumber, _players.Count));
            return;
        }
        // Find the player
        var player = _players[playerNumber];

        // Assign the reward
        player.SpeedModifier += increment;
    }

    [Command]
    public void CmdAssignReverseControls(int playerNumber, float modifier)
    {
        if (playerNumber >= _players.Count)
        {
            Debug.LogError(string.Format("Player number {0}, exceeds player count {1}", playerNumber, _players.Count));
            return;
        }
        // Find the player
        var player = _players[playerNumber];

        // Assign the reward
        player.DirectionModifier *= modifier;
    }

    [Server]
    private void StartTimer()
    {
        _level.StartRound();
    }

    [Server]
    public void Restart()
    {
        ChangeRoles();
        // Reset the obstacles
        GameObject.Find("LevelColliders/Rocks").GetComponent<ObstacleGeneration>().GenerateNewLevel(10);

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
}
