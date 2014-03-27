//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTileBehaviour.cs
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


public class CTileBehaviour : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CTile tile;
	
	public Vector2 screenPos;
	public bool onScreen;
	public bool selected = false;

	
	// Member Properties
	
	
	// Member Methods
	void Update()
	{
		//Track Screen position
		screenPos = Camera.main.WorldToScreenPoint(this.transform.position);

		//if within screen space
		if (CGrid.I.NodeWithinScreenSpace(screenPos))
		{
			if(!onScreen)
			{
				CGrid.I.m_TilesOnScreen.Add(this);
				onScreen = true;
			}
		}
		else if(onScreen)
		{
			CGrid.I.RemoveFromOnScreenUnts(this);
		}
	}	
}
