using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class Player : MovingObject
{

    public int DirectionModifier = 1;
    public float SpeedModifier = 1f;

    public enum Role
    {
        Unassigned = 0,
        Floater,
        Paddle_Left,
        Paddle_Right
    }

    public Role PlayerRole;

    private bool _moving = false;
    private float _direction = 0f;

    public override void Start()
    {
        MovementSpeed = 1f;
        CanFloat = false;
        PlayerCanInteract = false;
        PlayerCanHit = false;
        CanRespawn = true;

        // TODO Set repawn location
        RespawnLocation = transform.position;
    }

    public override void ResetObject()
    {

    }

    public void StartMoving(float direction)
    {
        _moving = true;
        _direction = direction;
    }

    public void StopMoving()
    {
        _moving = false;
        _direction = 0f;
    }

    void Update()
    {
        if (_moving)
        {
            Move(_direction);
        }
    }

    private void Move(float direction)
    {
        var newPosition = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z + (direction * MovementSpeed * DirectionModifier * SpeedModifier)    
        );

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime);
    }

    public void Interact()
    {
        switch (PlayerRole)
        {
            case Role.Floater:
                GameObject.FindGameObjectWithTag("Platform").GetComponent<FloatingPlatform>().PlaceOnWater(this);
                break;
            case Role.Paddle_Left:
            case Role.Paddle_Right:
                GameObject.FindGameObjectWithTag("Water").GetComponent<WaterBehaviour>().PaddleUsed(this);
                break;
            case Role.Unassigned:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
        }
    }
}
