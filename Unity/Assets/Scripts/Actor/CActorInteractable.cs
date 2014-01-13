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


	public delegate void NotifyInteraction(RaycastHit _RayHit, ushort _usPlayerActorViewId);


	public event NotifyInteraction EventPrimaryStart;
	public event NotifyInteraction EventSecondaryStart;
	public event NotifyInteraction EventUse;
	
	
// Member Properties


// Member Methods


	public override void InstanceNetworkVars()
    {
		
	}
	
	
	public void Awake()
	{
		if(gameObject.layer != LayerMask.NameToLayer("InteractableObject"))
			Debug.LogError("Interactable object not set to the interactableobject layer! Make sure its set on the prefab!" + " " + gameObject.name);
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

