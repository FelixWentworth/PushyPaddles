using System;
using System.Linq;
using UnityEngine;

public class Curriculum : MonoBehaviour {

    private enum Subject
    {
        Maths
    }

    [SerializeField] private Subject _subject;

    [SerializeField] private string _fileName;

    [SerializeField] private string _descriptionFileName;

    private CurriculumChallenges _challenges;
    private CurriculumDescriptions _descriptions;

    private string _levelIndex;

    public CurriculumChallenge GetNewChallenge(string year, string lesson)
    {
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
        PSL_LRSManager.Instance.SetNumRounds(challenges.Count);

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

        return challenge;

    }

    /// <summary>
    /// Returns an array of challenges that are available for a given year
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public CurriculumChallenge[] GetChallengesForYear(string year)
    {
        if (_challenges == null || _challenges.MathsProblems.Length == 0)
        {
            GetChallengeData();
        }

        if (_challenges.MathsProblems.Length == 0)
        {
            return null;
        }

        var challenges = _challenges.MathsProblems.Where(c => c.Year == year).ToArray();

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



    private void GetChallengeData()
    {
        var data = Resources.Load<TextAsset>(_fileName);
        if (data == null)
        {
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
