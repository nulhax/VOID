
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


public class CModulePower : MonoBehaviour
{

// Member Types


// Member Delegates & Events


	public delegate void PowerChangeHandler(GameObject _cModule, bool _bEnabled);
	public event PowerChangeHandler EventPowerStatusChange;


// Memeber Properties


	public float PowerConsumptionRate
	{
		get { return (m_fConsumptionRate); }
	}
	
	
	public bool IsConsumingPower
	{
		get { return (gameObject.GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().IsPowerActive); }
	}


// Member Methods


	void Start()
	{
        //gameObject.GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActiveChange += OnEventFacilityPowerActiveChange;
	}


	void OnDestroy()
	{
        //gameObject.GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActiveChange -= OnEventFacilityPowerActiveChange;
	}


    void OnEventFacilityPowerActiveChange(GameObject _cFacility, bool _bActive)
	{
        if (EventPowerStatusChange != null) EventPowerStatusChange(gameObject, _bActive);
	}


// Member Fields


    public float m_fConsumptionRate = 0.0f;


}
