using UnityEngine;
using UnityEngine.Networking;

public class MenuManager : NetworkBehaviour
{
    // Menu should be active at start
    [SyncVar] private bool _showMenu = true;
    [SyncVar] private bool _hideMenu;
    // Explicitly state that the other menus menus should be disabled
    [SyncVar] private bool _showRewards = false;
    [SyncVar] private bool _hideRewards = false;
    [SyncVar] private bool _showCharacterSelection = false;
    // not synched as players can hide when they want
    private bool _hideCharacterSelection = false;

    // Reward screen inputs
    private static bool _leftPressed;
    private static bool _rightPressed;
    private static bool _selectPressed;

    public RewardScreenManager RewardScreenManager;
    public GameObject TitleScreen;
    public GameObject CharacterSelectionScreen;

    void Start()
    {
        HideScreens();
    }

    // Update is called once per frame
	void FixedUpdate ()
	{
	    UpdateScreens();
        HandleInput();   
	}

    /// <summary>
    /// Update which screens which should be activated/deactivated as decided by the server
    /// </summary>
    private void UpdateScreens()
    {
        if (_showMenu && !TitleScreen.activeSelf)
        {
            _hideMenu = false;
            TitleScreen.SetActive(true);
        }
        if (_hideMenu && TitleScreen.activeSelf)
        {
            _showMenu = false;
            TitleScreen.SetActive(false);
        }
        if (_showRewards && !RewardScreenManager.IsShowing)
        {
            _hideRewards = false;
            RewardScreenManager.Show();
        }
        if (_hideRewards && RewardScreenManager.IsShowing)
        {
            _showRewards = false;
            RewardScreenManager.Hide();
        }
        if (!_hideCharacterSelection && _showCharacterSelection && !CharacterSelectionScreen.activeSelf)
        {
            _hideCharacterSelection = false;
            CharacterSelectionScreen.SetActive(true);
        }
        if (_hideCharacterSelection && CharacterSelectionScreen.activeSelf)
        {
            CharacterSelectionScreen.SetActive(false);
        }
    }

    /// <summary>
    /// Handle inputs on menus
    /// </summary>
    private void HandleInput()
    {
        if (RewardScreenManager.IsShowing)
        {
            if (_leftPressed)
            {
                _leftPressed = false;
                RewardScreenManager.RewardsManager.Left();
            }
            if (_rightPressed)
            {
                _rightPressed = false;
                RewardScreenManager.RewardsManager.Right();
            }
            if (_selectPressed)
            {
                _selectPressed = false;
                RewardScreenManager.RewardsManager.Select();
            }
        }
    }

    private void HideScreens()
    {
        RewardScreenManager.Hide();
    }

    [Command]
    public void CmdShowMenu()
    {
        _showMenu = true;
    }

    [Command]
    public void CmdHideMenu()
    {
        _hideMenu = true;
    }

    [Command]
    public void CmdShowCharacterSelect()
    {
        _showCharacterSelection = true;
    }

    public void HideCharacterSelect()
    {
        _hideCharacterSelection = true;
    }

    [Command]
    public void CmdShowRewards()
    {
        _showRewards = true;
    }

    [Command]
    public void CmdLeftPressed()
    {
        _leftPressed = true;
    }

    [Command]
    public void CmdRightPressed()
    {
        _rightPressed = true;
    }

    [Command]
    public void CmdSelectPressed()
    {
        _selectPressed = true;
    }
}
