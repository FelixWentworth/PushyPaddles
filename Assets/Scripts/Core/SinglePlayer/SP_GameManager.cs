using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SP_GameManager : MonoBehaviour
{

	public int _playerModel { get; private set; }

	public List<GameObject> NetworkObjectsToActivate;

	private GameManager _gameManager;
	private InstructionManager _instructionManager;

	private bool _modelSet;
	private bool _lessonSet;
	private bool _gamePaused;
	public bool _usedPaddle { get; set; }

	private string _year;
	private string _lesson;

	public void SetModel(int model)
	{
		_playerModel = model;
		_modelSet = true;
	}

	public int GetModelNumber()
	{
		return _playerModel;
	}

	public void ForceActive()
	{
		// activate objects which normally rely on a network
		foreach (var obj in NetworkObjectsToActivate)
		{
			if (obj != null)
			{
				// chance for objects that dont destroy on load to be null
				obj.SetActive(true);
			}
		}
		_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	public void CreatePlayers()
	{
		for (int i = 0; i < 3; i++)
		{
			_gameManager.CreatePlayer(i + 1);
		}
	}

	public void SetLesson(string year, string lesson)
	{
		_year = "Year " + year;
		_lesson = lesson;
		_gameManager.SetSpLesson("Year " + year, lesson);
		_lessonSet = true;
	}

	public string GetYear()
	{
		return _year;
	}

	public string GetLesson()
	{
		return _lesson;
	}

	public void TogglePause()
	{
		_gamePaused = !_gamePaused;
	}

	public bool GameSetup()
	{
		return _modelSet && _lessonSet && !_gamePaused;
	}

	public List<Player> GetPlayers()
	{
		return _gameManager.GetAllPlayers();
	}

	public void NextRound()
	{
		// Get a player
		var player = _gameManager.GetAllPlayers()[0];
		// Call next round
		player.NextRound();
	}

	public void ShowPlatformPickupIndicator()
	{
		if (_instructionManager == null)
		{
			_instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
		}

		_instructionManager.ShowMoveToPlatformIndicator();
	}

	public void ShowPlaceIndicator()
	{
		if (_instructionManager == null)
		{
			_instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
		}

		_instructionManager.ShowMoveToPlaceIndicator(Player.Role.Floater, 0);
	}

	public void ShowPushIndicator()
	{
		if (_instructionManager == null)
		{
			_instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
		}

		_instructionManager.ShowSinglePlayerPush();
	}

	public void UpdatePushIndicator(Player player)
	{
		if (_instructionManager == null)
		{
			_instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
		}

		var pos = new Vector3(0, 0, player.transform.position.z);
		_instructionManager.UpdatePushSinglePlayer(pos);
	}

	public void HideIndicators()
	{
		if (_instructionManager == null)
		{
			_instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
		}

		_instructionManager.DisableInsctructions();
	}

	public void HideSinglePlayerPushIndicator()
	{
		if (_instructionManager == null)
		{
			_instructionManager = GameObject.Find("PlayerInstructionManager").GetComponent<InstructionManager>();
		}

		_instructionManager.DisableTouchPushInstruction();
	}
}
