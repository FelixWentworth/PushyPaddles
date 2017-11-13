using System;
#if USE_PROSOCIAL
using PlayGen.Orchestrator.Common;
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProSocialEventListener : MonoBehaviour {

#if USE_PROSOCIAL
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

#if USE_PROSOCIAL
[CustomEditor(typeof(ProSocialEventListener))]
public class ServerEventEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		ProSocialEventListener myScript = (ProSocialEventListener)target;
		if (GUILayout.Button("Start"))
		{
			myScript.EditorStart();
		}
		if (GUILayout.Button("Pause"))
		{
			myScript.EditorPause();
		}
		if (GUILayout.Button("Stop"))
		{
			myScript.EditorStop();
		}
	}
}
#endif
