using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkAudioManager : NetworkBehaviour{

    [Server]
    public void Play(string name)
    {
        RpcPlay(name);
    }

    [ClientRpc]
    private void RpcPlay(string name)
    {
        AudioManager.Instance.Play(name);
    }
}
