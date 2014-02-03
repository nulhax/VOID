//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLiquidComponent.cs
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
public class CLiquidComponent : CNetworkMonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Properties
	public List<Transform> RatchetRepairPosition
	{
		get { return(m_RepairPositions);}
	}
	
	
	// Member Methods
	// Do the functionality in the on break. This will start when the eventcomponentbreak is triggered
	void OnBreak()
	{
		// TODO: swap between fixed to broken
		
	}
	
	// Do the functionality in the onfix. This will start when the eventcomponentfix is triggered
	void OnFix()
	{
		//TODO swap between broken to fixed
		
	}
	
	void ComponentHealth(GameObject gameObject, float prevHealth, float currHealth)
	{
		m_CurrentHealth = currHealth;
		m_PreviousHealth = prevHealth;
		
		transform.FindChild("Model").renderer.material.color = Color.Lerp(Color.red, Color.green, m_CurrentHealth / 100.0f);
		
	}
	
	void Start()
	{
		// Find all the children which are component transforms
		foreach(Transform child in transform)
		{
			if(child.tag == "ComponentTransform")
				m_RepairPositions.Add(child);
		}
		
		// Register events created in the inherited class CComponentInterface
		// This will call onbreak or onfix when the even is triggered.
		gameObject.GetComponent<CComponentInterface>().EventComponentBreak += OnBreak;
		gameObject.GetComponent<CComponentInterface>().EventComponentFix += OnFix;
		gameObject.GetComponent<CActorHealth>().EventOnSetHealth += ComponentHealth;
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
