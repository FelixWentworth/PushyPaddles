using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class AirConsoleManager : MonoBehaviour
{

    public Player[] Players;

    void Awake()
    {
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnect;
        //AirConsole.instance.onDisconnect += OnDisconnect;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnConnect(int device_id)
    {
        Debug.Log("Connected: " + device_id);
        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {
            if (AirConsole.instance.GetControllerDeviceIds().Count >= 3)
            {
                Debug.Log("Starting Game");
                StartGame();
            }
            else
            {
                //uiText.text = "NEED MORE PLAYERS";
            }
        }
    }

    void OnMessage(int device_id, JToken data)
    {
        
        var active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);
        if (active_player != -1)
        {
            if (data.ToString().Contains("Down"))
            {
                if ((bool) data["Down"]["pressed"])
                {
                    if (!MenuManager.IsMenuActive())
                    {
                        Players[active_player].StartMoving(-1f);
                    }
                    else if (active_player == 0)
                    {
                        // can control a menu
                        MenuManager.LeftPressed();
                    }
                }
                else
                {
                    Players[active_player].StopMoving();
                }
            }
            if (data.ToString().Contains("Up"))
            {
                if ((bool)data["Up"]["pressed"])
                {
                    if (!MenuManager.IsMenuActive())
                    {
                        Players[active_player].StartMoving(1f);
                    }
                    else if (active_player == 0)
                    {
                        // can control a menu
                        MenuManager.RightPressed();
                    }
                }
                else
                {
                    Players[active_player].StopMoving();
                }
            }
            if (data.ToString().Contains("Interact") && (bool)data["Interact"]["pressed"])
            {
                if (!MenuManager.IsMenuActive())
                {
                    Players[active_player].Interact();
                }
                else if (active_player == 0)
                {
                    // can control a menu
                    MenuManager.SelectPressed();
                }
            }   
        }
    }

    void StartGame()
    {
        AirConsole.instance.SetActivePlayers(3);
    }

}
