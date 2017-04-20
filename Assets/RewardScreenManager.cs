using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardScreenManager : MonoBehaviour
{
    public AirConsoleManager AirConsoleManager;
    public RewardsManager RewardsManager;
    private CanvasGroup _canvasGroup;

    public bool IsShowing = false;

    public void Show()
    {

        if (_canvasGroup == null)
        {
            _canvasGroup = this.GetComponent<CanvasGroup>();
        }
        RewardsManager.ResetRewards();
        _canvasGroup.alpha = 1f;
        IsShowing = true;
    }

    public void Hide()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = this.GetComponent<CanvasGroup>();
        }
        _canvasGroup.alpha = 0f;
        IsShowing = false;
    }

    public void SetReward(Reward.RewardType type, int playerIndex)
    {
        switch (type)
        {
            case Reward.RewardType.None:
                break;
            case Reward.RewardType.SpeedBoost:
                AirConsoleManager.Players[playerIndex].SpeedModifier += 0.5f;
                break;
            case Reward.RewardType.ReverseControls:
                AirConsoleManager.Players[playerIndex].DirectionModifier *= -1;
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }

}
