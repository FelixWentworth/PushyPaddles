using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private bool _useDefault;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private MenuManager _menuManager;

    public InputField IpAddress;
    public InputField Port;

    void Awake()
    {
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
