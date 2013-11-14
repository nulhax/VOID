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


public class CDUIInteraction : CNetworkMonoBehaviour 
{	
	// Member Types
	
	// Member Fields
	private bool m_bSubviewChanged = false;
	private CNetworkVar<uint> m_CurrentActiveSubviewId = null;

	// Member Properties
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		m_CurrentActiveSubviewId = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
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
		CInteractableObject IO = GetComponent<CInteractableObject>();
		IO.UseLeftClick += HandlerPlayerActorLeftClick;
		
		// Register the subview change event
		CDUI dui = GetComponent<CDUIConsole>().DUI; 
		dui.SubviewChanged += HandleSubviewChange;
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
	
	private void HandleSubviewChange(uint _iActiveSubview)
	{
		m_CurrentActiveSubviewId.Set(_iActiveSubview);
	}
	
	private void HandlerPlayerActorLeftClick(ushort _PlayerActorNetworkViewId, ushort _InteractableObjectNetworkViewId, RaycastHit _RayHit)
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
				ButtonPressedDown(dui, duiElement.ParentViewID, duiElement.ElementID);
			}
		}
	}
	
	private void ButtonPressedDown(CDUI _Dui, uint _duiViewId, uint _duiButtonId)
	{
		// Active the button press down call
		if(_duiViewId == 0)
		{
			((CDUIButton)_Dui.MainView.GetDUIElement(_duiButtonId)).OnPressDown();
		}
		else
		{
			((CDUIButton)_Dui.GetSubView(_duiViewId).GetDUIElement(_duiButtonId)).OnPressDown();
		}
	}
}
