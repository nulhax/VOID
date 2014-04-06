//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIConstructionPlanner.cs
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


public class CDUIConstructionPlanner : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	CGridUI m_GridUI = null;

	
	// Member Properties
	
	
	// Member Methods
	public void RegisterGridUI(CGridUI _GridUI)
	{
		m_GridUI = _GridUI;
	}

	public void OnTilePainterExteriorToggled()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Paint_Exterior;
	}

	public void OnTilePainterInteriorWallsToggled()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Paint_Interior_Walls;
	}

	public void OnTilePainterInteriorFloorsToggled()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Paint_Interior_Floors;
	}
}
