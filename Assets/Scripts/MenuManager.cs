using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private static bool _showRewards = false;
    private static bool _leftPressed;
    private static bool _rightPressed;
    private static bool _selectPressed;

    private static bool _isMenuActive;

    public RewardScreenManager RewardScreenManager;

    void Start()
    {
        HideScreens();
    }

    // Update is called once per frame
	void Update () {
	    if (_showRewards)
	    {
	        _showRewards = false;
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

	    _isMenuActive = RewardScreenManager.IsShowing;
	}

    private void HideScreens()
    {
        RewardScreenManager.Hide();
    }

    public static void ShowRewards()
    {
        _showRewards = true;
    }

    public static void LeftPressed()
    {
        _leftPressed = true;
    }

    public static void RightPressed()
    {
        _rightPressed = true;
    }

    public static void SelectPressed()
    {
        _selectPressed = true;
    }

    public static bool IsMenuActive()
    {
        return _isMenuActive;
    }
}
