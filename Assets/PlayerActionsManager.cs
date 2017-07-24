using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Microsoft.SqlServer.Server;
using PlayGen.Unity.AsyncUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerActionsManager : MonoBehaviour
{
    public enum GameAction
    {
        None = 0,

        Pushed,
        HitObstacle,
        ReachedChest,
        ReachedChestSuccess,
        ReachedChestFail,
        PickedUpPlatform,
        PlacedPlatform,
        IdleHoldingPlatform,
        SetReward
    }

    [Serializable]
    private class PlayerActions
    {
        public string PlayerId;
        public GameAction Action;
        public DateTime TimeStamp;
    }

    [Serializable]
    private class Criteria
    {
        public string Name;

        [Tooltip("Skill being shown")] public PSL_Verbs Skill;
        [Tooltip("Is the skill shown positive")] public bool Positive;
        [Tooltip("Action that must be met for criteria to be met")] public GameAction TriggerAction;
        [Tooltip("Action to check against for criteria to be met")] public GameAction PreviousAction;
        [Tooltip("Compare action between different players?")] public bool MultiplePlayers;
        [Tooltip("Time interval between actions, none = <0")] public float Interval;
    }

    [SerializeField] private List<Criteria> _skillCriteria;

    private List<string> _players = new List<string>();
    private List<PlayerActions> _playerActions = new List<PlayerActions>();

    private float _largestInterval
    {
        get { return _skillCriteria.Max(c => c.Interval); }
    }

    public void PerformedAction(GameAction action, string playerId)
    {
        Log(string.Format("Player: {0}, performed action: {1}", playerId, action));

        _playerActions.Add(new PlayerActions()
        {
            Action = action,
            PlayerId = playerId,
            TimeStamp = DateTime.Now
        });

        // remove actions older than largest interval
        if (_largestInterval > 0)
        { 
            _playerActions.RemoveAll(a => (DateTime.Now - a.TimeStamp).Seconds > _largestInterval);
        }

        // only check actions that trigger action meets this action
        var skillsToCheck = _skillCriteria.Where(c => c.TriggerAction == action);

        foreach (var criteria in skillsToCheck)
        {
            var actionsMeetingCriteria = _playerActions;

            // Check if criteria requires a previous action to be met as well
            actionsMeetingCriteria = criteria.PreviousAction != GameAction.None
                ? actionsMeetingCriteria.Where(a => a.Action != criteria.PreviousAction).ToList()
                : actionsMeetingCriteria;
            
            // Check is within interval or interval not important
            actionsMeetingCriteria = criteria.Interval > 0
               ? actionsMeetingCriteria.Where(a => (DateTime.Now - a.TimeStamp).Seconds <= criteria.Interval).ToList()
               : actionsMeetingCriteria;

            // check if the criteria requires multiple players interacting or from same player
            actionsMeetingCriteria = !criteria.MultiplePlayers 
                ? actionsMeetingCriteria.Where(a => a.PlayerId == playerId).ToList() 
                : actionsMeetingCriteria.Where(a => a.PlayerId != playerId).ToList();
            
            if (actionsMeetingCriteria.Count != 0)
            {
                RecordSkillValue(criteria.Skill, playerId, criteria.Positive);
            }
        }

    }

    private void RecordSkillValue(PSL_Verbs verb, string playerId, bool positiveSkill)
    {
        var increment = positiveSkill ? 1 : -1;
        PSL_LRSManager.Instance.PlayerShowedSkill(playerId, verb, increment);
    }
    
    private static void Log(string message)
    {
        LogProxy.Info(message);
    }
}
