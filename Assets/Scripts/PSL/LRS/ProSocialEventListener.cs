using System;
#if PSL_ENABLED
using PlayGen.Orchestrator.Common;
#endif
using UnityEngine;

public class ProSocialEventListener : MonoBehaviour
{

#if PSL_ENABLED
	public void EditorStart()
	{
		PlatformSelection.UpdateSeverState(GameState.Started);
	}

	public void EditorPause()
	{
		PlatformSelection.UpdateSeverState(GameState.Paused);
	}

	public void EditorStop()
	{
		PlatformSelection.UpdateSeverState(GameState.Stopped);
	}
#endif
}
