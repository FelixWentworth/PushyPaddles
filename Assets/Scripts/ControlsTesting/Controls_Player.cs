using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controls_Player : MonoBehaviour
{
    public List<Controls> ControlOptions;
    public Text ControlName;
    private int _index;

    private float _x;
    private float _z;

    private enum AnimationState
    {
        IDLE = 0,
        WALKING,
    }

    private AnimationState _animationState;

    // Use this for initialization
    void Start ()
	{
	    foreach (var option in ControlOptions)
	    {
	        option.ListenTo(MoveLeft, MoveRight, MoveUp, MoveDown, StopMoving, Interact);
            option.gameObject.SetActive(false);
        }
	    UseNextControl();
    }

    private void MoveLeft()
    {
        Move(-1f, 0f);
    }

    private void MoveRight()
    {
        Move(1f, 0f);

    }

    private void MoveUp()
    {
        Move(0f, 1f);

    }

    private void MoveDown()
    {
        Move(0f, -1f);
    }

    private void StopMoving()
    {
        Move(0f,0f);
    }

    private void Move(float x, float z)
    {
        x *= Time.deltaTime * 60;
        z *= Time.deltaTime * 60;

        x *= 0.05f; // same as player movement speed
        z *= 0.05f; // same as player movement speed

        // HACK clamp position to stop running out of the world
        var newX = Mathf.Clamp(transform.position.x + x, -5f, 5f);
        var newZ = Mathf.Clamp(transform.position.z + z, -1f, 16f);

        transform.position = new Vector3(newX, transform.position.y, newZ);
        transform.LookAt(new Vector3(transform.localPosition.x + x, transform.localPosition.y, transform.localPosition.z + z));

        if (x == 0f && z == 0f)
        {
            if (_animationState != AnimationState.IDLE)
            {
                _animationState = AnimationState.IDLE;
                SetAnimation();
            }
        }
        else
        {
            if (_animationState != AnimationState.WALKING)
            {
                _animationState = AnimationState.WALKING;
                SetAnimation();
            }
        }
     
    }


    private void Interact()
    {

    }
    private void SetAnimation()
    {
        var anim = GetComponentInChildren<Animator>();
        switch (_animationState)
        {
            case AnimationState.IDLE:
                // Set speed to 0f;
                anim.SetFloat("Speed_f", 0f);
                anim.SetInteger("Animation_int", 0);
                break;
            case AnimationState.WALKING:
                anim.SetFloat("Speed_f", 1f);
                anim.SetInteger("Animation_int", 0);
                break;
        }
    }

    public void NextControl()
    {
        _index += 1;
        if (_index >= ControlOptions.Count)
        {
            _index = 0;
        }
        UseNextControl();
    }

    public void PreviousControl()
    {
        _index -= 1;
        if (_index < 0)
        {
            _index = ControlOptions.Count -1;
        }
        UseNextControl();
    }

    private void UseNextControl()
    {
        foreach (var controlOption in ControlOptions)
        {
            controlOption.gameObject.SetActive(false);
        }
        ControlOptions[_index].gameObject.SetActive(true);
        ControlName.text = ControlOptions[_index].name;
    }
}
