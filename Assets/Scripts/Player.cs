using System;
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
        Paddle_Left,
        Paddle_Right
    }

    public Role PlayerRole;

    private bool _moving = false;
    private float _direction = 0f;

    private GameObject _holdingGameObject;

    public override void Start()
    {
        CanFloat = false;
        PlayerCanInteract = false;
        PlayerCanHit = false;
        CanRespawn = true;

        RespawnLocation.Add(transform.position);

        _holdingGameObject = null;

        GetComponent<Rigidbody>().isKinematic = !isServer;
    }

    public override void ResetObject()
    {

    }


    // Update is called once per frame
    void FixedUpdate()
    {
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
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var platform = GameObject.FindGameObjectWithTag("Platform");
            if (platform.GetComponent<FloatingPlatform>().CanPickUp && !HoldingPlatform)
            {
                // Drop Plaftorm
                CmdPickupPlatform(platform);
            }
            else if (HoldingPlatform)
            {
                // Pickup Platform
                CmdDropPlatform(platform);
            }
        }
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

    public void Interact()
    {
        var platform = GameObject.FindGameObjectWithTag("Platform").GetComponent<FloatingPlatform>();
        var platformStart = GameObject.FindGameObjectWithTag("PlatformStart").gameObject;
        switch (PlayerRole)
        {
            case Role.Floater:
                if (platform.InRange(gameObject))
                {
                    if (_holdingGameObject == null)
                    {
                        _holdingGameObject = platform.gameObject;
                        _holdingGameObject.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                    }
                    else if (Vector3.Distance(platformStart.transform.position, transform.position) < 3f)
                    {
                        platform.ResetObject();
                        _holdingGameObject.transform.position = platformStart.transform.position;

                        _holdingGameObject = null;
                        platform.PlaceOnWater(this);
                    }
                }
                break;
            case Role.Paddle_Left:
            case Role.Paddle_Right:
                if (platform.InRange(gameObject))
                {
                    if (_holdingGameObject == null)
                    {
                        _holdingGameObject = platform.gameObject;
                        _holdingGameObject.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                    }
                    else
                    {
                        _holdingGameObject.transform.position = new Vector3(_holdingGameObject.transform.position.x, 0.5f, _holdingGameObject.transform.position.z);
                        _holdingGameObject = null;
                    }
                }
                else
                {
                    GameObject.FindGameObjectWithTag("Water").GetComponent<WaterBehaviour>().PaddleUsed(this);
                    GetComponentInChildren<ParticleSystem>().Play();
                }
                break;
            case Role.Unassigned:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            other.gameObject.GetComponent<WaterBehaviour>().TouchedWater(this);       
        }
    }
}
