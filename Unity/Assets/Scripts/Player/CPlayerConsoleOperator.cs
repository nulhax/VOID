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
	public enum EConsoleEvent : short
	{
		INVALID = -1,
		
		Nothing,
		ButtonPressDown,
		
		MAX
	}
	
	// Member Fields
	private EConsoleEvent m_CurrentConsoleEvent = EConsoleEvent.Nothing;
	
	private ushort m_ConsoleNetworkId = 0;
	private uint m_DUIViewID = 0;
	private uint m_DUIButtonID = 0;
	
	// Member Properties
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		
	}
	
	public void Update()
	{
		// Mouse Down
		if(Input.GetMouseButtonDown(0) && gameObject == CGame.PlayerActor)
		{
			CPlayerHeadMotor playerHeadMotor = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>();
		
			// Raycast to find a console.
			Vector3 orig = playerHeadMotor.ActorHead.transform.position;
			Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
			float distance = 5.0f;
			RaycastHit hit = new RaycastHit();
			
			// Check if the player cast a ray on the screen
			if(CheckConsoleScreenRaycast(orig, direction, distance, out hit))
			{
				// Get the console
				GameObject console = hit.collider.transform.parent.gameObject;
				
				// Get the UI from the console hit
				CDUI dui = console.GetComponent<CDUIConsole>().DUI;
				
				// Find the element hit
				GameObject hitElement = dui.FindDUIElementCollisions(hit);
				
				// If it did get the element pressed on the screen
				if(hitElement != null)
				{
					CDUIElement duiElement = hitElement.GetComponent<CDUIElement>();
					if(duiElement.ElementType == CDUIElement.EElementType.Button)
					{
						m_CurrentConsoleEvent = EConsoleEvent.ButtonPressDown;
						m_ConsoleNetworkId = console.GetComponent<CNetworkView>().ViewId;
						m_DUIViewID = duiElement.ParentViewID;
						m_DUIButtonID = duiElement.ElementID;
					}
				}
			}
		}
	}
	
	private bool CheckConsoleScreenRaycast(Vector3 _origin, Vector3 _direction, float _fDistance, out RaycastHit _rh)
    {
		Ray ray = new Ray(_origin, _direction);
		
		if (Physics.Raycast(ray, out _rh, _fDistance, 1 << LayerMask.NameToLayer("Screen")))
		{
			return(true);
		}
		
		return(false); 
    }
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.PlayerActorViewId != 0)
		{
			CPlayerConsoleOperator actorConsoleOperator = CGame.PlayerActor.GetComponent<CPlayerConsoleOperator>();
			
			if(actorConsoleOperator.m_CurrentConsoleEvent != EConsoleEvent.Nothing)
			{
				_cStream.Write((short)actorConsoleOperator.m_CurrentConsoleEvent);
				_cStream.Write(actorConsoleOperator.m_ConsoleNetworkId);
				_cStream.Write(actorConsoleOperator.m_DUIViewID);
				_cStream.Write(actorConsoleOperator.m_DUIButtonID);
				
				// Reset the states
				actorConsoleOperator.m_CurrentConsoleEvent = EConsoleEvent.Nothing;
				actorConsoleOperator.m_ConsoleNetworkId = 0;
				actorConsoleOperator.m_DUIViewID = 0;
				actorConsoleOperator.m_DUIButtonID = 0;
			}
		}
    }

	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		CPlayerConsoleOperator actorConsoleOperator = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerConsoleOperator>();
		
		EConsoleEvent consoleEvent = (EConsoleEvent)_cStream.ReadShort();
		uint consoleViewID = _cStream.ReadUShort();
		uint duiViewID = _cStream.ReadUInt();
		uint duiButtonID = _cStream.ReadUInt();
		
		// Invoke the rpc to activate the event
		actorConsoleOperator.InvokeRpcAll("ButtonPressedDown", consoleViewID, duiViewID, duiButtonID);
    }
	
	[ANetworkRpc]
	public void ButtonPressedDown(ushort _ConsoleViewID, uint _duiViewId, uint _duiButtonId)
	{
		// Get the DUI from the console within the room
		CDUI consoleDIU = CNetwork.Factory.FindObject(_ConsoleViewID).GetComponent<CDUIConsole>().DUI;
		
		// Active the button press down call
		if(_duiViewId == 0)
		{
			((CDUIButton)consoleDIU.DUIMainView.GetDUIElement(_duiButtonId)).OnPressDown();
		}
		else
		{
			((CDUIButton)consoleDIU.GetSubView(_duiViewId).GetDUIElement(_duiButtonId)).OnPressDown();
		}
	}
}
