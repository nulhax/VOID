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


/* Implementation */


public class CDUIInteraction : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		
	}
	
	public void Start()
	{
		CInteractableObject IO = GetComponent<CInteractableObject>();
		
		IO.UseLeftClick += HandlerPlayerActorLeftClick;
	}
	
	private void HandlerPlayerActorLeftClick(ushort _PlayerActorNetworkViewId, ushort _InteractableObjectNetworkViewId)
	{
		// Placeholder: Create the ray from the players perspective to get the texture coords back
		CPlayerHeadMotor playerHeadMotor = CNetwork.Factory.FindObject(_PlayerActorNetworkViewId).GetComponent<CPlayerHeadMotor>();
		Vector3 orig = playerHeadMotor.ActorHead.transform.position;
		Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
		float distance = 5.0f;
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(new Ray(orig, direction), out hit, distance, 1 << LayerMask.NameToLayer("InteractableObject"));
		
		// Get the UI from the console hit
		CDUI dui = GetComponent<CDUIConsole>().DUI;
		
		// Find the element hit
		GameObject hitElement = dui.FindDUIElementCollisions(hit.textureCoord.x, hit.textureCoord.y);
		
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
	
	private void ButtonPressedDown(CDUI _dui, uint _duiViewId, uint _duiButtonId)
	{
		// Active the button press down call
		if(_duiViewId == 0)
		{
			((CDUIButton)_dui.MainView.GetDUIElement(_duiButtonId)).OnPressDown();
		}
		else
		{
			((CDUIButton)_dui.GetSubView(_duiViewId).GetDUIElement(_duiButtonId)).OnPressDown();
		}
	}
}
