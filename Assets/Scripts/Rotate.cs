using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    public float RotateXSpeed;
    public float RotateYSpeed;
    public float RotateZSpeed;

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(RotateXSpeed, RotateYSpeed, RotateZSpeed) * Time.fixedDeltaTime);
    }
}
