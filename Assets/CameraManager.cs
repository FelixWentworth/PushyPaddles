using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_end)
            {
                TransitionToStart();
            }
            else
            {
                TransitionToEnd();
            }
            _end = !_end;
        }
    }

    public void TransitionToEnd()
    {
        StartCoroutine(PlayAnimation(true));
    }

    public void TransitionToStart()
    {
        StartCoroutine(PlayAnimation(false));
    }

    private IEnumerator PlayAnimation (bool forward)
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
            OpenChest();
        }
        
    }

    private void OpenChest()
    {
        var anim = _chest.GetComponent<Animation>();
        if (!anim.isPlaying || anim[anim.clip.name].speed == 0f)
        {
            anim[anim.clip.name].time = 0f;
            anim[anim.clip.name].speed = 1f;

            anim.Play();
        }
    }
        
}
