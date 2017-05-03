using System;
using System.Collections;
using UnityEngine;

public class AlphaTransition : MonoBehaviour
{

    /// <summary>
    /// Transition between 2 alpha values
    /// </summary>

    public float MinAlpha = 0f;
    public float MaxAlpha = 1f;
    public float Speed = 1f;
    public bool PingPong;
    public bool Looping;

    private float _currentAlpha;
    private float _direction = -1f;
    private Material _material;

    void Awake()
    {
        _currentAlpha = MaxAlpha;
        _material = GetComponent<Renderer>().material;

    }

    void Update()
    {
        // Direction set to 0 when complete
        if (_direction != 0f)
        {
            if (_direction > 0f)
            {
                // Count up
                _currentAlpha += Time.deltaTime * Speed;
                if (_currentAlpha >= MaxAlpha)
                {
                    // We have reached our target
                    _currentAlpha = MaxAlpha;

                    SetMaterialAlpha();

                    if (Looping)
                    {
                        _currentAlpha = MinAlpha;
                    }
                    else if (PingPong)
                    {
                        _direction = -1f;
                    }
                    else
                    {
                        _direction = 0f;
                    }
                }
                else
                {
                    SetMaterialAlpha();
                }
            }
            else
            {
                // Count down
                _currentAlpha -= Time.deltaTime * Speed;
                if (_currentAlpha <= MinAlpha)
                {
                    // We have reached our target
                    _currentAlpha = MinAlpha;

                    SetMaterialAlpha();

                    if (Looping)
                    {
                        _currentAlpha = MaxAlpha;
                    }
                    else if (PingPong)
                    {
                        _direction = 1f;
                    }
                    else
                    {
                        _direction = 0f;
                    }
                }
                else
                {
                    SetMaterialAlpha();
                }
            }
            Debug.Log("YEYE");
        }
    }

    private void SetMaterialAlpha()
    {
        _material.color = new Color(_material.color.r, _material.color.g, _material.color.b, _currentAlpha);
    }
}
