using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSL_GameConfig : MonoBehaviour
{

    /// <summary>
    /// Values that should be set through PSL that determine how the game should be player
    /// </summary>
    public static PSL_GameConfig Instance;


    public string Level { get; private set; }
    public string LessonNumber { get; private set; }

    // "Maths", "Obstacle"
    public string GameType { get; private set; }
    // "Positive", "All"
    public string RewardType { get; private set; }

    void Awake()
    {
        if (PlatformSelection.ConnectionType == ConnectionType.Testing)
        {
            // Set to default setup
            SetGameConfig("1", "2", "Maths", "All");
        }
        else
        {
            // TODO Get from config
            SetGameConfig("1", "2", "Maths", "All");
        }

        Instance = this;
    }

    public void SetGameConfig(string level, string lesson, string gameType, string rewardType)
    {
        Level = level;
        LessonNumber = lesson;
        GameType = gameType;
        RewardType = rewardType;
    }
}
