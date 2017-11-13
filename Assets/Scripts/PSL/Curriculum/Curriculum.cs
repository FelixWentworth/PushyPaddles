using System;
using System.Linq;
using UnityEngine;

public class Curriculum : MonoBehaviour {

    private enum Subject
    {
        Maths
    }

    private static Subject _subject = Subject.Maths;

    private static string _fileName = "Content";

    private static string _descriptionFileName = "ContentDescriptions";

    private static CurriculumChallenges _challenges;
    private static CurriculumDescriptions _descriptions;

    private static CurriculumChallenge _currentChallenge;

    private static string _levelIndex;

    public static CurriculumChallenge GetNewChallenge(string year, string lesson)
    {
        year = year.Substring(5, year.Length - 5);

        if (_challenges == null || _challenges.MathsProblems.Length == 0)
        {
            GetChallengeData();
        }

        if (_challenges.MathsProblems.Length == 0)
        {
            return null;
        }

        var challenges = _challenges.MathsProblems.Where(c => c.Year == year && c.Lesson == lesson).ToList();
        _levelIndex = challenges[0].Level;
#if USE_PROSOCIAL
        PSL_LRSManager.Instance.SetNumRounds(challenges.Count);
#endif
        _currentChallenge = challenges[0];
        return challenges[0];
    }

    /// <summary>
    /// Return the next challenge for the key stage and lesson
    /// </summary>
    /// <param name="year">Target key stage</param>
    /// <param name="lesson">Target lesson number</param>
    /// <returns>Next challenge, or null if reached end</returns>
    public CurriculumChallenge GetNextChallenge(string year, string lesson)
    {
        year = year.Substring(5, year.Length - 5);

        if (_challenges == null || _challenges.MathsProblems.Length == 0)
        {
            GetChallengeData();
        }

        if (_challenges.MathsProblems.Length == 0)
        {
            return null;
        }

        _levelIndex = (Convert.ToInt16(_levelIndex) + 1).ToString();

        var challenge = _challenges.MathsProblems.FirstOrDefault(c => c.Year == year && c.Lesson == lesson && c.Level == _levelIndex);
        _currentChallenge = challenge;
        return challenge;

    }

    public void ResetLevel()
    {
        _levelIndex = "0";
    }

    /// <summary>
    /// Return the currnt challenge for the key stage and lesson
    /// </summary>
    /// <param name="year">Target key stage</param>
    /// <param name="lesson">Target lesson number</param>
    /// <returns>Next challenge, or null if reached end</returns>
    public CurriculumChallenge GetCurrentChallenge()
    {
        return _currentChallenge;
    }

    /// <summary>
    /// Returns an array of challenges that are available for a given year
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static CurriculumChallenge[] GetChallengesForYear(string year)
    {

        if (_challenges == null || _challenges.MathsProblems.Length == 0)
        {
            GetChallengeData();
        }
        if (_challenges.MathsProblems.Length == 0)
        {
            return null;
        }

        var challenges = _challenges.MathsProblems.Where(c => c.Year == year && c.Level == "1").ToArray();
        return challenges;
    }

    /// <summary>
    /// Returns the set description for a lesson
    /// </summary>
    /// <param name="lesson">the number of the lesson</param>
    /// <returns></returns>
    public CurriculumDescription GetDescriptionForLesson(string lesson)
    {
        if (_descriptions == null || _descriptions.LevelDescriptions.Length == 0)
        {
            GetChallengeDescriptions();
        }

        if (_descriptions.LevelDescriptions.Length == 0)
        {
            return null;
        }

        var description = _descriptions.LevelDescriptions.FirstOrDefault(d => d.Lesson == lesson);

        return description;
    }



    private static void GetChallengeData()
    {

        var data = Resources.Load<TextAsset>(_fileName);
        if (data == null)
        {
            Debug.Log("no Data");
            return;
        }

        
        _challenges = JsonUtility.FromJson<CurriculumChallenges>(data.text);
    }

    private void GetChallengeDescriptions()
    {
        var data = Resources.Load<TextAsset>(_descriptionFileName);
        if (data == null)
        {
            return;
        }

        _descriptions = JsonUtility.FromJson<CurriculumDescriptions>(data.text);
    }

}
