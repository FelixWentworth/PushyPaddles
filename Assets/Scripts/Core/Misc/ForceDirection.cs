using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDirection : MonoBehaviour
{

    public Vector3 direction;

    void Update ()
    {
		transform.LookAt(transform.position + direction);
	}
}
