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


public class CShipGalaxySimulatior : MonoBehaviour 
{
	// Member Types
	
	// Member Fields
	private GameObject m_GalaxyShip = null;
	
	// Member Properties
	public GameObject GalaxyShip
	{
		get { return(m_GalaxyShip); }
	}

	// Member Methods
	public void Awake()
	{
		if(CNetwork.IsServer)
		{
			m_GalaxyShip = CNetwork.Factory.CreateObject(CGame.ENetworkRegisteredPrefab.GalaxyShip);
		}
		else
		{
			m_GalaxyShip = GameObject.FindGameObjectWithTag("GalaxyShip");
		}
	}

	public Vector3 GetSimulationToGalaxyPos(Vector3 _SimulationPos)
	{
		return(m_GalaxyShip.transform.rotation * (_SimulationPos - transform.position) + m_GalaxyShip.transform.position);
	}

	public Quaternion GetSimulationToGalaxyRot(Quaternion _SimulationRot)
	{
		return(m_GalaxyShip.transform.rotation * _SimulationRot);
	}

	public Vector3 GetGalaxyToSimulationPos(Vector3 _GalaxyPos)
	{
		return(Quaternion.Inverse(m_GalaxyShip.transform.rotation) * (_GalaxyPos - m_GalaxyShip.transform.position) + transform.position);
	}
	
	public Quaternion GetGalaxyToSimulationRot(Quaternion _GalaxyRot)
	{
		return(Quaternion.Inverse(m_GalaxyShip.transform.rotation) *_GalaxyRot);
	}

	public void TransferFromSimulationToGalaxy(Vector3 _SimulationPos, Quaternion _SimulationRot, Transform _ToTransfer)
	{
		// Update the transform based off the transform relative to the ship
		_ToTransfer.position = GetSimulationToGalaxyPos(_SimulationPos);
		_ToTransfer.rotation = GetSimulationToGalaxyRot(_SimulationRot);
	}

	public void TransferFromGalaxyToSimulation(Vector3 _GalaxyPos, Quaternion _GalaxyRot, Transform _ToTransfer)
	{
		// Update the transform based off the transform relative to the galaxy ship
		_ToTransfer.position = GetGalaxyToSimulationPos(_GalaxyPos);
		_ToTransfer.rotation = GetGalaxyToSimulationRot(_GalaxyRot);
	}
	
	public Vector3 GetGalaxyVelocityRelativeToShip(Vector3 _GalaxyPos)
	{
		Vector3 velocity = m_GalaxyShip.GetComponent<CGalaxyShipMotor>().GetRelativePointVelocity(_GalaxyPos);
		return(velocity);
	}
}
