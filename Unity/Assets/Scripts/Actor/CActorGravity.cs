
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
	private CNetworkVar<bool> m_UnderGravityInfluence = null;

	private Vector3 m_GravityAcceleration = Vector3.zero;
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
	}

	public void OnNetworkVarSync(INetworkVar _SyncedVar)
	{
		if(m_UnderGravityInfluence == _SyncedVar)
		{
			if(m_UnderGravityInfluence.Get())
			{
				if(EventEnteredGravityZone != null)
					EventEnteredGravityZone();
			}
			else
			{
				if(EventExitedGravityZone != null)
					EventExitedGravityZone();
			}
		}
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			m_GravityAcceleration = Vector3.zero;

			if(!IsUnderGravityInfluence)
				return;

			foreach(GameObject facility in m_FacilitiesInfluencingGravity)
			{
				CFacilityGravity fg = facility.GetComponent<CFacilityGravity>();
				if(fg.IsGravityEnabled && fg.FacilityGravityAcceleration.sqrMagnitude > m_GravityAcceleration.sqrMagnitude)
				{
					m_GravityAcceleration = fg.FacilityGravityAcceleration;
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
				rigidbody.AddForce(m_GravityAcceleration, ForceMode.Acceleration);
			}
		}
	}

	[AServerOnly]
	public void ActorEnteredGravityTrigger(GameObject _Facility)
	{
		if(m_FacilitiesInfluencingGravity.Count == 0)
			m_UnderGravityInfluence.Set(true);

		if(!m_FacilitiesInfluencingGravity.Contains(_Facility))
			m_FacilitiesInfluencingGravity.Add(_Facility);
	}

	[AServerOnly]
	public void ActorExitedGravityTrigger(GameObject _Facility)
	{
		if(m_FacilitiesInfluencingGravity.Contains(_Facility))
			m_FacilitiesInfluencingGravity.Remove(_Facility);

		if(m_FacilitiesInfluencingGravity.Count == 0)
			m_UnderGravityInfluence.Set(false);
	}
}
