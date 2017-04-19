using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MovingObject {

    public override void Start()
    {
        MovementSpeed = 0f;
        CanFloat = true;
        PlayerCanInteract = true;
        PlayerCanHit = true;
        CanRespawn = false;
    }

    public override void ResetObject()
    {

    }
}
