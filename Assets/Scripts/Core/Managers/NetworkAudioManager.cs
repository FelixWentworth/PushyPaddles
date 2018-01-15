using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkAudioManager : NetworkBehaviour{

    [ServerAccess]
    public void Play(string name)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            ClientPlay(name);
        }
        else
        {
            RpcPlay(name);
        }
    }

    [ClientRpc]
    private void RpcPlay(string name)
    {   
        ClientPlay(name);
    }

    [ClientAccess]
    private void ClientPlay(string name)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        AudioManager.Instance.Play(name);
    }
}
