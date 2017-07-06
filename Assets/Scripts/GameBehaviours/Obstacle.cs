using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MovingObject {

    public override void Start()
    {
        MovementSpeed = 0f;
        CanFloat = true;
        PlayerCanInteract = false;
        PlayerCanHit = true;
        CanRespawn = false;
    }
}
