using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionManager : MonoBehaviour
{

    public enum Version
    {
        NotSet = 0,
        ProSocial,
        SinglePlayer
    };

    [SerializeField] private Version _version;

    public List<GameObject> ProSocialGameObjects;
    public List<GameObject> SinglePlayerGameObjects;

    void Awake()
    {
        if (_version == Version.NotSet)
        {
#if UNITY_ANDROID || UNITY_IPHONE
            _version = Version.SinglePlayer;
#else
            _version = Version.ProSocial;
#endif
        }
    }

    void Start () {
        switch (_version)
        {
            case Version.ProSocial:
                DisableObjects(SinglePlayerGameObjects);
                break;
            case Version.SinglePlayer:
                DisableObjects(ProSocialGameObjects);
                break;
            default:
                Debug.Log("version not specified");
                break;
        }	


	}

    private void DisableObjects(List<GameObject> objects)
    {
        foreach (var o in objects)
        {
            o.SetActive(false);
        }
    }

    public Version GetVersion()
    {
        return _version;
    }

}
