
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
[RequireComponent(typeof(CActorNetworkSyncronized))]
public class CActorBoardable : CNetworkMonoBehaviour 
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

	private CNetworkVar<EBoardingState> m_BoardingState = null;
	
// Member Properties		
	public EBoardingState BoardingState
	{
		set { m_BoardingState.Set(value); }
		get { return (m_BoardingState.Get()); }
	}

// Member Methods
	public void Awake()
	{
		// Save the original layer
		m_OriginalLayer = gameObject.layer;
	}
	
	public void Start()
	{	
		// Set the boarding state if it is still invalid
		if(CNetwork.IsServer && BoardingState == EBoardingState.INVALID)
		{
			BoardingState = m_InitialBoardingState;
		}
	}

    public override void InstanceNetworkVars()
    {
		m_BoardingState = new CNetworkVar<EBoardingState>(OnNetworkVarSync, m_InitialBoardingState);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{
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
