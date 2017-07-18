using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionManager : MonoBehaviour
{

    [SerializeField] private GameObject _indicatorPlatform;
    [SerializeField] private GameObject _indicatorPlaceLeft;
    [SerializeField] private GameObject _indicatorPlaceRight;
    [SerializeField] private GameObject _indicatorPlaceWater;

    [SerializeField] private GameObject _bridgeControl;
    [SerializeField] private GameObject _leftControl;
    [SerializeField] private GameObject _rightControl;

    [SerializeField] private GameObject _interactPress;

    void Awake()
    {
        DisableInsctructions();
    }

    /// <summary>
    /// Disable all of the instructions
    /// </summary>
    public void DisableInsctructions()
    {
        DisablePlatformInstruction();
        DisablePlaceInstruction();
        DisableMoveInstruction();
        DisableInteractInstruction();
    }

    public void DisablePlatformInstruction()
    {
        _indicatorPlatform.SetActive(false);
    }

    public void DisablePlaceInstruction()
    {
        _indicatorPlaceLeft.SetActive(false);
        _indicatorPlaceRight.SetActive(false);
        _indicatorPlaceWater.SetActive(false);
    }

    public void DisableMoveInstruction()
    {
        _bridgeControl.SetActive(false);
        _leftControl.SetActive(false);
        _rightControl.SetActive(false);
    }

    public void DisableInteractInstruction()
    {
        _interactPress.SetActive(false);
    }

    /// <summary>
    /// Show players their controls for movement
    /// </summary>
    /// <param name="role">Players current role</param>
    /// <param name="xPos">Current x position of player object</param>
    public void ShowMovement(Player.Role role, float xPos)
    {
        DisablePlatformInstruction();
        DisablePlaceInstruction();
        DisableInteractInstruction();

        // Show the correct controls, check they are not active prior to avoid resetting animations
        switch (role)
        {
            case Player.Role.Floater:
                if (!_bridgeControl.activeSelf)
                {
                    _bridgeControl.SetActive(true);
                }
                break;
            case Player.Role.Paddler:
                if (xPos <= 0)
                {
                    if (!_leftControl.activeSelf)
                    {
                        _leftControl.SetActive(true);
                    }
                }
                else
                {
                    if (!_rightControl.activeSelf)
                    {
                        _rightControl.SetActive(true);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException("role", role, null);
        }
    }

    /// <summary>
    /// Show the indicator on the actual platform
    /// </summary>
    public void ShowMoveToPlatformIndicator()
    {
        DisableMoveInstruction();
        DisablePlaceInstruction();

        if (!_indicatorPlatform.activeSelf)
        { 
            _indicatorPlatform.SetActive(true);
        }
    }

    /// <summary>
    /// Show the player where they should place the platform
    /// </summary>
    /// <param name="role">The players' role</param>
    /// <param name="xPos">Current x position of player object</param>
    public void ShowMoveToPlaceIndicator(Player.Role role, float xPos)
    {
        DisablePlatformInstruction();
        DisableMoveInstruction();

        if (role == Player.Role.Floater)
        {
            if (!_indicatorPlaceWater.activeSelf)
            {
                _indicatorPlaceWater.SetActive(true);
            }
        }
        else
        {
            if (xPos <= 0)
            {
                if (!_indicatorPlaceLeft.activeSelf)
                {
                    _indicatorPlaceLeft.SetActive(true);
                }
            }
            else
            {
                if (!_indicatorPlaceRight.activeSelf)
                {
                    _indicatorPlaceRight.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Called when a player has reached a point they should interact with, does not disable current indicators
    /// </summary>
    public void ShowInteractIndicator()
    {
        DisableMoveInstruction();

        if (!_interactPress.activeSelf)
        {
            _interactPress.SetActive(true);
        }
    }
}
