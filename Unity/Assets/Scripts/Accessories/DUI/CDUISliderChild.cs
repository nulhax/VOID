//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUISliderChild.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;


/* Implementation */

[RequireComponent(typeof(CDUIElement))]
public class CDUISliderChild : CNetworkMonoBehaviour 
{
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	private GameObject m_SliderParent = null;


	// Member Properties
	public GameObject SliderParent
	{
		get { return(m_SliderParent); }
		set { m_SliderParent = value; }
	}
	
	// Member Methods
	public override void InstanceNetworkVars()
	{

	}
	
	private void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{

	}
	
	public void Awake()
	{
		// Enable syncing of OnHover and OnPress
		CDUIElement duiElement = GetComponent<CDUIElement>();
		duiElement.m_SyncOnHover = true;
		duiElement.m_SyncOnPress = true;
	}
	
	public void OnPress(bool _IsPressed)
	{
		if(!CDUIElement.s_IsSyncingNetworkCallbacks)
		{
			SliderParent.GetComponent<CDUISlider>().SlidingSelf = _IsPressed;
		}
	}
}
