﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FloatingPlatform : MovingObject
{
    [SyncVar] public bool CanPickUp = true;
    [SyncVar] public bool OnWater;
    [SyncVar] public string PickupValue;
    private List<string> _operations = new List<string>();

    public WaterBehaviour Water;

    private Player _playerOnPlatform;

    private MeshRenderer _mesh;

    
    private Text _pickupText;

    public override void Start()
    {
        base.Start();
        MovementSpeed = 0f;
        CanFloat = true;
        PlayerCanInteract = false;
        PlayerCanHit = true;
        CanRespawn = true;

        _mesh = transform.GetChild(0).GetComponent<MeshRenderer>();

        RespawnLocation.Add(transform.position);
        var oppositeSide = new Vector3(transform.position.x * -1, transform.position.y, transform.position.z);

        RespawnLocation.Add(oppositeSide);

        _pickupText = GetComponentInChildren<Text>();
        PickupValue = "";
        _operations = new List<string>();
    }

    public override void ResetObject(Vector3 newPosition)
    {
        base.ResetObject(newPosition);

        CanFloat = true;
        MovementSpeed = 0f;
        PickupValue = "";
        _operations = new List<string>();
    }

    public override void Respawn()
    {
        base.Respawn();

        _playerOnPlatform = null;
        CanPickUp = true;
        OnWater = false;
    }

    public void PlaceOnWater(Player player)
    {
        _playerOnPlatform = player;
        OnWater = true;
    }

    void FixedUpdate()
    {
        if (_playerOnPlatform != null && isServer)
        {
            // On Water
            _playerOnPlatform.GetComponent<Rigidbody>().useGravity = false;
            _playerOnPlatform.transform.position = new Vector3(transform.position.x, _playerOnPlatform.transform.position.y, transform.position.z);

            var player = _playerOnPlatform.GetComponent<Player>();

            player.SyncForceMove(
                new Vector3(transform.position.x, _playerOnPlatform.transform.position.y, transform.position.z),
                _playerOnPlatform.transform.eulerAngles);
            
            Water.TouchedWater(this);
        }
        // Not on water
        _mesh.enabled = CanPickUp;
        _pickupText.text = PickupValue;
    }

    void OnCollisionEnter(Collision other)
    {
        // VICTORY CONDITION
        if (_playerOnPlatform == null)
        {
            return;
        }

        if (other.gameObject.tag == "Obstacle")
        {
            _playerOnPlatform.GetComponent<Player>().OnPlatform = false;
            _playerOnPlatform.GetComponent<Rigidbody>().useGravity = true;
            _playerOnPlatform = null;

            CanFloat = false;
            Water.TouchedWater(this);
        }
        else if (other.gameObject.tag == "Treasure")
        {
            StartCoroutine(GoalReached(other.gameObject));
        }
        else if (other.gameObject.tag == "Collectible")
        {
            var operation = other.gameObject.GetComponent<MathsCollectible>().Operation;

            if (_operations.Count == 0 && (operation.Contains("+") || operation.Contains("/") || operation.Contains("x")))
            {
                operation = operation.Substring(1, operation.Length-1);
            }

            PickupValue += operation;
            _operations.Add(operation);

            other.collider.enabled = false;

            StartCoroutine(DisableCollider(other));
        }
    }

    private IEnumerator DisableCollider(Collision other)
    {
        other.collider.enabled = false;

        yield return new WaitForSeconds(2.5f);

        other.collider.enabled = true;
    }

    public IEnumerator GoalReached(GameObject other)
    {
        var levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        var total = levelManager.Evaluate(_pickupText.text);
        if (isServer)
        {
            GameObject.Find("SpawnedObjects").GetComponent<CollectibleGeneration>().ResetColliders();

            RpcShowTotal(_pickupText.text, total.ToString(), total.ToString() == levelManager.Target);
        }
        
        yield return new WaitForSeconds(levelManager.TotalUI.AnimLength());

        if (isServer)
        {
            RpcDisableTotal();
        }

        if (total.ToString() == levelManager.Target)
        {
            var player = _playerOnPlatform.GetComponent<Player>();

            if (isServer)
            {
                player.SetGoalReached(false);
                player.SyncForceMove(other.transform.Find("VicrtoryLocation").position,
                    player.transform.eulerAngles);
            }

            _playerOnPlatform = null;

            // Notify the players that a reward has been reached
            player.RpcGoalReached();

            CanFloat = false;
            Water.TouchedWater(this);

        }
        else
        {
            // Behave as if hit object
            _playerOnPlatform.GetComponent<Player>().OnPlatform = false;
            _playerOnPlatform.GetComponent<Rigidbody>().useGravity = true;
            _playerOnPlatform = null;

            CanFloat = false;
            Water.TouchedWater(this);
        }
    }

    [ClientRpc]
    private void RpcShowTotal (string expression, string total, bool victory)
    {
        var levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        levelManager.TotalUI.GetComponent<RectTransform>().localScale = Vector3.zero;
        levelManager.TotalUI.gameObject.SetActive(true);
        levelManager.TotalUI.Show(expression + " =", total, victory );
    }

    [ClientRpc]
    private void RpcDisableTotal()
    {
        var levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        levelManager.TotalUI.gameObject.SetActive(false);
    }

    public bool InRange(GameObject other)
    {
        if (_playerOnPlatform != null)
        {
            // Player cannot interact
            return false;
        }
        var distance = Vector3.Distance(other.transform.position, transform.position);
        return distance < 1.5f;

    }

    public bool CanBePlacedInWater()
    {
        // Get Start Point
        var start = GameObject.Find("PlatformStartPoint");

        return Vector3.Distance(start.transform.position, transform.position) < 1.5f;
    }
}
