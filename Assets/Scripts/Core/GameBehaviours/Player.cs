using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MovingObject
{
    public string Name;
	public TEST_SimplePlayerAI SimplePlayerAi;
    [SyncVar] public float DirectionModifier = 1;
    [SyncVar] public float SpeedModifier = 1f;
    [SyncVar] public float RaftControlModifier = 1f;
    [SyncVar] public float StrengthModifier = 1f;

    [SyncVar] public bool HoldingPlatform;
    [SyncVar] public bool CanInteract;
	[SyncVar] public bool ControlledByServer;

    [SyncVar] public int PlayerNum;

    public GameObject Paddle;
    public GameObject Platform;
    public GameObject PaddlePrompt;

    public GameObject WaveEffect;

    public GameObject OnScreenControls;

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
    [SyncVar] public string SyncNickName = "";
    [SyncVar] public string PlayerID;
#if PSL_ENABLED
    private PlatformSelection.PSLPlayerData _playerData;
#endif
	[SyncVar] public Vector3 RealPosition;
    [SyncVar] public Vector3 RealRotation;
    private float _elapsedTime;
    private float _updateInterval = 0.11f; // 9 times a second

    [SyncVar] public bool OnPlatform;
    
    private Rigidbody _rigidbody;
	[SerializeField] private TextMesh _playerText;

	private GameManager _gameManager;

    private InstructionManager _instructionManager;
    private Controls_UI _controls;
    private bool _hasMoved;
    private bool _usedPaddle;
    private GameObject _raft;
    private FloatingPlatform _floatingPlatform;
    
    private float _timeSinceLastMove = 0f;
    [SerializeField] private float _idleTime;

    private Vector3 _targetPosition;
    private GameObject _targetGameObject;

    public override void Start()
    {
	    SimplePlayerAi.enabled = false;

		CanFloat = false;
        CanMove = true;
        PlayerCanInteract = false;
        PlayerCanHit = false;
        CanRespawn = true;

        RespawnLocation.Add(transform.position);

        _holdingGameObject = null;

        //GetComponent<Rigidbody>().isKinematic = !isServer;
        _rigidbody = GetComponent<Rigidbody>();
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

	    if (isServer)
	    {
			CanInteract = true;
		}

        if (!isServer && isLocalPlayer)
        {
	        
            // Client needs to be ready to send command
            if (!ClientScene.ready)
            {
                ClientScene.Ready(NetworkManager.singleton.client.connection);
            }

            GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowHowToPlay();
#if PSL_ENABLED
            if (!SP_Manager.Instance.IsSinglePlayer())
            {

                var platformSelection = GameObject.Find("PlatformManager").GetComponent<PlatformSelection>();
                _playerData = platformSelection.PlayerData;
            
                CmdSetPlayerData(_playerData);

            }

            _playerText.text = _playerData.NickName;
#endif
		}
	    _targetPosition = transform.position;

		if (SP_Manager.Instance.IsSinglePlayer())
        {
            SP_Manager.Instance.Get<SP_Menus>().ShowHowToPlay();
            GameObject.Find("CharacterSelection").GetComponent<CharacterSelection>().Set(this);
        }
    }

    public override void ResetObject(Vector3 newPosition)
    {
        base.ResetObject(newPosition);

        if (SP_Manager.Instance.IsSinglePlayer() || isServer)
        {
            SyncRespawn(transform.eulerAngles);
        }
        else if (!isServer)
        {
            CmdSyncRespawn(transform.eulerAngles);
        }
    }
    //
//#if UNITY_ANDROID || UNITY_IPHONE
//    private void SetupOnScreenControls()
//    {
//        var go = Instantiate(OnScreenControls);
//        _controls = go.GetComponent<Controls_UI>();

//        _controls.ListenTo(MoveLeft, MoveRight, MoveUp, MoveDown, StopMoving, Interact);
//        _controls.SetPlayer(this);
//        if (_instructionManager == null)
//        {
//            _instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
//        }
//        _instructionManager.SetUIControls(_controls);
//    }
//#endif

    private void MoveLeft()
    {
        if (!OnPlatform)
        {
            Move(gameObject, -1, 0);
        }
        else
        {
            UpdatePlayerPosition();
            // slowly tilt the platform
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                TiltRaft(-1);
            }
            else
            {
                CmdTiltRaft(-1);
            }
        }
    }
    private void MoveRight()
    {
        if (!OnPlatform)
        {
            Move(gameObject, 1, 0);
        }
        else
        {
            UpdatePlayerPosition();
            // slowly tilt the platform
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                TiltRaft(1);
            }
            else
            {
                CmdTiltRaft(1);
            }
        }
    }
    private void MoveUp()
    {
        Move(gameObject, 0, 1);

    }
    private void MoveDown()
    {
        Move(gameObject, 0, -1);
    }

    private void StopMoving()
    {
        if (_animationState != AnimationState.IDLE)
        {
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                ChangeState((int) AnimationState.IDLE);
            }
            else
            {
                CmdChangeState((int) AnimationState.IDLE);
            }
        }
    }
    public void Interact()
    {
        InteractPressed();
    }

    public void MoveToAndUse(Vector3 position, GameObject moveToObject)
    {
        // Clear the previous destination if it was still active
        if (_targetGameObject != null)
        {
            DestroyImmediate(_targetGameObject);
        }
        _targetPosition = position;
        _targetGameObject = moveToObject;
    }

	public void MoveTo(Vector3 position)
	{
		_targetPosition = position;
	}

    private void UpdatePosition(bool interactWhenReachPosition)
    {
        if (Vector3.Distance(transform.position, _targetPosition) < 0.35f)
        {
            if (_targetGameObject != null)
            {
                Destroy(_targetGameObject);
            }
            _targetPosition = transform.position;
	        if (interactWhenReachPosition)
	        {
		        InteractPressed();
	        }
            return;
        }

        if (PlayerRole == Role.Floater)
        {
            // left/right
            if (transform.position.x > _targetPosition.x)
            {
                Move(gameObject, -1, 0);
            }
            else
            {
                Move(gameObject, 1, 0);
            }
        }
        else
        {
            // up/down
            if (transform.position.z > _targetPosition.z)
            {
                Move(gameObject, 0, -1);
            }
            else
            {
                Move(gameObject, 0, 1);
            }
        }
    }

