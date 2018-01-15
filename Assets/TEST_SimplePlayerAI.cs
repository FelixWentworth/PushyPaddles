using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TEST_SimplePlayerAI : NetworkBehaviour {

	// Allows for the running of the game without need of interaction, used for testing

	private Player _player;

	private FloatingPlatform _platform;

	private RewardScreenManager _rewardScreenManager;

	// set to 0 for commands every frame - Input test
	private float interactTime = 0f;
	private float paddlerPickupTime = 0f;

	private float currentTime = 0f;

	private float currentPaddlerTime = 3f;
	// Use this for initialization
	void Start ()
	{
		_player = GetComponent<Player>();
		_platform = GameObject.FindGameObjectWithTag("Platform").GetComponent<FloatingPlatform>();
	}
	
	// Update is called once per frame
	void Update ()
	{ 
		currentTime += Time.deltaTime;
		//_player.AIPressed();
		if (_player.CanMove && _player.PlayerRole == Player.Role.Floater && isLocalPlayer)
		{
			if (_rewardScreenManager == null)
			{
				_rewardScreenManager = GameObject.Find("RewardScreen").GetComponent<RewardScreenManager>();
			}
			if (_rewardScreenManager != null && _rewardScreenManager.IsShowing)
			{
				if (currentTime >= interactTime)
				{
					currentTime = 0f;
					Log("Selecting Rewards");
					_rewardScreenManager.AIPressed();
				}
			}
			else if (!_platform.OnWater)
			{
				// place platform in water
				if (_player.HoldingPlatform)
				{
					// place in water
					if (_platform.CanBePlacedInWater())
					{
						if (currentTime >= interactTime)
						{
							currentTime = 0f;
							Log("Placing platform in water");
							_player.AIPressed();
						}
					}
					else
					{
						Move(new Vector3(0, transform.position.y, transform.position.z));
					}
				}
				else
				{
					// pick up
					if (_platform.InRange(gameObject) && _player.IsNextToGetPlatform())
					{
						if (currentTime >= interactTime )
						{
							currentTime = 0f;
							Log("Pickup Platform");
							_player.AIPressed();
						}
					}
					else
					{
						Move(_platform.transform.position);
					}
				}
			}
		}
		if (_player.PlayerRole == Player.Role.Paddler && !_platform.OnWater)
		{
			// place platform in water
			if (_player.HoldingPlatform)
			{
				// place in water
				if (_platform.CanBePlacedOnLand())
				{
					if (currentTime >= interactTime)
					{
						currentTime = 0f;
						Log("Drop Platform");
						_player.AIPressed();
					}
				}
				else
				{
					MoveUpDown(new Vector3(transform.position.x, transform.position.y, 0.25f));
				}
			}
			else
			{
				// pick up
				if (_platform.InRange(gameObject) && _player.IsNextToGetPlatform())
				{
					if (currentPaddlerTime >= paddlerPickupTime)
					{
						currentPaddlerTime = 0f;
						Log("Pickup Platform");
						_player.AIPressed();
					}
				}
				else
				{
					MoveUpDown(_platform.transform.position);
				}
			}
		}
	}

	private void Move(Vector3 destination)
	{
		if (gameObject.transform.position.x > destination.x)
		{
			Log("Moving Left");
			_player.AIMove(gameObject, -0.5f * _player.DirectionModifier, 0);
		}
		else
		{
			Log("Moving Right");
			_player.AIMove(gameObject, 0.5f * _player.DirectionModifier, 0);
		}
	}
	private void MoveUpDown(Vector3 destination)
	{
		if (gameObject.transform.position.z > destination.z)
		{
			Log("Moving Down");
			_player.AIMove(gameObject, 0f, -0.5f * _player.DirectionModifier);
		}
		else
		{
			Log("Moving Up");
			_player.AIMove(gameObject, 0f, 0.5f * _player.DirectionModifier);
		}
	}

	private void Log(string msg)
	{
		//Debug.Log(DateTime.Now + "\n" + msg);
	}
}
