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

    public GameObject PlayAgainButton;
    public GameObject QuitButton;

    private StarRating _starRating;

    public override void Show()
    {
        base.Show();
        _starRating = GetComponentInChildren<StarRating>();
        
    }

    public override void Hide()
    {
        base.Hide();

    }

    public void SetText(bool victory, int timeTaken)
    {
        ConditionText.text = victory ? Localization.Get("UI_END_GAME_COMPLETE") : Localization.Get("UI_END_TIME_UP");

        var rounds = GameObject.Find("LevelManager").GetComponent<LevelManager>().RoundNumber;
        var totalRounds = GameObject.Find("LevelManager").GetComponent<LevelManager>().TotalRounds;
        CalculateRating(rounds, totalRounds);
        RoundsText.text = string.Format(Localization.Get("FORMATTED_UI_END_ROUNDS_COMPLETED"), rounds-1); // did not complete the current round

        TimeTakenText.text = Localization.Get("UI_END_TIME_TAKEN") + " " + (timeTaken / 60) + ":" + (timeTaken % 60).ToString("00");
    }

    private void CalculateRating(int completed, int total)
    {
        var rating = 1;
        if (completed >= total)
        {
            // game completed, 3 stars
            rating = 3;
        }
        else if (completed < total && completed >= (total /2))
        {
            // completed at least 50% of levels
            rating = 2;
        }
        else if (completed <= 1)
        {
            // no rounds completed
            rating = 0;
        }
        SetRating(rating);

        // Save the rating
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            SP_Manager.Instance.Get<SP_Levels>().SaveRating(rating);
        }
    }

    private void SetRating(int rating)
    {
        _starRating.ShowStarRating(rating);
    }

    public void SetButtonsEnabled(bool enabled)
    {
        PlayAgainButton.SetActive(false);
        QuitButton.SetActive(enabled);
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
            NetworkManager.singleton.StopClient();
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
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            manager.GetAllPlayers()[0].StartTimer();
        }
        else
        {
            manager.GetLocalPlayer().StartTimer();
        }
    }

    /// <summary>
    /// Play the game again but change character
    /// </summary>
    public void BtnChangeCharacter()
    {
        // TODO Implement    
    }
}
