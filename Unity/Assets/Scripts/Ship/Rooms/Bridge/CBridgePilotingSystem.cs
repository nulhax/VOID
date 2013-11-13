//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CBridgePilotingSystem.cs
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


public class CBridgePilotingSystem : MonoBehaviour 
{
    // Member Types


    // Member Delegates & Events

	
	// Member Fields
	public GameObject m_Cockpit = null;
	
    // Member Properties


    // Member Methods
	public void Start()
	{
		// Initialise the cockpit
		InitialiseCockpit();
	}
	
	private void InitialiseCockpit()
	{
		m_Cockpit.AddComponent<CBridgeCockpit>();
	}
}

