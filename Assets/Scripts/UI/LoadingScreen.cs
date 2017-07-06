using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : UIScreen
{
    public Text _statusText;

    private Action _cancelAction;

    void Start()
    {
        Hide();
    }   

    public void ShowScreen(string message, Action cancel)
    {
        Show();
        _statusText.text = message;

        _cancelAction = cancel;
    }

    public void Complete()
    {
        Hide();
    }

    public void CancelPressed()
    {
        Hide();
        _cancelAction();
    }

}
