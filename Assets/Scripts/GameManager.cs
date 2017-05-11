﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    [SyncVar] private bool _gamePlaying = false;

    public GameObject Platform;
    public GameObject PauseScreen;

    public GameObject PlayerPrefab;
    private List<Player> _players = new List<Player>();
    private MenuManager _menu;
    private LevelManager _level;

    void Start()
    {
        Platform.SetActive(false);
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
            if (_level.IsGameOver)
            {
                _level.ResetRound();
                Restart();
            }
        }
#if USE_PROSOCIAL_EVENTS
        PauseScreen.SetActive(!_gamePlaying);
#else

#endif
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
        // Place the platform in the scene
        Platform.SetActive(true);
        NetworkServer.Spawn(Platform);

        // Start the game timer
        StartTimer();
    }

    [Server]
    private void SetPlayerRole(int playerIndex, Player player)
    {
        //only the 1st player should be able to ride the platform
        player.PlayerID = playerIndex;
        player.SetRole(playerIndex == 0 ? Player.Role.Floater : Player.Role.Paddler);
    }

    

    

    [Server]
    private void StartTimer()
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
        ChangeRoles();
        // Reset the obstacles
        GameObject.Find("LevelColliders/Rocks").GetComponent<ObstacleGeneration>().GenerateNewLevel(_level.RoundNumber * 2);

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


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), Mathf.RoundToInt(1 / Time.deltaTime).ToString());
    }
}
