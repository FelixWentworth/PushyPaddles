using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SP_Movement : MonoBehaviour
{
    public LayerMask Mask;
    
    [SerializeField] private GameObject ValidPress;
    [SerializeField] private GameObject InvalidPress;
	
	// Update is called once per frame
	void Update ()
	{
	    if (SP_Manager.Instance.Get<SP_GameManager>().GameSetup())
	    {
	        if (Input.GetMouseButtonDown(0))
	        {
	            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	            RaycastHit hit;

	            if (Physics.Raycast(ray, out hit, Mask))
	            {
	                Debug.Log(hit.collider.name);
	                // Check if hit a player track
	                if (hit.collider.CompareTag("MovementTrack"))
	                {
	                    var player = hit.collider.GetComponent<SP_MovementTrack>().GetPlayer();
	                    if (player == null || !player.CanMove || player.OnPlatform)
	                    {
	                        return;
	                    }

	                    var pressPos = hit.point + (Vector3.up * 0.25f);
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

	                    p.Interact();
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
}
