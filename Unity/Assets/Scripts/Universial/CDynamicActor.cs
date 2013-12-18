
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
		Boarding,
		Disembarking,
	}
	
// Member Delegates and Events
	public delegate void BoardingHandler();
	
	public event BoardingHandler EventBoard;
	public event BoardingHandler EventDisembark;
	
	
// Member Fields
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
		// Set the dynamic actor as currently boarding
		else
		{
			BoardingState = EBoardingState.Boarding;
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
		
		m_BoardingState = new CNetworkVar<EBoardingState>(OnNetworkVarSync, EBoardingState.INVALID);
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
			Debug.LogError(BoardingState.ToString() + " " + GetComponent<CNetworkView>().ViewId);

			if(BoardingState == EBoardingState.Boarding)
			{
				if(EventBoard != null)
					EventBoard();
			}
			else if(BoardingState == EBoardingState.Disembarking)
			{
				if(EventDisembark != null)
					EventDisembark();
			}
			else if(BoardingState == EBoardingState.Onboard)
			{
				SetOriginalLayer();
			}
			else if(BoardingState == EBoardingState.Offboard)
			{
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
	}
	
	private void SetOriginalLayer()
	{
		// Resursively set the original layer on the actor
		CUtility.SetLayerRecursively(gameObject, m_OriginalLayer);
	}
	
	[AServerMethod]
	public void TransferActorToGalaxySpace()
	{	 	
		GameObject galaxyShip =  CGame.Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip;
		bool childOfPlayer = false;
		
		if(transform.parent != null)
			if(transform.parent.tag == "Player")
				childOfPlayer = true;
		
		if(!childOfPlayer)
		{
			// Get the actors position relative to the ship
			Vector3 relativePos = Quaternion.Inverse(CGame.Ship.transform.rotation) * (transform.position - CGame.Ship.transform.position);
			Quaternion relativeRot = Quaternion.Inverse(CGame.Ship.transform.rotation) * transform.rotation;
			
			// Temporarily parent to galaxy ship
			transform.parent = galaxyShip.transform;
			
			// Update the position and unparent
			transform.localPosition = relativePos;
			transform.localRotation = relativeRot;	
			transform.parent = null;
			
			// Sync over the network and apply the galaxy ship force
			if(GetComponent<CNetworkView>() != null)
			{
				// Add a compensation force to the actor
				Vector3 transferedVelocity = galaxyShip.rigidbody.GetRelativePointVelocity(relativePos);
				rigidbody.AddForce(transferedVelocity, ForceMode.VelocityChange);
				
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
	public void TransferActorToShipSpace()
	{
		GameObject galaxyShip = CGame.Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip;
		bool childOfPlayer = false;
		
		if(transform.parent != null)
			if(transform.parent.tag == "Player")
				childOfPlayer = true;
		
		if(!childOfPlayer)
		{
			// Get the actors position relative to the ship
			Vector3 relativePos = Quaternion.Inverse(galaxyShip.transform.rotation) * (transform.position - galaxyShip.transform.position);
			Quaternion relativeRot = Quaternion.Inverse(galaxyShip.transform.rotation) * transform.rotation;
			
			// Parent the actor to the ship
			transform.parent = CGame.Ship.transform;
			
			// Update the position and unparent
			transform.localPosition = relativePos;
			transform.localRotation = relativeRot;	
			
			// Sync over the network
			if(GetComponent<CNetworkView>() != null)
			{
				// Add a compensation force to the actor
				Vector3 transferedVelocity = -galaxyShip.rigidbody.GetRelativePointVelocity(relativePos);
				rigidbody.AddForce(transferedVelocity, ForceMode.VelocityChange);
				
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
