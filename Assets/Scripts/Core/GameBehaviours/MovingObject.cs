using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class MovingObject : NetworkBehaviour
{
    [SyncVar] public float MovementSpeed;
    [SyncVar] public float RotationSpeed;
    public float RespawnTime = 1f;

    public bool CanFloat;
    public bool CanMove;
    public bool CanRespawn;
    // If the player can use the action button on this object
    public bool PlayerCanInteract;
    // If the player collides with this object
    public bool PlayerCanHit;
    // If the object falls
    public bool CanFall;

    public bool Respawning;

    public List<Vector3> RespawnLocation = new List<Vector3>();
    private Vector3 _initialRotation;


    public virtual void Start()
    {
        _initialRotation = transform.eulerAngles;
    }

    public virtual void ResetObject(Vector3 newPosition)
    {
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.Sleep();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        transform.eulerAngles = _initialRotation;
        SetPosition(newPosition);
    }

    [ServerAccess]
    private void SetPosition(Vector3 position)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        transform.position = position;
    }

    public virtual void Respawn()
    {
        if (!CanRespawn)
        {
            return;
        }
        StartCoroutine(WaitToRespawn());
    }

    public virtual void FellInWater()
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            GameObject.Find("AudioManager").GetComponent<NetworkAudioManager>().Play("Splash");
        }
        if (!isServer)
        {
            CmdFellInWater();
        }
        
    }

    [Command]
    private void CmdFellInWater()
    {
        GameObject.Find("AudioManager").GetComponent<NetworkAudioManager>().Play("Splash");
    }



    private IEnumerator WaitToRespawn()
    {
        Respawning = true;
        yield return new WaitForSeconds(RespawnTime);
        GetComponent<BoxCollider>().enabled = true;

        var randomRespawn = Random.Range(0, RespawnLocation.Count);
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            gameObject.transform.position = RespawnLocation[randomRespawn];
        }
        else if (!isServer)
        {
            CmdRespawn(gameObject, RespawnLocation[randomRespawn]);
        }
        ResetObject(RespawnLocation[randomRespawn]);

        GetComponent<Rigidbody>().useGravity = CanFall;
        Respawning = false;
    }

    [Command]
    private void CmdRespawn(GameObject go, Vector3 pos)
    {
        go.transform.position = pos;
    }
}
