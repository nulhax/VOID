//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomAtmosphere.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.BooN@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityAtmosphere : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Fields
		
	private CNetworkVar<float> m_AtmosphereQuantity = null;

	private CNetworkVar<float> m_fAtmosphereConsumptionRate = null;
	private CNetworkVar<float> m_fAtmosphereRefillRate = null;
	
	private List<GameObject> m_AtmosphericConsumers = new List<GameObject>();
	
	private float m_AtmosphereVolume = 1000.0f;


// Member Properties
	
    public float AtmosphereQuantity
    {
        get { return (m_AtmosphereQuantity.Get()); }
    }

    public float AtmospherePercentage
    {
		get { return ((AtmosphereQuantity / m_AtmosphereVolume) * 100.0f); }
    }

	public float AtmosphereVolume
	{
		get { return (m_AtmosphereVolume); } 
	}

	public float AtmosphereRefillRate
	{
		get { return(m_fAtmosphereRefillRate.Get()); }

		[AServerOnly]
		set { m_fAtmosphereRefillRate.Set(value); }
	}

	public float AtmosphereConsumeRate
	{
		get { return(m_fAtmosphereConsumptionRate.Get()); }

		[AServerOnly]
		set { m_fAtmosphereConsumptionRate.Set(value); }
	}

	public bool RequiresAtmosphereRefill
	{
		get { return(AtmosphereConsumeRate != 0.0f || AtmospherePercentage != 1.0f); } 
	}


// Member Methods
	
    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_AtmosphereQuantity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fAtmosphereRefillRate = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fAtmosphereConsumptionRate = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }

	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{

	}

	public void Start()
	{
		if(CNetwork.IsServer)
		{
			// Debug: Atmosphere starts at half the total volume
			m_AtmosphereQuantity.Set(AtmosphereVolume / 2);
		}
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			// Remove consumers that are now null
			m_AtmosphericConsumers.RemoveAll(item => item == null);

			UpdateConsumptionRate();
			UpdateAtmosphereQuantity();
		}
	}

	[AServerOnly]
	public void RegisterAtmosphericConsumer(GameObject _Consumer)
	{
		if(!m_AtmosphericConsumers.Contains(_Consumer))
		{
			m_AtmosphericConsumers.Add(_Consumer);
		}
	}

	[AServerOnly]
	public void UnregisterAtmosphericConsumer(GameObject _Consumer)
	{
		if(m_AtmosphericConsumers.Contains(_Consumer))
		{
			m_AtmosphericConsumers.Remove(_Consumer);
		}
	}

	[AServerOnly]
	private void UpdateConsumptionRate()
	{
		// Calulate the combined consumption rate within the facility
		float consumptionRate = 0.0f;
		foreach(GameObject consumer in m_AtmosphericConsumers)
		{
			CActorAtmosphericConsumer aac = consumer.GetComponent<CActorAtmosphericConsumer>();

			if(aac.IsConsumingAtmosphere)
				consumptionRate += aac.AtmosphericConsumptionRate;
		}

		// Set the consumption rate
		AtmosphereConsumeRate = consumptionRate;
	}

	[AServerOnly]
	private void UpdateAtmosphereQuantity()
    {
		float consumptionAmount = 0.0f;
		float refillAmount = 0.0f;

		// If the atmosphere is being consumed, calculate the consumption rate
		if(m_AtmosphericConsumers.Count != 0)
		{
			// Calculate the consumption amount
			consumptionAmount = -AtmosphereConsumeRate * Time.deltaTime;
		}

		// If the facility requires a refill, calculate the refill rate
		if(RequiresAtmosphereRefill)
        {
			refillAmount = AtmosphereRefillRate * Time.deltaTime;
        }

		// Combine the refill and consumption amounts to get the final rate
		float finalAmount = consumptionAmount + refillAmount;

		// Calculate the new quantity
		float newQuantity = AtmosphereQuantity + finalAmount;

		// Clamp atmosphere
		if(newQuantity > AtmosphereVolume)
		{
			newQuantity = AtmosphereVolume;
		}
		else if(newQuantity < 0.0f)
		{
			newQuantity = 0.0f;

			// There was inssuficent atmosphere, let the consumers know
			foreach(GameObject consumer in m_AtmosphericConsumers)
			{
				CActorAtmosphericConsumer aac = consumer.GetComponent<CActorAtmosphericConsumer>();
				
				if(aac.IsConsumingAtmosphere)
					aac.InsufficientAtmosphere();
			}
		}
		
		// Increase atmosphere amount
		m_AtmosphereQuantity.Set(newQuantity);
	}
};
