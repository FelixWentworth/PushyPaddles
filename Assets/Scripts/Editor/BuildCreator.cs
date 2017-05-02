using UnityEditor;

public class BuildCreator
{
	private static readonly BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
	private static readonly BuildOptions buildOptions = BuildOptions.None;

	[MenuItem("Build/Master Server", false, 100)]
	public static void BuildMasterServer()
	{
		var levels = new string[] { "Assets/Scenes/PushyPaddles_MasterServer.unity" };
		BuildPipeline.BuildPlayer(levels, "Builds/MasterServer.exe", buildTarget, buildOptions);
	}

	[MenuItem("Build/Zone Server", false, 100)]
	public static void BuildZoneServer()
	{
		var levels = new string[] { "Assets/Scenes/PushyPaddles_ZoneServer.unity" };
		BuildPipeline.BuildPlayer(levels, "Builds/ZoneServer.exe", buildTarget, buildOptions);
	}

	[MenuItem("Build/Game Server", false, 100)]
	public static void BuildGameServer()
	{
		var levels = new string[]
						{
                            "Assets/Scenes/PushyPaddles_GameServer.unity",
                            "Assets/Scenes/PushyPaddles.unity",
						};
			
		BuildPipeline.BuildPlayer(levels, "Builds/GameServer.exe", buildTarget, buildOptions);
	}

	[MenuItem("Build/Game Client", false, 100)]
	public static void BuildGameClient()
	{
		var levels = new string[] {
                                        "Assets/Scenes/PushyPaddles_Client.unity",
                                        "Assets/Scenes/PushyPaddles.unity",
									};
		BuildPipeline.BuildPlayer(levels, "Builds/GameClient.exe", buildTarget, buildOptions);
	}

	[MenuItem("Build/WebGL Game Client", false, 100)]
	public static void BuildWebGLGameClient()
	{
		var levels = new string[] {
                                        "Assets/Scenes/PushyPaddles_Client.unity",
                                        "Assets/Scenes/PushyPaddles.unity",
									};
		BuildPipeline.BuildPlayer(levels, "Builds/GameClient", BuildTarget.WebGL, BuildOptions.AutoRunPlayer);
	}

	[MenuItem("Build/Build All", false, 50)]
	public static void BuildAll()
	{
		BuildMasterServer();
		BuildZoneServer();
		BuildGameServer();
		BuildGameClient();
	}

	[MenuItem("Build/Build Games", false, 50)]
	public static void BuildGame()
	{
		BuildGameServer();
		BuildGameClient();
	}
}
