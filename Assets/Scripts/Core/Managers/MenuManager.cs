using UnityEngine;
using UnityEngine.Networking;

public class MenuManager : NetworkBehaviour
{
    // Menu should be active at start
    private bool _showMenu = true;
    private bool _hideMenu;

    // Reward screen inputs
    private static bool _leftPressed;
    private static bool _rightPressed;
    private static bool _selectPressed;

    public GameManager GameManager;

    public RewardScreenManager RewardScreenManager;
    public GameOverScreen GameOverScreen;
    public GameObject TitleScreen;
    public GameObject CharacterSelectionScreen;
    public GameObject HowToPlayScreen;
    public GameObject LevelInfo;
    public GameObject WaitingForPlayersPrompt;

    void Start()
    {
        SetupScreens();
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
        if (GameManager.AllPlayersReady && WaitingForPlayersPrompt.activeSelf)
        {
            WaitingForPlayersPrompt.SetActive(false);
        }
        else if (!GameManager.AllPlayersReady && !WaitingForPlayersPrompt.activeSelf)
        {
            WaitingForPlayersPrompt.SetActive(true);
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

    /// <summary>
    /// Setup screens and put them in their default state
    /// </summary>
    private void SetupScreens()
    {
        // Make sure that the screens are hidden but ready to be used
        RewardScreenManager.gameObject.SetActive(true);
        RewardScreenManager.Hide();
        GameOverScreen.gameObject.SetActive(true);
        GameOverScreen.Hide();

        CharacterSelectionScreen.SetActive(false);
        HowToPlayScreen.SetActive(false);
        WaitingForPlayersPrompt.gameObject.SetActive(false);

        LevelInfo.SetActive(true);

        TitleScreen.SetActive(false);
    }

    /// <summary>
    /// Hide the menu screen for all players
    /// </summary>
    public void HideMenu()
    {
        _showMenu = false;
        _hideMenu = true;
    }

    public void ShowHowToPlay()
    {
        HowToPlayScreen.SetActive(true);
    }

    public void HideHowToPlay()
    {
        HowToPlayScreen.SetActive(false);
        ShowCharacterSelect();
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
    }
    
    /// <summary>
    /// Hide the rewards screen for all users
    /// </summary>
    public void HideRewards()
    {
        RewardScreenManager.Hide();
    }

    /// <summary>
    /// Notify the server to show game over for all clients
    /// </summary>
    [Server]
    public void ShowGameOver()
    {
        RpcShowGameOver();
    }

    /// <summary>
    /// Notify clients from the server that they should be showing the game over screen
    /// </summary>
    [ClientRpc]
    private void RpcShowGameOver()
    {
        GameOverScreen.Show();
    }

    /// <summary>
    /// Hide the game over screen
    /// </summary>
    public void HideGameOver()
    {
        GameOverScreen.Hide();
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
