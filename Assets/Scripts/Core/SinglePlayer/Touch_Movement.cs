using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch_Movement : MonoBehaviour
{
	private static bool _useTouch;
	public static bool UseTouch
	{
		get { return _useTouch; }
		set
		{
			_useTouch = value;
			SetObject();
		}
	}

	public LayerMask Mask;
    
    [SerializeField] private GameObject ValidPress;
    [SerializeField] private GameObject InvalidPress;

	[SerializeField] private GameObject _touchControlsOption;
	private static GameObject _touchEnabledGameObject;

	void Awake()
	{
		_touchEnabledGameObject = _touchControlsOption.transform.Find("Toggle/On").gameObject;

		var touchOptionAvailable = Application.platform != RuntimePlatform.Android &&
		                           Application.platform != RuntimePlatform.IPhonePlayer;

		_touchControlsOption.SetActive(touchOptionAvailable);
		if (!touchOptionAvailable)
		{
			UseTouch = true;
		}
	}

	// Update is called once per frame
	void Update ()
	{
		var sp = SP_Manager.Instance.IsSinglePlayer();
		if (sp && _touchControlsOption.activeSelf)
		{
			_touchControlsOption.SetActive(false);
			_useTouch = true;
		}
		// TODO check if touch movemoent is enabled
	    if (UseTouch || (sp && SP_Manager.Instance.Get<SP_GameManager>().GameSetup()))
	    {
		    if (!UseTouch && _touchControlsOption.activeSelf)
		    {
				// Touch controls can be toggled, but have not been activated so dont allow touch movements
				return;
		    }
		    if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
	        {
				var ray = new Ray();
		        if (Input.touchCount > 0)
		        {
			        ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
		        }
		        else
		        {
			        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				}

				RaycastHit hit;

	            if (Physics.Raycast(ray, out hit, Mask))
	            {
	                //Debug.Log(hit.collider.name);
	                // Check if hit a player track
	                if (hit.collider.CompareTag("MovementTrack"))
	                {
		                var track = hit.collider.GetComponent<SP_MovementTrack>();
		                if (track == null)
		                {
			                return;
		                }

						var pressPos = hit.point + (Vector3.up * track.DistanceToGround);
		                Player player;
		                if (sp)
		                {
			                player = track.GetPlayer(pressPos.x);
		                }
		                else
		                {
			                // Find the player to move
			                player = GameObject.Find("GameManager").GetComponent<GameManager>().GetLocalPlayer();
		                }
		                if (player == null || !player.CanMove || player.OnPlatform)
		                {
			                return;
		                }

		                pressPos = player.PlayerRole == Player.Role.Floater
			                ? new Vector3(pressPos.x, pressPos.y, player.transform.position.z)
			                : new
				                Vector3(player.transform.position.x, pressPos.y, pressPos.z);
						var press = ShowPress(true, pressPos);
		                player.MoveToAndUse(pressPos, press);
					}
	                else if (hit.collider.CompareTag("Player"))
	                {
	                    var player = hit.collider.gameObject;
	                    var p = player.GetComponent<Player>();

		                if (sp || (!sp && p == GameObject.Find("GameManager").GetComponent<GameManager>().GetLocalPlayer()))
		                {
			                p.Interact();
		                }
	                }
	                else
	                {
	                    ShowPress(false, hit.point + (Vector3.up * 0.25f));
	                }
	            }
	        }
	    }
	}

    private GameObject ShowPress(bool valid, Vector3 position)
    {
        var pressObject = valid ? ValidPress : InvalidPress;
        var go = Instantiate(pressObject, position, Quaternion.Euler(0, -90, 0));
        return go;
    }

	public void ToggleEnabled()
	{
		UseTouch = !UseTouch;	
	}

	public static void SetObject()
	{
		if (_touchEnabledGameObject != null)
		{
			_touchEnabledGameObject.SetActive(UseTouch);
		}
	}
}
