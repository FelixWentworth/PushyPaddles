using System;
using System.Collections;
using System.Linq;

using PlayGen.Orchestratror.Unity.Client;
using PlayGen.Orchestratror.Unity.Server;
using PlayGen.Unity.AsyncUtilities;

using UnityEngine;
using UnityEngine.Networking;

public class PlatformSelection : MonoBehaviour {
	[SerializeField]
	private ConnectionType _connectionType;
	[SerializeField]
	private PlatformManager[] _platformManagers;

	private static PlatformSelection _instance;
	public static ConnectionType ConnectionType { get; private set; }

	void Awake () {
		if (_instance)
		{
			Destroy(gameObject);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(this);
		ConnectionType = _connectionType;

        var manager = _platformManagers.FirstOrDefault(p => p.ConnectionType == _connectionType);
        if (manager != null && manager.NetworkManagerObj != null)
        {
            Instantiate(manager.NetworkManagerObj);
        }

        gameObject.AddComponent<AsyncConfigLoader>();
		switch (_connectionType)
		{
			case ConnectionType.Server:
				StartCoroutine(StartServer());
				break;
			case ConnectionType.Client:
				StartCoroutine(StartClient());
				break;
			case ConnectionType.Testing:
				break;
		}
	}

	private IEnumerator StartServer()
	{
		while (!NetworkManager.singleton.isNetworkActive)
		{
			yield return new WaitForSeconds(1);
            OrchestratedGameServer.StartServer();
			yield return new WaitForSeconds(1);
		}
	}

	private IEnumerator StartClient()
	{
		var started = false;
		while (!started)
		{
			started = OrchestrationClient.StartClient();
			yield return new WaitForSeconds(1);
		}
	}

	private void OnError(Exception obj)
	{
		Debug.Log(obj.Message);
	}
}
