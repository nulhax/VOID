
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
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CActorNetworkSyncronized))]
public class CActorGravity : CNetworkMonoBehaviour 
{
	// Member Types


	// Member Delegates and Events
	public delegate void NotifyGravityInfulenceChange();

	public event NotifyGravityInfulenceChange EventEnteredGravityZone;
	public event NotifyGravityInfulenceChange EventExitedGravityZone;

	// Member Fields
	public bool m_bSimulateClientGravity;

	private CNetworkVar<bool> m_UnderGravityInfluence = null;

	private CNetworkVar<Vector3> m_GravityAcceleration = null;

	private List<GameObject> m_FacilitiesInfluencingGravity = new List<GameObject>();

	// Member Properties
	public bool IsUnderGravityInfluence
	{
		get { return(m_UnderGravityInfluence.Get()); }
	}

	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_UnderGravityInfluence = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
		m_GravityAcceleration = _cRegistrar.CreateNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
	}

	public void OnNetworkVarSync(INetworkVar _SyncedVar)
	{
		if(_SyncedVar == m_UnderGravityInfluence)
		{
			if(!m_UnderGravityInfluence.Value)
			{
				// Give a slight force to the object to get it moving
				if(CNetwork.IsServer && rigidbody != null)
				{
					rigidbody.AddForce(Random.onUnitSphere * 0.1f, ForceMode.VelocityChange);
				}
			}
		}
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			m_GravityAcceleration.Set(Vector3.zero);

			CheckGravityInfluence();

			if(!IsUnderGravityInfluence)
				return;

			foreach(GameObject facility in m_FacilitiesInfluencingGravity)
			{
				CFacilityGravity fg = facility.GetComponent<CFacilityGravity>();
				if(fg.IsGravityEnabled && fg.FacilityGravityAcceleration.sqrMagnitude > m_GravityAcceleration.Get().sqrMagnitude)
				{
					m_GravityAcceleration.Set (fg.FacilityGravityAcceleration);
				}
			}
		}

	}

	public void FixedUpdate()
	{
		if(CNetwork.IsServer)
		{
			if(rigidbody != null && IsUnderGravityInfluence)
			{
                Rigidbody[] rigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();

                foreach(Rigidbody body in rigidBodies)
				{
					body.AddForce(m_GravityAcceleration.Get(), ForceMode.Acceleration);
				}
			}
		}
		else if(!CNetwork.IsServer && m_bSimulateClientGravity) // Var which is "clientside gravity"
        {
			if(rigidbody != null && IsUnderGravityInfluence)
			{
				Rigidbody[] rigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();
				
				foreach(Rigidbody body in rigidBodies)
				{
					body.AddForce(m_GravityAcceleration.Get(), ForceMode.Acceleration);
				}
			}
		}
	}

	[AServerOnly]
	private void CheckGravityInfluence()
	{
		if(m_FacilitiesInfluencingGravity.Count == 0)
		{
			if(m_UnderGravityInfluence.Value)
			{
				m_UnderGravityInfluence.Value = false;
				
				if(EventExitedGravityZone != null)
					EventExitedGravityZone();
			}
		}
		else
		{
			bool gravityFound = false;
			foreach(GameObject facility in m_FacilitiesInfluencingGravity)
			{
				if(facility.GetComponent<CFacilityGravity>().IsGravityEnabled)
				{
					gravityFound = true;
					break;
				}
			}
			
			if(!gravityFound)
			{
				if(m_UnderGravityInfluence.Value)
				{
					m_UnderGravityInfluence.Value = false;
					
					if(EventExitedGravityZone != null)
						EventExitedGravityZone();
				}
			}
			else
			{
				if(!m_UnderGravityInfluence.Value)
				{
					m_UnderGravityInfluence.Value = true;
					
					if(EventEnteredGravityZone != null)
						EventEnteredGravityZone();
				}
			}
		}
	}

	[AServerOnly]
	public void ActorEnteredGravityTrigger(GameObject _Facility)
	{
		if(!m_FacilitiesInfluencingGravity.Contains(_Facility))
			m_FacilitiesInfluencingGravity.Add(_Facility);
	}

	[AServerOnly]
	public void ActorExitedGravityTrigger(GameObject _Facility)
	{
		if(m_FacilitiesInfluencingGravity.Contains(_Facility))
			m_FacilitiesInfluencingGravity.Remove(_Facility);
	}
}
