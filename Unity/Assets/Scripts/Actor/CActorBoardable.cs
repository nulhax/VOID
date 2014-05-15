
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
public class CActorBoardable : CNetworkMonoBehaviour 
{
	
	// Member Types
	public enum EBoardingState
	{
		INVALID,

		Onboard,
		Offboard,

        MAX
	}
	

	// Member Delegates and Events


	// Member Fields
	public EBoardingState m_InitialBoardingState = EBoardingState.Onboard;
	public bool m_CanBoard = true;
	public bool m_CanDisembark = true;


	CNetworkVar<EBoardingState> m_BoardingState = null;
	int m_iOriginalLayer = 0;

	
	// Member Properties	
	public EBoardingState BoardingState
	{
		get { return (m_BoardingState.Get()); }
	}


	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_BoardingState = _cRegistrar.CreateReliableNetworkVar<EBoardingState>(OnNetworkVarSync, EBoardingState.INVALID);
	}


    [AServerOnly]
    public void BoardActor()
    {
        // Check if this actor isnt a child of another boardable actor
        if (CUtility.FindInParents<CActorBoardable>(gameObject) == null && m_CanBoard)
        {
			if(rigidbody.isKinematic || m_BoardingState.Value == EBoardingState.Onboard)
				return;

			// Set the boarding state
			m_BoardingState.Set(EBoardingState.Onboard);

            // Get the inverse of the relative velocity of the actor boarding
            Vector3 transferedVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position) * -1.0f;

            // Add the current velocity of the actor transformed to simulation space
            Vector3 currentVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(Quaternion.identity) * rigidbody.velocity;
            transferedVelocity += currentVelocity;

            // Transfer the actor to ship space
			Vector3 newPosition = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationPos(transform.position);
			Quaternion newRotation = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(transform.rotation);

			CNetworkView nv = GetComponent<CNetworkView>();
			nv.SetPosition(newPosition);
			nv.SetRotation(newRotation);

			if(rigidbody.isKinematic == false)
			{
	            // Set the compensation velocity of the actor
	            rigidbody.velocity = transferedVelocity;
			}
        }
    }


    [AServerOnly]
    public void DisembarkActor()
    {
        // Check if this actor isnt a child of another boardable actor
		if (CUtility.FindInParents<CActorBoardable>(gameObject) == null && m_CanDisembark)
        {
			if(rigidbody.isKinematic || m_BoardingState.Value == EBoardingState.Offboard)
				return;

            // Set the boarding state
            m_BoardingState.Set(EBoardingState.Offboard);

            // Transfer the actor to galaxy ship space
			Vector3 newPosition = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(transform.position);
			Quaternion newRotation = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(transform.rotation);

			CNetworkView nv = GetComponent<CNetworkView>();
			nv.SetPosition(newPosition);
			nv.SetRotation(newRotation);

            // Get the relative velocity of the actor disembarking and apply the compensation force to the actor
            Vector3 transferedVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position);

            // Add the current velocity of the actor transformed to galaxy space
            Vector3 currentVelocity = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(Quaternion.identity) * rigidbody.velocity;
            transferedVelocity += currentVelocity;

			if(rigidbody.isKinematic == false)
			{
            	// Set the compensation velocity of the actor
            	rigidbody.velocity = transferedVelocity;
			}
        }
    }


	void Awake()
	{
		// Save the original layer
		m_iOriginalLayer = gameObject.layer;
	}


	void Start()
	{	
		// Set the boarding state if it is still invalid
		if (CNetwork.IsServer && 
            BoardingState == EBoardingState.INVALID)
		{
			m_BoardingState.Set(m_InitialBoardingState);
		}
	}


	void SetGalaxyLayer()
	{
		// Resursively set the galaxy layer on the actor
		CUtility.SetLayerRecursively(gameObject, CGalaxy.layerEnum_Gubbin);

		// Add the galaxy shiftable component
		gameObject.AddMissingComponent<GalaxyShiftable>();
	}
	

	void SetOriginalLayer()
	{
		// Resursively set the original layer on the actor
		CUtility.SetLayerRecursively(gameObject, m_iOriginalLayer);

		// Remove the galaxy shiftable component
        if (gameObject.GetComponent<GalaxyShiftable>() != null)
        {
            Destroy(gameObject.GetComponent<GalaxyShiftable>());
        }
	}


    void OnNetworkVarSync(INetworkVar _SyncedVar)
    {
		// Boarding state
		if (_SyncedVar == m_BoardingState)
		{
			if (BoardingState == EBoardingState.Onboard)
			{
				SetOriginalLayer();
			}
			else if (BoardingState == EBoardingState.Offboard)
			{
				SetGalaxyLayer();
			}
		}
    }
}