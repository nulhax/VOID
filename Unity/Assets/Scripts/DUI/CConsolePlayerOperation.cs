using UnityEngine;
using System.Collections;

public class CConsolePlayerOperation : CNetworkMonoBehaviour
{
	// Member Types
	public enum EConsoleEvent
	{
		INVALID = -1,
		
		Nothing,
		ButtonPressDown,
		
		MAX
	}
	
	// Member Fields
	static public EConsoleEvent s_BufferedEvent = EConsoleEvent.Nothing;
	
	static public uint s_RoomConsoleID;
	static public uint s_ButtonID;
	
	// Member Properties
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		
	}
	
	public void Update()
	{		
		// Mouse Down
		if(Input.GetMouseButtonDown(0))
		{
			// Placeholder: Check console for collisions with the screen
			CDUIConsole console = GetComponent<CDUIConsole>();
			
			CPlayerHeadMotor playerHeadMotor = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>();
		
			Vector3 orig = playerHeadMotor.ActorHead.transform.position;
			Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
			float distance = 5.0f;
			RaycastHit hit = new RaycastHit();
			
			if(console.CheckScreenRaycast(orig, direction, distance, out hit))
			{
				GameObject hitObj = console.MainView.FindButtonCollisions(hit);
				
				if(hitObj != null)
				{
					hitObj.GetComponent<CDUIButton>().OnPressDown();
					
					s_BufferedEvent = EConsoleEvent.ButtonPressDown;
					s_RoomConsoleID = transform.parent.GetComponent<CRoomInterface>().RoomId;
					s_ButtonID = hitObj.GetComponent<CDUIButton>().ElementID;
				}
			}
		}
	}
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.PlayerActorViewId != 0)
		{
			if(s_BufferedEvent != EConsoleEvent.Nothing)
			{

			}
		}
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		
    }
}
