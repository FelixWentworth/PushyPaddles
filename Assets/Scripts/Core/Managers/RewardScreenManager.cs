using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class RewardScreenManager : UIScreen
{
    public RewardsManager RewardsManager;
    private float _speedBoost = 0.05f;
    private float _controlsModifier = -1f;

    private GameManager _gameManager;
    private Player _player;

    public override void Show()
    {
        base.Show();
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        var players = _gameManager.GetPlayerIds();
        RewardsManager.ResetRewards(players.Count, players);
    }

    public override void Hide()
    {
        base.Hide();
    }

    void Update()
    {
        if (IsShowing && _gameManager.GamePlaying())
        {

            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _player = _gameManager.GetLocalPlayer();
            if (_player == null)
            {
                // Should not be able to control
                return;
            }
            // Determine if the local player has control
            if (_player.PlayerRole == Player.Role.Floater)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    RewardsManager.Left();
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    RewardsManager.Right();
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    RewardsManager.Select();
                }
            }
        }
    }

    public void SetReward(Reward.RewardType type, string playerId)
    {
        var manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        var player = manager.GetLocalPlayer();

        player.GaveReward();

        switch (type)
        {
            case Reward.RewardType.None:
                break;
            case Reward.RewardType.SpeedBoost:  
                player.AssignSpeedBoost(playerId, _speedBoost);
                break;
            case Reward.RewardType.ReverseControls:
                player.AssignReverseControls(playerId, _controlsModifier);
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }

}
