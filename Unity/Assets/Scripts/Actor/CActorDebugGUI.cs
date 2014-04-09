//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDispenserBehaviour.cs
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


[RequireComponent(typeof(CActorInteractable))]
public class CActorDebugGUI : MonoBehaviour
{
	// Member Types
	

	// Member Delegates & Events


	// Member Fields
	bool bShowName = false;

	
	// Member Properties
	
	
	// Member Methods
	void Start()
	{
		GetComponent<CActorInteractable>().EventHover += OnHover;
	}

	void OnHover(RaycastHit _RayHit, TNetworkViewId _cPlayerActorViewId, bool _bHover)
	{
        bShowName = _bHover;
	}
	
	public void OnGUI()
	{
		if (bShowName)
		{
			float fScreenCenterX = Screen.width / 2;
			float fScreenCenterY = Screen.height / 2;
			float fWidth = 150.0f;
			float fHeight = 20.0f;
			float fOriginX = fScreenCenterX + 25.0f;
			float fOriginY = fScreenCenterY - 10.0f;
		
			GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), gameObject.name);
		}
	}
	
};