using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : MovingObject
{
    public string Name;
    [SyncVar] public float DirectionModifier = 1;
    [SyncVar] public float SpeedModifier = 1f;

    [SyncVar] public bool HoldingPlatform;

    [SyncVar] public int PlayerNum;

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
        ONPLATFORM,
        HOLDING,
        FALLING
    }

    private AnimationState _animationState;

    [SyncVar] public int _animState;

    private bool _moving = false;
    private float _direction = 0f;

    private GameObject _holdingGameObject;

    [SyncVar] private bool _usePaddle;
    [SyncVar] public bool IsReady;
    [SyncVar] public int PlayerID;
    [SyncVar] public int ConnectionId;
    private int _currentModel;
    [SyncVar] private int _playerModel;

    [SyncVar] public Vector3 RealPosition;
    [SyncVar] public Vector3 RealRotation;
    private float _elapsedTime;
    private float _updateInterval = 0.11f; // 9 times a second

    [SyncVar] public bool OnPlatform;
    
    private Rigidbody _rigidbody;
    private TextMesh _playerText;

    private GameManager _gameManager;

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
        _playerText.text = string.Format(Localization.Get("FORMATTED_UI_GAME_PLAYER"),PlayerID + 1);

        SetModel();
        _currentModel = _playerModel;
        if (!isServer && isLocalPlayer)
        { 
            GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowHowToPlay();
        }
    }

    public override void ResetObject(Vector3 newPosition)
    {
        base.ResetObject(newPosition);

        if (!isServer)
        {
            CmdSyncRespawn(transform.eulerAngles);
        }
    }

    public override void OnStartLocalPlayer()
    {
        GameObject.Find("CharacterSelection").GetComponent<CharacterSelection>().Set(this);
    }

    void Update()
    {
        // Don't allow players to move whilst rewards are being distributed
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        if (!_gameManager.DistributingRewards)
        {
            if (isLocalPlayer)
            {
                if (OnPlatform)
                {
                    // Stopped moving
                    if (_animationState != AnimationState.ONPLATFORM)
                    {
                        CmdChangeState((int) AnimationState.ONPLATFORM);
                    }

                    UpdatePlayerPosition();
                }
                else
                {
                    if (!_gameManager.GamePlaying())
                    {
                        // Game Paused, Cannot move
                        return;
                    }
                    if (!_rigidbody.useGravity)
                    {
                        // when the player has full control, they should be affected by gravity
                        _rigidbody.useGravity = true;
                    }
                    var x = Input.GetAxis("Horizontal");
                    //var z = Input.GetAxis("Vertical");

                    if (x != 0 )
                    {
                        // Move Player Command
                        if (PlayerRole == Role.Floater)
                        {
                            // Move Left/Right
                            Move(gameObject, x, 0);
                        }
                        else
                        {
                            // Move Up/Down
                            Move(gameObject, 0, x);
                        }
                        if (_animationState != AnimationState.WALKING)
                        {
                            CmdChangeState((int) AnimationState.WALKING);
                        }
                    }
                    else
                    {
                        // Stopped moving
                        if (_animationState != AnimationState.IDLE)
                        {
                            CmdChangeState((int) AnimationState.IDLE);
                        }

                    }
                    _elapsedTime += Time.deltaTime;
                    if (_elapsedTime > _updateInterval)
                    {
                        _elapsedTime = 0f;
                        CmdSyncMove(transform.position, transform.eulerAngles);
                    }
                }
            }

            else
            {
                UpdatePlayerPosition();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
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

    private void UpdatePlayerPosition()
    {
        if (_rigidbody.useGravity)
        {
            // Don't override actual location using gravity, causes player jumping
            _rigidbody.useGravity = false;
        }
        // Lerp to real position
        transform.position = Vector3.Lerp(transform.position, RealPosition, 0.5f);
        transform.eulerAngles = RealRotation;
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

    [Server]
    public void SetGoalReached(bool onPlatform)
    {
        _gameManager.DistributingRewards = true;
        OnPlatform = onPlatform;
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

    [Server]
    public void SyncRespawn(Vector3 rotation)
    {
        SyncForceMove(_gameManager.GetPlayerRespawn(PlayerNum), rotation);

        RpcRespawn(_gameManager.GetPlayerRespawn(PlayerNum), rotation);
    }

    [Command]
    private void CmdSyncRespawn(Vector3 rotation)
    {
        SyncRespawn(rotation);
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
        }
        if (_playerModel != _currentModel)
        {
            SetModel();
            _currentModel = _playerModel;
        }

        /////////////////////////
        // Local Player Controls
        /////////////////////////
        if (isLocalPlayer && !Respawning && Input.GetKeyDown(KeyCode.Space))
        {
            var platform = GameObject.FindGameObjectWithTag("Platform");
            var fp = platform.GetComponent<FloatingPlatform>();

            if (fp.CanPickUp && !fp.OnWater && fp.InRange(gameObject) && !HoldingPlatform)
            {
                // Pickup Plaftorm
                CmdPickupPlatform(platform);
                return;
            }
            
            // Make sure the player is looking in the correct direction
            transform.LookAt(PlayerRole == Role.Floater
                ? new Vector3(transform.position.x, transform.position.y, 100f)
                : new Vector3(0f, transform.position.y, transform.position.z));

            if (HoldingPlatform)
            {
                if (fp.CanBePlacedInWater() && PlayerRole == Role.Floater)
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
        if (OnPlatform)
            return;
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

    [Server]
    public void DropPlatform(GameObject platform)
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
            GameObject.Find("Water").GetComponent<WaterBehaviour>().PaddleUsed(this);

            _usePaddle = true;
        }
        GameObject.Find("AudioManager").GetComponent<NetworkAudioManager>().Play("Paddle");

    }

    [Command]
    public void CmdSetModel(int model)
    {
        IsReady = true;
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
                anim.SetInteger("Animation_int", 0);
                break;
            case AnimationState.WALKING:
                anim.SetFloat("Speed_f", 1f);
                anim.SetInteger("Animation_int", 0);
                break;
            case AnimationState.ONPLATFORM:
                anim.SetFloat("Speed_f", 0f);
                //anim.SetBool("Crouch_b", true);
                anim.SetInteger("Animation_int", 9);
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
            CmdRestartGame();
        }
    }

    [Command]
    public void CmdRestartGame()
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        _gameManager.Restart();
    }

    public void NextRound()
    {
        if (isLocalPlayer)
        {
            CmdNextRound();
        }
    }

    [Command]
    public void CmdNextRound()
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        _gameManager.DistributingRewards = false;
        _gameManager.NextRound();
    }

    public void StartTimer()
    {
        if (isLocalPlayer)
        {
            CmdStartRoundTimer();
        }
    }

    [Command]
    public void CmdStartRoundTimer()
    {
        _gameManager.StartTimer();
    }

    public void AssignSpeedBoost(int playerIndex, float speedIncrement)
    {
        if (isLocalPlayer)
        {
            CmdAssignSpeedBoost(playerIndex, speedIncrement);
        }
    }

    [Command]
    public void CmdAssignSpeedBoost(int playerNumber, float increment)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        var player = _gameManager.GetPlayer(playerNumber);

        // Assign the reward
        player.SpeedModifier += increment;
    }

    public void AssignReverseControls(int playerIndex, float modifier)
    {
        if (isLocalPlayer)
        {
            CmdAssignReverseControls(playerIndex, modifier);
        }
    }

    [Command]
    public void CmdAssignReverseControls(int playerNumber, float modifier)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        var player = _gameManager.GetPlayer(playerNumber);

        // Assign the reward
        player.DirectionModifier *= modifier;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
        }
    }
}
