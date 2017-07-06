using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTestPlay : MonoBehaviour
{

    public string Name;
    public bool Test;

    void Update()
    {
        if (Test)
        {
            AudioManager.Instance.Play(Name);
            Test = false;
        }
    }
}
