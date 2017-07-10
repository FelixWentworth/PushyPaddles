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

    public CurriculumChallenge GetNewChallenge(int level)
    {
        if (_challenges == null || _challenges.Items.Length == 0)
        {
            GetChallengeData();
        }

        if (_challenges.Items.Length == 0)
        {
            return null;
        }

        var challenges = _challenges.Items.Where(c => c.Level == level).ToList();
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
