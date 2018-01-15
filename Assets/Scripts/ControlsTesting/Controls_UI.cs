using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controls_UI : Controls
{
    public GameObject LeftRight;
    public GameObject UpDown;
    public GameObject Interact;
    private Player _player;

    private int x;
    private int z;

    private Animation _leftRightAnimation;
    private Animation _upDownAnimation;
    private Animation _interactAnimation;

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public void LeftPressed()
    {
        x = -1;
    }

    public void RightPressed()
    {
        x = 1;
    }

    public void UpPressed()
    {
        z = 1;
    }

    public void DownPressed()
    {
        z = -1;
    }

    void Update()
    {
        if (x != 0)
        {
            if (x == -1)
                MoveLeft();
            else
                MoveRight();
        }
        if (z != 0)
        {
            if (z == -1)
                MoveDown();
            else
                MoveUp();
        }

        if (_player != null)
        {
            LeftRight.SetActive(_player.PlayerRole == Player.Role.Floater);
            UpDown.SetActive(_player.PlayerRole == Player.Role.Paddler);
        }
    }

    public void InteractPressed()
    {
        Interact();
    }

    public void Stop()
    {
        x = 0;
        z = 0;
        StopMoving();
    }

    public void AnimateControls(bool animate)   
    {
        GetAnimations();
        if (animate)
        {
            // We can play both as only 1 will be active
            _leftRightAnimation.Play();
            _upDownAnimation.Play();
        }
        else
        {
            _leftRightAnimation[_leftRightAnimation.clip.name].time = _leftRightAnimation.clip.length;
            _upDownAnimation[_upDownAnimation.clip.name].time = _upDownAnimation.clip.length;
            _leftRightAnimation.Stop();
            _upDownAnimation.Stop();
        }
    }

    public void AnimateInteract(bool animate)
    {
        GetAnimations();
        if (animate)
        {
            _interactAnimation.Play();
        }
        else
        {
            _interactAnimation[_interactAnimation.clip.name].time = _interactAnimation.clip.length;
            _interactAnimation.Stop();
        }
    }

    private void GetAnimations()
    {
        if (_interactAnimation == null || _leftRightAnimation == null || _upDownAnimation == null)
        {
            _interactAnimation = Interact.GetComponent<Animation>();
            _leftRightAnimation = LeftRight.GetComponent<Animation>();
            _upDownAnimation = UpDown.GetComponent<Animation>();
        }
    }
}
