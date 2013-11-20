//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CShipSimulator.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipPhysicsSimulationActor : MonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	private bool m_bCreated = false;
	
	
	// Member Properties
	
	// Member Methods
	public void Start()
	{
		CreateWorldActor();
	}
	
	private void CreateWorldActor()
	{
		// Get the ship physics simulator
		CShipPhysicsSimulatior simulator = CGame.Ship.GetComponent<CShipPhysicsSimulatior>();
		
		// Add this actor to the simulation
		simulator.AddWorldActor(gameObject);
		
		m_bCreated = true;
	}
}
