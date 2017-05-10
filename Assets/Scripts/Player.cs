using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MovingObject
{
    public string Name;
    [SyncVar] public float DirectionModifier = 1;
    [SyncVar] public float SpeedModifier = 1f;

    [SyncVar] public bool HoldingPlatform;

    public GameObject Paddle;
    public GameObject Platform;

    public enum Role
    {
        Unassigned = 0,
        Floater,
        Paddler
    }

    [SyncVar] public Role PlayerRole;

    public void SetRole(Role role)
    {
        PlayerRole = role;
    }
    
    private enum AnimationState{
        IDLE = 0,
        
        WALKING,
        HOLDING,
        FALLING
    }

    private AnimationState _animationState;

    [SyncVar] public int _animState;

    private bool _moving = false;
    private float _direction = 0f;

    private GameObject _holdingGameObject;

    [SyncVar] private bool _usePaddle;
    [SyncVar] public int PlayerID;
    private int _currentModel;
    [SyncVar] private int _playerModel;

    [SyncVar] public Vector3 RealPosition;
    [SyncVar] public Vector3 RealRotation;
    private float _elapsedTime;
    private float _updateInterval = 0.11f; // 9 times a second

    [SyncVar] public bool OnPlatform;
    
    private Rigidbody _rigidbody;
    private TextMesh _playerText;

    public override void Start()
    {
        CanFloat = false;
        PlayerCanInteract = false;
        PlayerCanHit = false;
        CanRespawn = true;

        RespawnLocation.Add(transform.position);

        _holdingGameObject = null;

        //GetComponent<Rigidbody>().isKinematic = !isServer;
        _rigidbody = GetComponent<Rigidbody>();
        _playerText = GetComponentInChildren<TextMesh>();
        _playerText.text = "Player " + (PlayerID + 1);

        SetModel();
        _currentModel = _playerModel;
    }

    public override void ResetObject(Vector3 newPosition)
    {
        base.ResetObject(newPosition);

        OnPlatform = false;

        if (!isServer)
        {
            CmdSyncRespawn(newPosition, transform.eulerAngles);
        }
    }

    public override void OnStartLocalPlayer()
    {
        GameObject.Find("CharacterSelection").GetComponent<CharacterSelection>().Set(this);
    }

    void Update()
    {
        if (isLocalPlayer && !OnPlatform)
        {
            if (!_rigidbody.useGravity)
            {
                // when the player has full control, they should be affected by gravity
                _rigidbody.useGravity = true;
            }
            var x = Input.GetAxis("Horizontal");
            var z = Input.GetAxis("Vertical");

            if (x != 0 || z != 0)
            {
                // Move Player Command
                //CmdMove(gameObject, x, z);
                Move(gameObject, x, z);
                if (_animationState != AnimationState.WALKING)
                {
                    CmdChangeState((int)AnimationState.WALKING);
                }
            }
            else
            {
                // Stopped moving
                if (_animationState != AnimationState.IDLE)
                {
                    CmdChangeState((int)AnimationState.IDLE);
                }
            }
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > _updateInterval)
            {
                _elapsedTime = 0f;
                CmdSyncMove(transform.position, transform.eulerAngles);
            }
        }
        else
        {
            if (_rigidbody.useGravity)
            {
                // Don't override actual location using gravity, causes player jumping
                _rigidbody.useGravity = false;
            }
            // Lerp to real position
            transform.position = RealPosition;
            transform.eulerAngles = RealRotation;
        }  
    }

    private void Move(GameObject go, float x, float z)
    {
        x *= MovementSpeed * SpeedModifier * DirectionModifier;
        z *= MovementSpeed * SpeedModifier * DirectionModifier;

        // HACK clamp position to stop running out of the world
        var newX = Mathf.Clamp(go.transform.position.x + x, -5f, 5f);
        var newZ = Mathf.Clamp(go.transform.position.z + z, -1f, 16f);

        go.transform.position = new Vector3(newX, go.transform.position.y, newZ);
        go.transform.LookAt(new Vector3(go.transform.localPosition.x + x, go.transform.localPosition.y, go.transform.localPosition.z + z));
    }

    // Server moves the player and forces them to a position
    [Server]
    public void SyncForceMove(Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.eulerAngles = rotation;

        RealPosition = position;
        RealRotation = rotation;
    }

    [Command]
    public void CmdSyncMove(Vector3 position, Vector3 rotation)
    {
        if (position.y < -10f)
        {
            return;
        }
        RealPosition = position;
        RealRotation = rotation;
    }
    [Command]
    private void CmdSyncRespawn(Vector3 position, Vector3 rotation)
    {
        RealPosition = position;
        RealRotation = rotation;

        RpcRespawn(position, rotation);
    }

    [ClientRpc]
    private void RpcRespawn(Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.eulerAngles = rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetAnimation();
        SetPaddle();
        SetPlatform();

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
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.Space))
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
                if (fp.CanBePlacedInWater())
                {
                    // Place in water
                    CmdDropPlatform(platform);
                    CmdPlacePlatformInWater(platform, gameObject);
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();  
        }

        /////////////////////////////
        // End Local Player Controls
        /////////////////////////////
    }

    [Command]
    private void CmdMove(GameObject go, float x, float z)
    {
        x *= MovementSpeed * SpeedModifier * DirectionModifier;
        z *= MovementSpeed * SpeedModifier * DirectionModifier;

        go.transform.position += new Vector3(x, 0, z);
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
    private void CmdPlacePlatformInWater(GameObject platform, GameObject go)
    {
        var player = go.GetComponent<Player>();
        player.OnPlatform = true;
        
        if (player.PlayerRole != Role.Floater)
        {
            // only the server knows each player role, so do this check here
            return;
        }
        var start = GameObject.Find("PlatformStartPoint");

        platform.transform.position = start.transform.position;

        var fp = platform.GetComponent<FloatingPlatform>();

        fp.CanPickUp = true;
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

    private void SetPaddle()
    {
        if (PlayerRole == Role.Floater && Paddle.activeSelf)
        {
            Paddle.SetActive(false);
        }
        if (PlayerRole == Role.Paddler && !Paddle.activeSelf)
        {
            Paddle.SetActive(true);
        }
    }

    private void SetPlatform()
    {
        if (HoldingPlatform != Platform.activeSelf)
        {
            Platform.SetActive(HoldingPlatform);
        }
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
        var transforms = child.GetComponentsInChildren<Transform>();
        foreach (var t in transforms)
        {
            if (t.gameObject.name.Contains("CH_")) // CH marks model
            {
                t.gameObject.SetActive(false);
            }
        }
        
        transform.GetChild(0).GetChild(_playerModel).gameObject.SetActive(true);
    }

    [ClientRpc]
    public void RpcGoalReached()
    {
        if (!isLocalPlayer)
        { 
            return;
        }
        GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowRewards();
    }

    public void RestartGame()
    {
        if (isLocalPlayer)
        {
            Debug.LogError("Restart Game");

            CmdRestartGame();
        }
    }

    [Command]
    public void CmdRestartGame()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().Restart();
    }

    public void NextRound()
    {
        if (isLocalPlayer)
        {
            Debug.LogError("Round Complete");

            CmdNextRound();
        }
    }

    [Command]
    public void CmdNextRound()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().NextRound();
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
        }
    }
}
