//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGravityTriggerResponse.cs
//  Description :   Class script for the gravity trigger
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;

// Implementation
public class CGravityTriggerResponse : CNetworkMonoBehaviour
{
	// Member Data
	SphereCollider     m_Trigger;
	CNetworkVar<float> m_fTriggerRadius;	
	CNetworkVar<float> m_fCurrentGravityOutput;
	
	// Member Properties
	float TriggerRadius        { get { return (m_fTriggerRadius.Get()); } set { TriggerRadius = value; } }
	float CurrentGravityOutput { get { return (m_fCurrentGravityOutput.Get()); } set { CurrentGravityOutput = value; } }
	
	// Member Functions
	public override void InstanceNetworkVars()
	{
		m_fTriggerRadius        = new CNetworkVar<float>(OnNetworkVarSync);
		m_fCurrentGravityOutput = new CNetworkVar<float>(OnNetworkVarSync);
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_fTriggerRadius)
		{
			// Update radius using trigger radius
			m_Trigger.radius = TriggerRadius;
		}
		
		else if (_cVarInstance == m_fCurrentGravityOutput)
		{
			// Do nothing
		}
	}
	
	void Start()
	{
		CGame.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyCreate += new CShipFacilities.OnFacilityCreate(OnfacilityCreate);
		// Get sphere collider gravity trigger and save locally
		m_Trigger = transform.FindChild("GravityTrigger").GetComponent<SphereCollider>();
	}
			
			
	void OnfacilityCreate(GameObject _NewFaciltiy)
	{
		Debug.Log ("OnFacilityCreate is successful!");
		// distance checking
	}
			
	[AServerMethod]
	void UpdateTriggerRadius(float _fNewTriggerRadius)
	{
		m_fTriggerRadius.Set(_fNewTriggerRadius);
	}
	
	[AServerMethod]
	void UpdateCurrentGravityOutput(float _fNewCurrentGravityOutput)
	{
		m_fCurrentGravityOutput.Set(_fNewCurrentGravityOutput);
	}
	
	void OnTriggerEnter(Collider _CollisionObject)
	{
		// Set triggered facility's gravity
	}
	
	void OnTriggerStay(Collider _CollidionObject)
	{
		// Update triggered facility's gravity
	}
	
	void OnTriggerExit(Collider _CollisionObject)
	{
		// Disable triggered facility's gravity
		// NOTE: Ensure that only gravity provided by this
		// specific generator is disabled, not all gravity for that room.
	}
	
	void Update(){}
}