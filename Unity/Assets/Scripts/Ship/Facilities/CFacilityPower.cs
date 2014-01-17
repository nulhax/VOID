//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomPower.cs
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

	[AServerOnly]
	public event FacilityPowerToggleHandler EventFacilityInsufficientPower;

// Member Fields

	[AServerOnly]
	public float m_CurrentPowerConsumption = 0.0f;

	private CNetworkVar<float> m_PowerConsumption = null;
	private CNetworkVar<bool> m_PowerActive = null;

	private float m_PrevPowerConsumptionRate = 0.0f;


// Member Properties
	
    public float PowerConsumption
    {
        get { return (m_PowerConsumption.Get()); }

		[AServerOnly]
		set { m_PowerConsumption.Set(value); }
    }

	public bool IsPowerActive
	{
		get { return (m_PowerActive.Get()); }
		
		[AServerOnly]
		set { m_PowerActive.Set(value); }
	}


// Member Functions
	
	public override void InstanceNetworkVars()
	{
		m_PowerConsumption = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PowerActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
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
			if(m_PrevPowerConsumptionRate != m_CurrentPowerConsumption)
			{
				PowerConsumption = m_CurrentPowerConsumption;

				m_PrevPowerConsumptionRate = m_CurrentPowerConsumption;
			}
		}
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
		if(EventFacilityInsufficientPower != null)
			EventFacilityInsufficientPower(gameObject);

		DeactivatePower();
	}
};
