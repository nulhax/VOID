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


public class CPrefabricatorBehaviour: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CDUIConsole m_ConstructionInterface = null;
	public CPrefabricatorGridUI m_GridUI = null;


	// Member Properties
	public CDUIPrefabricator PrefabricatorUI
	{
		get { return(m_ConstructionInterface.DuiRoot.GetComponent<CDUIPrefabricator>()); }
	}
	
	// Member Methods
	public void Start()
	{
		m_ConstructionInterface.DuiRoot.GetComponent<CDUIPrefabricator>().RegisterGridUI(m_GridUI, m_GridUI.GetComponent<CGrid>());
	}
}
