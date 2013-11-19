//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CBridgeCockpit.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CInteractableObject : CNetworkMonoBehaviour 
{
    // Member Delegates
    public delegate void PlayerInteractionHandler(RaycastHit _RayHit);
    
	// Member events
	public event PlayerInteractionHandler UseLeftClick;
	public event PlayerInteractionHandler UseRightClick;
	public event PlayerInteractionHandler UseAction1;
	public event PlayerInteractionHandler UseAction2;
	
	// Member Fields
	
	
    // Member Properties


    // Member Methods
	public override void InstanceNetworkVars()
    {
		
	}
	
	public void Start()
	{
		// Set the layer of myself and all children to be of "InteractableObject"
		CUtility.SetLayerRecursively(gameObject, LayerMask.NameToLayer("InteractableObject"));
	}
	
	public void OnInteractionEvent(CPlayerInteractor.EInteractionType _InteractionEvent, GameObject _PlayerInteractor, RaycastHit _RayHit)
	{	
		ushort networkViewId = GetComponent<CNetworkView>().ViewId;
		
		switch(_InteractionEvent)
		{
		case CPlayerInteractor.EInteractionType.PrimaryStart:
			if(UseLeftClick != null)
				UseLeftClick(_RayHit);
			break;
			
		case CPlayerInteractor.EInteractionType.SecondaryStart:
			if(UseRightClick != null)
				UseRightClick(_RayHit);
			break;
			
		case CPlayerInteractor.EInteractionType.Use:
			if(UseAction1 != null)
				UseAction1(_RayHit);
			break;
			
		case CPlayerInteractor.EInteractionType.Action2:
			if(UseAction2 != null)
				UseAction2(_RayHit);
			break;
			
		default:
			break;
		}
	}
}

