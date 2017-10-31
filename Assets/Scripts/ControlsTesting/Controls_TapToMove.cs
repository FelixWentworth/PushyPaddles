using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls_TapToMove : Controls
{

    private Transform _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButton(0))
	    {
	        var mouse = Input.mousePosition;
	        var player = Camera.main.WorldToScreenPoint(_player.transform.position);

            // Determine which way the player should move
	        if (mouse.x < player.x)
	        {
	            MoveLeft();
	        }
	        else if (mouse.x > player.x)
	        {
	            MoveRight();
	        }
	        if (mouse.y > player.y)
	        {
	            MoveUp();
	        }
	        else if (mouse.y < player.y)
	        {
	            MoveDown();
	        }
	    }
	    if (Input.GetMouseButtonUp(0))
	    {
	        StopMoving();
	    }
	}
}
