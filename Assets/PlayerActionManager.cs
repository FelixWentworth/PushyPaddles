using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Microsoft.SqlServer.Server;
#if PSL_ENABLED
using PlayGen.Orchestrator.PSL.Common.LRS;
using PlayGen.Unity.AsyncUtilities;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerActionManager : MonoBehaviour
{
#if PSL_ENABLED

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

        [Tooltip("Skill being shown")] public LRSSkillVerb Skill;
        [Tooltip("Is the skill shown positive")] public bool Positive;
        [Tooltip("Action that must be met for criteria to be met")] public PlayerAction TriggerAction;
        [Tooltip("Action to check against for criteria to be met")] public PlayerAction PreviousAction;
        [Tooltip("Compare action between different players?")] public bool MultiplePlayers;
        [Tooltip("Time interval between actions, none = <0")] public float Interval;
    }
	[Tooltip("Set in code to ensure values are kept when changing platform")]
    [SerializeField] private List<Criteria> _skillCriteria = new List<Criteria>
    {
		#region seed
		new Criteria()
	    {
		    Name = "Pushed against another player",
			Skill = LRSSkillVerb.Cooperated,
			Positive = false,
			TriggerAction = PlayerAction.Pushed,
			PreviousAction = PlayerAction.Pushed,
			MultiplePlayers = true,
			Interval = 0.5f
	    },
	    new Criteria()
	    {
		    Name = "Reached Chest",
		    Skill = LRSSkillVerb.Cooperated,
		    Positive = true,
		    TriggerAction = PlayerAction.ReachedChest,
		    PreviousAction = PlayerAction.None,
		    MultiplePlayers = false,
		    Interval = 0f
	    },
	    new Criteria()
	    {
		    Name = "Pushed into Obstacle",
		    Skill = LRSSkillVerb.Cooperated,
		    Positive = false,
		    TriggerAction = PlayerAction.HitObstacle,
		    PreviousAction = PlayerAction.Pushed,
		    MultiplePlayers = true,
		    Interval = 1f
	    },
	    new Criteria()
	    {
		    Name = "Pushed into collectible",
		    Skill = LRSSkillVerb.Cooperated,
		    Positive = true,
		    TriggerAction = PlayerAction.GotCollectible,
		    PreviousAction = PlayerAction.Pushed,
		    MultiplePlayers = true,
		    Interval = 1f
	    },
	    new Criteria()
	    {
		    Name = "Gave reward self",
		    Skill = LRSSkillVerb.Shared,
		    Positive = false,
		    TriggerAction = PlayerAction.GaveRewardSelf,
		    PreviousAction = PlayerAction.None,
		    MultiplePlayers = false,
		    Interval = 0f
	    },
	    new Criteria()
	    {
		    Name = "Gave reward other",
		    Skill = LRSSkillVerb.Shared,
		    Positive = true,
		    TriggerAction = PlayerAction.GaveRewardOther,
		    PreviousAction = PlayerAction.None,
		    MultiplePlayers = false,
		    Interval = 0f
	    },
	    new Criteria()
	    {
		    Name = "Reached Chest Correct",
		    Skill = LRSSkillVerb.SolvedAsGroup,
		    Positive = true,
		    TriggerAction = PlayerAction.ReachedChestSuccess,
		    PreviousAction = PlayerAction.None,
		    MultiplePlayers = false,
		    Interval = 0f
	    },
	    new Criteria()
	    {
		    Name = "Reached Chest Wrong",
		    Skill = LRSSkillVerb.SolvedAsGroup,
		    Positive = false,
		    TriggerAction = PlayerAction.ReachedChestFail,
		    PreviousAction = PlayerAction.None,
		    MultiplePlayers = false,
		    Interval = 0f
	    },
	    new Criteria()
	    {
		    Name = "Idle",
		    Skill = LRSSkillVerb.TookTurns,
		    Positive = false,
		    TriggerAction = PlayerAction.Idle,
		    PreviousAction = PlayerAction.PickedUpPlatform,
		    MultiplePlayers = false,
		    Interval = 5f
	    },
	    new Criteria()
	    {
		    Name = "Took Turns Positive",
		    Skill = LRSSkillVerb.TookTurns,
		    Positive = true,
		    TriggerAction = PlayerAction.ReachedChest,
		    PreviousAction = PlayerAction.None,
		    MultiplePlayers = false,
		    Interval = 0f
	    }

#endregion
	};

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

    private void RecordSkillValue(LRSSkillVerb verb, string playerId, bool positiveSkill)
    {
        Debug.Log("Player " + playerId + " showed Skill: " + verb);
        var increment = positiveSkill ? 1 : -1;
        PSL_LRSManager.Instance.PlayerShowedSkill(playerId, verb, increment);
    }
    
    private static void Log(string message)
    {
        //LogProxy.Info(message);
    }

#endif
}
