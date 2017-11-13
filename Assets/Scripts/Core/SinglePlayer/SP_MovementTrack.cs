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
        Left,
        Right,
        Bridge
    }

    [SerializeField] private Position _position;

    private Player _player;

    public Player GetPlayer()
    {
        if (_player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player").ToList();
            // Identify which player is using this track
            switch (_position)
            {
                case Position.Left:
                    foreach (var p in players)
                    {
                        var player = p.GetComponent<Player>();
                        if (player.PlayerRole == Player.Role.Paddler && player.transform.position.x < 0)
                        {
                            _player = player;
                        }
                    }       
                    break;
                case Position.Right:
                    foreach (var p in players)
                    {
                        var player = p.GetComponent<Player>();
                        if (player.PlayerRole == Player.Role.Paddler && player.transform.position.x > 0)
                        {
                            _player = player;
                        }
                    }
                    break;
                case Position.Bridge:
                    foreach (var p in players)
                    {
                        var player = p.GetComponent<Player>();
                        if (player.PlayerRole == Player.Role.Floater)
                        {
                            _player = player;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        return _player;
    }

}
