using UnityEngine;
using System.Collections;

public class CPlayerConsoleOperation : CNetworkMonoBehaviour
{
	// Member Types
	public enum EConsolePressState
	{
		INVALID = -1,
		
		NotPressing,
		PressDown,
		PressHold,
		PressRelease,
	}
	
	// Member Fields
	private EConsolePressState m_CurrentConsolePressState;
	
	
	// Member Properties
	
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
				m_CurrentConsolePressState = EConsolePressState.PressDown;
			}
			else if(Input.GetMouseButton(0))
			{
				m_CurrentConsolePressState = EConsolePressState.PressHold;
			}
			else if(Input.GetMouseButtonUp(0))
			{
				m_CurrentConsolePressState = EConsolePressState.PressRelease;
			}
			
			// Mouse Down
			if(m_CurrentConsolePressState == EConsolePressState.PressDown)
			{
				// Placeholder: Check all consoles for collisions with the screen
				foreach(CRoomGeneral roomGeneral in CGame.Ship.GetComponentsInChildren<CRoomGeneral>())
				{	
					DUIConsole console = roomGeneral.RoomControlConsole.GetComponent<DUIConsole>();
					
					CPlayerHeadMotor playerHeadMotor = GetComponent<CPlayerHeadMotor>();
				
					Vector3 orig = playerHeadMotor.ActorHead.transform.position;
					Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
					
					console.CheckScreenCollision(orig, direction);
				}
			}
		}
	}
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.PlayerActorViewId != 0)
		{
			
		}
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		
    }
}
