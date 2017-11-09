using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SP_Manager : MonoBehaviour
{
    public static SP_Manager Instance;
    private bool _singlePlayer;

    void Start()
    {
        Instance = this;
        _singlePlayer = GameObject.Find("VersionManager").GetComponent<VersionManager>().GetVersion() ==
                        VersionManager.Version.SinglePlayer;
    }

    public T Get<T>()
    {
        if (!_singlePlayer)
        {
            Debug.LogError("Called when not in single player, are you sure you meant to do this?");
        }
        return GetComponent<T>();
    }

    public bool IsSinglePlayer()
    {
        return _singlePlayer;
    }
}
