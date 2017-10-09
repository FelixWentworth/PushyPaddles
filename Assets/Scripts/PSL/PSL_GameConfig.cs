using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class PSL_GameConfig : MonoBehaviour
{

    /// <summary>
    /// Values that should be set through PSL that determine how the game should be player
    /// </summary>

    public static string Level;
    public static string LessonNumber;

    // "Maths", "Obstacle"
    public static string GameType;
    // "Positive", "All"
    public static string RewardType;

    public static bool LessonSelectionRequired;

    public Curriculum Curriculum;

    void Awake()
    {
        // Set lesson selection required as no lesson has beem set yet
        LessonSelectionRequired = true;
    }

    public static int GetLessonCountForScenario(string year)
    {   
        return Curriculum.GetChallengesForYear(year).Length;
    }

    public static void SetGameConfig(string level, string lesson, string gameType, string rewardType)
    {
        Debug.Log(string.Format("Setting game config, {0} {1} {2} {3}", level, lesson, gameType, rewardType));
        Level = level;
        LessonNumber = lesson;
        GameType = gameType;
        RewardType = rewardType;

        LessonSelectionRequired = false;
    } 
}
