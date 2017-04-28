using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MovingObject
{
    public string Name;
    public int DirectionModifier = 1;
    public float SpeedModifier = 1f;

    [SyncVar] public bool HoldingPlatform;

    public enum Role
    {
        Unassigned = 0,
        Floater,
        Paddler
    }

    public Role PlayerRole;

    private enum AnimationState{
        IDLE = 0,
        
        WALKING,
        HOLDING,
        FALLING
    }

    private AnimationState _animationState;

    [SyncVar] public int _animState = 0;

    private bool _moving = false;
    private float _direction = 0f;

    private GameObject _holdingGameObject;

    [SyncVar] private bool _usePaddle;
    [SyncVar] public int PlayerID;
    private int _currentModel;
    [SyncVar] private int _playerModel;

    public override void Start()
    {
        CanFloat = false;
        PlayerCanInteract = false;
        PlayerCanHit = false;
        CanRespawn = true;

        RespawnLocation.Add(transform.position);

        _holdingGameObject = null;

        GetComponent<Rigidbody>().isKinematic = !isServer;

        SetModel();
        _currentModel = _playerModel;
    }

    public override void ResetObject()
    {

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        SetAnimation();
        if (_usePaddle)
        {
            _usePaddle = false;
            GetComponentInChildren<ParticleSystem>().Play();
            GameObject.Find("Water").GetComponent<WaterBehaviour>().PaddleUsed(this);
        }
        if (_playerModel != _currentModel)
        {
            SetModel();
            _currentModel = _playerModel;
        }

        /////////////////////////
        // Local Player Controls
        /////////////////////////

        if (!isLocalPlayer)
        {
            return;
        }
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        if (x != 0 || z != 0)
        {
            // Move Player Command
            CmdMove(gameObject, x, z);
            CmdChangeState((int) AnimationState.WALKING);
        }
        else
        {
            CmdChangeState((int) AnimationState.IDLE);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var platform = GameObject.FindGameObjectWithTag("Platform");
            var fp = platform.GetComponent<FloatingPlatform>();

            if (fp.CanPickUp && fp.InRange(gameObject) && !HoldingPlatform)
            {
                // Pickup Plaftorm
                CmdPickupPlatform(platform);
            }
            else if (HoldingPlatform)
            {
                if (fp.CanBePlacedInWater() && PlayerRole == Role.Floater)
                {
                    // Place in water
                    CmdDropPlatform(platform);
                    CmdPlacePlatformInWater(platform);
                }
                else
                {
                    // Drop Platform
                    CmdDropPlatform(platform);
                }
            }
            else
            {
                // Use paddle in water
                CmdUsePaddle();
            }
        }

        /////////////////////////////
        // End Local Player Controls
        /////////////////////////////
    }

    [Command]
    private void CmdMove(GameObject go, float x, float z)
    {
        go.transform.position += new Vector3(x * MovementSpeed, 0, z * MovementSpeed);
        go.transform.LookAt(new Vector3(go.transform.localPosition.x + x, go.transform.localPosition.y, go.transform.localPosition.z + z));
    }

    [Command]
    private void CmdPickupPlatform(GameObject platform)
    {
        HoldingPlatform = true;
        platform.GetComponent<FloatingPlatform>().CanPickUp = !HoldingPlatform;
        platform.transform.SetParent(this.transform, true);
        platform.transform.localPosition = new Vector3(0f, 1.0f, 1.5f);
    }

    [Command]
    private void CmdDropPlatform(GameObject platform)
    {
        platform.transform.position = new Vector3(transform.position.x, -0.6f, transform.position.z);
        platform.transform.SetParent(null, true);
        HoldingPlatform = false;
        platform.GetComponent<FloatingPlatform>().CanPickUp = !HoldingPlatform;
    }

    [Command]
    private void CmdPlacePlatformInWater(GameObject platform)
    {
        var start = GameObject.Find("PlatformStartPoint");

        platform.transform.position = start.transform.position;

        var fp = platform.GetComponent<FloatingPlatform>();

        fp.CanPickUp = false;
        fp.PlaceOnWater(this);
    }

    [Command]
    private void CmdChangeState(int newState)
    {
        _animState = newState;
    }

    [Command]
    private void CmdUsePaddle()
    {
        // Check here if the right player as only the server knows the current roles
        if (PlayerRole == Role.Paddler)
        { 
            _usePaddle = true;
        }
    }

    [Command]
    public void CmdSetModel(int model)
    {
        _playerModel = model;
    }

   
    private void SetAnimation()
    {
        if ((int) _animationState == _animState)
        {
            return;
        }

        var nextState = (AnimationState) _animState;

        var anim = GetComponentInChildren<Animator>();
        switch (nextState)
        {
            case AnimationState.IDLE:
                // Set speed to 0f;
                anim.SetFloat("Speed_f", 0f);
                break;
            case AnimationState.WALKING:
                anim.SetFloat("Speed_f", 1f);
                break;
            case AnimationState.HOLDING:
                break;
            case AnimationState.FALLING:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _animationState = nextState;
    }

    private void SetModel()
    {
        // Disable all children
        var child = transform.GetChild(0);
        Debug.Log(child.gameObject.name);
        var transforms = child.GetComponentsInChildren<Transform>();
        foreach (var t in transforms)
        {
            if (t != child)
            {
                t.gameObject.SetActive(false);
            }
        }
        transform.GetChild(0).GetChild(_playerModel).gameObject.SetActive(true);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
        }
    }
}
