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
	private GameObject m_PlayerGalaxyCamera = null;
	
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
	
	public GameObject PlayerGalaxyCamera
	{
		get { return(m_PlayerGalaxyCamera); }
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
		// Create a world ship to explore the galaxy
		m_GalaxyShip = GameObject.Instantiate(Resources.Load("Prefabs/Ship/GalaxyShip", typeof(GameObject))) as GameObject;
	}
	
	public void AddPlayerActorGalaxyCamera(GameObject _PlayerActorCamera)
	{	
		// Create the galaxy camera and attach it to the galaxy ship
		m_PlayerGalaxyCamera = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Player/PlayerGalaxyCamera"));
		m_PlayerGalaxyCamera.transform.parent = m_GalaxyShip.transform;
	}
	
	public void LateUpdate()
	{
		UpdateGalaxyCameraTransforms();
		
		if(CNetwork.IsServer)
		{
			SyncWorldShipTransform();
		}
	}
	
	private void SyncWorldShipTransform()
	{
		Position = m_GalaxyShip.rigidbody.position;
		EulerAngles = m_GalaxyShip.transform.eulerAngles;
	}
	
	private void UpdateGalaxyCameraTransforms()
	{	
		// Get the players ship camera
		GameObject playerShipCamera = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>().ActorHead.GetComponentInChildren<CPlayerShipCamera>().gameObject;
			
		// Get the simulation actors position relative to the ship
		Vector3 relativePos = playerShipCamera.transform.position - transform.position;
		Quaternion relativeRot = playerShipCamera.transform.rotation * Quaternion.Inverse(transform.rotation);
			
		// Update the transform
		if(m_PlayerGalaxyCamera.transform.localPosition != relativePos)
			m_PlayerGalaxyCamera.transform.localPosition = relativePos;
		
		if(m_PlayerGalaxyCamera.transform.localRotation != relativeRot)
			m_PlayerGalaxyCamera.transform.localRotation = relativeRot;
	}
}
