using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterSelection : MonoBehaviour
{
    private List<GameObject> AvailableCharacters = new List<GameObject>();

    private int _currentModelIndex;

    private Player _player;

    void Awake()
    {
        var playerModel = transform.Find("SimplePeople").gameObject;

        // -1 as model limbs are the last object in the list
        for (var i = 0; i < playerModel.transform.childCount-1; i++)
        {
            AvailableCharacters.Add(playerModel.transform.GetChild(i).gameObject);
        }
    }

    // Use this for initialization
    public void Set(Player player)
    {
        _player = player;
        DisableAll();
        SetCharacter();
        transform.GetChild(0).GetComponent<Animator>().SetFloat("Speed_f", 1f);
    }

    public void NextCharacter()
    {
        AvailableCharacters[_currentModelIndex].SetActive(false);
        _currentModelIndex = _currentModelIndex >= AvailableCharacters.Count - 1 ? 0 : _currentModelIndex + 1;
        SetCharacter();
    }

    public void PrevCharacter()
    {
        AvailableCharacters[_currentModelIndex].SetActive(false);
        _currentModelIndex = _currentModelIndex <= 0 ? AvailableCharacters.Count - 1 : _currentModelIndex - 1;
        SetCharacter();
    }

    public void SelectCharacter()
    {
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            GameObject.Find("SinglePlayerManager").GetComponent<SP_GameManager>().SetModel(_currentModelIndex);
        }
        else
        {
            // Client needs to be ready to send command
            if (!ClientScene.ready)
            {
                ClientScene.Ready(NetworkManager.singleton.client.connection);
            }
            if (SP_Manager.Instance.IsSinglePlayer())
            {
                _player.SetSPModel(_currentModelIndex);
            }
            else
            {
                _player.CmdSetModel(_currentModelIndex);
            }
        }
        DisableAll();
        GameObject.Find("MenuManager").GetComponent<MenuManager>().HideCharacterSelect();
    }

    private void DisableAll()
    {
        foreach (var availableCharacter in AvailableCharacters)
        {
            availableCharacter.SetActive(false);
        }
    }

    private void SetCharacter()
    {
        AvailableCharacters[_currentModelIndex].SetActive(true);
    }

}
