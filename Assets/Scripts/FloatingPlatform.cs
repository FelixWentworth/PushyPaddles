using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FloatingPlatform : MovingObject
{
    [SyncVar] public bool CanPickUp = true;

    public WaterBehaviour Water;

    private Player _playerOnPlatform;

    private MeshRenderer _mesh;

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
    }

    public override void ResetObject(Vector3 newPosition)
    {
        base.ResetObject(newPosition);

        CanFloat = true;
        MovementSpeed = 0f;
    }

    public override void Respawn()
    {
        base.Respawn();

        _playerOnPlatform = null;
        if (!isServer)
        {
            CmdReset();
        }
    }

    public void PlaceOnWater(Player player)
    {
        Debug.Log(player);
        _playerOnPlatform = player;
    }

    void FixedUpdate()
    {
        if (_playerOnPlatform != null && isServer)
        {
            // On Water
            _playerOnPlatform.GetComponent<Rigidbody>().useGravity = false;
            _playerOnPlatform.transform.position = new Vector3(transform.position.x, _playerOnPlatform.transform.position.y, transform.position.z);

            var player = _playerOnPlatform.GetComponent<Player>();
            player.SyncForceMove(new Vector3(transform.position.x, _playerOnPlatform.transform.position.y, transform.position.z), _playerOnPlatform.transform.eulerAngles);

            Water.TouchedWater(this);
        }
        // Not on water
        _mesh.enabled = CanPickUp;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            if (_playerOnPlatform != null)
            {
                _playerOnPlatform.GetComponent<Player>().OnPlatform = false;
                _playerOnPlatform.GetComponent<Rigidbody>().useGravity = true;
                _playerOnPlatform = null;

                CanFloat = false;
                Water.TouchedWater(this);
            }
        }
        else if (other.gameObject.tag == "Treasure")
        {
            // VICTORY CONDITION
            if (_playerOnPlatform == null)
            {
                return;
            }

            var player = _playerOnPlatform.GetComponent<Player>();
            _playerOnPlatform = null;

            player.OnPlatform = false;
            if (isServer)
            {
                player.SyncForceMove(other.transform.FindChild("VicrtoryLocation").position,
                    player.transform.eulerAngles);
                
            }
            // Notify the players that a reward has been reached
            player.RpcGoalReached();

            CanFloat = false;
            Water.TouchedWater(this);

        }
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

    [Command]
    private void CmdReset()
    {
        CanPickUp = true;
    }
}
