using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class Player : MovingObject
{
    public string Name;
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

    private GameObject _holdingGameObject;

    public override void Start()
    {
        MovementSpeed = 1f;
        CanFloat = false;
        PlayerCanInteract = false;
        PlayerCanHit = false;
        CanRespawn = true;

        // TODO Set repawn location
        RespawnLocation = transform.position;

        _holdingGameObject = null;
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
        if (PlayerRole == Role.Floater)
        {
            newPosition = new Vector3(
                transform.position.x + (direction * MovementSpeed * DirectionModifier * SpeedModifier) ,
                transform.position.y,
                transform.position.z 
            );
        } 

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime);
        if (_holdingGameObject != null)
        {
            _holdingGameObject.transform.position = transform.position + new Vector3(0f, 1f, 0f);
        }
    }

    public void Interact()
    {
        var platform = GameObject.FindGameObjectWithTag("Platform").GetComponent<FloatingPlatform>();
        var platformStart = GameObject.FindGameObjectWithTag("PlatformStart").gameObject;
        switch (PlayerRole)
        {
            case Role.Floater:
                if (platform.InRange(gameObject))
                {
                    if (_holdingGameObject == null)
                    {
                        _holdingGameObject = platform.gameObject;
                        _holdingGameObject.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                    }
                    else if (Vector3.Distance(platformStart.transform.position, transform.position) < 3f)
                    {
                        platform.ResetObject();
                        _holdingGameObject.transform.position = platformStart.transform.position;

                        _holdingGameObject = null;
                        platform.PlaceOnWater(this);
                    }
                }
                break;
            case Role.Paddle_Left:
            case Role.Paddle_Right:
                if (platform.InRange(gameObject))
                {
                    if (_holdingGameObject == null)
                    {
                        _holdingGameObject = platform.gameObject;
                        _holdingGameObject.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                    }
                    else
                    {
                        _holdingGameObject.transform.position = new Vector3(_holdingGameObject.transform.position.x, 0.5f, _holdingGameObject.transform.position.z);
                        _holdingGameObject = null;
                    }
                }
                else
                {
                    GameObject.FindGameObjectWithTag("Water").GetComponent<WaterBehaviour>().PaddleUsed(this);
                }
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
