﻿using System.Reflection;
#if USE_PROSOCIAL
using PlayGen.Orchestrator.Common;
#endif
using UnityEngine.Networking;

public class PSL_GameState : NetworkBehaviour {
#if USE_PROSOCIAL
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

    [ServerAccess]
    private void StateChange(GameState state)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

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
#endif
}
