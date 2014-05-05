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
	}


// Member Fields


    CNaniteStorage m_cNaniteStorage = null;


}
