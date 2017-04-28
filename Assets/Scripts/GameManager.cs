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
            if (NetworkServer.connections.Count(c => c != null && c.isReady) > 1 || Input.GetKeyDown(KeyCode.Space))
            {
                // Start game
                GameObject.Find("MenuManager").GetComponent<MenuManager>().CmdToggleMenu();
                _gameStarted = true;
                Debug.Log("Start Game");
                StartGame();

            }
        }
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
        player.PlayerRole = (Player.Role) playerIndex +1;
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
