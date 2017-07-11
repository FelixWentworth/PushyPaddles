using System;
using UnityEngine.Networking;

public enum ConnectionType
{
	Client,
	Server,
	Testing
}

[Serializable]
public class PlatformManager
{
	public ConnectionType ConnectionType;
	public NetworkManager NetworkManagerObj;
}