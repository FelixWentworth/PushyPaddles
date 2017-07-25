using PlayGen.Orchestrator.Common;
using UnityEngine.Networking;

public class PSL_GameState : NetworkBehaviour {

    /// <summary>
    /// This class hooks up to the event listener and handle any events that are fired
    /// </summary>

    public bool IsPaused { get; private set; }

    public GameManager GameManager;

	// Use this for initialization
	void OnEnable ()
	{
	    PlatformSelection.ServerStateChanged += StateChange;
    }
    void OnDisable()
    {
        PlatformSelection.ServerStateChanged -= StateChange;
    }

    [Server]
    private void StateChange(GameState state)
    {
        if (state == GameState.Started)
        {
            GameManager.StartGameTimer();
            // TODO check if paused
            GameManager.ResumeGame();
        }
        if (state == GameState.Paused)
        {
            GameManager.PauseGame();
        }
        if (state == GameState.Stopped)
        {
            GameManager.StopGame();
        }
    }
}
