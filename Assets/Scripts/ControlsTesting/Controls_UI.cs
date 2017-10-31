using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controls_UI : Controls
{

    private int x;
    private int z;

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
}
