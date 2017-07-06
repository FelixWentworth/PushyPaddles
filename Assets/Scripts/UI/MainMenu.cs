using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using PlayGen.Unity.Utilities.Localization;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private bool _useDefault;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private MenuManager _menuManager;

    public InputField IpAddress;
    public InputField Port;

    public LoadingScreen LoadingScreen;

    void Awake()
    {
        LoadingScreen.gameObject.SetActive(true);

        if (_useDefault)
        {
            IpAddress.text = _networkManager.networkAddress;
            Port.text = _networkManager.networkPort.ToString();
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
                        LoadingScreen.ShowScreen("Connecting to: " + _networkManager.networkAddress + ":" + _networkManager.networkPort, CancelClient);
                    }

            }
        }
        else if (LoadingScreen.IsShowing)
        {
            LoadingScreen.Complete();
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
