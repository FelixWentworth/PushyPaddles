using System.Collections;
using System.Collections.Generic;
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
        CollectedAnimation[CollectedAnimation.clip.name].speed = 1f;
        CollectedAnimation[CollectedAnimation.clip.name].time = 0f;

        CollectedAnimation.Play();
        _pickedUp = true;
    }

    [ClientRpc]
    public void RpcReset()
    {
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
