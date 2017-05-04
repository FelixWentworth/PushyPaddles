using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    private bool _gameStarted = false;

    public GameObject PlayerPrefab;
    private List<Player> _players = new List<Player>();

    void Update()
    {
        if (isServer && !_gameStarted)
        {
            var menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Start game
                
                menu.CmdHideMenu();
                _gameStarted = true;
                Debug.Log("Start Game");
                StartGame();
                menu.CmdShowCharacterSelect();
            }
        }
    }

    public Player GetLocalPlayer()
    {
        foreach (var player in _players)
        {
            if (player.isLocalPlayer)
            {
                return player;
            }
        }
        return null;
    }

    public int GetPlayerCount()
    {
        return _players.Count;
    }

    [Server]
    public void StartGame()
    {
        var index = 0;
        foreach (NetworkConnection conn in NetworkServer.connections)
        {
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
                index++;
            }
        }
    }

    [Server]
    private void SetPlayerRole(int playerIndex, Player player)
    {
        //only the 1st player should be able to ride the platform
        player.PlayerID = playerIndex;
        player.PlayerRole = playerIndex == 0 ? Player.Role.Floater : Player.Role.Paddler;
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

    [Command]
    public void CmdChangeRoles()
    {
        var floaterIndex = 0;
        for (var i=0; i<_players.Count; i++)
        {
            if (_players[i].PlayerRole == Player.Role.Floater)
            {
                floaterIndex = i;
            }
            _players[i].PlayerRole = Player.Role.Paddler;
        }
        // increment to next player
        floaterIndex = floaterIndex >= _players.Count - 1 ? 0 : floaterIndex + 1;
        _players[floaterIndex].PlayerRole = Player.Role.Floater;
    }

    public void RestartGame()
    {
        CmdChangeRoles();

        // Reset the obstacles
        GameObject.Find("Level/Rocks").GetComponent<ObstacleGeneration>().GenerateNewLevel(10);

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
}
