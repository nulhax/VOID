//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CConstructionPlannerBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CConstructionPlannerBehaviour: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CDUIConsole m_ConstructionInterface = null;
	public CGridUI m_GridUI = null;


	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_ConstructionInterface.DUIRoot.GetComponent<CDUIConstructionPlanner>().RegisterGridUI(m_GridUI, m_GridUI.GetComponent<CGrid>());
	}
}
