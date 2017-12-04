using System;
using UnityEngine;
using UnityEngine.Networking;

[AttributeUsage(AttributeTargets.Method)]
public class ServerAccess : Attribute
{
    private readonly bool _hasAccess;

    public ServerAccess()
    {
        _hasAccess = SP_Manager.Instance.IsSinglePlayer() || NetworkServer.active || GameObject.Find("GameManager").GetComponent<NetworkIdentity>()
                   .isServer;

    }
    public virtual bool HasAccess
    {
        get { return _hasAccess; }
    }
}


[AttributeUsage(AttributeTargets.Method)]
public class ClientAccess : Attribute
{
    private readonly bool _hasAccess;

    public ClientAccess()
    {
        _hasAccess = SP_Manager.Instance.IsSinglePlayer() || GameObject.Find("GameManager").GetComponent<NetworkIdentity>()
                         .isClient;

    }
    public virtual bool HasAccess
    {
        get { return _hasAccess; }
    }
}