using UnityEngine;
using UnityEngine.Networking;

public class MenuManager : NetworkBehaviour
{
    // Menu should be active at start
    [SyncVar] private bool _showMenu = true;
    // Explicitly state that the other menus menus should be disabled
    [SyncVar] private bool _showRewards = false;
    [SyncVar] private bool _showCharacterSelection = false;

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
	void Update () {
	    if (TitleScreen.activeSelf != _showMenu)
	    {
	        TitleScreen.SetActive(_showMenu);
	    }
        if (_showRewards && !RewardScreenManager.IsShowing)
	    {
            RewardScreenManager.Show();
	    }

        // Check a menu is open
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
        _showMenu = false;
    }

    [Command]
    public void CmdShowCharacterSelect()
    {
        CharacterSelectionScreen.SetActive(true);
        SetCharacterPlayer();
    }

    [Command]
    public void CmdToggleRewards()
    {
        _showRewards = !_showRewards;
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

    private void SetCharacterPlayer()
    {
        GameObject.Find("CharacterSelection").GetComponent<CharacterSelection>().Set(GameObject.Find("GameManager").GetComponent<GameManager>().GetLocalPlayer());
    }
}
