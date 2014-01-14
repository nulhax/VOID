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
		
		ButtonPressedDown,
		
		MAX
	}
	
	// Member Fields
	private bool m_bSubviewChanged = false;
	private CNetworkVar<uint> m_CurrentActiveSubviewId = null;
	
	static private CNetworkStream s_CurrentDUIInteractions = new CNetworkStream();
	
	// Member Properties
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		m_CurrentActiveSubviewId = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
	}
	
	public static void SerializeDUIInteractions(CNetworkStream _cStream)
    {
		if(s_CurrentDUIInteractions.HasUnreadData)
		{
			_cStream.Write(s_CurrentDUIInteractions);
			s_CurrentDUIInteractions.Clear();
		}
    }

	public static void UnserializeDUIInteraction(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {	
		while(_cStream.HasUnreadData)
		{
			// Get the interaction event and network view from the stream
			EInteractionEvent interactionEvent = (EInteractionEvent)_cStream.ReadByte();
			ushort duiConsoleNetworkViewId = _cStream.ReadUShort();
			
			switch(interactionEvent)
			{
			case EInteractionEvent.ButtonPressedDown:
				uint duiViewId = _cStream.ReadUInt();
				uint duiButtonId = _cStream.ReadUInt();
				CNetwork.Factory.FindObject(duiConsoleNetworkViewId).GetComponent<CDUIInteraction>().
						ButtonPressedDown(duiConsoleNetworkViewId, duiViewId, duiButtonId);
				break;
			}
		}
    }
	
  	public void OnNetworkVarSync(INetworkVar _rSender)
    {
        if (_rSender == m_CurrentActiveSubviewId)
        {	
			m_bSubviewChanged = true;
        }
	}
	
	public void Initialise()
	{
		// Register the interactable object event
		CActorInteractable IO = GetComponent<CActorInteractable>();
		IO.EventPrimaryStart += HandlerPlayerActorLeftClick;
		
		if(CNetwork.IsServer)
		{
			// Register the subview change event
			CDUI dui = GetComponent<CDUIConsole>().DUI; 
			dui.SubviewChanged += HandleSubviewChange;
		}
	}
	
	public void Update()
	{
		if(m_bSubviewChanged)
		{
			CDUI dui = GetComponent<CDUIConsole>().DUI;
			dui.SetActiveSubView(dui.GetSubView(m_CurrentActiveSubviewId.Get()).gameObject);
			m_bSubviewChanged = false;
		}
	}
	
	[AClientOnly]
	private void HandlerPlayerActorLeftClick(RaycastHit _RayHit, ushort _usPlayerActorViewId)
	{	
		// Get the UI from the console hit
		CDUI dui = GetComponent<CDUIConsole>().DUI;
		
		// Find the element hit
		GameObject hitElement = dui.FindDUIElementCollisions(_RayHit.textureCoord.x, _RayHit.textureCoord.y);
		
		// If it did get the element pressed on the screen
		if(hitElement != null)
		{
			CDUIElement duiElement = hitElement.GetComponent<CDUIElement>();
			if(duiElement.ElementType == CDUIElement.EElementType.Button)
			{
				// Add this information to the network stream to serialise
				s_CurrentDUIInteractions.Write((byte)EInteractionEvent.ButtonPressedDown);
				s_CurrentDUIInteractions.Write(GetComponent<CNetworkView>().ViewId);
				s_CurrentDUIInteractions.Write(duiElement.ParentViewID);
				s_CurrentDUIInteractions.Write(duiElement.ElementID);
			}
		}
	}
	
	[AServerOnly]
	private void HandleSubviewChange(uint _iActiveSubview)
	{
		m_CurrentActiveSubviewId.Set(_iActiveSubview);
	}
	
	[AServerOnly]
	private void ButtonPressedDown(ushort _duiConsoleNetworkId, uint _duiViewId, uint _duiButtonId)
	{
		CDUI dui = CNetwork.Factory.FindObject(_duiConsoleNetworkId).GetComponent<CDUIConsole>().DUI;
		
		// Active the button press down call
		if(_duiViewId == 0)
		{
			((CDUIButton)dui.MainView.GetDUIElement(_duiButtonId)).OnPressDown();
		}
		else
		{
			((CDUIButton)dui.GetSubView(_duiViewId).GetDUIElement(_duiButtonId)).OnPressDown();
		}
	}
}
