using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float MovementSpeed;
    public float RespawnTime = 1f;

    public bool CanFloat;
    public bool CanRespawn;
    // If the player can use the action button on this object
    public bool PlayerCanInteract;
    // If the player collides with this object
    public bool PlayerCanHit;

    public Vector3 RespawnLocation;
    private Vector3 _initialRotation;


    public virtual void Start()
    {
        _initialRotation = transform.eulerAngles;
        Debug.Log(_initialRotation);
    }

    public virtual void ResetObject()
    {
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.Sleep();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        transform.eulerAngles = _initialRotation;
    }

    public virtual void Respawn()
    {
        if (!CanRespawn)
        {
            return;
        }
        StartCoroutine(WaitToRespawn());
    }

    private IEnumerator WaitToRespawn()
    {
        yield return new WaitForSeconds(RespawnTime);
        GetComponent<BoxCollider>().enabled = true;
        ResetObject();
        transform.position = RespawnLocation;

    }

}
