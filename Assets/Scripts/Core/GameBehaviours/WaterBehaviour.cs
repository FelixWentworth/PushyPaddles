using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WaterBehaviour : NetworkBehaviour
{

    [SerializeField] private float _tideStrength = 0.01f;
    [SerializeField] private float _maxPaddleStrength = 1f;
    // The strength of paddle power from another player
    [Range(-1f, 1f)] private float _paddleStrength = 0f;

    // the time between max to 0 paddle strength
    private float _paddleCooldownTime = 1f;

    // the z position that once an object passes they will be Respawned
    private float _respawnZPos;

    // Our clamp positions for the x axis, the object should not leave the water
    private float _minXClamp;
    private float _maxXClamp;

    private GameObject _currentPlatform;

    public Material Material;

    private GameManager _gameManager;

    void Awake()
    {
        _respawnZPos = transform.position.z + (transform.localScale.y / 2f);

        _minXClamp = transform.position.x - (transform.localScale.x / 2f);
        _maxXClamp = transform.position.x + (transform.localScale.x / 2f);

        StartCoroutine(ParallaxWater());
    }

    public void TouchedWater(MovingObject movingObject)
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        if (!_gameManager.GamePlaying())
        {
            // Nothing should move
            return;
        }
        // Detect if this object floats
        if (movingObject.CanFloat)
        {
            if (movingObject.gameObject.tag == "Platform")
            {
                _currentPlatform = movingObject.gameObject;
            }
            Move(movingObject);
        }
        else
        {
            movingObject.GetComponent<BoxCollider>().enabled = false;
            movingObject.FellInWater();
            movingObject.Respawn();
        }
    }

    public void Move(MovingObject go)
    { 
        // Get the tide strength and the x transform
        var newPosition = new Vector3(
            go.transform.position.x + (_paddleStrength * _maxPaddleStrength),
            go.transform.position.y,
            go.transform.position.z + _tideStrength
        );

        MoveFloatingObject(go.gameObject, newPosition);
    }

    [Server]
    private void MoveFloatingObject(GameObject go, Vector3 pos)
    {
        go.transform.position = pos;
        // Clamp the x axis
        ClampX(go.gameObject);
    }

    private void ClampX(GameObject go)
    {
        // Add padding as the object pivot is in the center
        var padding = go.transform.localScale.x / 2f;
        var minX = _minXClamp + padding;
        var maxX = _maxXClamp - padding;
        var newX = go.transform.position.x;
        if (newX <= minX)
        {
            newX = minX;
        }
        else if (newX >= maxX)
        {
            newX = maxX;
        }

        if (newX != go.transform.position.x)
        {
            go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
        }
    }

    [Command]
    public void CmdPaddleUsed(int playerId)
    {
        var player = _gameManager.GetPlayer(playerId);
        PaddleUsed(player);
    }

    // Only allow the server to run this as to avoid players exploiting
    [Server]
    public void PaddleUsed(Player player)
    {
        switch (player.PlayerRole)
        {
            case Player.Role.Paddler:
                _paddleStrength = player.transform.position.x > 0f ? -1f : 1f;
                break;
            case Player.Role.Unassigned:
            case Player.Role.Floater:
            default:
                // Player Cannot interact with water
                return;
        }

        // get the angle between the player and the object in the water
        var modifier = 1f;

        if (_currentPlatform != null)
        {

            var playerPosition = player.transform.position;
            var platformPosition = _currentPlatform.transform.position;
            var zDist = platformPosition.z - playerPosition.z;

            zDist = zDist < 0 ? zDist * -1f : zDist;
            if (zDist <= 0.5f)
            {
                modifier = 1f;
            }
            else if (zDist > 0.5f && zDist <= 1.25f)
            {
                modifier = 0.5f;
            }
            else
            {
                modifier = 0f;
            }

        }

        StartCoroutine(PaddleUsed(modifier));

    }

    private IEnumerator PaddleUsed(float modifier)
    {
        var t = 0f;

        var startStrength = _paddleStrength * modifier;
        while (t < _paddleCooldownTime)
        {
            _paddleStrength = Mathf.Lerp(startStrength, 0f, t/ _paddleCooldownTime);
            t += Time.deltaTime;
            yield return null;
        }
        _paddleStrength = 0f;
    }

    private IEnumerator ParallaxWater()
    {
        while (true)
        {
            var y = Mathf.Repeat(Time.time * _tideStrength * 0.01f, 1);
            var offset = new Vector2(Material.mainTextureOffset.x + (_paddleStrength * 0.01f), y);
            Material.SetTextureOffset("_MainTex", offset);

            yield return null;
        }
    }
}
