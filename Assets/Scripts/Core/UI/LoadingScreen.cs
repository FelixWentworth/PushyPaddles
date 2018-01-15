using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : UIScreen
{
    public Text _statusText;

    private Action _cancelAction;

    void Awake()
    {
        Hide();
    }   

    public void ShowScreen(string message, Action cancel)
    {
        Show();
        _statusText.text = message;

        _cancelAction = cancel;

        var button = GetComponentInChildren<Button>();
        button.gameObject.SetActive(cancel != null);
    }

    public void Complete()
    {
        StartCoroutine(WaitToHide());
    }

    private IEnumerator WaitToHide()
    {
        yield return new WaitForSeconds(0.5f);
        Hide();
    }

    public void CancelPressed()
    {
        Hide();
        if (_cancelAction != null)
        {
            _cancelAction();
        }
    }

}
