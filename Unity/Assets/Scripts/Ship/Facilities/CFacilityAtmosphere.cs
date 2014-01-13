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
		
	private CNetworkVar<float> m_AtmosphereQuantity;
	
	private List<GameObject> m_AtmosphericConsumers = new List<GameObject>();

	private float m_fAtmosphereRefillRate = 0.0f;

	private float m_fAtmosphereVolume = 1000.0f;


// Member Properties
	
    public float AtmosphereQuantity
    {
        get { return (m_AtmosphereQuantity.Get()); }
    }

    public float AtmospherePercentage
    {
		get { return (AtmosphereQuantity / m_fAtmosphereVolume); }
    }

	public float AtmosphereVolume
	{
		get { return (m_fAtmosphereVolume); } 
	}

	public float AtmosphereRefillRate
	{
		get { return(m_fAtmosphereRefillRate); }
		set { m_fAtmosphereRefillRate = value; }
	}

	public float AtmosphereConsumeRate
	{
		get 
		{ 
			// Calulate the combined consumption rate within the facility
			float consumptionRate = 0.0f;
			foreach(GameObject consumer in m_AtmosphericConsumers)
			{
				consumptionRate += consumer.GetComponent<CActorAtmosphericConsumer>().AtmosphericConsumptionRate;
			}
			return(consumptionRate); 
		}
	}

	public bool RequiresAtmosphereRefill
	{					
		get { return(AtmosphereQuantity != AtmosphereVolume); } 
	}


// Member Methods
	
    public override void InstanceNetworkVars()
    {
        m_AtmosphereQuantity = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }

	public void OnNetworkVarSync(INetworkVar _cVarInstance)
	{

	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateAtmosphereRefill();
			UpdateAtmosphereConsumption();
		}
	}

	public void LateUpdate()
	{
		if(CNetwork.IsServer)
		{
			// Reset the refill rate
			AtmosphereRefillRate = 0.0f;
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

	private void UpdateAtmosphereRefill()
    {
		// If the facility requires a refill, do so
		if(RequiresAtmosphereRefill)
        {
			// Check atmosphere level is below area volume 
            if (AtmosphereQuantity < AtmosphereVolume)
            {
                float fNewQuantity = AtmosphereQuantity + AtmosphereRefillRate * Time.deltaTime;

                // Clamp atmosphere to gas capacity
				if (fNewQuantity > AtmosphereVolume)
                {
					fNewQuantity = AtmosphereVolume;
                }

                // Increase atmosphere amount
                m_AtmosphereQuantity.Set(fNewQuantity);
            }
        }
	}

	private void UpdateAtmosphereConsumption()
	{
		// Check atmosphere is higher than 0 and that there are consumers
		if (AtmosphereQuantity > 0.0f && m_AtmosphericConsumers.Count != 0)
	    {
			// Remove obsolete consumers
			m_AtmosphericConsumers.RemoveAll((item) => item == null);

			float fNewQuantity = AtmosphereQuantity - AtmosphereConsumeRate * Time.deltaTime;
			
			// Clamp atmosphere to gas capacity
			if (fNewQuantity < 0.0f)
			{
				fNewQuantity = 0.0f;
			}

			// Increase atmosphere amount
			m_AtmosphereQuantity.Set(fNewQuantity);
    	}
    }
};
