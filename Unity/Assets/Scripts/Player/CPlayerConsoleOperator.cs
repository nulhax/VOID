//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CPlayerConsoleOperator : CNetworkMonoBehaviour
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
	private EConsoleEvent m_CurrentConsoleEvent = EConsoleEvent.Nothing;
	
	private uint m_RoomConsoleID = 0;
	private uint m_ButtonID = 0;
	
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
			// Raycast to find a console.
			
//			// Get the console component
//			CDUIConsole console = GetComponent<CDUIConsole>();
//			
//			// Get the clients player actor
//			CPlayerHeadMotor playerHeadMotor = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>();
//		
//			Vector3 orig = playerHeadMotor.ActorHead.transform.position;
//			Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
//			float distance = 5.0f;
//			RaycastHit hit = new RaycastHit();
//		
//			// Check if the player cast a ray on the screen
//			if(console.CheckScreenRaycast(orig, direction, distance, out hit))
//			{
//				GameObject hitObj = console.MainView.FindDUIElementCollisions(hit);
//				
//				// If it did get the element pressed on the screen
//				if(hitObj != null)
//				{
//					hitObj.GetComponent<CDUIButton>().OnPressDown();
//					
//					//m_CurrentConsoleEvent = EConsoleEvent.ButtonPressDown;
//					//m_RoomConsoleID = transform.parent.GetComponent<CRoomInterface>().RoomId;
//					//m_ButtonID = hitObj.GetComponent<CDUIButton>().ElementID;
//				}
//			}
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
