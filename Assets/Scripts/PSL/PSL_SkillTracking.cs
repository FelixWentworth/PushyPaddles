using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PSL_SkillTracking : MonoBehaviour {

    // This class will contain a list of players and their current skills, ready for sending at the end of the game. Skills will be incremented and normalised as appropriate
    // Eg. if a player is cooperative, cooperative +1 then the most cooperative score at the end will be closest to 1

    public struct PlayerSkill
    {
        public string Id;
        public PSL_Verbs Skill;
    }

    private readonly Dictionary<PlayerSkill, int> _playerSkills = new Dictionary<PlayerSkill, int>();

    private readonly Dictionary<PSL_Verbs, int> _verbCount = new Dictionary<PSL_Verbs, int>();

    public void AddSkill(string playerId, PSL_Verbs skill, int increment)
    {
        var playerSkill = new PlayerSkill()
        {
            Id = playerId,
            Skill = skill
        };

        if (_playerSkills.ContainsKey(playerSkill))
        {
            _playerSkills[playerSkill] += increment;
            if (_playerSkills[playerSkill] <= 0)
            {
                _playerSkills[playerSkill] = 0;
            }

            
        }
        else
        {
            _playerSkills.Add(playerSkill, increment);

        }

        if (_verbCount.ContainsKey(playerSkill.Skill))
        {
            // incrememnt the total possible
            _verbCount[playerSkill.Skill] += 1;
        }
        else
        {
            _verbCount.Add(playerSkill.Skill, 1);
        }
    }

    public void SendSkillData()
    {
        foreach (var playerSkill in _playerSkills)
        {
            var id = playerSkill.Key.Id;
            var skill = playerSkill.Key.Skill;

            var value = GetNormalizedValue(playerSkill.Key);

            // TODO send data to PLS
            
        }   
    }

    private float GetNormalizedValue(PlayerSkill playerSkill)
    {
        var playersSkillValues = _playerSkills.Where(p => p.Key.Skill == playerSkill.Skill )
            .Select(p => p.Value)
            .ToList();

        var totalValue = _verbCount.First(c => c.Key == playerSkill.Skill).Value;

        var normalized = GetNormalized(_playerSkills[playerSkill], totalValue, 0, playerSkill.Skill.GetMinRange(), 1);

        return normalized;
    }

    private float GetNormalized(float value, float totalValue, float minValue, float rangeMin, float rangeMax)
    {
        // Normalised Formula
        // Normalised value between a and b
        //        
        //                       x - min x
        //     n = (b - a) ------------------- + a
        //                     max x - min x
        // 

        return (rangeMax - rangeMin) * (((value - minValue) / (totalValue - minValue)) + rangeMin);
    }
}
