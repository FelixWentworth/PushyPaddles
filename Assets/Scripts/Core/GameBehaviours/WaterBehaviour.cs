﻿using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class WaterBehaviour : NetworkBehaviour
{
	[SerializeField] private float _tideStrength = 0.01f;
    [SerializeField] private float _maxPaddleStrength = 1f;

	private float _tideStrengthModifier { get { return _tideStrength / 10f; } }

	[SyncVar] public int TideStrengthMultiplier;
	

    private float _raftTilt = 0f;
    // The strength of paddle power from another player
    [Range(-1f, 1f)] private float _paddleStrength = 0f;

    // the time between max to 0 paddle strength
    private float _paddleCooldownTime = 1f;

    // the z position that once an object passes they will be Respawned
    private float _respawnZPos;

    // Our clamp positions for the x axis, the object should not leave the water
    private float _minXClamp;
    private float _maxXClamp;


    private GameObject _currentPlatform;

    public Material Material;

    private GameManager _gameManager;

    void Awake()
    {
        _respawnZPos = transform.position.z + (transform.localScale.y / 2f);

        _minXClamp = transform.position.x - (transform.localScale.x / 2f);
        _maxXClamp = transform.position.x + (transform.localScale.x / 2f);

        StartCoroutine(ParallaxWater());
    }

    public void TouchedWater(MovingObject movingObject)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        if (!_gameManager.GamePlaying())
        {
            // Nothing should move
            return;
        }
        // Detect if this object floats
        if (movingObject.CanFloat)
        {
            if (movingObject.gameObject.tag == "Platform")
            {
                _currentPlatform = movingObject.gameObject;
            }
            Move(movingObject);
        }
        else
        {
            movingObject.GetComponent<BoxCollider>().enabled = false;
            movingObject.FellInWater();
            movingObject.Respawn();
        }
    }

    public void Move(MovingObject go)
    { 
        // Get the tide strength and the x transform
        var newPosition = new Vector3(
            go.transform.position.x + (_paddleStrength * _maxPaddleStrength) + _raftTilt,
            go.transform.position.y,
            go.transform.position.z + _tideStrength + (_tideStrengthModifier * TideStrengthMultiplier)
        );

        MoveFloatingObject(go.gameObject, newPosition);
    }

    [ServerAccess]
    private void MoveFloatingObject(GameObject go, Vector3 pos)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        go.transform.position = pos;
        // Clamp the x axis
        ClampX(go.gameObject);
    }

	[ServerAccess]
	public void IncrementTideModifier(int increment)
	{
		var method = MethodBase.GetCurrentMethod();
		var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
		if (!attr.HasAccess)
		{
			return;
		}

		TideStrengthMultiplier += increment;
		TideStrengthMultiplier = Mathf.Clamp(TideStrengthMultiplier, -4, 10);  // clamp between 0.6x speed and 2x speed
	}

    private void ClampX(GameObject go)
    {
        // Add padding as the object pivot is in the center
        var padding = go.transform.localScale.x / 2f;
        var minX = _minXClamp + padding;
        var maxX = _maxXClamp - padding;
        var newX = go.transform.position.x;
        if (newX <= minX)
        {
            newX = minX;
        }
        else if (newX >= maxX)
        {
            newX = maxX;
        }

        if (newX != go.transform.position.x)
        {
            go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
        }
    }

    [Command]
    public void CmdPaddleUsed(string playerId)
    {
        var player = _gameManager.GetPlayer(playerId);
        PaddleUsed(player, player.StrengthModifier);
    }

    // Only allow the server to run this as to avoid players exploiting
    [ServerAccess]
    public float PaddleUsed(Player player, float playerModifier)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return 0f;
        }
        var strength = 0f;
        switch (player.PlayerRole)
        {
            case Player.Role.Paddler:
                strength = player.transform.position.x > 0f ? -1f : 1f;
                strength*= playerModifier;
                break;
            case Player.Role.Unassigned:
            case Player.Role.Floater:
            default:
                // Player Cannot interact with water
                return 0f;
        }

        // get the angle between the player and the object in the water
        var modifier = 1f;

        if (_currentPlatform != null)
        {

            var playerPosition = player.transform.position;
            var platformPosition = _currentPlatform.transform.position;
            var zDist = platformPosition.z - playerPosition.z;

            zDist = zDist < 0 ? zDist * -1f : zDist;
            if (zDist <= 0.5f)
            {
                modifier = 1f;
            }
            else if (zDist > 0.5f && zDist <= 1.25f)
            {
                modifier = 0.5f;
            }
            else
            {
                modifier = 0f;
            }

        }
        _paddleStrength = strength * modifier;
        //StartCoroutine(PaddleUsed(strength, modifier));
        return _paddleStrength;
    }

    private IEnumerator PaddleUsed(float strength, float modifier)
    {
        Debug.Log(_paddleStrength + ", " + modifier);
        var t = 0f;

        var startStrength = strength * modifier;
        while (t < _paddleCooldownTime)
        {
            _paddleStrength = Mathf.Lerp(startStrength, 0f, t/ _paddleCooldownTime);
            t += Time.deltaTime;
            yield return null;
        }
        _paddleStrength = 0f;
    }

    void Update()
    {
        if (_paddleStrength <= 0f)
        {
            _paddleStrength += Time.deltaTime;
        }
        else if (_paddleStrength >= 0)
        {
            _paddleStrength -= Time.deltaTime;
        }
    }

    [ServerAccess]
    public void RaftTilt(Player player, float tilt)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        var raftTilt = player.RaftControlModifier * tilt;

        _raftTilt = raftTilt;
    }

    private IEnumerator ParallaxWater()
    {
        while (true)
        {
            var y = Mathf.Repeat(Time.time * _tideStrength * 0.01f, 1);
            var offset = new Vector2(Material.mainTextureOffset.x + (_paddleStrength * 0.01f), y);
            Material.SetTextureOffset("_MainTex", offset);

            yield return null;
        }
    }
}
