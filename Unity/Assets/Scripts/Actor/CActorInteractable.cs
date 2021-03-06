﻿//  Auckland
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


public class CActorInteractable : CNetworkMonoBehaviour 
{

// Member Delegates


	public delegate void NotifyHoverInteraction(RaycastHit _RayHit, TNetworkViewId _cPlayerActorViewId, bool _bHover);
	public event NotifyHoverInteraction EventHover;


    public delegate void NotifyInputInteraction(RaycastHit _tRayHit, TNetworkViewId _cPlayerActorViewId, bool _bDown);
    public event NotifyInputInteraction EventPrimary;
    public event NotifyInputInteraction EventSecondary;
    public event NotifyInputInteraction EventUse;


// Member Fields

	
// Member Properties


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
		// Empty
	}


    [ALocalOnly]
    public void OnInteractionHover(GameObject _cPlayerActor, RaycastHit _cRaycastHit, bool _bHover)
    {
        TNetworkViewId cNetworkViewId = _cPlayerActor.GetComponent<CNetworkView>().ViewId;

		if(EventHover != null)
            EventHover(_cRaycastHit, cNetworkViewId, _bHover);
    }


    [ALocalOnly]
    public void OnInteractionInput(CPlayerInteractor.EInputInteractionType _eInputInteractionEvent, GameObject _cPlayerActor, RaycastHit _cRaycastHit, bool _bDown)
	{
        TNetworkViewId cNetworkViewId = _cPlayerActor.GetComponent<CNetworkView>().ViewId;

		switch(_eInputInteractionEvent)
		{
		case CPlayerInteractor.EInputInteractionType.Primary:
			if(EventPrimary != null)
                EventPrimary(_cRaycastHit, cNetworkViewId, _bDown);
			break;
			
		case CPlayerInteractor.EInputInteractionType.Secondary:
			if(EventSecondary != null)
                EventSecondary(_cRaycastHit, cNetworkViewId, _bDown);
			break;
			
		case CPlayerInteractor.EInputInteractionType.Use:
			if (EventUse != null)
                EventUse(_cRaycastHit, cNetworkViewId, _bDown);
			break;
			
		default:
			break;
		}
	}

}

