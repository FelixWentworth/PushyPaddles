﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPlatform : MovingObject
{

    public WaterBehaviour Water;

    private Player _playerOnPlatform;

    public override void Start()
    {
        base.Start();
        MovementSpeed = 0f;
        CanFloat = true;
        PlayerCanInteract = false;
        PlayerCanHit = true;
        CanRespawn = true;

        RespawnLocation = transform.position;
    }

    public override void ResetObject()
    {
        base.ResetObject();

        CanFloat = true;
        MovementSpeed = 0f;
    }

    public override void Respawn()
    {
        base.Respawn();
        _playerOnPlatform = null;
    }

    public void PlaceOnWater(Player player)
    {
        _playerOnPlatform = player;
    }

    void FixedUpdate()
    {
        if (_playerOnPlatform != null)
        {
            _playerOnPlatform.transform.position = new Vector3(transform.position.x, _playerOnPlatform.transform.position.y, transform.position.z);
            Water.TouchedWater(this);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            CanFloat = false;
        }
        else if (other.gameObject.tag == "Treasure")
        {
            // VICTORY CONDITION

            var victoryPosition = other.transform.FindChild("VicrtoryLocation").position;
            _playerOnPlatform.transform.position = victoryPosition;
            _playerOnPlatform = null;
            CanFloat = false;
            Water.TouchedWater(this);


            // Show the reward screen
            MenuManager.ShowRewards();
        }
    }

    public bool InRange(GameObject other)
    {
        var distance = Vector3.Distance(other.transform.position, transform.position);
        return distance < 1.5f;

    }
}
