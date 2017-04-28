using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterSelection : MonoBehaviour
{

    public List<GameObject> AvailableCharacters;

    private int _currentModelIndex;

    private Player _player;

    // Use this for initialization
    public void Set(Player player)
    {
        _player = player;
        DisableAll();
        SetCharacter();
        transform.GetChild(0).GetComponent<Animator>().SetFloat("Speed_f", 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevCharacter();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCharacter();
        }
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
        _player.CmdSetModel(_currentModelIndex);
        DisableAll();
        GameObject.Find("MenuManager").GetComponent<MenuManager>().CharacterSelectionScreen.SetActive(false);
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
