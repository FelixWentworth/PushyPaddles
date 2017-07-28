using System.Linq;
using UnityEngine;

public class Curriculum : MonoBehaviour {

    private enum Subject
    {
        Maths
    }

    [SerializeField]
    private Subject _subject;

    [SerializeField]
    private string _fileName;

    private CurriculumChallenges _challenges;

    private int _levelIndex;

    public CurriculumChallenge GetNewChallenge(int keyStage, int lesson)
    {
        if (_challenges == null || _challenges.MathsProblems.Length == 0)
        {
            GetChallengeData();
        }

        if (_challenges.MathsProblems.Length == 0)
        {
            return null;
        }

        var challenges = _challenges.MathsProblems.Where(c => c.KeyStage == keyStage && c.Lesson == lesson).ToList();

        _levelIndex = challenges[0].Level;
        PSL_LRSManager.Instance.SetNumRounds(challenges.Count);

        return challenges[0];
    }

    /// <summary>
    /// Return the next challenge for the key stage and lesson
    /// </summary>
    /// <param name="keyStage">Target key stage</param>
    /// <param name="lesson">Target lesson number</param>
    /// <returns>Next challenge, or null if reached end</returns>
    public CurriculumChallenge GetNextChallenge(int keyStage, int lesson)
    {
        if (_challenges == null || _challenges.MathsProblems.Length == 0)
        {
            GetChallengeData();
        }

        if (_challenges.MathsProblems.Length == 0)
        {
            return null;
        }

        _levelIndex += 1;
        var challenge = _challenges.MathsProblems.FirstOrDefault(c => c.KeyStage == keyStage && c.Lesson == lesson && c.Level == _levelIndex);

        return challenge;

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

}
