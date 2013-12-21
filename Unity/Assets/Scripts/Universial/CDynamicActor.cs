
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CDynamicActor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CNetworkView))]
public class CDynamicActor : CNetworkMonoBehaviour 
{
	
// Member Types
	public enum EBoardingState
	{
		INVALID,
		
		Onboard,
		Offboard,
	}
	
// Member Delegates and Events
	public delegate void BoardingHandler();
	
	public event BoardingHandler EventBoard;
	public event BoardingHandler EventDisembark;
	
	
// Member Fields
	public EBoardingState m_InitialBoardingState = EBoardingState.Onboard;
	public bool m_CanBoard = true;
	public bool m_CanDisembark = true;

	private int m_OriginalLayer = 0;
	private bool m_bRotationYDisabled = false;
	
	private Vector3 m_GravityAcceleration = Vector3.zero;
	
    private CNetworkVar<float> m_cPositionX    = null;
    private CNetworkVar<float> m_cPositionY    = null;
    private CNetworkVar<float> m_cPositionZ    = null;
	
    private CNetworkVar<float> m_EulerAngleX    = null;
    private CNetworkVar<float> m_EulerAngleY    = null;
    private CNetworkVar<float> m_EulerAngleZ    = null;
	
	private CNetworkVar<EBoardingState> m_BoardingState = null;
	
// Member Properties	
	public Vector3 GravityAcceleration
	{
		set { m_GravityAcceleration = value; }
		get { return(m_GravityAcceleration); }
	}
	
	public EBoardingState BoardingState
	{
		set { m_BoardingState.Set(value); }
		get { return (m_BoardingState.Get()); }
	}
	
	public Vector3 Position
    {
        set 
		{ 
			m_cPositionX.Set(value.x); m_cPositionY.Set(value.y); m_cPositionZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cPositionX.Get(), m_cPositionY.Get(), m_cPositionZ.Get())); 
		}
    }
	
	public Vector3 EulerAngles
    {
        set 
		{ 
			m_EulerAngleX.Set(value.x); m_EulerAngleY.Set(value.y); m_EulerAngleZ.Set(value.z);
		}
        get 
		{ 
			return (new Vector3(m_EulerAngleX.Get(), (RotationYDisabled) ? transform.eulerAngles.y : m_EulerAngleY.Get(), m_EulerAngleZ.Get())); 
		}
    }

	public bool RotationYDisabled
	{
		set { m_bRotationYDisabled = value; }
		get { return (m_bRotationYDisabled); }
	}

// Member Methods
	public void Awake()
	{
		// Save the original layer
		m_OriginalLayer = gameObject.layer;
	}
	
	public void Start()
	{	
		// Set to kinematic on the client
		if(!CNetwork.IsServer)
		{
			rigidbody.isKinematic = true;
		}
		// Set the boarding state if it is still invalid
		else if(BoardingState == EBoardingState.INVALID)
		{
			BoardingState = m_InitialBoardingState;
		}
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			SyncTransform();
		}
	}
	
	public void FixedUpdate()
	{
		if(rigidbody != null && CNetwork.IsServer)
		{	
			// Apply the gravity to the rigid body
			rigidbody.AddForce(m_GravityAcceleration, ForceMode.Acceleration);
		}
	}
	
    public override void InstanceNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_EulerAngleX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_EulerAngleY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_EulerAngleZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
		m_BoardingState = new CNetworkVar<EBoardingState>(OnNetworkVarSync, m_InitialBoardingState);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		if(!CNetwork.IsServer && CGame.PlayerActor != gameObject)
		{
			// Position
	        if (_rSender == m_cPositionX || _rSender == m_cPositionY || _rSender == m_cPositionZ)
			{
				transform.position = Position;
			}
			
			// Rotation
	        else if (_rSender == m_EulerAngleX || _rSender == m_EulerAngleY || _rSender == m_EulerAngleZ)
	        {	
	            transform.eulerAngles = EulerAngles;
	        }
		}
		
		// Boarding state
 		if(_rSender == m_BoardingState)
		{
			if(BoardingState == EBoardingState.Onboard && m_CanBoard)
			{
				if(CNetwork.IsServer)
				{
					TransferActorToShipSpace();
				}

				if(EventBoard != null)
					EventBoard();
				
				SetOriginalLayer();
			}
			else if(BoardingState == EBoardingState.Offboard && m_CanDisembark)
			{
				if(CNetwork.IsServer)
				{
					TransferActorToGalaxySpace();
				}

				if(EventDisembark != null)
					EventDisembark();
				
				SetGalaxyLayer();
			}
		}
	}
	
	private void SyncTransform()
	{
		Position = rigidbody.position;
		EulerAngles = transform.eulerAngles;
	}
	
	private void SetGalaxyLayer()
	{
		// Resursively set the galaxy layer on the actor
		CUtility.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Galaxy"));

		// Add the galaxy shiftable component
		gameObject.AddComponent<GalaxyShiftable>();
	}
	
	private void SetOriginalLayer()
	{
		// Resursively set the original layer on the actor
		CUtility.SetLayerRecursively(gameObject, m_OriginalLayer);

		// Remove the galaxy shiftable component
		Destroy(gameObject.GetComponent<GalaxyShiftable>());
	}
	
	[AServerMethod]
	private void TransferActorToGalaxySpace()
	{	 	
		bool childOfPlayer = false;
		
		if(transform.parent != null)
			if(transform.parent.tag == "Player")
				childOfPlayer = true;
		
		if(!childOfPlayer)
		{
			// Transfer the actor to galaxy ship space
			CGame.ShipGalaxySimulator.TransferFromSimulationToGalaxy(transform.position, transform.rotation, transform);

			// Unparent Actor
			transform.parent = null;

			// Get the relative velocity of the actor boarding and apply the compensation force to the actor
			Vector3 transferedVelocity = CGame.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position);
			rigidbody.AddForce(transferedVelocity, ForceMode.VelocityChange);
			
			// Sync over the network and apply the galaxy ship force
			if(GetComponent<CNetworkView>() != null)
			{
				// Sync the parent
				GetComponent<CNetworkView>().SyncParent();
				GetComponent<CNetworkView>().SyncTransformPosition();
				GetComponent<CNetworkView>().SyncTransformRotation();
			}
			else
			{
				Debug.LogError("No network view found on dynamic actor!");
			}
		}
	}
	
	[AServerMethod]
	private void TransferActorToShipSpace()
	{
		bool childOfPlayer = false;
		
		if(transform.parent != null)
			if(transform.parent.tag == "Player")
				childOfPlayer = true;
		
		if(!childOfPlayer)
		{
			// Get the inverse of the relative velocity of the actor boarding
			Vector3 transferedVelocity = CGame.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position) * -1.0f;

			// Transfer the actor to ship space
			CGame.ShipGalaxySimulator.TransferFromGalaxyToSimulation(transform.position, transform.rotation, transform);

			// Parent the actor to the ship
			transform.parent = CGame.Ship.transform;

			// Apply the compensation velocity to the actor
			rigidbody.AddForce(transferedVelocity, ForceMode.VelocityChange);
			
			// Sync over the network
			if(GetComponent<CNetworkView>() != null)
			{
				// Sync the new states
				GetComponent<CNetworkView>().SyncParent();
				GetComponent<CNetworkView>().SyncTransformPosition();
				GetComponent<CNetworkView>().SyncTransformRotation();
			}
			else
			{
				Debug.LogError("No network view found on dynamic actor!");
			}
		}
	}
}
