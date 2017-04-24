using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsManager : MonoBehaviour
{

    public Reward[] Rewards;

    private int _currentlyHighlighting = 0;
    private int _assignedRewards = 0;

    public void ResetRewards()
    {
        _assignedRewards = 0;
        _currentlyHighlighting = 0;
        foreach (var reward in Rewards)
        {
            reward.SetAvailable(true);
        }
        UpdateHighlighted();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Left();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Right();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Select();
        }
    }

    public void Left()
    {
        if (_assignedRewards >= Rewards.Length)
        {
            return;
        }
        _currentlyHighlighting = _currentlyHighlighting == 0 ? Rewards.Length - 1 : _currentlyHighlighting-1;

        if (!Rewards[_currentlyHighlighting].IsAvailable)
        {
            Left();
            return;
        }
        UpdateHighlighted();

    }
    public void Right()
    {
        if (_assignedRewards >= Rewards.Length)
        {
            return;
        }
        _currentlyHighlighting = _currentlyHighlighting == Rewards.Length-1 ? 0 : _currentlyHighlighting+1;

        if (!Rewards[_currentlyHighlighting].IsAvailable)
        {
            Right();
            return;
        }
        UpdateHighlighted();
    }

    public void Select()
    {
        var type = Rewards[_currentlyHighlighting].Type;
        var assignedPlayer = _assignedRewards;
        RewardSelected(type, assignedPlayer);

        Rewards[_currentlyHighlighting].SetAvailable(false);
        _assignedRewards++;
        if (_assignedRewards >= Rewards.Length)
        {
            // TODO Restart Level
            Complete();
        }
        else
        {
            Right();
        }
    }

    private void UpdateHighlighted()
    {
        foreach (var reward in Rewards)
        {
            reward.SetHighlight(false);
        }
        Rewards[_currentlyHighlighting].SetHighlight(true);
        
        // TODO Set to correct user name
        Rewards[_currentlyHighlighting].SetName("Player " + (_assignedRewards+1));
    }

    public void RewardSelected(Reward.RewardType type, int playerIndex)
    {
        transform.parent.GetComponent<RewardScreenManager>().SetReward(type, playerIndex);
    }

    private void Complete()
    {
        transform.parent.GetComponent<RewardScreenManager>().Hide();
        GameManager.RestartGame();
    }
}
