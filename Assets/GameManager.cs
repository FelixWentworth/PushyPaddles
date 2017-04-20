using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
