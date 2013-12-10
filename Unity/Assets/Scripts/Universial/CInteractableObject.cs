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


	public delegate void NotifyInteraction(RaycastHit _RayHit, ushort _usPlayerActorViewId);


// Member Delegates & Events


	public event NotifyInteraction EventPrimaryStart;
	public event NotifyInteraction EventSecondaryStart;
	public event NotifyInteraction EventUse;
	
	
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
		ushort usPlayerActorViewId = _PlayerInteractor.GetComponent<CNetworkView>().ViewId;

		switch(_InteractionEvent)
		{
		case CPlayerInteractor.EInteractionType.PrimaryStart:
			if(EventPrimaryStart != null)
				EventPrimaryStart(_RayHit, usPlayerActorViewId);
			break;
			
		case CPlayerInteractor.EInteractionType.SecondaryStart:
			if(EventSecondaryStart != null)
				EventSecondaryStart(_RayHit, usPlayerActorViewId);
			break;
			
		case CPlayerInteractor.EInteractionType.Use:
			if (EventUse != null)
				EventUse(_RayHit, usPlayerActorViewId);
			break;
			
		default:
			break;
		}
	}


// Member Fields


}

