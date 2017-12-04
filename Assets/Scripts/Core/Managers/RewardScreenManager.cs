using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class RewardScreenManager : UIScreen
{
    [SerializeField]private GameObject _selectRecipientGameObject;
    public RewardsManager RewardsManager;
    private float _speedBoost = 0.05f;
    private float _controlsModifier = -1f;
    private float _boatControlBoost = 0.0005f;
    private float _strengthBoost = 0.1f;

    private GameManager _gameManager;
    private Player _player;

    private bool _spWaiting;

    public enum RewardType
    {
        None = 0,

        SpeedBoost,
        ReverseControls,
        MoreBoatControl,
        MorePaddleStrength,
		IncreaseTide,
		DecreaseTide
    }

    [Serializable]
    public class RewardIcons
    {
        public RewardType Type;
        public Sprite Icon;
        public string LocalizationKey;
        public bool Positive;
	    public bool SupportsSinglePlayer;
    }

    [SerializeField] private List<RewardIcons> _rewardIcons;

    public override void Show()
    {
        base.Show();

        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        _selectRecipientGameObject.SetActive(!SP_Manager.Instance.IsSinglePlayer());

        var players = _gameManager.GetPlayerIds();
        
        // Get a random number of rewards to give
	    var rand = UnityEngine.Random.Range(1, _rewardIcons.Count);

        RewardsManager.ShowReward(3, players, _rewardIcons, rand);

    }

    public override void Hide()
    {
        base.Hide();
    }

    void Update()
    {
        if (IsShowing && _gameManager.GamePlaying())
        {

            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            if (SP_Manager.Instance.IsSinglePlayer() && !_spWaiting)
            {
                StartCoroutine(WaitToSelect());
            }
            else
            {
                _player = _gameManager.GetLocalPlayer();
                if (_player == null)
                {
                    // Should not be able to control
                    return;
                }
                // Determine if the local player has control
                if (_player.PlayerRole == Player.Role.Floater)
                {
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        RewardsManager.Left();
                    }
                    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        RewardsManager.Right();
                    }
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        RewardsManager.Select();
                    }
                }
            }
        }
    }

    private IEnumerator WaitToSelect()
    {
        _spWaiting = true;
        yield return new WaitForSeconds(3.0f);
        _spWaiting = false;
        RewardsManager.Select();
    }

    public void SetReward(RewardType type, string playerId)
    {

        if (SP_Manager.Instance.IsSinglePlayer())
        {
            // all players receive reward
            var players = SP_Manager.Instance.Get<SP_GameManager>().GetPlayers();
            foreach (var p in players)
            {
                AssignReward(type, p);
            }
        }
        else
        {
            var manager = GameObject.Find("GameManager").GetComponent<GameManager>();
            var player = manager.GetLocalPlayer();

            player.GaveReward(playerId);

            AssignReward(type, player);
        }
    }

    private void AssignReward(RewardType type, Player player)
    {
        switch (type)
        {
            case RewardType.None:
                break;
            case RewardType.SpeedBoost:
                player.AssignSpeedBoost(player, _speedBoost);
                break;
            case RewardType.ReverseControls:
                player.AssignReverseControls(player, _controlsModifier);
                break;
            case RewardType.MoreBoatControl:
                player.AssignMoreControl(player, _boatControlBoost);
                break;
            case RewardType.MorePaddleStrength:
                player.AssignMoreStrength(player, _strengthBoost);
                break;
			case RewardType.IncreaseTide:
				player.ChangeTide(increase: true);
				break;
			case RewardType.DecreaseTide:
				player.ChangeTide(increase: false);
				break;
			default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }

}
