
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
[RequireComponent(typeof(CActorLocator))]
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


	public delegate void BoardingHandler();
	
	public event BoardingHandler EventBoard;
	public event BoardingHandler EventDisembark;

	
// Member Properties	
	

	public EBoardingState BoardingState
	{
		get { return (m_eBoardingState.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_eBoardingState = _cRegistrar.CreateReliableNetworkVar<EBoardingState>(OnNetworkVarSync, EBoardingState.INVALID);
	}


    [AServerOnly]
    public void BoardActor()
    {
        // Check if this actor isnt a child of another boardable actor
        if (CUtility.FindInParents<CActorBoardable>(gameObject) == null)
        {
            if (CNetwork.IsServer)
            {
                // Set the boarding state
                m_eBoardingState.Set(EBoardingState.Onboard);
            }

            // Get the inverse of the relative velocity of the actor boarding
            Vector3 transferedVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position) * -1.0f;

            // Add the current velocity of the actor transformed to simulation space
            Vector3 currentVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(Quaternion.identity) * rigidbody.velocity;
            transferedVelocity += currentVelocity;

            // Transfer the actor to ship space
            CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(transform.position, transform.rotation, transform);

            // Set the compensation velocity of the actor
            rigidbody.velocity = transferedVelocity;
        }
    }


    [AServerOnly]
    public void DisembarkActor()
    {
        // Check if this actor isnt a child of another boardable actor
        if (CUtility.FindInParents<CActorBoardable>(gameObject) == null)
        {
            // Set the boarding state
            m_eBoardingState.Set(EBoardingState.Offboard);

            // Transfer the actor to galaxy ship space
            CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(transform.position, transform.rotation, transform);

            // Get the relative velocity of the actor disembarking and apply the compensation force to the actor
            Vector3 transferedVelocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position);

            // Add the current velocity of the actor transformed to galaxy space
            Vector3 currentVelocity = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(Quaternion.identity) * rigidbody.velocity;
            transferedVelocity += currentVelocity;

            // Set the compensation velocity of the actor
            rigidbody.velocity = transferedVelocity;
        }
    }


	void Awake()
	{
		// Save the original layer
		m_iOriginalLayer = gameObject.layer;

		// Register the boarding/disembarking handlers
		EventBoard += SetOriginalLayer;
		EventDisembark += SetGalaxyLayer;
	}


	void Start()
	{	
		// Set the boarding state if it is still invalid
		if (CNetwork.IsServer && 
            BoardingState == EBoardingState.INVALID)
		{
			m_eBoardingState.Set(m_InitialBoardingState);
		}
	}


	void SetGalaxyLayer()
	{
		// Resursively set the galaxy layer on the actor
		CUtility.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Galaxy"));

		// Add the galaxy shiftable component
        if (gameObject.GetComponent<GalaxyShiftable>() == null)
        {
            gameObject.AddComponent<GalaxyShiftable>();
        }
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


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Boarding state
        if (_cSyncedVar == m_eBoardingState)
        {
            if (BoardingState == EBoardingState.Onboard && 
                m_bCanBoard)
            {
                // Notify observers about this actor boarding
                if (EventBoard != null) EventBoard();
            }
            else if (BoardingState == EBoardingState.Offboard && 
                     m_bCanDisembark)
            {
                // Notify observers about this actor disembarking
                if (EventDisembark != null) EventDisembark();
            }
        }
    }


// Member Fields


    public EBoardingState m_InitialBoardingState = EBoardingState.Onboard;
    public bool m_bCanBoard = true;
    public bool m_bCanDisembark = true;


    CNetworkVar<EBoardingState> m_eBoardingState = null;


    int m_iOriginalLayer = 0;


}