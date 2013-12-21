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


public class CShipGalaxySimulatior : CNetworkMonoBehaviour 
{
	// Member Types
	
	// Member Fields
	private GameObject m_GalaxyShip = null;
	
    protected CNetworkVar<float> m_GalaxyShipPositionX    = null;
    protected CNetworkVar<float> m_GalaxyShipPositionY    = null;
    protected CNetworkVar<float> m_GalaxyShipPositionZ    = null;
	
    protected CNetworkVar<float> m_GalaxyShipEulerAngleX    = null;
    protected CNetworkVar<float> m_GalaxyShipEulerAngleY    = null;
    protected CNetworkVar<float> m_GalaxyShipEulerAngleZ    = null;
	
	// Member Properties
	public GameObject GalaxyShip
	{
		get { return(m_GalaxyShip); }
	}

	public Vector3 Position
    {
        set 
		{ 
			m_GalaxyShipPositionX.Set(value.x); m_GalaxyShipPositionY.Set(value.y); m_GalaxyShipPositionZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_GalaxyShipPositionX.Get(), m_GalaxyShipPositionY.Get(), m_GalaxyShipPositionZ.Get())); 
		}
    }
	
	public Vector3 EulerAngles
    {
        set 
		{ 
			m_GalaxyShipEulerAngleX.Set(value.x); m_GalaxyShipEulerAngleY.Set(value.y); m_GalaxyShipEulerAngleZ.Set(value.z);
		}
        get 
		{ 
			return (new Vector3(m_GalaxyShipEulerAngleX.Get(), m_GalaxyShipEulerAngleY.Get(), m_GalaxyShipEulerAngleZ.Get())); 
		}
    }
	
	// Member Methods
    public override void InstanceNetworkVars()
    {
		m_GalaxyShipPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_GalaxyShipPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_GalaxyShipPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_GalaxyShipEulerAngleX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_GalaxyShipEulerAngleY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_GalaxyShipEulerAngleZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		if(!CNetwork.IsServer)
		{
			// Position
	        if (_rSender == m_GalaxyShipPositionX || _rSender == m_GalaxyShipPositionY || _rSender == m_GalaxyShipPositionZ)
			{
				m_GalaxyShip.rigidbody.position = Position;
			}
			
			// Rotation
	        else if (_rSender == m_GalaxyShipEulerAngleX || _rSender == m_GalaxyShipEulerAngleY || _rSender == m_GalaxyShipEulerAngleZ)
	        {	
	            m_GalaxyShip.transform.eulerAngles = EulerAngles;
	        }
		}
	}
	
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
	
	public void LateUpdate()
	{
		if(CNetwork.IsServer)
		{
			SyncGalaxyShipTransform();
		}
	}
	
	private void SyncGalaxyShipTransform()
	{
		Position = m_GalaxyShip.rigidbody.position;
		EulerAngles = m_GalaxyShip.transform.eulerAngles;
	}
}
