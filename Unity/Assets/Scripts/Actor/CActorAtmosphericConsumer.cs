
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorAtmosphericConsumer.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


[RequireComponent(typeof(CActorLocator))]
public class CActorAtmosphericConsumer : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates and Events
	public delegate void NotifyAtmosphereConsumptionState();

	public event NotifyAtmosphereConsumptionState EventInsufficientAtmosphere;


	// Member Fields
	public float m_AtmosphericConsumptionRate = 0.0f;
	public bool m_ConsumingAtmosphere = false;

	private float m_InitialConsumptionRate = 0.0f;
	private CActorLocator m_ActorLocator = null;

	// Member Properties	
	[AServerOnly]
	public float AtmosphericConsumptionRate
	{
		set { m_AtmosphericConsumptionRate = value; }
		get { return (m_AtmosphericConsumptionRate); }
	}

	[AServerOnly]
	public float InitialAtmosphericConsumptionRate
	{
		get { return(m_InitialConsumptionRate); }
	}

	[AServerOnly]
	public bool IsConsumingAtmosphere
	{
		get { return (m_ConsumingAtmosphere); }
	}

	// Member Methods
	public void Awake()
	{
		m_ActorLocator = gameObject.GetComponent<CActorLocator>();
		m_ActorLocator.EventEnteredFacility += OnEnterFacility;
		m_ActorLocator.EventExitedFacility += OnExitFacility;

		m_InitialConsumptionRate = m_AtmosphericConsumptionRate;
	}

	[AServerOnly]
	public void InsufficientAtmosphere()
	{
		if(EventInsufficientAtmosphere != null)
			EventInsufficientAtmosphere();

		m_ConsumingAtmosphere = false;
	}

	[AServerOnly]
	public void SetAtmosphereConsumption(bool _State)
	{
		m_ConsumingAtmosphere = _State;
	}

	[AServerOnly]
	public void OnEnterFacility(GameObject _Facility)
	{
		// Unregister self from other facility atmosphere
		foreach (GameObject facility in m_ActorLocator.ContainingFacilities)
		{
			if(facility != _Facility)
				facility.GetComponent<CFacilityAtmosphere>().UnregisterAtmosphericConsumer(gameObject);
		}

		// Register myself to the facility atmosphere
		_Facility.GetComponent<CFacilityAtmosphere>().RegisterAtmosphericConsumer(gameObject);
	}
	
	[AServerOnly]
	public void OnExitFacility(GameObject _Facility)
	{
		// Unregister self from facility
		_Facility.GetComponent<CFacilityAtmosphere>().UnregisterAtmosphericConsumer(gameObject);

		if (m_ActorLocator.CurrentFacility == null)
        {
            InsufficientAtmosphere();
        }
	}
}
