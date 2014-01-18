//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CDUIInteraction.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System;


/* Implementation */

[RequireComponent(typeof(CActorInteractable))]
public class CDUIInteraction : CNetworkMonoBehaviour 
{	
	// Member Types
	public enum EInteractionEvent : byte
	{
		INVALID,
		
		ButtonPressed,
		ButtonReleased,
		
		MAX
	}
	
	// Member Fields


	// Member Properties
	public CDUI DUI
	{
		get { return(gameObject.GetComponent<CDUI>());}
	}

	// Member Methods
	
//	public static void SerializeDUIInteractions(CNetworkStream _cStream)
//    {
//		if(s_DUIInteractions.HasUnreadData)
//		{
//			_cStream.Write(s_DUIInteractions);
//			s_DUIInteractions.Clear();
//		}
//    }
//
//	public static void UnserializeDUIInteraction(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
//    {	
//		while(_cStream.HasUnreadData)
//		{
//			// Get the interaction event and network view from the stream
//			EInteractionEvent interactionEvent = (EInteractionEvent)_cStream.ReadByte();
//			CNetworkViewId duiConsoleNetworkViewId = _cStream.ReadNetworkViewId();
//			
//			switch(interactionEvent)
//			{
//			case EInteractionEvent.ButtonPressed:
//				uint duiViewId = _cStream.ReadUInt();
//				uint duiButtonId = _cStream.ReadUInt();
//				CNetwork.Factory.FindObject(duiConsoleNetworkViewId).GetComponent<CDUIInteraction>().
//						ButtonPressedDown(duiConsoleNetworkViewId, duiViewId, duiButtonId);
//				break;
//			}
//		}
//    }

	public override void InstanceNetworkVars()
	{
	}

  	public void OnNetworkVarSync(INetworkVar _rSender)
	{
	}
	
	public void Initialise()
	{
		// Register the interactable object event
		CActorInteractable IO = GetComponent<CActorInteractable>();
		IO.EventHover += HandlePlayerHover;
		IO.EventPrimaryStart += HandlePlayerPrimaryStart;
		IO.EventPrimaryEnd += HandlePlayerPrimaryEnd;
	}

	[AClientOnly]
	private void HandlePlayerHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
	{	
		// Update the NGUI camera ray position
		//DUI.DUICamera.GetComponent<>()
	}
	
	[AClientOnly]
	private void HandlePlayerPrimaryStart(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
	{	
		// Find the element hit
		GameObject hitElement = DUI.FindDUIElementCollisions(_RayHit.textureCoord.x, _RayHit.textureCoord.y);
		
		// If it did get the element pressed on the screen
		if(hitElement != null)
		{
			CDUIElement duiElement = hitElement.GetComponent<CDUIElement>();
			if(duiElement.ElementType == CDUIElement.EElementType.Button)
			{
//				// Add this information to the network stream to serialise
//				s_DUIInteractions.Write((byte)EInteractionEvent.ButtonPressed);
//				s_DUIInteractions.Write(GetComponent<CNetworkView>().ViewId);
//				s_DUIInteractions.Write(duiElement.ParentViewID);
//				s_DUIInteractions.Write(duiElement.ElementID);
			}
		}
	}

	[AClientOnly]
	private void HandlePlayerPrimaryEnd(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
	{	
		// Find the element hit
		GameObject hitElement = DUI.FindDUIElementCollisions(_RayHit.textureCoord.x, _RayHit.textureCoord.y);
		
		// If it did get the element pressed on the screen
		if(hitElement != null)
		{
			CDUIElement duiElement = hitElement.GetComponent<CDUIElement>();
			if(duiElement.ElementType == CDUIElement.EElementType.Button)
			{
//				// Add this information to the network stream to serialise
//				s_DUIInteractions.Write((byte)EInteractionEvent.ButtonReleased);
//				s_DUIInteractions.Write(GetComponent<CNetworkView>().ViewId);
//				s_DUIInteractions.Write(duiElement.ParentViewID);
//				s_DUIInteractions.Write(duiElement.ElementID);
			}
		}
	}
}
