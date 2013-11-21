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
	public event PlayerInteractionHandler InteractionPrimaryStart;
	public event PlayerInteractionHandler InteractionSecondaryStart;
	public event PlayerInteractionHandler InteractionUse;
	
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
			if(InteractionPrimaryStart != null)
				InteractionPrimaryStart(_RayHit);
			break;
			
		case CPlayerInteractor.EInteractionType.SecondaryStart:
			if(InteractionSecondaryStart != null)
				InteractionSecondaryStart(_RayHit);
			break;
			
		case CPlayerInteractor.EInteractionType.Use:
			if(InteractionUse != null)
				InteractionUse(_RayHit);
			break;
			
		default:
			break;
		}
	}
}

