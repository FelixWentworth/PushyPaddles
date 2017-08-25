using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CurriculumLevelList : MonoBehaviour
{

    public GameObject LevelElement;

    [SerializeField] private Text _titleText;

    private ScrollRect _scrollRect;

    private List<GameObject> _generatedObjects = new List<GameObject>();

    private string _year = "1";

    // Use this for initialization
    void Start ()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();

        ShowLevelsForYear(_year);
    }

    public void ShowLevelsForYear(string year)
    {
        // get the array of challenges
        _titleText.text = string.Format(Localization.Get("FORMATTED_UI_YEAR"), _year);

        var curriculum = GameObject.Find("CurriculumManager").GetComponent<Curriculum>();
        var challenges = curriculum.GetChallengesForYear(year);

        var lessons = challenges.Where(c => c.Level == "1");

        foreach (var curriculumChallenge in lessons)
        {
            var description = curriculum.GetDescriptionForLesson(curriculumChallenge.Lesson);
            AddElement(curriculumChallenge.Lesson, description.Description);
        }
    }

    private void AddElement(string lesson, string description)
    {
        var go = Instantiate(LevelElement);

        var lessonText = string.Format(Localization.Get("FORMATTED_UI_LESSON"), lesson);
        lessonText = lessonText.Replace(" ", "\u00a0");

        go.GetComponent<LevelElement>().Setup(lessonText, description);
        go.GetComponent<LevelElement>().Button.onClick.AddListener(delegate { LessonSelected(lesson); });

        go.transform.SetParent(_scrollRect.content);
        _generatedObjects.Add(go);
    }

    private void RemoveElements()
    {
        foreach (var obj in _generatedObjects)
        {
            Destroy(obj);
        }
        _generatedObjects = new List<GameObject>();
    }

    public void Navigate(int increment)
    {
        var tempYear = Convert.ToInt16(_year) + increment;

        // check if there are any challenges for this year
        var curriculum = GameObject.Find("CurriculumManager").GetComponent<Curriculum>();
        var challenges = curriculum.GetChallengesForYear(tempYear.ToString());

        if (challenges == null || challenges.Length == 0)
        {
            return;
        }
        else
        {
            _year = tempYear.ToString();
            _titleText.text = string.Format(Localization.Get("FORMATTED_UI_YEAR"), _year);
            RemoveElements();

        }
        var lessons = challenges.Where(c => c.Level == "1");

        foreach (var curriculumChallenge in lessons)
        {
            var description = curriculum.GetDescriptionForLesson(curriculumChallenge.Lesson);
            AddElement(curriculumChallenge.Lesson, description.Description);
        }
    }

    public void LessonSelected(string lesson)
    {
        if (!ClientScene.ready)
        {
            ClientScene.Ready(NetworkManager.singleton.client.connection);
        }
        GameObject.Find("GameManager").GetComponent<GameManager>().GetLocalPlayer().SetLesson(_year, lesson);
        GameObject.Find("MenuManager").GetComponent<MenuManager>().HideLessonSelect();
    }
}
