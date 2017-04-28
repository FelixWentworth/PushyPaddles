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
            if (NetworkServer.connections.Count(c => c != null && c.isReady) > 1 || Input.GetKeyDown(KeyCode.Space))
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

    [Server]
    public void StartGame()
    {
        var index = 0;
        foreach (NetworkConnection conn in NetworkServer.connections)
        {
            if (conn != null)
            {
                var playerObject = Instantiate(PlayerPrefab);
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


    public static void RestartGame()
    {
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
