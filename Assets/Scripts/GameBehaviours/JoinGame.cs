using UnityEngine;
using UnityEngine.SceneManagement;

using MasterServerKit;

public class JoinGame : MonoBehaviour
{

	bool _tryJoin;

	// Use this for initialization
	void Update()
	{
		if (!_tryJoin && ClientAPI.masterServerClient != null)
		{
			_tryJoin = true;
			PlayNow();
		}
	}

	private void PlayNow()
	{
		ClientAPI.ConnectToMasterServer(() =>
				{
					ClientAPI.LoginAsGuest(
						() =>
							{
								ClientAPI.PlayNow(JoinGameServer,
									JoinGameServer,
									error =>
										{
											Debug.Log("No available games.");
										});
							},
						error =>
							{
								var errorMsg = "";
								switch (error)
								{
									case LoginError.DatabaseConnectionError:
										errorMsg = "There was an error connecting to the database.";
										break;

									case LoginError.NonexistingUser:
										errorMsg = "This user does not exist.";
										break;

									case LoginError.InvalidCredentials:
										errorMsg = "Invalid credentials.";
										break;

									case LoginError.ServerFull:
										errorMsg = "The server is full.";
										break;

									case LoginError.AuthenticationRequired:
										errorMsg = "Authentication is required.";
										break;

									case LoginError.UserAlreadyLoggedIn:
										errorMsg = "This user is already logged in.";
										break;
								}
								Debug.Log(errorMsg);
							});
				},
			() =>

				{
					Debug.Log("Could not connect to master server.");
				});
	}

	private void JoinGameServer(string ip, int port)
	{
		ClientAPI.JoinGameServer(ip, port);
	}
}
