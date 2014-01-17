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
	
    public override void InstanceNetworkVars()
    {
        m_AtmosphereQuantity = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fAtmosphereRefillRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fAtmosphereConsumptionRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
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
			CalculateConsumptionRate();
			UpdateAtmosphereQuantity();
		}
	}

	public void AddAtmosphericConsumer(GameObject _Consumer)
	{
		if(!m_AtmosphericConsumers.Contains(_Consumer))
		{
			m_AtmosphericConsumers.Add(_Consumer);
		}
	}
	
	public void RemoveAtmosphericConsumer(GameObject _Consumer)
	{
		if(m_AtmosphericConsumers.Contains(_Consumer))
		{
			m_AtmosphericConsumers.Remove(_Consumer);
		}
	}

	private void CalculateConsumptionRate()
	{
		// Calulate the combined consumption rate within the facility
		float consumptionRate = 0.0f;
		bool bHasNullGameObject = false;
		foreach(GameObject consumer in m_AtmosphericConsumers)
		{
			if (consumer != null)
			{
				consumptionRate += consumer.GetComponent<CActorAtmosphericConsumer>().AtmosphericConsumptionRate;
			}
			else
			{
				bHasNullGameObject = true;
			}
		}

		m_AtmosphericConsumers.RemoveAll(cEntry => cEntry == null);

		// Set the consumption rate
		AtmosphereConsumeRate = consumptionRate;
	}

	private void UpdateAtmosphereQuantity()
    {
		float consumptionAmount = 0.0f;
		float refillAmount = 0.0f;

		// If the atmosphere is being consumed, calculate the consumption rate
		if(m_AtmosphericConsumers.Count != 0)
		{
			// Remove obsolete consumers
			m_AtmosphericConsumers.RemoveAll((item) => item == null);

			// Calculate the consumption amount
			consumptionAmount = -AtmosphereConsumeRate * Time.deltaTime;
		}

		// If the facility requires a refill, calculate the refill rate
		if(RequiresAtmosphereRefill)
        {
			refillAmount = AtmosphereRefillRate * Time.deltaTime;
        }

		// Combine the refill and consumption amounts to get the final rate
		float finalRate = consumptionAmount + refillAmount;

		// Calculate the new quantity
		float newQuantity = AtmosphereQuantity + finalRate;

		// Clamp atmosphere
		if(newQuantity > AtmosphereVolume)
		{
			newQuantity = AtmosphereVolume;
		}
		else if(newQuantity < 0.0f)
		{
			newQuantity = 0.0f;
		}
		
		// Increase atmosphere amount
		m_AtmosphereQuantity.Set(newQuantity);
	}
};
