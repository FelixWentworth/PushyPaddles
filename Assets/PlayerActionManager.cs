using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Microsoft.SqlServer.Server;
using PlayGen.Unity.AsyncUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerActionManager : MonoBehaviour
{
    

    [Serializable]
    private class PlayerActions
    {
        public string PlayerId;
        public PlayerAction Action;
        public DateTime TimeStamp;
        public bool Removable;
    }

    [Serializable]
    private class Criteria
    {
        public string Name;

        [Tooltip("Skill being shown")] public PSL_Verbs Skill;
        [Tooltip("Is the skill shown positive")] public bool Positive;
        [Tooltip("Action that must be met for criteria to be met")] public PlayerAction TriggerAction;
        [Tooltip("Action to check against for criteria to be met")] public PlayerAction PreviousAction;
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

    public void PerformedAction(PlayerAction action, string playerId, bool removable, PlayerAction removeAction = PlayerAction.None)
    {
        Log(string.Format("Player: {0}, performed action: {1}", playerId, action));

        _playerActions.Add(new PlayerActions()
        {
            Action = action,
            PlayerId = playerId,
            TimeStamp = DateTime.Now,
            Removable = removable
        });

        //// remove actions older than largest interval
        //if (_largestInterval > 0)
        //{ 
        //    _playerActions.RemoveAll(a => (DateTime.Now - a.TimeStamp).Seconds > _largestInterval && a.Removable);
        //}

        if (removeAction != null)
        {
            _playerActions.RemoveAll(a => a.PlayerId == playerId && a.Action == removeAction);
        }

        // only check actions that trigger action meets this action
        var skillsToCheck = _skillCriteria.Where(c => c.TriggerAction == action);

        foreach (var criteria in skillsToCheck)
        {
            var actionsMeetingCriteria = _playerActions;

            // Check if criteria requires a previous action to be met as well
            actionsMeetingCriteria = criteria.PreviousAction != PlayerAction.None
                ? actionsMeetingCriteria.Where(a => a.Action == criteria.PreviousAction).ToList()
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
        Debug.Log("Player " + playerId + " showed Skill: " + verb);
        var increment = positiveSkill ? 1 : -1;
        PSL_LRSManager.Instance.PlayerShowedSkill(playerId, verb, increment);
    }
    
    private static void Log(string message)
    {
        //LogProxy.Info(message);
    }
}
