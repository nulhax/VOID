//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipPropulsionSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

// Namespaces
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/* Implementation */

public class CShipPropulsionSystem : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private List<GameObject> m_PropulsionGenerators = new List<GameObject>();
	
	private float m_fTotalPropulsion = 0.0f; 


	// Member Properties
		

	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}


	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (CNetwork.IsServer) 
		{
			UpdateShipPropulsionPool ();
		}
	}

	public void RegisterPropulsionGeneratorSystem(GameObject _PropulsionGeneratorSystem)
	{
		if(!m_PropulsionGenerators.Contains(_PropulsionGeneratorSystem))
		{
			m_PropulsionGenerators.Add(_PropulsionGeneratorSystem);
		}
	}
	
	public void UnregisterPropulsionGeneratorSystem(GameObject _PropulsionGeneratorSystem)
	{
		if(m_PropulsionGenerators.Contains(_PropulsionGeneratorSystem))
		{
			m_PropulsionGenerators.Remove(_PropulsionGeneratorSystem);
		}
	}

	[AServerOnly]
	public void UpdateShipPropulsionPool()
	{
		// Get the combined power battery charge from each power generator
		float fTotalPropulsion = m_PropulsionGenerators.Sum((pg) => {	CPropulsionGeneratorSystem pgs = pg.GetComponent<CPropulsionGeneratorSystem>();
																		return(pgs.IsPropulsionGeneratorActive ? pgs.PropulsionForce : 0.0f); });
		
		// Set the battery charge pool
		m_fTotalPropulsion = fTotalPropulsion;
	}
}
