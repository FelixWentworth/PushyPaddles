﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MovingObject
{
    public string Name;
    [SyncVar] public float DirectionModifier = 1;
    [SyncVar] public float SpeedModifier = 1f;
    [SyncVar] public float RaftControlModifier = 1f;
    [SyncVar] public float StrengthModifier = 1f;

    [SyncVar] public bool HoldingPlatform;
    [SyncVar] public bool ControlledByServer;

    [SyncVar] public int PlayerNum;

    public GameObject Paddle;
    public GameObject Platform;
    public GameObject PaddlePrompt;

    public GameObject WaveEffect;

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
    [SyncVar] public int ConnectionId;
    private int _currentModel;
    [SyncVar] private int _playerModel;
    [SyncVar] public string SyncNickName;
    [SyncVar] public string PlayerID;
    private PlatformSelection.PSLPlayerData _playerData;

    [SyncVar] public Vector3 RealPosition;
    [SyncVar] public Vector3 RealRotation;
    private float _elapsedTime;
    private float _updateInterval = 0.11f; // 9 times a second

    [SyncVar] public bool OnPlatform;
    
    private Rigidbody _rigidbody;
    private TextMesh _playerText;

    private GameManager _gameManager;

    private InstructionManager _instructionManager;
    private bool _hasMoved;
    private bool _usedPaddle;
    private GameObject _raft;
    private FloatingPlatform _floatingPlatform;

    private float _timeSinceLastMove = 0f;
    [SerializeField] private float _idleTime;


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
        if (SyncNickName == "")
        {
            _playerText.text = SyncNickName;
        }
        else
        {
            _playerText.text = SyncNickName;
        }
        SetModel();
        _currentModel = _playerModel;

        PaddlePrompt.SetActive(false);

        if (!isServer && isLocalPlayer)
        {
            GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowHowToPlay();

            var platformSelection = GameObject.Find("PlatformManager").GetComponent<PlatformSelection>();
            _playerData = platformSelection.PlayerData;
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

                    var x = Input.GetAxis("Horizontal");

                    // slowly tilt the platform
                    CmdTiltRaft(x);

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
                    var z = Input.GetAxis("Vertical");

                    if (PlayerRole == Role.Floater && x != 0)
                    {
                        Move(gameObject, x, 0);
                    }
                    else if (PlayerRole == Role.Paddler && z != 0)
                    {
                        Move(gameObject, 0, z);
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
        if (ControlledByServer)
        {
            UpdatePlayerPosition();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        if (_raft == null)
        {
            _raft = GameObject.FindGameObjectWithTag("Platform");
            _floatingPlatform = _raft.GetComponent<FloatingPlatform>();
        }
        if (_instructionManager == null)
        {
            _instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
        }
        if (isLocalPlayer)
        {
            ShowInstructions();
        }
        if (_playerText.text != SyncNickName && SyncNickName != "")
        {
            _playerText.text = SyncNickName;
        }
        if (isLocalPlayer && PlayerID == "")
        {
            CmdSetPlayerData(_playerData);
        }

        if (isLocalPlayer)
        {
            _timeSinceLastMove += Time.deltaTime;
            if (_timeSinceLastMove >= _idleTime)
            {
                _timeSinceLastMove = 0;
                // Player has been idle, send a message
                CmdBeenIdle();
            }
        }
    }

    /// <summary>
    /// Determine which instructions should be shown to the player
    /// </summary>
    private void ShowInstructions()
    {
        if (!_floatingPlatform.OnWater)
        {
            if (!_hasMoved)
            {
                _instructionManager.ShowMovement(PlayerRole, transform.position.x);
            }
            else
            {
                _instructionManager.DisableMoveInstruction();
            }
            if (!HoldingPlatform && _floatingPlatform.CanPickUp && IsNextToGetPlatform())
            {
                _instructionManager.ShowMoveToPlatformIndicator();
                if (_floatingPlatform.InRange(gameObject))
                {
                    _instructionManager.ShowInteractIndicator();
                }
                else
                {
                    _instructionManager.DisableInteractInstruction();
                }
            }
            else if (HoldingPlatform)
            {
                _instructionManager.ShowMoveToPlaceIndicator(PlayerRole, transform.position.x);
                if ((_floatingPlatform.CanBePlacedOnLand() && PlayerRole == Role.Paddler) ||
                    (_floatingPlatform.CanBePlacedInWater() && PlayerRole == Role.Floater))
                {
                    _instructionManager.ShowInteractIndicator();
                }
                else
                {
                    _instructionManager.DisableInteractInstruction();
                }
            }
            else if (_hasMoved)
            {
                _instructionManager.DisableInsctructions();
            }
        }
        else
        {
            _instructionManager.DisableInsctructions();
            if (PlayerRole == Role.Paddler && !PaddlePrompt.activeSelf && !_usedPaddle)
            {
                var multiplier = transform.position.x < 0 ? 1f : -1f;
                PaddlePrompt.transform.localScale = new Vector3(PaddlePrompt.transform.localScale.x, PaddlePrompt.transform.localScale.y, PaddlePrompt.transform.localScale.z * multiplier);
                PaddlePrompt.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Check if the current player is next to grab the platfomr
    /// </summary>
    /// <returns>current player is next</returns>
    private bool IsNextToGetPlatform()
    {
        if (PlayerRole == Role.Floater && _raft.transform.position.z <= 1.0f)
        {
            return true;
        }
        else if (PlayerRole == Role.Paddler && _raft.transform.position.z > 1.0f)
        {
            if (transform.position.x < 0 && _raft.transform.position.x < 0)
            {
                // the player and raft are on the left
                return true;
            }
            else if (transform.position.x > 0 && _raft.transform.position.x > 0)
            {
                // the player and raft are on the right
                return true;
            }
        }
        return false;
    }

    private void Move(GameObject go, float x, float z)
    {
        _hasMoved = true;

        x *= MovementSpeed * SpeedModifier * DirectionModifier;
        z *= MovementSpeed * SpeedModifier * DirectionModifier;

        // HACK clamp position to stop running out of the world
        var newX = Mathf.Clamp(go.transform.position.x + x, -5f, 5f);
        var newZ = Mathf.Clamp(go.transform.position.z + z, -1f, 16f);

        go.transform.position = new Vector3(newX, go.transform.position.y, newZ);
        go.transform.LookAt(new Vector3(go.transform.localPosition.x + x, go.transform.localPosition.y, go.transform.localPosition.z + z));

        if (_animationState != AnimationState.WALKING)
        {
            CmdChangeState((int)AnimationState.WALKING);
        }
        _timeSinceLastMove = 0f;
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

    [Command]
    private void CmdSetPlayerData(PlatformSelection.PSLPlayerData data)
    {
        if (string.IsNullOrEmpty(PlayerID))
        {
            if (string.IsNullOrEmpty(data.PlayerId))
            {
                data.NickName = "Testing";
                data.MatchId = "-1";
                data.PlayerId = System.Guid.NewGuid().ToString();
            }

            SyncNickName = data.NickName;
            PlayerID = data.PlayerId;
            PSL_LRSManager.Instance.JoinedGame(data.MatchId, data.PlayerId);
            PlatformSelection.UpdatePlayers(_gameManager.GetAllPlayers().Select(p =>p.PlayerID).ToList());
        }

    }


    // Server moves the player and forces them to a position
    [Server]
    public void SyncForceMove(Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.eulerAngles = rotation;

        RealPosition = position;
        RealRotation = rotation;

        RpcUpdatePosition(position, rotation);
    }

    [Server]
    public void SetGoalReached(bool onPlatform)
    {
        _gameManager.DistributingRewards = true;
        OnPlatform = onPlatform;
    }
    
    [ClientRpc]
    private void RpcUpdatePosition(Vector3 position, Vector3 rotation)
    {
        RealPosition = position;
        RealRotation = rotation;

        UpdatePlayerPosition();
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

        ControlledByServer = false;
    }

    public void HitObstacle()
    {
        if (isLocalPlayer)
        {
            CmdHitObstacle();
        }
    }

    #region game actions

    [Command]
    private void CmdHitObstacle()
    {
        _gameManager.PlayerAction(PlayerAction.HitObstacle, PlayerID);
    }

    public void GotCollectible()
    {
        if (isLocalPlayer)
        {
            CmdGotCollectible();
        }
    }

    [Command]
    private void CmdGotCollectible()
    {
        _gameManager.PlayerAction(PlayerAction.GotCollectible, PlayerID);
    }

    [Command]
    private void CmdBeenIdle()
    {
        _gameManager.PlayerAction(PlayerAction.Idle, PlayerID);
    }

    public void ReachedChest()
    {
        if (isLocalPlayer)
        {
            CmdReachedGoal();
        }
        else if (isServer)
        {
            _gameManager.GroupAction(PlayerAction.ReachedChest);
        }
    }

    public void ReachedChest(bool success)
    {
        if (isLocalPlayer)
        {
            CmdTargetCalculated(success);
        }
        else if (isServer)
        {
            if (success)
            {
                _gameManager.GroupAction(PlayerAction.ReachedChestSuccess);
            }
            else
            {
                _gameManager.GroupAction(PlayerAction.ReachedChestFail);
            }
            
        }
    }

    [Command]
    private void CmdReachedGoal()
    {
        _gameManager.GroupAction(PlayerAction.ReachedChest);
    }

    [Command]
    private void CmdTargetCalculated(bool success)
    {
        if (success)
        {
            _gameManager.GroupAction(PlayerAction.ReachedChestSuccess);
        }
        else
        {
            _gameManager.GroupAction(PlayerAction.ReachedChestFail);
        }
    }

    public void GaveReward(string id)
    {
        if (isLocalPlayer)
        {
            if (id == PlayerID)
            {
                // gave reward to self
                CmdGaveRewardSelf();
            }
            else
            {
                // Shared Reward
                CmdGaveRewardOther();
            }
        }
    }

    [Command]
    private void CmdGaveRewardSelf()
    {
        _gameManager.PlayerAction(PlayerAction.GaveRewardSelf, PlayerID);
    }

    [Command]
    private void CmdGaveRewardOther()
    {
        _gameManager.PlayerAction(PlayerAction.GaveRewardOther, PlayerID);
    }
    #endregion

    // Update is called once per frame
    void FixedUpdate()
    {
        SetAnimation();
        SetPaddle();
        SetPlatform();
        if (_usePaddle)
        {
            if (PaddlePrompt.activeSelf)
            {
                _usedPaddle = true;
                PaddlePrompt.SetActive(false);
            }
            _usePaddle = false;
            SpawnWaveEffect(transform.position);
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

            if (_floatingPlatform.CanPickUp && !_floatingPlatform.OnWater && _floatingPlatform.InRange(gameObject) && !HoldingPlatform)
            {
                // Pickup Plaftorm
                CmdPickupPlatform(_raft);
                return;
            }
            
            // Make sure the player is looking in the correct direction
            transform.LookAt(PlayerRole == Role.Floater
                ? new Vector3(transform.position.x, transform.position.y, 100f)
                : new Vector3(0f, transform.position.y, transform.position.z));

            if (HoldingPlatform)
            {
                if (_floatingPlatform.CanBePlacedInWater() && PlayerRole == Role.Floater)
                {
                    // Place in water
                    CmdDropPlatform(_raft);
                    CmdPlacePlatformInWater(_raft, gameObject);
                }
                else if (_floatingPlatform.CanBePlacedOnLand() && PlayerRole == Role.Paddler)
                {
                    // Drop Platform
                    CmdDropPlatform(_raft);
                }
            }
            else
            {

                // Use paddle in water
                CmdUsePaddle(_playerData.PlayerId);
            }
        }

        /////////////////////////////
        // End Local Player Controls
        /////////////////////////////
    }

    private void SpawnWaveEffect(Vector3 playerPos)
    {
        var go = Instantiate(WaveEffect, playerPos, Quaternion.identity);
        var zRot = playerPos.x < 0 ? 0f : 180f;
        go.transform.rotation = Quaternion.Euler(-90f, 0f, zRot);
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

        _gameManager.PlayerAction(PlayerAction.PickedUpPlatform, PlayerID);
    }

    [Command]
    private void CmdDropPlatform(GameObject platform)
    {
        platform.transform.position = new Vector3(transform.position.x, -0.6f, transform.position.z);
        platform.transform.SetParent(null, true);
        HoldingPlatform = false;
        platform.GetComponent<FloatingPlatform>().CanPickUp = !HoldingPlatform;

        _gameManager.PlayerAction(PlayerAction.PlacedPlatform, PlayerID);

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
    private void CmdUsePaddle(string playerId)
    {
        // Check here if the right player as only the server knows the current roles
        if (PlayerRole == Role.Paddler)
        {
            GameObject.Find("Water").GetComponent<WaterBehaviour>().PaddleUsed(this, StrengthModifier);

            _usePaddle = true;
        }
        GameObject.Find("AudioManager").GetComponent<NetworkAudioManager>().Play("Paddle");
        _gameManager.PlayerAction(PlayerAction.Pushed, playerId);
    }

    [Command]
    private void CmdTiltRaft(float direction)
    {
        GameObject.Find("Water").GetComponent<WaterBehaviour>().RaftTilt(this, direction);
    }

    [Command]
    public void CmdSetModel(int model)
    {
        
        Debug.Log("Player set model to: " + model);
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
    public void RpcGoalReached(string player)
    {
        StartCoroutine(FocusOnChest(player));
    }

    private IEnumerator FocusOnChest(string player)
    {
        yield return GameObject.Find("CameraManager").GetComponent<CameraManager>().TransitionToEnd();

        if (isLocalPlayer)
        {
            GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowRewards();
        }
        else
        {
            // Show player selecting reward UI
            GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowPlayerChoosingRewards(player);
        }
    }

    [ClientRpc]
    public void RpcShowSwitchingRoles()
    {
        StartCoroutine(ShowSwitchingRoles());
    }
    private IEnumerator ShowSwitchingRoles()
    {
        var menu = GameObject.Find("MenuManager").GetComponent<MenuManager>();

        menu.ShowSwitchingRolesPrompt();

        yield return new WaitForSeconds(1.5f);

        menu.HideSwitchingRolesPrompt();
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
        _gameManager.NextRound();
        RpcResetCamera();
        RpcRemoveRewards();
        _gameManager.DistributingRewards = false;
    }

    [ClientRpc]
    private void RpcResetCamera()
    {
        StartCoroutine(GameObject.Find("CameraManager").GetComponent<CameraManager>().TransitionToStart());
    }

    [ClientRpc]
    private void RpcRemoveRewards()
    {
        // make sure rewards are removed
        var rewards = GameObject.FindGameObjectsWithTag("Reward");
        foreach (var reward in rewards)
        {
            Destroy(reward);
        }
        GameObject.Find("MenuManager").GetComponent<MenuManager>().HideRewards();
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

    public void AssignSpeedBoost(string playerID, float speedIncrement)
    {
        if (isLocalPlayer)
        {
            CmdAssignSpeedBoost(playerID, speedIncrement);
        }
    }

    [Command]
    public void CmdAssignSpeedBoost(string playerID, float increment)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        var player = _gameManager.GetPlayer(playerID);

        // Assign the reward
        player.SpeedModifier += increment;
    }

    public void AssignReverseControls(string playerIndex, float modifier)
    {
        if (isLocalPlayer)
        {
            CmdAssignReverseControls(playerIndex, modifier);
        }
    }

    [Command]
    public void CmdAssignReverseControls(string playerID, float modifier)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        var player = _gameManager.GetPlayer(playerID);

        // Assign the reward
        player.DirectionModifier *= modifier;
    }

    public void AssignMoreControl(string playerIndex, float increment)
    {
        if (isLocalPlayer)
        {
            CmdAssignMoreControl(playerIndex, increment);
        }
    }

    [Command]
    public void CmdAssignMoreControl(string playerID, float increment)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        var player = _gameManager.GetPlayer(playerID);

        // Assign the reward
        player.RaftControlModifier += increment;
    }

    public void AssignMoreStrength(string playerIndex, float increment)
    {
        if (isLocalPlayer)
        {
            CmdAssignMoreStrength(playerIndex, increment);
        }
    }

    [Command]
    public void CmdAssignMoreStrength(string playerID, float increment)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        var player = _gameManager.GetPlayer(playerID);

        // Assign the reward
        player.StrengthModifier += increment;
    }

    public void SetLesson(string year, string lesson)
    {
        if (isLocalPlayer)
        {
            CmdSetLesson(year, lesson);
        }
    }

    [Command]
    public void CmdSetLesson(string year, string lesson)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
            
        _gameManager.SetLesson(year, lesson);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
        }
    }
}
