using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameOverScreen : UIScreen
{

    public Text RoundsText;

    public override void Show()
    {
        base.Show();

        // TODO Set the number of rounds text
        var rounds = GameObject.Find("LevelManager").GetComponent<LevelManager>().RoundNumber;

        RoundsText.text = string.Format(Localization.Get("FORMATTED_UI_END_ROUNDS_COMPLETED"), rounds);
    }

    public override void Hide()
    {
        base.Hide();

    }

    /// <summary>
    /// Disconnect from the current game
    /// </summary>
    public void BtnQuitGame()
    {
        var manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        var player = manager.GetLocalPlayer();

        var ni = player.GetComponent<NetworkIdentity>();

        ni.connectionToServer.Disconnect();
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
