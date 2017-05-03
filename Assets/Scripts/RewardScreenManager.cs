using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardScreenManager : UIScreen
{
    public RewardsManager RewardsManager;
    private float _speedBoost = 0.05f;
    private float _controlsModifier = -1f;

    private GameManager _gameManager;

    public override void Show()
    {
        base.Show();
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        RewardsManager.ResetRewards(_gameManager.GetPlayerCount());
    }

    public override void Hide()
    {
        base.Hide();
    }

    void Update()
    {
        if (IsShowing)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            // Determine if the local player has control
            if (_gameManager.GetLocalPlayer().PlayerRole == Player.Role.Floater)
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

    public void SetReward(Reward.RewardType type, int playerIndex)
    {
        var manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        switch (type)
        {
            case Reward.RewardType.None:
                break;
            case Reward.RewardType.SpeedBoost:
                manager.CmdAssignSpeedBoost(playerIndex, _speedBoost);
                break;
            case Reward.RewardType.ReverseControls:
                manager.CmdAssignReverseControls(playerIndex, _controlsModifier);
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }

}