//#endif
    public override void OnStartLocalPlayer()
    {
        GameObject.Find("CharacterSelection").GetComponent<CharacterSelection>().Set(this);
    }

    void Update()
    {
		// Switch between autopilot and normal
	    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
	    {
		    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		    {
			    if (Input.GetKeyDown(KeyCode.L))
			    {
				    SimplePlayerAi.enabled = !SimplePlayerAi.enabled;
			    }
		    }
	    }

        // Don't allow players to move whilst rewards are being distributed
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
	    if (_gameManager != null)
	    {
		    _floatingPlatform = _gameManager.FloatingPlatform;
		    _raft = _floatingPlatform.gameObject;
	    }
        if (!_gameManager.DistributingRewards)
        { 
            if (isLocalPlayer || SP_Manager.Instance.IsSinglePlayer())
            {
				if (OnPlatform)
                {
                    // Stopped moving
                    if (_animationState != AnimationState.ONPLATFORM)
                    {
                        if (SP_Manager.Instance.IsSinglePlayer())
                        {
                            ChangeState((int)AnimationState.ONPLATFORM);
                        }
                        else
                        {
                            CmdChangeState((int)AnimationState.ONPLATFORM);
                        }
                    }

                    UpdatePlayerPosition();

                    var x = Input.GetAxis("Horizontal");

                    // slowly tilt the platform
                    if (SP_Manager.Instance.IsSinglePlayer())
                    {
                        TiltRaft(x);
                    }
                    else
                    {
                        CmdTiltRaft(x);
                    }

                }
                else 
                {
                    if (!_gameManager.GamePlaying())
                    {
                        // Game Paused, Cannot move
                        return;
                    }
//#if UNITY_ANDROID || UNITY_IPHONE
//                    if (_controls == null)
//                    {
//                        SetupOnScreenControls();
//                    }
//#endif
                    if (!_rigidbody.useGravity)
                    {
                        // when the player has full control, they should be affected by gravity
                        _rigidbody.useGravity = true;
                    }

                    if (SP_Manager.Instance.IsSinglePlayer() || Touch_Movement.UseTouch)
                    {
                        if (_targetPosition != transform.position && !Respawning)
                        {
	                        UpdatePosition(_targetGameObject != null);
                        }
                    }
	                if (!Touch_Movement.UseTouch)
	                {
						// keep target position updated
		                _targetPosition = transform.position;
	                
					
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
						else if (_targetGameObject == null)
		                {
			                // Stopped moving
			                if (_animationState != AnimationState.IDLE)
			                {
				                if (SP_Manager.Instance.IsSinglePlayer())
				                {
					                ChangeState((int) AnimationState.IDLE);
				                }
				                else
				                {
					                CmdChangeState((int) AnimationState.IDLE);
				                }
			                }


		                }
	                }
	                else
	                {
						if (_targetGameObject == null)
						{
							// Stopped moving
							if (_animationState != AnimationState.IDLE)
							{
								if (SP_Manager.Instance.IsSinglePlayer())
								{
									ChangeState((int)AnimationState.IDLE);
								}
								else
								{
									CmdChangeState((int)AnimationState.IDLE);
								}
							}
						}
					}
                    _elapsedTime += Time.deltaTime;
                    if (_elapsedTime > _updateInterval)
                    {
                        _elapsedTime = 0f;
                        if (SP_Manager.Instance.IsSinglePlayer())
                        {
                            Move(transform.position, transform.eulerAngles);
                        }
                        else
                        {
                            CmdSyncMove(transform.position, transform.eulerAngles);
                        }
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

        if (_instructionManager == null)
        {
            _instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
        }
        // Game Instructions
        if (isLocalPlayer)
        {
            ShowInstructions();
		}
        else if (SP_Manager.Instance.IsSinglePlayer())
        {
            if (PlayerRole == Role.Floater)
            {
                if (_floatingPlatform.CanPickUp)
                {
                    SP_Manager.Instance.Get<SP_GameManager>().ShowPlatformPickupIndicator();
                }
                else
                {
                    SP_Manager.Instance.Get<SP_GameManager>().ShowPlaceIndicator();
					HoldPlatform(_raft);
				}

                if (OnPlatform)
                {
                    SP_Manager.Instance.Get<SP_GameManager>().UpdatePushIndicator(this);
                }
            }
            else 
            {
                if (_floatingPlatform.OnWater)
                {
                    if (!SP_Manager.Instance.Get<SP_GameManager>()._usedPaddle)
                    {
                        SP_Manager.Instance.Get<SP_GameManager>().ShowPushIndicator();
                    }
                    else 
                    {
                        SP_Manager.Instance.Get<SP_GameManager>().HideIndicators();
                    }
                }
                else
                {
					SP_Manager.Instance.Get<SP_GameManager>().HideSinglePlayerPushIndicator();
				}
            }
        }

        if (_playerText.text != SyncNickName && SyncNickName != "")
        {
	        _playerText.text = SyncNickName;
        }
        if (isLocalPlayer && PlayerID == "" && !SP_Manager.Instance.IsSinglePlayer())
        {
#if PSL_ENABLED
            CmdSetPlayerData(_playerData);
#endif
		}

		if (!SP_Manager.Instance.IsSinglePlayer())
        {
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
    }

    /// <summary>
    /// Determine which instructions should be shown to the player
    /// </summary>
    private void ShowInstructions()
    {
        if (!_floatingPlatform.OnWater)
        {
	        PaddlePrompt.SetActive(false);
			_instructionManager.DisableTouchPushInstruction();
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
	            if (_floatingPlatform.CanPickUp || _floatingPlatform.gameObject.activeSelf)
	            {
		            // player should be holding this
			        CmdHoldPlatform(_raft);
	            }
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
            if (PlayerRole == Role.Paddler && !_usedPaddle)
            {
                if (_controls != null)
                {
                    _controls.AnimateInteract(true);
                }
                else if (!Touch_Movement.UseTouch && !PaddlePrompt.activeSelf)
                {
                    var multiplier = transform.position.x < 0 ? 1f : -1f;
                    PaddlePrompt.transform.localScale = new Vector3(PaddlePrompt.transform.localScale.x, PaddlePrompt.transform.localScale.y, PaddlePrompt.transform.localScale.z * multiplier);
                    PaddlePrompt.SetActive(true);
                }
                else if (Touch_Movement.UseTouch)
                {
	                _instructionManager.ShowTouchPush(transform.position.x < 0);
                }
			}
	        if (Touch_Movement.UseTouch)
	        {
		        if (!_usedPaddle)
		        {
			        _instructionManager.UpdatePushSinglePlayer(new Vector3(0f, 0f, _raft.transform.position.z));
		        }
		        else
		        {
			        _instructionManager.DisableTouchPushInstruction();
		        }
	        }
        }
    }

    /// <summary>
    /// Check if the current player is next to grab the platfomr
    /// </summary>
    /// <returns>current player is next</returns>
    public bool IsNextToGetPlatform()
    {
        if (PlayerRole == Role.Floater && _raft.transform.position.z <= 1.5f)
        {
            return true;
        }
        if (PlayerRole == Role.Paddler && _raft.transform.position.z > 1.5f)
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

	public void AIMove(GameObject go, float x, float z)
	{
		Move(go, x, z);
	}

    private void Move(GameObject go, float x, float z)
    {
        _hasMoved = true;

        x *= Time.deltaTime * 60;
        z *= Time.deltaTime * 60;

        x *= MovementSpeed * SpeedModifier * DirectionModifier;
        z *= MovementSpeed * SpeedModifier * DirectionModifier;

		// make sure if players are using touch their direction is not reversed.
		//TODO make it so players cannot received reversed controls if they are currently using touch
	    if (Touch_Movement.UseTouch)
	    {
			// reapply direction modifier to make direction positive again
		    x *= DirectionModifier;
			z *= DirectionModifier;
		}

        // HACK clamp position to stop running out of the world
        var newX = Mathf.Clamp(go.transform.position.x + x, -5f, 5f);
        var newZ = Mathf.Clamp(go.transform.position.z + z, -1f, 16f);

        go.transform.position = new Vector3(newX, go.transform.position.y, newZ);
        go.transform.LookAt(new Vector3(go.transform.localPosition.x + x, go.transform.localPosition.y, go.transform.localPosition.z + z));

        if (_animationState != AnimationState.WALKING)
        {
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                ChangeState((int) AnimationState.WALKING);
            }
            else if (isLocalPlayer)
            {
                CmdChangeState((int) AnimationState.WALKING);
            }
        }
        _timeSinceLastMove = 0f;
    }

	void OnApplicationFocus(bool hasFocus)
	{
		// Verify UI for rewards should be shown
		if (CanMove && !_gameManager.DistributingRewards)
		{
			GameObject.Find("MenuManager").GetComponent<MenuManager>().HideRewards();
		}
		else
		{
			
		}
	}

	private void UpdatePlayerPosition()
    {
	    if (Respawning)
	    {
		    return;
	    }
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            // no need to synchronise movements
            return;
        }
        if (_rigidbody.useGravity)
        {
            // Don't override actual location using gravity, causes player jumping
            _rigidbody.useGravity = false;
        }
        // Lerp to real position
        transform.position = Vector3.Lerp(transform.position, RealPosition, 0.5f);
        transform.eulerAngles = RealRotation;
    }

#if PSL_ENABLED
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
            if (!SP_Manager.Instance.IsSinglePlayer())
            {
                _gameManager.RpcSetLanguage(Localization.SelectedLanguage.Name);
            }
            else
            {
                _gameManager.ClientSetLanguage(Localization.SelectedLanguage.Name);
            }
        }

    }

#endif

	// Server moves the player and forces them to a position
	[ServerAccess]
    public void SyncForceMove(Vector3 position, Vector3 rotation)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        transform.position = position;
        transform.eulerAngles = rotation;

        RealPosition = position;
        RealRotation = rotation;

        if (!SP_Manager.Instance.IsSinglePlayer())
        {
            RpcUpdatePosition(position, rotation);
        }
        else
        {
            ClientUpdatePosition(position, rotation);
        }
    }

    [ServerAccess]
    public void SetGoalReached(bool onPlatform)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        _gameManager.DistributingRewards = true;
        OnPlatform = onPlatform;
    }
    
    [ClientRpc]
    private void RpcUpdatePosition(Vector3 position, Vector3 rotation)
    {
        ClientUpdatePosition(position, rotation);
    }

    [ClientAccess]
    private void ClientUpdatePosition(Vector3 position, Vector3 rotation)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        RealPosition = position;
        RealRotation = rotation;

        UpdatePlayerPosition();
    }

    [Command]
    public void CmdSyncMove(Vector3 position, Vector3 rotation)
    {
        Move(position, rotation);
    }

    [ServerAccess]
    public void Move(Vector3 position, Vector3 rotation)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (position.y < -10f)
        {
            return;
        }
        RealPosition = position;
        RealRotation = rotation;
    }

    [ServerAccess]
    public void SyncRespawn(Vector3 rotation)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        SyncForceMove(_gameManager.GetPlayerRespawn(PlayerNum), rotation);

        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ClientRespawn(RespawnLocation[0], rotation);
	        HoldingPlatform = false;
		}
		else
        {
            RpcRespawn(_gameManager.GetPlayerRespawn(PlayerNum), rotation);
        }
    }

    [Command]
    private void CmdSyncRespawn(Vector3 rotation)
    {
        SyncRespawn(rotation);
    }


    [ClientRpc]
    private void RpcRespawn(Vector3 position, Vector3 rotation)
    {
        ClientRespawn(position, rotation);
    }

    [ClientAccess]
    private void ClientRespawn(Vector3 position, Vector3 rotation)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        transform.position = position;
        transform.eulerAngles = rotation;

        ControlledByServer = false;
    }

    public void HitObstacle()
    {
        if (isLocalPlayer && !SP_Manager.Instance.IsSinglePlayer())
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
        if (isLocalPlayer && !SP_Manager.Instance.IsSinglePlayer())
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
        if (!SP_Manager.Instance.IsSinglePlayer())
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
    }

    public void ReachedChest(bool success)
    {
        if (!SP_Manager.Instance.IsSinglePlayer())
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
        if (!SP_Manager.Instance.IsSinglePlayer())
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
            if (_controls != null)
            {
                _controls.AnimateInteract(false);
            }
            if (PaddlePrompt.activeSelf ||(SP_Manager.Instance.IsSinglePlayer() || Touch_Movement.UseTouch) && _floatingPlatform.OnWater)
            {
                _usedPaddle = true;
                PaddlePrompt.SetActive(false);
			}
	        _instructionManager.DisableTouchPushInstruction();

			_usePaddle = false;
            SpawnWaveEffect(transform.position);
        }
        if (_playerModel != _currentModel)
        {
            SetModel();
            _currentModel = _playerModel;
        }
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            if (SP_Manager.Instance.Get<SP_GameManager>().GetModelNumber() != _playerModel)
            {
                _playerModel = SP_Manager.Instance.Get<SP_GameManager>().GetModelNumber();
                _currentModel = _playerModel;
                SetModel();
            }
        }

        /////////////////////////
        // Local Player Controls
        /////////////////////////

        
        if ((isLocalPlayer || SP_Manager.Instance.IsSinglePlayer()) && !Respawning && Input.GetKeyDown(KeyCode.Space))
        {
            InteractPressed();
        }

        /////////////////////////////
        // End Local Player Controls
        /////////////////////////////
    }

	public void AIPressed()
	{
		InteractPressed();
	}

	public override void Respawned()
	{
		base.Respawned();
		CanInteract = true;
	}

	private void InteractPressed()
    {
	    if (!CanInteract && !SP_Manager.Instance.IsSinglePlayer() && !Touch_Movement.UseTouch)
	    {
		    return;
	    }
        if (_floatingPlatform != null && _floatingPlatform.CanPickUp && !_floatingPlatform.OnWater && IsNextToGetPlatform() && _floatingPlatform.InRange(gameObject) && !HoldingPlatform)
        {
            // Pickup Plaftorm
            if (SP_Manager.Instance.IsSinglePlayer() && PlayerRole == Role.Floater)
            {
                // We do not need to pass platform from paddler to floater in Single player
                PickupPlatform(_raft);
            }
            else if (isLocalPlayer)
            {
                CmdPickupPlatform(_raft);
            }
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
                if (SP_Manager.Instance.IsSinglePlayer())
                {
                    DropPlatform(_raft);
                    PlacePlatformInWater(_raft, gameObject);
                }
                else if (isLocalPlayer)
                {
                    // Place in water
                    CmdDropPlatform(_raft);
                    CmdPlacePlatformInWater(_raft, gameObject);
                }
            }
            else if (_floatingPlatform.CanBePlacedOnLand() && PlayerRole == Role.Paddler)
            {
                if (SP_Manager.Instance.IsSinglePlayer())
                {
                    DropPlatform(_raft);
                }
                else if (isLocalPlayer)
                {
                    // Drop Platform
                    CmdDropPlatform(_raft);
                }
            }
        }
        else
        {
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                UsePaddle(PlayerID);
            }
            else if (isLocalPlayer)
            {
				// Use paddle in water
#if PSL_ENABLED
                CmdUsePaddle(_playerData.PlayerId);
#endif
			}
		}
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
        PickupPlatform(platform);

    }

    [ServerAccess]
    private void PickupPlatform(GameObject platform)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
		// Check if any other players are holding the platform
	    var platformHeld = _gameManager.GetAllPlayers().Any(p => p.HoldingPlatform);

		if (platformHeld)
	    {
			// Dont let the platform be picked up as someone else has picked it up
		    return;
	    }


        if (OnPlatform)
            return;
        HoldingPlatform = true;
        
        if (!SP_Manager.Instance.IsSinglePlayer())
        {
            _gameManager.PlayerAction(PlayerAction.PickedUpPlatform, PlayerID);
        }
    }

	[Command]
	private void CmdHoldPlatform(GameObject platform)
	{
		HoldPlatform(platform);
	}

	[ServerAccess]
	private void HoldPlatform(GameObject platform)
	{
		var method = MethodBase.GetCurrentMethod();
		var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
		if (!attr.HasAccess)
		{
			return;
		}
		var fp = platform.GetComponent<FloatingPlatform>();
		//platform.transform.SetParent(this.transform, true);
		platform.transform.localPosition = new Vector3(transform.position.x, -0.75f, transform.position.z);
	}

	[Command]
    private void CmdDropPlatform(GameObject platform)
    {
        DropPlatform(platform);
    }

    [ServerAccess]
    public void DropPlatform(GameObject platform)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
	    //platform.transform.SetParent(null, true);
		platform.transform.position = new Vector3(transform.position.x, -0.75f, transform.position.z);
        HoldingPlatform = false;

		if (!SP_Manager.Instance.IsSinglePlayer())
        {
            _gameManager.PlayerAction(PlayerAction.PlacedPlatform, PlayerID);
        }
    }

    [Command]
    private void CmdPlacePlatformInWater(GameObject platform, GameObject go)
    {
        PlacePlatformInWater(platform, go);
    }

    [ServerAccess]
    private void PlacePlatformInWater(GameObject platform, GameObject go)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
	    CanInteract = false;

		var player = go.GetComponent<Player>();
        player.OnPlatform = true;

        if (player.PlayerRole != Role.Floater)
        {
            // only the server knows each player role, so do this check here
            return;
        }
        var start = GameObject.Find("PlatformStartPoint/PlatformStart");
        var fp = platform.GetComponent<FloatingPlatform>();

        //fp.CanPickUp = true;
        fp.PlaceOnWater(this, start.transform.position);

    }

    [Command]
    private void CmdChangeState(int newState)
    {
        ChangeState(newState);
    }

    [ServerAccess]
    private void ChangeState(int newState)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        _animState = newState;
    }

    [Command]
    private void CmdUsePaddle(string playerId)
    {
        UsePaddle(playerId);
    }

    [ServerAccess]
    private void UsePaddle(string playerId)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        // Check here if the right player as only the server knows the current roles
        if (PlayerRole == Role.Paddler)
        {
            var strength =  GameObject.Find("Water").GetComponent<WaterBehaviour>().PaddleUsed(this, StrengthModifier);
            if (strength != 0f && SP_Manager.Instance.IsSinglePlayer())
            {
                SP_Manager.Instance.Get<SP_GameManager>()._usedPaddle = true;
            }
            _usePaddle = true;
        }
        GameObject.Find("AudioManager").GetComponent<NetworkAudioManager>().Play("Paddle");
        _gameManager.PlayerAction(PlayerAction.Pushed, playerId);
    }

    [Command]
    private void CmdTiltRaft(float direction)
    {
        TiltRaft(direction);
    }

    [ServerAccess]
    private void TiltRaft(float direction)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        GameObject.Find("Water").GetComponent<WaterBehaviour>().RaftTilt(this, direction);
    }

    [Command]
    public void CmdSetModel(int model)
    {
        
        Debug.Log("Player set model to: " + model);
	    if (PlayerRole == Role.Paddler || !_gameManager.LessonSelectRequired)
	    {
		    IsReady = true;
	    }

	    _playerModel = model;
    }


    /// <summary>
    /// Set single player model
    /// </summary>
    /// <param name="model"></param>
    public void SetSPModel(int model)
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
        ClientGoalReached(player);
    }

    [ClientAccess]
    public void ClientGoalReached(string player)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        StartCoroutine(FocusOnChest(player));

    }

    private IEnumerator FocusOnChest(string player)
    {
		_playerText.gameObject.SetActive(false);
        yield return GameObject.Find("CameraManager").GetComponent<CameraManager>().TransitionToEnd();

        if (SP_Manager.Instance.IsSinglePlayer())
        {
            SP_Manager.Instance.Get<SP_Menus>().ShowRewards();
        }
        else
        {
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
    }

    [ClientRpc]
    public void RpcShowSwitchingRoles()
    {
        ClientShowSwitchingRoles();
    }

    [ClientAccess]
    public void ClientShowSwitchingRoles()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

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
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerRestartGame();
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdRestartGame();
            }
        }
    }

    [Command]
    public void CmdRestartGame()
    {
        ServerRestartGame();
    }

    [ServerAccess]
    public void ServerRestartGame()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        _gameManager.Restart();
    }

    public void NextRound()
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerNextRound();
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdNextRound();

            }
        }
    }

    [Command]
    public void CmdNextRound()
    {
        ServerNextRound();
    }

    [ServerAccess]
    public void ServerNextRound()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
		_gameManager.DistributingRewards = false;
        _gameManager.NextRound();
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ClientResetCamera();
            ClientRemoveRewards();
        }
        else
        {
            RpcResetCamera();
            RpcRemoveRewards();
        }
        
    }

    [ClientRpc]
    private void RpcResetCamera()
    {
        ClientResetCamera();
    }

    [ClientAccess]
    private void ClientResetCamera()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        StartCoroutine(GameObject.Find("CameraManager").GetComponent<CameraManager>().TransitionToStart());
	    _playerText.gameObject.SetActive(true);
	}

	[ClientRpc]
    private void RpcRemoveRewards()
    {
        ClientRemoveRewards();        
    }

    [ClientAccess]
    private void ClientRemoveRewards()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

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
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerStartRoundTimer();
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdStartRoundTimer();
            }
        }
    }

    [Command]
    public void CmdStartRoundTimer()
    {
        ServerStartRoundTimer();
    }

    [ServerAccess]
    public void ServerStartRoundTimer()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        _gameManager.StartGameTimer();
    }

    public void AssignSpeedBoost(Player player, float speedIncrement)
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerAssignSpeedBoost(player, speedIncrement);
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdAssignSpeedBoost(player.PlayerID, speedIncrement);
            }
        }
    }

    [Command]
    public void CmdAssignSpeedBoost(string playerID, float increment)
    {
        var player = _gameManager.GetPlayer(playerID);
        ServerAssignSpeedBoost(player, increment);
    }

    [ServerAccess]
    public void ServerAssignSpeedBoost(Player player, float increment)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Assign the reward
        player.SpeedModifier += increment;
    }

    public void AssignReverseControls(Player player, float modifier)
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerAssignReverseControls(player, modifier);
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdAssignReverseControls(player.PlayerID, modifier);
            }
        }
    }

    [Command]
    public void CmdAssignReverseControls(string playerID, float modifier)
    {
        var player = _gameManager.GetPlayer(playerID);
        ServerAssignReverseControls(player, modifier);
    }

    [ServerAccess]
    public void ServerAssignReverseControls(Player player, float modifier)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Assign the reward
        player.DirectionModifier *= modifier;
    }

    public void AssignMoreControl(Player player, float increment)
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerAssignMoreControl(player, increment);
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdAssignMoreControl(player.PlayerID, increment);
            }
        }
    }

    [Command]
    public void CmdAssignMoreControl(string playerID, float increment)
    {
        var player = _gameManager.GetPlayer(playerID);
        ServerAssignMoreControl(player, increment);
    }

    [ServerAccess]
    public void ServerAssignMoreControl(Player player, float increment)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Assign the reward
        player.RaftControlModifier += increment;
    }

    public void AssignMoreStrength(Player player, float increment)
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerAssignMoreStrength(player, increment);
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdAssignMoreStrength(player.PlayerID, increment);
            }
        }
    }

    [Command]
    public void CmdAssignMoreStrength(string playerID, float increment)
    {
        var player = _gameManager.GetPlayer(playerID);
        ServerAssignMoreStrength(player, increment);
    }

    [ServerAccess]
    public void ServerAssignMoreStrength(Player player, float increment)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Assign the reward
        player.StrengthModifier += increment;
    }

    public void SetLesson(string year, string lesson)
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ServerSetLesson(year, lesson);
        }
        else
        {
            if (isLocalPlayer)
            {
                CmdSetLesson(year, lesson);
            }
        }
    }

    [Command]
    public void CmdSetLesson(string year, string lesson)
    {
        ServerSetLesson(year, lesson);
	    IsReady = true;
	}

	[ServerAccess]
    public void ServerSetLesson(string year, string lesson)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
	    _gameManager.SetLesson(year, lesson);
		PSL_GameConfig.SetGameConfig(year, lesson, "Maths", "Any");
    }


	public void ChangeTide(bool increase)
	{
		if (SP_Manager.Instance.IsSinglePlayer())
		{
			ServerIncreaseTide(increase);
		}
		else
		{
			if (isLocalPlayer)
			{
				CmdIncreaseTide(increase);
			}
		}
	}

	[Command]
	public void CmdIncreaseTide(bool increase)
	{
		ServerIncreaseTide(increase);
	}

	[ServerAccess]
	private void ServerIncreaseTide(bool increase)
	{
		var method = MethodBase.GetCurrentMethod();
		var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
		if (!attr.HasAccess)
		{
			return;
		}
		GameObject.Find("Water").GetComponent<WaterBehaviour>().IncrementTideModifier(increase ? 1 : -1);
	}


	//void OnCollisionEnter(Collision other)
	//{
	//    if (other.gameObject.tag == "Water" && !OnPlatform && !)
	//    {
	//        other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
	//    }
	//}
}
