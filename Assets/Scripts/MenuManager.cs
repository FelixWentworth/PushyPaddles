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
        if (isServer)
        {
            return;
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
        // Make sure that the screens are hidden but ready to be used
        RewardScreenManager.gameObject.SetActive(true);
        RewardScreenManager.Hide();
        TitleScreen.SetActive(false);
        CharacterSelectionScreen.SetActive(false);
    }

    /// <summary>
    /// Show menu for all players
    /// </summary>
    [Command]
    public void CmdShowMenu()
    {
        _showMenu = true;
    }

    /// <summary>
    /// Hide the menu screen for all players
    /// </summary>
    public void HideMenu()
    {
        Debug.Log("Hide");

        _hideMenu = true;
    }

    /// <summary>
    /// Show all players the character selection screen
    /// </summary>
    public void ShowCharacterSelect()
    {
        Debug.Log("Here");

        _showCharacterSelection = true;
    }

    /// <summary>
    /// Hide character select screen for the current player
    /// </summary>
    public void HideCharacterSelect()
    {
        _hideCharacterSelection = true;
    }

    /// <summary>
    /// Show all players the reward screen, only the player who reached the goal will be able to control this menu
    /// </summary>
    [Command]
    public void CmdShowRewards()
    {
        _showRewards = true;
    }
    /// <summary>
    /// We can have a command to hide the rewards screen as only 1 player has control
    /// </summary>
    [Command]
    public void CmdHideRewards()
    {
        _hideRewards = true;
    }

    /// <summary>
    /// Left button Pressed
    /// </summary>
    [Command]
    public void CmdLeftPressed()
    {
        _leftPressed = true;
    }

    /// <summary>
    /// Right Button Pressed
    /// </summary>
    [Command]
    public void CmdRightPressed()
    {
        _rightPressed = true;
    }

    /// <summary>
    /// Select button Pressed
    /// </summary>
    [Command]
    public void CmdSelectPressed()
    {
        _selectPressed = true;
    }
}
