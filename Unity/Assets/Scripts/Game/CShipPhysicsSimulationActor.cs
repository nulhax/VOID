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
	public enum EWorldActorExecution
	{
		OnStart,
		OnAwake,
	}
	
	// Member Fields
	public EWorldActorExecution m_ExecutionTime = EWorldActorExecution.OnStart;
	private bool m_bCreated = false;
	// Member Properties
	
	// Member Methods
	public void Awake()
	{
		if(m_ExecutionTime == EWorldActorExecution.OnAwake)
			CreateWorldActor();
	}
	
	public void Start()
	{
		if(m_ExecutionTime == EWorldActorExecution.OnStart)
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
	
	private void OnDestroy()
	{
		if(m_bCreated && CNetwork.Instance != null)
		{
			// Get the ship physics simulator
			CShipPhysicsSimulatior simulator = CGame.Ship.GetComponent<CShipPhysicsSimulatior>();
			
			// Destroy this actor
			simulator.DestroyWorldActor(gameObject);
		}
	}
}
