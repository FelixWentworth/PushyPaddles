using UnityEngine;
using UnityEngine.Networking;

public class MenuManager : NetworkBehaviour
{
    // Menu should be active at start
    [SyncVar] private bool _showMenuSync = true;
    [SyncVar] private bool _hideMenuSync;

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
       
        if (_showMenuSync && !TitleScreen.activeSelf)
        {
            _hideMenuSync = false;
            TitleScreen.SetActive(true);
        }
        if (_hideMenuSync && TitleScreen.activeSelf)
        {
            _showMenuSync = false;
            TitleScreen.SetActive(false);
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
        _showMenuSync = true;
    }

    /// <summary>
    /// Hide the menu screen for all players
    /// </summary>
    public void HideMenu()
    {
        _hideMenuSync = true;
    }

    /// <summary>
    /// Show player the character selection screen
    /// </summary>
    public void ShowCharacterSelect()
    {
        CharacterSelectionScreen.SetActive(true);
    }

    /// <summary>
    /// Hide character select screen for the current player
    /// </summary>
    public void HideCharacterSelect()
    {
        CharacterSelectionScreen.SetActive(false);
    }

    /// <summary>
    /// Show all players the reward screen, only the player who reached the goal will be able to control this menu
    /// </summary>
    public void ShowRewards()
    {
        RewardScreenManager.Show();
        //_showRewards = true;
    }
    
    /// <summary>
    /// Hide the rewards screen for all users
    /// </summary>
    public void HideRewards()
    {
        RewardScreenManager.Hide();
        //_hideRewards = true;
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
