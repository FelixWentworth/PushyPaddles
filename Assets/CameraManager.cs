using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject _chest;
    private bool _end;
    private Camera _camera;
    private Animation _animation;

    void Awake()
    {
        _camera = Camera.main;
        _animation = _camera.GetComponent<Animation>();
        SetChestDefault();
    }

    private void SetChestDefault()
    {
        var anim = _chest.GetComponent<Animation>();

        anim[anim.clip.name].time = 0f;
        anim[anim.clip.name].speed = 0f;

        anim.Play();
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.C))
    //    {
    //        if (_end)
    //        {
    //            StartCoroutine(TransitionToStart());
    //        }
    //        else
    //        {
    //            StartCoroutine(TransitionToEnd());
    //        }
    //        _end = !_end;
    //    }
    //}

    public IEnumerator TransitionToEnd()
    {
        yield return StartCoroutine(PlayAnimation(true));
    }

    public IEnumerator TransitionToStart()
    {
        yield return StartCoroutine(PlayAnimation(false));
    }

    private IEnumerator PlayAnimation (bool forward, string rewardName = "")
    {

        while (_animation.isPlaying)
        {
            yield return null;
        }

        if (!forward)
        {
            SetChestDefault();
        }

        _animation[_animation.clip.name].time = forward ? 0f : 1f;
        _animation[_animation.clip.name].speed = forward ? 1f : -1f;

        _animation.Play();
        while (_animation.isPlaying)
        {
            yield return null;
        }

        if (forward)
        {
            yield return StartCoroutine(OpenChest(rewardName));
        }
        
    }

    private IEnumerator OpenChest(string rewardName)
    {
        var anim = _chest.GetComponent<Animation>();
        if (!anim.isPlaying || anim[anim.clip.name].speed == 0f)
        {
            anim[anim.clip.name].time = 0f;
            anim[anim.clip.name].speed = 1f;

            anim.Play();
            
            yield return new WaitForSeconds(anim.clip.length * 0.5f);
        }

    }
        
}
