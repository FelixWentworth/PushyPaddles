using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PSL_SkillTracking : MonoBehaviour {

    // This class will contain a list of players and their current skills, ready for sending at the end of the game. Skills will be incremented and normalised as appropriate
    // Eg. if a player is cooperative, cooperative +1 then the most cooperative score at the end will be closest to 1

    public class PlayerSkill
    {
        public string Id;
        public PSL_Verbs Skill;

        public override bool Equals(object other)
        {
            var otherSkill = other as PlayerSkill;
            if (otherSkill == null)
                return false;
            return Id == otherSkill.Id && Skill == otherSkill.Skill;
        }
    }

    private Dictionary<PlayerSkill, int> _playerSkills = new Dictionary<PlayerSkill, int>();

    private Dictionary<PlayerSkill, int> _verbCount = new Dictionary<PlayerSkill, int>();

    public void AddSkill(string playerId, PSL_Verbs skill, int increment)
    {
        var playerSkill = new PlayerSkill()
        {
            Id = playerId,
            Skill = skill
        };

        var key = _playerSkills.Keys.FirstOrDefault(p => p.Equals(playerSkill));

        if (key != null)
        {
            _playerSkills[key] += increment;
            if (_playerSkills[key] <= 0)
            {
                _playerSkills[key] = 0;
            }
        }
        else
        {
            _playerSkills.Add(playerSkill, increment);

        }

        key = _verbCount.Keys.FirstOrDefault(p => p.Equals(playerSkill));

        if (key != null)
        {
            // incrememnt the total possible
            _verbCount[key] += 1;
        }
        else
        {
            _verbCount.Add(playerSkill, 1);
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

    public string OutputSkillData()
    {
        var data = "";

        foreach (var playerSkill in _playerSkills)
        {
            var id = playerSkill.Key.Id;
            var skill = playerSkill.Key.Skill;

            var value = GetNormalizedValue(playerSkill.Key);

            data += string.Format("\nPlayer: {0}, showed skill: {1}, with value: {2}", id, skill, value);
        }
        return data;
    }

    private float GetNormalizedValue(PlayerSkill playerSkill)
    {
        var playersSkillValues = _playerSkills.Where(p => p.Key.Skill == playerSkill.Skill )
            .Select(p => p.Value)
            .ToList();

        var totalValue = _verbCount[playerSkill];

        var normalized = GetNormalized(_playerSkills[playerSkill], totalValue, 0, playerSkill.Skill.GetMinRange(), 1);

        return normalized;
    }

    private float GetNormalized(float value, float totalValue, float minValue, float rangeMin, float rangeMax)
    {
        // Normalised Formula
        // Normalised value between a and b
        //        
        //                       x - min x
        //     n = (b - a) -------------------- + a
        //                     max x - min x
        // 

        return (rangeMax - rangeMin) * ((value - minValue) / (totalValue - minValue)) + rangeMin;
    }
}
