using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TEST_SimplePlayerAI : NetworkBehaviour {

	// Allows for the running of the game without need of interaction, used for testing

	private Player _player;

	private FloatingPlatform _platform;

	private RewardScreenManager _rewardScreenManager;

	private float inputTime = .2f;

	private float currentTime = 0f;
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
		if (currentTime >= inputTime)
		{
			currentTime = 0f;
			if (_player.CanMove && _player.PlayerRole == Player.Role.Floater && isLocalPlayer)
			{
				if (_rewardScreenManager == null)
				{
					_rewardScreenManager = GameObject.Find("RewardScreen").GetComponent<RewardScreenManager>();
				}
				if (_rewardScreenManager != null && _rewardScreenManager.IsShowing)
				{
					_rewardScreenManager.AIPressed();
				}
				else if (!_platform.OnWater)
				{
					// place platform in water
					if (_player.HoldingPlatform)
					{
						// place in water
						if (_platform.CanBePlacedInWater())
						{
							_player.AIPressed();
						}
						else
						{
							Move(new Vector3(0, transform.position.y, transform.position.z));
						}
					}
					else
					{
						// pick up
						if (_platform.InRange(gameObject))
						{
							_player.AIPressed();
						}
						else
						{
							Move(_platform.transform.position);
						}
					}
				}
			}
		}
	}

	private void Move(Vector3 destination)
	{
		Debug.Log("Trying to reach " + destination);
		if (gameObject.transform.position.x > destination.x)
		{
			Debug.Log("Moving Left");
			_player.AIMove(gameObject, -1 * _player.DirectionModifier, 0);
		}
		else
		{
			Debug.Log("Moving Right");
			_player.AIMove(gameObject, 1 * _player.DirectionModifier, 0);
		}
	}
}
