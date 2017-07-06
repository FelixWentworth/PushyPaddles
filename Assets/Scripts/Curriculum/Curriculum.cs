using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

    private List<CurriculumChallenge> _challenges = new List<CurriculumChallenge>();

    public CurriculumChallenge GetNewChallenge(int level)
    {
        if (_challenges.Count == 0)
        {
            GetChallengeData();
        }

        if (_challenges.Count == 0)
        {
            return null;
        }

        var challenges = _challenges.Where(c => c.Level == level).ToList();
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

        _challenges = JsonConvert.DeserializeObject<List<CurriculumChallenge>>(data.text);
    }

}
