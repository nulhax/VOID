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
	
	private float m_ShipPropulsionPotential = 0.0f;
	private float m_ShipCurrentPropulsion = 0.0f;


	// Member Properties
	public float ShipPropulsionPotential
	{
		get { return (m_ShipPropulsionPotential); } 
	}
	
	public float ShipCurentPropulsion
	{
		get { return (m_ShipCurrentPropulsion); } 
	}


	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Start() 
	{
		
	}

	public void Update() 
	{
		// Update propulsion variables
		UpdatePropulsionVariables();
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

	private void UpdatePropulsionVariables()
	{
		m_ShipPropulsionPotential = 0.0f;
		m_ShipCurrentPropulsion = 0.0f;
		foreach(GameObject pg in m_PropulsionGenerators)
		{
			CPropulsionGeneratorBehaviour pgs = pg.GetComponent<CPropulsionGeneratorBehaviour>();
			if(pgs.IsPropulsionGeneratorActive)
			{
				m_ShipCurrentPropulsion += pgs.PropulsionForce;
			}
			m_ShipPropulsionPotential += pgs.PropulsionPotential;
		}
	}
}
