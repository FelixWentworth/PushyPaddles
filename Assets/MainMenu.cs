using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private bool _useDefault;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private MenuManager _menuManager;

    public InputField IpAddress;

    void Awake()
    {
        if (_useDefault)
        {
            IpAddress.text = _networkManager.networkAddress;
        }
    }

    public void Connect()
    {
        _networkManager.networkAddress = IpAddress.text;
        _networkManager.StartClient();
        _menuManager.HideMenu();

    }

    public void HostServer()
    {
        _networkManager.networkAddress = IpAddress.text;
        _networkManager.StartServer();
        _menuManager.HideMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
