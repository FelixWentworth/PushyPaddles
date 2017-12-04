using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SP_MovementTrack : MonoBehaviour
{
    [Serializable]
    private enum Position
    {
        Water,
        Bridge
    }

    [SerializeField] private Position _position;

	public float DistanceToGround;

    public Player GetPlayer(float xMousePos)
    {
        var players = GameObject.FindGameObjectsWithTag("Player").ToList();
	    var floatingPlayer = players.First(p => p.GetComponent<Player>().PlayerRole == Player.Role.Floater);
        // Identify which player is using this track
        switch (_position)
        {
            case Position.Water:
                foreach (var p in players)
                {
                    var player = p.GetComponent<Player>();
						
                    if (player.PlayerRole == Player.Role.Paddler)
                    {
	                    if (player.transform.position.x < 0 && xMousePos < floatingPlayer.transform.position.x)
	                    {
							// left player pushes
		                    return player;
						}
						if
	                    (player.transform.position.x > 0 && xMousePos > floatingPlayer.transform.position.x)
	                    {
		                    // right player pushes
		                    return player;
	                    }
					}
                }
	            return null;
            case Position.Bridge:
	            return floatingPlayer.GetComponent<Player>();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}
