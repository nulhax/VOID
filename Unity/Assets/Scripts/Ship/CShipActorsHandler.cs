//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipInhabitants.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CShipActorsHandler : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events


	// Member Fields
	private GameObject m_ShipActorsContainer = null;
	private List<GameObject> m_ActorsCurrentlyOnboard = new List<GameObject>();
	
	// Member Properties
	
	
	// Member Methods
	public void RegisterActorEnterExit(Action<GameObject> _EnterHandlerDelegate, Action<GameObject> _ExitHandlerDelegate)
	{
		_EnterHandlerDelegate += ActorEnteredFacility;
		_ExitHandlerDelegate += ActorExitedFacility;
	}
	
	private void ActorEnteredFacility(GameObject _Actor)
	{
		if(!m_ActorsCurrentlyOnboard.Contains(_Actor))
		{
			m_ActorsCurrentlyOnboard.Add(_Actor);
		}
	}
	
	private void ActorExitedFacility(GameObject _Actor)
	{
		if(m_ActorsCurrentlyOnboard.Contains(_Actor))
		{
			m_ActorsCurrentlyOnboard.Remove(_Actor);
		}
	}
}
