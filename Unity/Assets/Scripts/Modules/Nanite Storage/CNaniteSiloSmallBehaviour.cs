//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNaniteSiloSmallBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/* Implementation */


[RequireComponent(typeof(CNaniteStorage))]
public class CNaniteSiloSmallBehaviour: MonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
	
	
// Member Properties
	
	
// Member Methods


	void Start()
	{
		m_cNaniteStorage = gameObject.GetComponent<CNaniteStorage>();
		
		// Get the DUI of the power generator
		m_cDuiNaniteCapsuleRoot = m_cDuiConsole.DUIRoot.GetComponent<CDUINaniteCapsuleRoot>();
		m_cDuiNaniteCapsuleRoot.RegisterNaniteCapsule(gameObject);
	}


// Member Fields


    public CDUIConsole m_cDuiConsole = null;


    CNaniteStorage m_cNaniteStorage = null;
    CDUINaniteCapsuleRoot m_cDuiNaniteCapsuleRoot = null;


}
