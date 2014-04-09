
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


	void Awake()
	{
		m_InitialConsumptionRate = m_AtmosphericConsumptionRate;
	}


    void Start()
    {
        m_cActorLocator = gameObject.GetComponent<CActorLocator>();

        if (CNetwork.IsServer)
        {
            m_cActorLocator.EventFacilityChangeHandler += OnEventFacilityChange;
        }
    }


     
    void OnDestroy()
    {
        if (CNetwork.IsServer)
        {
            m_cActorLocator.EventFacilityChangeHandler -= OnEventFacilityChange;
        }

		if (CNetwork.IsServer && m_cRegisteredFacilityObject != null)
		{
	        m_cRegisteredFacilityObject.GetComponent<CFacilityAtmosphere>().UnregisterAtmosphericConsumer(gameObject);
	        m_bRegistered = false;
		}
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
	void OnEventFacilityChange(GameObject _cPreviousFacility, GameObject _cNewFacility)
	{
        /*
		// Unregister self from other facility atmosphere
		foreach (GameObject facility in m_cActorLocator.ContainingFacilities)
		{
			if(facility != _Facility)
				facility.GetComponent<CFacilityAtmosphere>().UnregisterAtmosphericConsumer(gameObject);
		}

		// Register myself to the facility atmosphere
		_Facility.GetComponent<CFacilityAtmosphere>().RegisterAtmosphericConsumer(gameObject);
        m_bRegistered = true;
        m_cRegisteredFacilityObject = _Facility;
         * */
	}

	
	[AServerOnly]
	void OnExitFacility(GameObject _Facility)
	{/*
        //if (_Facility != m_cRegisteredFacilityObject)
        //    Debug.LogError("Actor consumer is unregistering from a facility that it was not registered to");

		// Unregister self from facility
		_Facility.GetComponent<CFacilityAtmosphere>().UnregisterAtmosphericConsumer(gameObject);
        m_bRegistered = false;
        m_cRegisteredFacilityObject = null;

		if (m_cActorLocator.CurrentFacility == null)
        {
            InsufficientAtmosphere();
        }
      * */
	}


// Member Fields


    public float m_AtmosphericConsumptionRate = 0.0f;
    public bool m_ConsumingAtmosphere = false;

   float m_InitialConsumptionRate = 0.0f;
   CActorLocator m_cActorLocator = null;

   GameObject m_cRegisteredFacilityObject = null;
   bool m_bRegistered = false;


}
