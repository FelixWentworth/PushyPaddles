using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayGen.Unity.Utilities.Localization;
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

    private int _rewardsRemaining;

    private GameObject _rewardObjectInScene;

    private int _playerCount;
    private List<RewardScreenManager.RewardIcons> _rewardData = new List<RewardScreenManager.RewardIcons>();

    public void ShowReward(int playerCount, List<string> ids, List<RewardScreenManager.RewardIcons> rewards,
        int rewardsToGive)
    {
        _rewardsRemaining = rewardsToGive;//
        // Check if we only care about positive rewards
        if (PSL_GameConfig.RewardType == "Positive")
        {
            rewards = rewards.Where(r => r.Positive).ToList();
        }
		// check if in single player
	    if (SP_Manager.Instance.IsSinglePlayer())
	    {
		    rewards = rewards.Where(r => r.SupportsSinglePlayer).ToList();
	    }

	    var rand = UnityEngine.Random.Range(0, rewards.Count);
        var rewardData = rewards[rand];

        if (_rewardObjectInScene != null)
        {
            Destroy(_rewardObjectInScene);
        }

        _type = rewardData.Type;
        _currentlyHighlighting = 0;
        _playerIds = ids;
        _currentId = 0;
        _rewardData = rewards;

        UpdateHighlighted();
        _playerCount = playerCount;

        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        _rewardObjectInScene = Instantiate(_rewardObject, _chest.transform.position, Quaternion.identity);
        _rewardObjectInScene.GetComponent<RewardSetup>().Setup(Localization.Get(rewardData.LocalizationKey), rewardData.Icon);

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
        RewardSelected(_type, _playerIds[_currentlyHighlighting]);

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
        transform.GetComponentInParent<RewardScreenManager>().SetReward(type, playerId);
    }

    private void Complete()
    {
        _rewardsRemaining -= 1;


        if (_rewardsRemaining <= 0)
        {
            // Notify the server that all rewards are given out
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                SP_Manager.Instance.Get<SP_GameManager>().NextRound();
            }
            else
            {
                var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

                gameManager.GetLocalPlayer().NextRound();
            }
        }
        else
        {
            ShowReward(3, _playerIds, _rewardData, _rewardsRemaining);
        }

    }
}
