using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MathsCollectible : NetworkBehaviour
{
    [SyncVar] public string Operation;

    private Text _text;

    private string _sortingLayerName = "Default";
    private int _sorthingOrder = 0;

    public void Set(string operation)
    {
        Operation = operation;
    }

    void FixedUpdate()
    {
        if (_text == null)
        {
            _text = transform.GetComponentInChildren<Text>();
        }
        _text.text = Operation;
    }
}
