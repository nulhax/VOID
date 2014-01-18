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


[RequireComponent(typeof(CActorNetworkSyncronized))]
public class CActorInteractable : CNetworkMonoBehaviour 
{

// Member Delegates


	public delegate void NotifyInteraction(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId);


	public event NotifyInteraction EventPrimaryStart;
	public event NotifyInteraction EventSecondaryStart;
	public event NotifyInteraction EventUse;
	
	
// Member Properties


// Member Methods


	public override void InstanceNetworkVars()
    {
		// Empty
	}


	void Start()
	{
		if(gameObject.layer != LayerMask.NameToLayer("InteractableObject"))
			Debug.LogWarning("Interactable object not set to the interactableobject layer! Make sure its set on the prefab!" + " " + gameObject.name);
	}
	

	public void OnInteractionEvent(CPlayerInteractor.EInteractionType _InteractionEvent, GameObject _PlayerInteractor, RaycastHit _RayHit)
	{
		CNetworkViewId cNetworkViewId = GetComponent<CNetworkView>().ViewId;

		switch(_InteractionEvent)
		{
		case CPlayerInteractor.EInteractionType.PrimaryStart:
			if(EventPrimaryStart != null)
				EventPrimaryStart(_RayHit, cNetworkViewId);
			break;
			
		case CPlayerInteractor.EInteractionType.SecondaryStart:
			if(EventSecondaryStart != null)
				EventSecondaryStart(_RayHit, cNetworkViewId);
			break;
			
		case CPlayerInteractor.EInteractionType.Use:
			if (EventUse != null)
				EventUse(_RayHit, cNetworkViewId);
			break;
			
		default:
			break;
		}
	}


// Member Fields


}

