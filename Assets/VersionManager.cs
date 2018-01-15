using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionManager : MonoBehaviour
{

    public enum Version
    {
#if PSL_ENABLED
        ProSocial,
#endif
        SinglePlayer
    };

    [SerializeField] private Version _version;
	
	public Version GetVersion()
    {
        return _version;
    }

}
