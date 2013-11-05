using UnityEngine;
using System.Collections;

public class CPlayerConsoleInteraction : CNetworkMonoBehaviour
{
	// Member Types
	
	// Member Properties
	
	// Member Fields
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		
	}
	
	public void Update()
	{
		if(CGame.PlayerActor == gameObject)
		{
			if(Input.GetMouseButtonDown(0))
			{
				// Check all consoles for collisions with the screen
				foreach(CRoomGeneral roomGeneral in CGame.Ship.GetComponentsInChildren<CRoomGeneral>())
				{	
					DUIConsole console = roomGeneral.RoomControlConsole.GetComponent<DUIConsole>();
					
					CPlayerHeadMotor playerHeadMotor = GetComponent<CPlayerHeadMotor>();
				
					Vector3 orig = playerHeadMotor.ActorHead.transform.position;
					Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
					
					console.CheckScreenCollision(orig, direction);
					Debug.Log("Checking Console");
				}
			}
		}
	}
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
		
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		
    }
}
