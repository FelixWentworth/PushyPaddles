using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameOverScreen : UIScreen
{

    public Text RoundsText;
    public Text ConditionText;
    public Text TimeTakenText;


    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();

    }

    public void SetText(bool victory, int timeTaken)
    {
        ConditionText.text = victory ? Localization.Get("UI_END_GAME_COMPLETE") : Localization.Get("UI_END_TIME_UP");

        var rounds = GameObject.Find("LevelManager").GetComponent<LevelManager>().RoundNumber;
        RoundsText.text = string.Format(Localization.Get("FORMATTED_UI_END_ROUNDS_COMPLETED"), rounds-1); // did not complete the current round

        TimeTakenText.text = Localization.Get("UI_END_TIME_TAKEN") + " " + (timeTaken / 60) + ":" + (timeTaken % 60).ToString("00");
    }

    /// <summary>
    /// Disconnect from the current game
    /// </summary>
    public void BtnQuitGame()
    {
        var gm = GameObject.Find("GameManager");
        if (gm == null)
        {
            // We have been disconnected but haven't left the game properly, auto restart scene
            SceneManager.LoadScene(0);
        }
        var manager = gm.GetComponent<GameManager>();
        
        var player = manager.GetLocalPlayer();

        var ni = manager.GetComponent<NetworkIdentity>();

        if (player != null)
        {
            ni.connectionToServer.Disconnect();
        }
        else if (ni.isServer)
        {
            NetworkManager.singleton.StopServer();
        }
        // Load the current scene
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Play the game again without changing characters
    /// </summary>
    public void BtnPlayAgain()
    {
        GameObject.Find("MenuManager").GetComponent<MenuManager>().HideGameOver();
        var manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        manager.GetLocalPlayer().StartTimer();
    }

    /// <summary>
    /// Play the game again but change character
    /// </summary>
    public void BtnChangeCharacter()
    {
        // TODO Implement    
    }
}
