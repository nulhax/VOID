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
    public delegate void PlayerInteractionHandler(ushort _PlayerActorNetworkViewId, ushort _InteractableObjectNetworkViewId);
    
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
		SetLayerRecursively(gameObject, LayerMask.NameToLayer("InteractableObject"));
	}
	
	public bool IsInteractionEventRegistered(CPlayerInteractor.EInteractionEvent _InteractionEvent)
	{
		bool eventRegistered = false;
		
		switch(_InteractionEvent)
		{
		case CPlayerInteractor.EInteractionEvent.LeftClick:
			eventRegistered = UseLeftClick != null;
			break;
			
		case CPlayerInteractor.EInteractionEvent.RightClick:
			eventRegistered = UseRightClick != null;
			break;
			
		case CPlayerInteractor.EInteractionEvent.Action1:
			eventRegistered = UseAction1 != null;
			break;
			
		case CPlayerInteractor.EInteractionEvent.Action2:
			eventRegistered = UseAction2 != null;
			break;
			
		default:
			break;
		}
		
		return(eventRegistered);
	}
	
	[ANetworkRpc]
	public void OnInteractionEvent(CPlayerInteractor.EInteractionEvent _InteractionEvent, ushort _InteractingPlayerActorViewId)
	{	
		ushort networkViewId = GetComponent<CNetworkView>().ViewId;
		
		switch(_InteractionEvent)
		{
		case CPlayerInteractor.EInteractionEvent.LeftClick:
			if(UseLeftClick != null)
				UseLeftClick(_InteractingPlayerActorViewId, networkViewId);
			break;
			
		case CPlayerInteractor.EInteractionEvent.RightClick:
			if(UseRightClick != null)
				UseRightClick(_InteractingPlayerActorViewId, networkViewId);
			break;
			
		case CPlayerInteractor.EInteractionEvent.Action1:
			if(UseAction1 != null)
				UseAction1(_InteractingPlayerActorViewId, networkViewId);
			break;
			
		case CPlayerInteractor.EInteractionEvent.Action2:
			if(UseAction2 != null)
				UseAction2(_InteractingPlayerActorViewId, networkViewId);
			break;
			
		default:
			break;
		}
	}
	
	private void SetLayerRecursively(GameObject _Obj, int _Layer)
	{
		_Obj.layer = _Layer;
		
		for(int i = 0; i < _Obj.transform.childCount; ++i)
		{
			SetLayerRecursively(_Obj.transform.GetChild(i).gameObject, _Layer);
		}
	}
}

