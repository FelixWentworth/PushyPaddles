using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProSocialEventHandler : MonoBehaviour {

    /// <summary>
    /// This class hooks up to the event listener and handle any events that are fired
    /// </summary>

    public bool IsPaused { get; private set; }

    public GameManager GameManager;

	// Use this for initialization
	void Start ()
	{
	    ProSocialEventListener.StartGame += StartGame;
	    ProSocialEventListener.StopGame += StopGame;
	    ProSocialEventListener.PausedGame += TogglePause;
	}

    private void StartGame()
    {
        GameManager.StartGameTimer();
    }

    private void TogglePause()
    {
        IsPaused = !IsPaused;
        if (IsPaused)
        {
            GameManager.PauseGame();
        }
        else
        {
            GameManager.ResumeGame();
        }
    }

    private void StopGame()
    {
        GameManager.StopGame();
    }
}
