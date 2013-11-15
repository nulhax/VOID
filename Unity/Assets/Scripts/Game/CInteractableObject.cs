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
    public delegate void PlayerInteractionHandler(GameObject _PlayerInteractor, RaycastHit _RayHit);
    
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
	
	public void OnInteractionEvent(CPlayerInteractor.EPlayerInteractionEvent _InteractionEvent, GameObject _PlayerInteractor, RaycastHit _RayHit)
	{	
		ushort networkViewId = GetComponent<CNetworkView>().ViewId;
		
		switch(_InteractionEvent)
		{
		case CPlayerInteractor.EPlayerInteractionEvent.LeftClick:
			if(UseLeftClick != null)
				UseLeftClick(_PlayerInteractor, _RayHit);
			break;
			
		case CPlayerInteractor.EPlayerInteractionEvent.RightClick:
			if(UseRightClick != null)
				UseRightClick(_PlayerInteractor, _RayHit);
			break;
			
		case CPlayerInteractor.EPlayerInteractionEvent.Action1:
			if(UseAction1 != null)
				UseAction1(_PlayerInteractor, _RayHit);
			break;
			
		case CPlayerInteractor.EPlayerInteractionEvent.Action2:
			if(UseAction2 != null)
				UseAction2(_PlayerInteractor, _RayHit);
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

