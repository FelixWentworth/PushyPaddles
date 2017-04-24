using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;

public class Reward : MonoBehaviour
{
    public enum RewardType
    {
        None = 0,

        SpeedBoost,
        ReverseControls
    }

    public RewardType Type;

    public bool Highlighted;
    public bool IsAvailable { get; private set; }

    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private GameObject _nameObject;

    void Start()
    {
        IsAvailable = true;
    }

    public void SetHighlight(bool highlight)
    {
        Debug.Log("Set Highlight" + highlight);
        if (!IsAvailable)
        {
            return;
        }
        _highlightObject.SetActive(highlight);
        _nameObject.SetActive(highlight);
        Highlighted = highlight;
    }

    public void SetName(string username)
    {
        _nameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = username;
    }

    public void SetAvailable(bool available)
    {
        IsAvailable = available;
        
        GetComponent<CanvasGroup>().alpha = IsAvailable ? 1f : 0.5f;
    }
}
