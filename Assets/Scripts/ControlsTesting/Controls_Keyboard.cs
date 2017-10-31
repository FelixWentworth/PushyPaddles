using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls_Keyboard : Controls{

	// Update is called once per frame
	void Update ()
	{
	    ControlsInput();
	}

    private void ControlsInput()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.UpArrow))
            StopMoving();
        if (Input.GetKey(KeyCode.LeftArrow))
            MoveLeft();
        if (Input.GetKey(KeyCode.RightArrow))
            MoveRight();
        if (Input.GetKey(KeyCode.DownArrow))
            MoveDown();
        if (Input.GetKey(KeyCode.UpArrow))
            MoveUp();
        if (Input.GetKey(KeyCode.Space))
            Interact();
    }
}
