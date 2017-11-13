using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SP_Menus : MonoBehaviour
{

    private MenuManager _menuManager;

    public void ShowHowToPlay()
    {
        _menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        _menuManager.ShowHowToPlay();
    }

    public void ShowCharacterSelect()
    {
        _menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        _menuManager.ShowCharacterSelect();
    }

    public void HideLessonSelect()
    {
        _menuManager.HideLessonSelect();
    }

    public void ShowGameOver(bool victory, int time)
    {
        _menuManager.ShowGameOver(victory, time, false);
    }

    public void ShowRewards()
    {
        _menuManager.ShowRewards();
    }

    // navigate menus
}
