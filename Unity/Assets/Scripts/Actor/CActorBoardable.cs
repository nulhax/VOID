
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
		get { return (m_BoardingState.Get()); }
	}

// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_BoardingState= _cRegistrar.CreateNetworkVar<EBoardingState>(OnNetworkVarSync, EBoardingState.INVALID);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		// Boarding state
		if(_rSender == m_BoardingState)
		{
			if(BoardingState == EBoardingState.Onboard && m_CanBoard)
			{
				if(EventBoard != null)
					EventBoard();
			}
			else if(BoardingState == EBoardingState.Offboard && m_CanDisembark)
			{
				if(EventDisembark != null)
					EventDisembark();
			}
		}
	}

	public void Awake()
	{
		// Save the original layer
		m_OriginalLayer = gameObject.layer;

		// Register the boarding/disembarking handlers
		EventBoard += SetOriginalLayer;
		EventDisembark += SetGalaxyLayer;
	}

	public void Start()
	{	
		// Set the boarding state if it is still invalid
		if(CNetwork.IsServer && BoardingState == EBoardingState.INVALID)
		{
			m_BoardingState.Set(m_InitialBoardingState);
		}
	}

	private void SetGalaxyLayer()
	{
		// Resursively set the galaxy layer on the actor
		CUtility.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Galaxy"));

		// Add the galaxy shiftable component
		if(gameObject.GetComponent<GalaxyShiftable>() == null)
			gameObject.AddComponent<GalaxyShiftable>();

		// Set as parent of nothing
		//transform.parent = null;
	}
	
	private void SetOriginalLayer()
	{
		// Resursively set the original layer on the actor
		CUtility.SetLayerRecursively(gameObject, m_OriginalLayer);

		// Remove the galaxy shiftable component
		if(gameObject.GetComponent<GalaxyShiftable>() != null)
			Destroy(gameObject.GetComponent<GalaxyShiftable>());

		// Set as parent of the ship
		//transform.parent = CGameShips.Ship.transform;
	}

	[AServerOnly]
	public void DisembarkActor()
	{
		// Check if this actor isnt a child of another boardable actor
		if(CUtility.FindInParents<CActorBoardable>(gameObject) == null)
		{
			// Set the boarding state
			m_BoardingState.Set(EBoardingState.Offboard);

			// Transfer the actor to galaxy ship space
			CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(transform.position, transform.rotation, transform);

			// Get the relative velocity of the actor boarding and apply the compensation force to the actor
			Vector3 transferedVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position);

			// Add the current velocity of the actor transformed to simulation space
			Vector3 currentVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(Quaternion.LookRotation(rigidbody.velocity.normalized)) * Vector3.forward * rigidbody.velocity.magnitude;
			transferedVelocity += currentVelocity;

			// Set the compensation velocity of the actor
			rigidbody.velocity = transferedVelocity;
		}
	}

	[AServerOnly]
	public void BoardActor()
	{
		// Check if this actor isnt a child of another boardable actor
		if(CUtility.FindInParents<CActorBoardable>(gameObject) == null)
		{
			// Set the boarding state
			m_BoardingState.Set(EBoardingState.Onboard);

			// Get the inverse of the relative velocity of the actor boarding
			Vector3 transferedVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position) * -1.0f;

			// Add the current velocity of the actor transformed to simulation space
			Vector3 currentVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(Quaternion.LookRotation(rigidbody.velocity.normalized)) * Vector3.forward * rigidbody.velocity.magnitude;
			transferedVelocity += currentVelocity;

			// Transfer the actor to ship space
			CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(transform.position, transform.rotation, transform);
			
			// Set the compensation velocity of the actor
			rigidbody.velocity = transferedVelocity;
		}
	}
}