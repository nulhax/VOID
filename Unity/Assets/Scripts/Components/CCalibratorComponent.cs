﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCalibratorComponent.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */

[RequireComponent(typeof(CComponentInterface))]
public class CCalibratorComponent : CNetworkMonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Properties
	public List<Transform> ComponentRepairPosition
	{
		get { return(m_RepairPositions);}
	}
	
	
	// Member Methods
	// Do the functionality in the on break. This will start when the eventcomponentbreak is triggered
	void OnBreak(CComponentInterface _Sender)
	{
		// TODO: swap between fixed to broken
        gameObject.GetComponent<CAudioCue>().Play(0.4f, true, 0);
	}
	
	// Do the functionality in the onfix. This will start when the eventcomponentfix is triggered
	void OnFix(CComponentInterface _Sender)
	{
		//TODO swap between broken to fixed
        gameObject.GetComponent<CAudioCue>().StopAllSound();
	}
	
	void OnHealthChange(CComponentInterface _Sender, CActorHealth _SenderHealth)
	{
		m_CurrentHealth = _SenderHealth.health;
		m_PreviousHealth = _SenderHealth.health_previous;
		float maxHealth = _SenderHealth.health_initial;

        transform.FindChild("Model").renderer.material.color = Color.Lerp(Color.red, Color.magenta, m_CurrentHealth / maxHealth);
		
	}
	
	void Start()
	{
		// Find all the children which are component transforms
		foreach(Transform child in transform)
		{
			if(child.tag == "ComponentTransform")
				m_RepairPositions.Add(child);
		}

        transform.FindChild("Model").renderer.material.color = Color.Lerp(Color.red, Color.magenta, GetComponent<CActorHealth>().health / GetComponent<CActorHealth>().health_initial);

		// Register events created in the inherited class CComponentInterface
		gameObject.GetComponent<CComponentInterface>().EventComponentBreak += OnBreak;
		gameObject.GetComponent<CComponentInterface>().EventComponentFix += OnFix;
		gameObject.GetComponent<CComponentInterface>().EventHealthChange += OnHealthChange;
	}
	
	void OnDestroy()
	{
		
	}
	
	
	void Update()
	{
		
	}
	
	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		
	}
	
	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	// Member Fields
	
	private List<Transform> m_RepairPositions = new List<Transform>();
	private float m_CurrentHealth = 0.0f;
	private float m_PreviousHealth = 0.0f;
	
	private bool m_IsLerping = false;
};
