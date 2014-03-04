
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorLocator.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Rigidbody))]
public class CActorLocator : MonoBehaviour 
{
	// Member Types
	


	// Member Delegates and Events
	public delegate void NotifyLocationChange(GameObject _Facility);
	
	public event NotifyLocationChange EventEnteredFacility;
	public event NotifyLocationChange EventExitedFacility;


	// Member Fields
	private List<GameObject> m_ContainingFacilities = new List<GameObject>();
	private GameObject m_CurrentFacility = null;


	// Member Properties	
	public GameObject CurrentFacility
	{
		get { return(m_CurrentFacility); }
	}

	public List<GameObject> ContainingFacilities
	{
		get { return(m_ContainingFacilities); }
	}

	// Member Methods
	[AServerOnly]
	public void ActorEnteredFacility(GameObject _Facility)
	{
		m_ContainingFacilities.Add(_Facility);
		
		m_CurrentFacility = _Facility;

		if(EventEnteredFacility != null)
			EventEnteredFacility(_Facility);
	}
	
	[AServerOnly]
	public void ActorExitedFacility(GameObject _Facility)
	{
		m_ContainingFacilities.Remove(_Facility);
		
		if(m_ContainingFacilities.Count == 0)
			m_CurrentFacility = null;

		if(EventExitedFacility != null)
			EventExitedFacility(_Facility);
	}
}
