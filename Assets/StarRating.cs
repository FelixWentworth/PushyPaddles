using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarRating : MonoBehaviour
{
    private float _timePerStar = 0.3f;

    [SerializeField] private List<RectTransform> _stars;
    [SerializeField] private RectTransform _flare;

    void Start()
    {
        Setup();
    }

    void Setup() {
        foreach (var star in _stars)
        {
            star.localScale = Vector3.zero;
        }
        _flare.localScale = Vector3.zero;
    }
	// Update is called once per frame
	public void ShowStarRating(int rating) {
		Setup();
	    StartCoroutine(ShowStars(rating));
	}

    private IEnumerator ShowStars(int number)
    {
        yield return new WaitForSeconds(0.5f);
        for (var i = 0; i < number; i++)
        {
            var time = 0f;
            if (i == 2)
            {
                StartCoroutine(ShowFlare());
            }
            while (time <= _timePerStar)
            {
                _stars[i].localScale = Vector3.Slerp(Vector3.zero, Vector3.one, time/_timePerStar);
               
                time += Time.deltaTime;
                yield return null;
            }
            _stars[i].localScale = Vector3.one;
        }
    }

    private IEnumerator ShowFlare()
    {
        yield return  new WaitForSeconds(0.1f);
        var time = 0f;
        var timeToShow = _timePerStar * .6f;
        while (time <= timeToShow)
        {
            _flare.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, time / timeToShow);
            time += Time.deltaTime;
            yield return null;
        }
        _flare.localScale = Vector3.one;
    }
}
