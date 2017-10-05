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
        if (PlatformSelection.ConnectionType != ConnectionType.Testing)
        {
            _useDefault = false;
            _showMenu = false;
        }
//#if UNITY_WEBGL
//        _networkManager.useWebSockets = true;
//#elif UNITY_STANDALONE_WIN
//        _networkManager.useWebSockets = false;
//#endif
        LoadingScreen.gameObject.SetActive(true);

        if (_useDefault)
        {
            IpAddress.text = NetworkManager.singleton.networkAddress;
            Port.text = NetworkManager.singleton.networkPort.ToString();
        }

        if (!_showMenu)
        {
            if (PlatformSelection.ConnectionType == ConnectionType.Testing)
            {
#if UNITY_STANDALONE_LINUX
// TODO shut down if no players after x seconds (30?)
            //_networkManager.StartServer();
            _menuManager.HideMenu();
#elif UNITY_WEBGL
                LoadingScreen.ShowScreen(Localization.Get("UI_MAIN_LOADING"), null);
                StartCoroutine(GetConnectionConfig());
#endif
            }
            else if (PlatformSelection.ConnectionType == ConnectionType.Client)
            {
                LoadingScreen.ShowScreen(Localization.Get("UI_MAIN_CONNECTING"), null);
                _connecting = true;
            }
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
            LoadingScreen.ShowScreen(Localization.Get("UI_MAIN_CONNECTING"), CancelClient);

            IpAddress.text = config.Address;
            Port.text = config.Port.ToString();

            NetworkManager.singleton.networkAddress = config.Address;
            NetworkManager.singleton.networkPort = config.Port;
            NetworkManager.singleton.StartClient();

            _connecting = true;

        }
    }

    public void Connect()
    {

        NetworkManager.singleton.networkAddress = IpAddress.text;
        NetworkManager.singleton.networkPort = Convert.ToInt16(Port.text);

        NetworkManager.singleton.StartClient();
        _menuManager.HideMenu();
    }

    public void HostServer()
    {
        NetworkManager.singleton.networkAddress = IpAddress.text;
        NetworkManager.singleton.networkPort = Convert.ToInt16(Port.text);

        NetworkManager.singleton.StartServer();
        _menuManager.HideMenu();
    }

    void Update()
    {
        if (_showMenu)
        {
            bool noConnection = (NetworkManager.singleton.client == null || NetworkManager.singleton.client.connection == null ||
                                 NetworkManager.singleton.client.connection.connectionId == -1);

            if (!NetworkManager.singleton.IsClientConnected() && !NetworkServer.active && NetworkManager.singleton.matchMaker == null)
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
                           Localization.Get("UI_MAIN_CONNECTING") + ":" + NetworkManager.singleton.networkAddress + ":" + NetworkManager.singleton.networkPort,
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
            bool connected = (NetworkManager.singleton.client != null && NetworkManager.singleton.client.connection != null && NetworkManager.singleton.client.connection.connectionId != -1);

            if (NetworkManager.singleton.IsClientConnected() && !NetworkServer.active && NetworkManager.singleton.matchMaker == null)
            {
                if (connected)
                {
                    _menuManager.HideMenu();
                    LoadingScreen.Complete();
                    _connecting = false;
                }
            }
        }
        
    }

    public void CancelClient()
    {
        NetworkManager.singleton.StopClient();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
