using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SP_Levels : MonoBehaviour
{
    // Save and handle level progress

    public void SaveRating(int rating)
    {
        var year = SP_Manager.Instance.Get<SP_GameManager>().GetYear();
        var lesson = SP_Manager.Instance.Get<SP_GameManager>().GetLesson();

        var key = year + ":" + lesson;

        // Dont override with worse rating
        var previous = PlayerPrefs.GetInt(key);
        if (rating > previous)
        {
            PlayerPrefs.SetInt(key, rating);
        }
    }

    public int GetRating(string year, string lesson)
    {
        if (!year.Contains("year"))
        {
            year = "Year " + year;
        }
        var key = year + ":" + lesson;
        return PlayerPrefs.GetInt(key);
    }
}
