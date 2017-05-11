using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProSocialEventListener : MonoBehaviour {

	public static event Action StartGame = delegate { };
	public static event Action PausedGame = delegate { };
	public static event Action StopGame = delegate { };

	#if UNITY_EDITOR
	public void EditorStart()
	{
		StartGame();
	}

	public void EditorPause()
	{
		PausedGame();
	}

	public void EditorStop()
	{
		StopGame();
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
