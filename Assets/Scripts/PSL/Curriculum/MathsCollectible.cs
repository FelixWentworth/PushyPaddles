using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MathsCollectible : NetworkBehaviour
{
    public Animation CollectedAnimation;
    [SyncVar] public string Operation;

    private Text _text;

    private string _sortingLayerName = "Default";
    private int _sorthingOrder = 0;

    private bool _pickedUp;

    public void Set(string operation)
    {
        Operation = operation;
        ResetObject();
    }

    void FixedUpdate()
    {
        if (_text == null)
        {
            _text = transform.GetComponentInChildren<Text>();
        }
        _text.text = Operation;
    }

    [ClientRpc]
    public void RpcPlayCollectedAnimation()
    {
        ClientPlayCollectedAnimation();
    }   

    [ClientAccess]
    public void ClientPlayCollectedAnimation()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        CollectedAnimation[CollectedAnimation.clip.name].speed = 1f;
        CollectedAnimation[CollectedAnimation.clip.name].time = 0f;

        CollectedAnimation.Play();
        _pickedUp = true;
    }

    [ClientRpc]
    public void RpcReset()
    {
        ClientReset();
    }

    [ClientAccess]
    public void ClientReset()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        ResetObject();
    }


    public void ResetObject()
    {
        if (!_pickedUp)
        {
            // no need to return state to initial
            return;
        }
        CollectedAnimation[CollectedAnimation.clip.name].speed = -1f;
        CollectedAnimation[CollectedAnimation.clip.name].time = 1f;

        CollectedAnimation.Play();
    }
}
