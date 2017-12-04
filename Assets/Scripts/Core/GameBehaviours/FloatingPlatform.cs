using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PlayGen.Unity.Utilities.Localization;
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

    private LevelManager _levelManager;
    
    private Text _pickupText;

    public override void Start()
    {
        base.Start();
        MovementSpeed = 0f;
        CanFloat = true;
        PlayerCanInteract = false;
        PlayerCanHit = true;
        CanRespawn = true;
        CanMove = true;
        

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
        CanMove = true;
        OnWater = false;
        if (isServer)
        {
#if USE_PROSOCIAL
            PSL_LRSManager.Instance.NewAttempt();
#endif
        }
    }

    public void PlaceOnWater(Player player)
    {
        _playerOnPlatform = player;
        OnWater = true;
    }

    void FixedUpdate()
    {
        if (_levelManager == null)
        {
            _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        }
        if (_playerOnPlatform != null && (isServer || SP_Manager.Instance.IsSinglePlayer()) && CanMove)
        {
            // On Water
            GetComponent<BoxCollider>().enabled = true;
            _playerOnPlatform.GetComponent<Rigidbody>().useGravity = false;
            _playerOnPlatform.transform.position = new Vector3(transform.position.x,
                _playerOnPlatform.transform.position.y, transform.position.z);

            var player = _playerOnPlatform.GetComponent<Player>();

            player.SyncForceMove(
                new Vector3(transform.position.x, _playerOnPlatform.transform.position.y, transform.position.z),
                _playerOnPlatform.transform.eulerAngles);

            Water.TouchedWater(this);
        }
        else
        {
            // Not on water
            GetComponent<BoxCollider>().enabled = false;
        }
        _mesh.enabled = CanPickUp;
        if (_levelManager != null)
        {
            _levelManager.Current = PickupValue;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // VICTORY CONDITION
        if (_playerOnPlatform == null)
        {
            return;
        }

        if (other.gameObject.tag == "Obstacle")
        {
			ResetPositions();
		}
		else if (other.gameObject.tag == "Treasure")
        {
#if USE_PROSOCIAL
            PSL_LRSManager.Instance.ChestReached();
#endif
            _playerOnPlatform.GetComponent<Player>().ReachedChest();
            CanMove = false;
            StartCoroutine(GoalReached(other.gameObject));
        }
        else if (other.gameObject.tag == "Collectible")
        {
            _playerOnPlatform.GetComponent<Player>().GotCollectible();

            var operation = other.gameObject.GetComponent<MathsCollectible>().Operation;


            if (_operations.Count == 0 &&
                (operation.Contains("+") || operation.Contains("/") || operation.Contains("x")))
            {
                operation = operation.Substring(1, operation.Length - 1);
                PickupValue = operation;
            }
            else
            {
                PickupValue = _levelManager.Evaluate(PickupValue + operation).ToString();
            }

            _operations.Add(operation);

            if (isServer || SP_Manager.Instance.IsSinglePlayer())
            {
                GameObject.Find("AudioManager").GetComponent<NetworkAudioManager>().Play("Pickup");
                if (isServer)
                {
                    other.GetComponent<MathsCollectible>().RpcPlayCollectedAnimation();
                }
                else if (SP_Manager.Instance.IsSinglePlayer())
                {
                    other.GetComponent<MathsCollectible>().ClientPlayCollectedAnimation();
                }
            }

        }
    }


    public IEnumerator GoalReached(GameObject other)
    {
        var levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        if (levelManager.MathsVersion)
        {
            if (isServer)
            {
                RpcShowTotal(PickupValue == levelManager.Target);
            }
            else if (SP_Manager.Instance.IsSinglePlayer())
            {
                ClientShowTotal(PickupValue == levelManager.Target);
            }

            yield return new WaitForSeconds(levelManager.TotalUI.AnimLength());
        }

        if (PickupValue == levelManager.Target || !levelManager.MathsVersion)
        {
            var player = _playerOnPlatform.GetComponent<Player>();

            if (isServer || SP_Manager.Instance.IsSinglePlayer())
            {
                player.SetGoalReached(false);
                player.ControlledByServer = true;
                player.SyncForceMove(other.transform.Find("VictoryLocation").position,
                    Vector3.zero);
            }
            
            // Get the name of the player who reached the chest
            var playerName = _playerOnPlatform.SyncNickName;

            _playerOnPlatform = null;

            // Notify the players that a reward has been reached
            if (isServer)
            {
                player.RpcGoalReached(playerName);
            }
            else if (SP_Manager.Instance.IsSinglePlayer())
            {
                player.ClientGoalReached(playerName);
            }

            CanFloat = false;
            Water.TouchedWater(this);
        }
        else
        {
            ResetPositions();
        }


        if (isServer && levelManager.MathsVersion && !SP_Manager.Instance.IsSinglePlayer())
        {
            RpcDisableTotal();
        }
        else if (SP_Manager.Instance.IsSinglePlayer() && levelManager.MathsVersion)
        {
            ClientDisableTotal();
        }
    }
	[ServerAccess]
	private void ResetPositions()
	{
		var method = MethodBase.GetCurrentMethod();
		var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
		if (!attr.HasAccess)
		{
			return;
		}
		// Behave as if hit object
		var player = _playerOnPlatform.GetComponent<Player>();
		_playerOnPlatform = null;

		player.GetComponent<BoxCollider>().enabled = false;
		player.FellInWater();
		player.Respawn();
		player.OnPlatform = false;

		CanFloat = false;
		Water.TouchedWater(this);
		MovePaddlersToStart();
		if (isServer || SP_Manager.Instance.IsSinglePlayer())
		{
			GameObject.Find("SpawnedObjects").GetComponent<CollectibleGeneration>().ResetColliders();
		}
	}

	[ServerAccess]
	private void MovePaddlersToStart()
	{
		var method = MethodBase.GetCurrentMethod();
		var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
		if (!attr.HasAccess)
		{
			return;
		}
		var players = GameObject.FindGameObjectsWithTag("Player")
			.Where(p => p.GetComponent<Player>().PlayerRole == Player.Role.Paddler);
		foreach (var player in players)
		{
			var p = player.GetComponent<Player>();
			p.MoveTo(p.RespawnLocation[0]);
		}
	}

	[ClientRpc]
    private void RpcShowTotal (bool victory)
    {
        ClientShowTotal(victory);
    }

    [ClientAccess]
    private void ClientShowTotal(bool victory)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        var levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        levelManager.TotalUI.GetComponent<RectTransform>().localScale = Vector3.zero;
        levelManager.TotalUI.gameObject.SetActive(true);
        levelManager.TotalUI.Show(Localization.Get("UI_GAME_TOTAL"), PickupValue, victory);

        if (victory)
        {
            AudioManager.Instance.Play("Victory");
        }
        else
        {
            AudioManager.Instance.Play("Failure");
        }

    }


    [ClientRpc]
    private void RpcDisableTotal()
    {
        ClientDisableTotal();
    }

    [ClientAccess]
    private void ClientDisableTotal()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

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
        return distance < 2f;

    }

    public bool CanBePlacedInWater()
    {
        // Get Start Point
        var start = GameObject.Find("PlatformStartPoint");

        return Vector3.Distance(start.transform.position, transform.position) < 2.0f;
    }

    public bool CanBePlacedOnLand()
    {
        var leftPlacement = GameObject.FindWithTag("PlatformPlaceLeft");
        var rightPlacement = GameObject.FindWithTag("PlatformPlaceRight");

        return Vector3.Distance(leftPlacement.transform.position, transform.position) < 1.5f ||
                      Vector3.Distance(rightPlacement.transform.position, transform.position) < 1.5f;

    }

}
