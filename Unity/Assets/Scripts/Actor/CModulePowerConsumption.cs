
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorPowerConsumer.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


public class CModulePowerConsumption : MonoBehaviour
{
	public delegate void NotifyConsumptionState();

	public event NotifyConsumptionState EventInsufficientPower;


	// Member Fields
	public float m_PowerConsumptionRate = 0.0f;
	public bool m_ConsumingPower = true;

	private float m_InitialConsumptionRate = 0.0f;

	// Memeber Properties
	[AServerOnly]
	public float PowerConsumptionRate
	{
		set { m_PowerConsumptionRate = value; }
		get { return (m_PowerConsumptionRate); }
	}
	
	[AServerOnly]
	public float InitialPowerConsumptionRate
	{
		get { return(m_InitialConsumptionRate); }
	}
	
	[AServerOnly]
	public bool IsConsumingPower
	{
		get { return (m_ConsumingPower); }
	}


	// Member Methods
	private void Start()
	{
		m_InitialConsumptionRate = m_PowerConsumptionRate;

		if(CNetwork.IsServer)
		{
			// Find the facility to which this module belongs to
			CFacilityPower fp = GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>();

			// Register myself as a consumer
			fp.RegisterPowerConsumer(gameObject);
		}
	}

	private void OnDestroy()
	{
		if(CNetwork.IsServer)
		{
			// Find the facility to which this module belongs to
			CFacilityPower fp = GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>();
			
			// Unregister myself as a consumer
			fp.UnregisterPowerConsumer(gameObject);
		}
	}

	[AServerOnly]
	public void InsufficientPower()
	{
		if(EventInsufficientPower != null)
		{
			EventInsufficientPower();
		}
		
		m_ConsumingPower = false;
	}
}
