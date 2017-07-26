using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RewardsManager : MonoBehaviour
{
    private RewardScreenManager.RewardType _type;

    [SerializeField] private GameObject _chest;
    [SerializeField] private GameObject _rewardObject;

    public Reward[] Rewards;

    private int _currentlyHighlighting = 0;
    private List<string> _playerIds;
    private int _currentId;
    
    private int _playerCount;
    
    public void ResetRewards(int playerCount, List<string> ids, RewardScreenManager.RewardType type)
    {
        _type = type;
        _currentlyHighlighting = 0;
        _playerIds = ids;
        _currentId = 0;

        UpdateHighlighted();
        _playerCount = playerCount;

        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        var go = Instantiate(_rewardObject, _chest.transform.position, Quaternion.identity);
        go.GetComponentInChildren<TextMesh>().text = EnumToString(type.ToString());

        var i = 0;
        foreach (var reward in Rewards)
        {
            var playerName = gameManager.GetPlayer(ids[i]).SyncNickName;
            reward.SetName(playerName);
            i++;
        }
    }

    private string EnumToString(string enumValue)
    {
        var charArray = enumValue.ToCharArray();
        var newWord = "";
        for (var i = 0; i < charArray.Length; i++)
        {
            if (i > 0 && char.IsUpper(charArray[i]))
            {
                newWord += ' ';
            }
            newWord += charArray[i];
        }

        return newWord;
    }

    public void Left()
    {
        _currentlyHighlighting = _currentlyHighlighting == 0 ? Rewards.Length - 1 : _currentlyHighlighting-1;

        UpdateHighlighted();

    }
    public void Right()
    {
        _currentlyHighlighting = _currentlyHighlighting == Rewards.Length-1 ? 0 : _currentlyHighlighting+1;

        UpdateHighlighted();
    }

    public void Select()
    {
        RewardSelected(_type, _playerIds[_currentId]);

        Complete();
    }

    private void UpdateHighlighted()
    {
        foreach (var reward in Rewards)
        {
            reward.SetHighlight(false);
        }
        Rewards[_currentlyHighlighting].SetHighlight(true);
    }

    public void RewardSelected(RewardScreenManager.RewardType type, string playerId)
    {
        transform.parent.GetComponent<RewardScreenManager>().SetReward(type, playerId);
    }

    private void Complete()
    {
        // Notify the server that all rewards are given out
        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        gameManager.GetLocalPlayer().NextRound();
        gameManager.HideRewards();

    }
}
