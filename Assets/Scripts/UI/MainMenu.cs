using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using PlayGen.Unity.Utilities.Localization;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private bool _showMenu;

    [SerializeField] private bool _useDefault;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private MenuManager _menuManager;

    public InputField IpAddress;
    public InputField Port;

    private bool _connecting;

    struct Config
    {
        public string Address;
        public int Port;
    }

    public LoadingScreen LoadingScreen;

    void Start()
    {
//#if UNITY_WEBGL
//        _networkManager.useWebSockets = true;
//#elif UNITY_STANDALONE_WIN
//        _networkManager.useWebSockets = false;
//#endif
        LoadingScreen.gameObject.SetActive(true);

        if (_useDefault)
        {
            IpAddress.text = _networkManager.networkAddress;
            Port.text = _networkManager.networkPort.ToString();
        }

        if (!_showMenu)
        {
#if UNITY_STANDALONE_LINUX
            // TODO shut down if no players after x seconds (30?)
            _networkManager.StartServer();
            _menuManager.HideMenu();
#elif UNITY_WEBGL
            LoadingScreen.ShowScreen("Loading", null);
            StartCoroutine(GetConnectionConfig());
#endif
        }
    }

    private IEnumerator GetConnectionConfig()
    {
        var path = Application.streamingAssetsPath + "/ConnectionConfig.json";

        if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WSAPlayerX86 ||
                Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            path = "file:///" + path;
        }

        var www = new WWW(path);

        yield return www;
        if (www.text != null)
        {
            var config = JsonUtility.FromJson<Config>(www.text);
            LoadingScreen.ShowScreen("Connecting", CancelClient);

            IpAddress.text = config.Address;
            Port.text = config.Port.ToString();

            _networkManager.networkAddress = config.Address;
            _networkManager.networkPort = config.Port;
            _networkManager.StartClient();

            _connecting = true;

        }
    }

    public void Connect()
    {

        _networkManager.networkAddress = IpAddress.text;
        _networkManager.networkPort = Convert.ToInt16(Port.text);

        _networkManager.StartClient();
        _menuManager.HideMenu();
    }

    public void HostServer()
    {
        _networkManager.networkAddress = IpAddress.text;
        _networkManager.networkPort = Convert.ToInt16(Port.text);

        _networkManager.StartServer();
        _menuManager.HideMenu();
    }

    void Update()
    {
        if (_showMenu)
        {
            bool noConnection = (_networkManager.client == null || _networkManager.client.connection == null ||
                                 _networkManager.client.connection.connectionId == -1);

            if (!_networkManager.IsClientConnected() && !NetworkServer.active && _networkManager.matchMaker == null)
            {
                if (noConnection)
                {
                    if (LoadingScreen.IsShowing)
                    {
                        LoadingScreen.Hide();
                    }
                }
                else
                {
                    if (!LoadingScreen.IsShowing)
                    {
                        LoadingScreen.ShowScreen(
                            "Connecting to: " + _networkManager.networkAddress + ":" + _networkManager.networkPort,
                            CancelClient);
                    }
                }
            }
            else if (LoadingScreen.IsShowing)
            {
                LoadingScreen.Hide();
            }
        }
        if (LoadingScreen.IsShowing && _connecting)
        {
            bool connected = (_networkManager.client != null && _networkManager.client.connection != null && _networkManager.client.connection.connectionId != -1);

            if (_networkManager.IsClientConnected() && !NetworkServer.active && _networkManager.matchMaker == null)
            {
                if (connected)
                {
                    LoadingScreen.Complete();
                    _menuManager.HideMenu();
                    _connecting = false;
                }
            }
        }
        
    }

    public void CancelClient()
    {
        _networkManager.StopClient();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
