//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityPower.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CFacilityPower : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events

	public delegate void FacilityPowerToggleHandler(GameObject _Sender);

	public event FacilityPowerToggleHandler EventFacilityPowerActivated;
	public event FacilityPowerToggleHandler EventFacilityPowerDeactivated;


// Member Fields
	
	public float m_SelfConsumptionRate = 0.0f;

	private CNetworkVar<float> m_PowerConsumptionRate = null;
	private CNetworkVar<bool> m_PowerActive = null;

	private List<GameObject> m_PowerConsumers = new List<GameObject>();


// Member Properties
	
    public float PowerConsumptionRate
    {
        get { return (m_PowerConsumptionRate.Get()); }

		[AServerOnly]
		set { m_PowerConsumptionRate.Set(value); }
    }

	public List<GameObject> PowerConsumers
	{
		get { return (m_PowerConsumers); }
	}

	public bool IsPowerActive
	{
		get { return (m_PowerActive.Get()); }
		
		[AServerOnly]
		set { m_PowerActive.Set(value); }
	}


// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_PowerConsumptionRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PowerActive = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		if(_VarInstance == m_PowerActive)
		{
			if(IsPowerActive)
			{
				if(EventFacilityPowerActivated != null) 
					EventFacilityPowerActivated(gameObject);
			}
			else
			{
				if(EventFacilityPowerDeactivated != null) 
					EventFacilityPowerDeactivated(gameObject);
			}
		}
	}

	public void Start()
	{
		if(CNetwork.IsServer)
			ActivatePower();
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			// Remove consumers that are now null
			m_PowerConsumers.RemoveAll(item => item == null);

			// Calculate current power consumption
			UpdateConsumptionRate();
		}
	}

	[AServerOnly]
	public void RegisterPowerConsumer(GameObject _Consumer)
	{
		if(!m_PowerConsumers.Contains(_Consumer))
		{
			m_PowerConsumers.Add(_Consumer);
		}
	}
	
	[AServerOnly]
	public void UnregisterPowerConsumer(GameObject _Consumer)
	{
		if(m_PowerConsumers.Contains(_Consumer))
		{
			m_PowerConsumers.Remove(_Consumer);
		}
	}

	[AServerOnly]
	private void UpdateConsumptionRate()
	{
		// Calulate the combined consumption rate within the facility
		float consumptionRate = 0.0f;
		foreach(GameObject consumer in m_PowerConsumers)
		{
			CModulePowerConsumption mpc = consumer.GetComponent<CModulePowerConsumption>();
			
			if(mpc.IsConsumingPower)
				consumptionRate += mpc.PowerConsumptionRate;
		}
		
		// Set the consumption rate
		PowerConsumptionRate = consumptionRate + m_SelfConsumptionRate;
	}

	[AServerOnly]
	public void ActivatePower()
	{
		m_PowerActive.Set(true);
	}

	[AServerOnly]
	public void DeactivatePower()
	{
		m_PowerActive.Set(false);
	}

	[AServerOnly]
	public void InsufficienttPower()
	{
		// There was inssuficent power, let the consumers know
		foreach(GameObject consumer in m_PowerConsumers)
		{
			CModulePowerConsumption mpc = consumer.GetComponent<CModulePowerConsumption>();
			
			if(mpc.IsConsumingPower)
				mpc.InsufficientPower();
		}

		DeactivatePower();
	}
};
