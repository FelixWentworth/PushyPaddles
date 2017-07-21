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
        var rand = Random.Range(0, challenges.Count());

        return challenges[rand];
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
