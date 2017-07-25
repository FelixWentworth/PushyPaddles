using System;
using PlayGen.Orchestrator.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProSocialEventListener : MonoBehaviour {

#if UNITY_EDITOR
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

#if UNITY_EDITOR
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
