//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomAtmosphere.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	:  
//  Mail    	:  
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CFacilityWarningSystem : CNetworkMonoBehaviour
{

// Member Types

	
// Member Delegates & Events


    //private delegate void HandleWarningInstanceEvent(TWarningInstance _WarningInstance);
    //private event HandleWarningInstanceEvent EventWarningEventStarted;
    //private event HandleWarningInstanceEvent EventWarningEventEnded;


// Member Properties


// Member Methods
    

   
	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{

	}

     /*
	void Start()
	{
		if(CNetwork.IsServer)
		{
			m_CachedFacilityInterface = gameObject.GetComponent<CFacilityInterface>();
			m_CachedFacilityAtmosphere = gameObject.GetComponent<CFacilityAtmosphere>();
			m_CachedShipPowerSystem = CGameShips.Ship.GetComponent<CShipPowerSystem>();

			// Register facility related warning events
			gameObject.GetComponent<CFacilityAtmosphere>().EventDecompression += OnAtmosphereDecompression;
		}
	}


	void Update()
	{
		if(CNetwork.IsServer)
		{
			CheckAtmosphericConditions();
			CheckPowerConditions();
		}
	}


	[AServerOnly]
	void CheckAtmosphericConditions()
	{
		// Check if there is a atmospheric + major warning instance
		if(DoesWarningInstanceExist(EWarningType.Atmosphere, EWarningSeverity.Major))
		{
			if(m_CachedFacilityAtmosphere.QuantityPercent > 25.0f)
			{
				RemoveWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Major);
			}
		}
		// Create the warning instance if it doesnt exist yet
		else if(m_CachedFacilityAtmosphere.QuantityPercent < 25.0f)
		{
			AddWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Major);
		}
	}


	[AServerOnly]
	void CheckPowerConditions()
	{
		// Calculate the ships consumption vs generation
		float diffGenCons = m_CachedShipPowerSystem.TotalCapacity - m_CachedShipPowerSystem.ShipCurrentConsumptionRate;

		// Calculate the ships current battery power percentage
		float powerRatio = m_CachedShipPowerSystem.ShipCurrentCharge / m_CachedShipPowerSystem.ShipCurrentChargeCapacity;

		// Check if there is a power + major warning instance
		if(DoesWarningInstanceExist(EWarningType.Power, EWarningSeverity.Major))
		{
			// If positive can remove the warning
			if(Mathf.Sign(diffGenCons) == 1.0f)
			{
				RemoveWarningInstance(EWarningType.Power, EWarningSeverity.Major);
			}
		}
		// Create the warning instance if it doesnt exist yet
		else if(Mathf.Sign(diffGenCons) == -1.0f)
		{
			AddWarningInstance(EWarningType.Power, EWarningSeverity.Major);
		}

		// Check if there is a power + critical warning instance
		if(DoesWarningInstanceExist(EWarningType.Power, EWarningSeverity.Critical))
		{
			// If percentage is above 25% can remove the warning
			if(powerRatio > 0.25f)
			{
				RemoveWarningInstance(EWarningType.Power, EWarningSeverity.Critical);
			}
		}
		// Create the warning instance if it doesnt exist yet
		else if(powerRatio < 0.25f)
		{
			AddWarningInstance(EWarningType.Power, EWarningSeverity.Critical);
		}
	}
	

	[AServerOnly]
	void AddWarningInstance(EWarningType _Type, EWarningSeverity _Severity)
	{
		//Debug.Log("AddWarningInstance: " + _Type + ", " + _Severity);

		// Add the warning instance
		m_ActiveWarningInstances.Add(new TWarningInstance(_Type, _Severity));

		// Activate the alarm for this severity level if not currently active
		if(!m_ActiveAlarms[(int)_Severity])
		{
			//Debug.Log("Alarms for: " + _Severity + ": ON");

			SetAlarmsState(_Severity, true);
			m_ActiveAlarms[(int)_Severity] = true;
		}
		else
		{
			//Debug.Log("Alarms Uneffected for: " + _Severity + ": ON");
		}
	}


	[AServerOnly]
	void RemoveWarningInstance(EWarningType _Type, EWarningSeverity _Severity)
	{
		//Debug.Log("RemoveWarningInstance: " + _Type + ", " + _Severity);

		// Remove the first warning instance of same type
		m_ActiveWarningInstances.Remove(
			m_ActiveWarningInstances.Find(wi => 
		                              wi.m_WarningSeverity == _Severity && 
		                              wi.m_WarningType == _Type));

		// Find instances with this severity
		int numSameSeverity = 
			m_ActiveWarningInstances.FindAll(wi => 
			                                 wi.m_WarningSeverity == _Severity).Count;

		// Turn off alarms if there are no more of the same severity
		if(m_ActiveAlarms[(int)_Severity] && numSameSeverity == 0)
		{
			//Debug.Log("Alarms for: " + _Severity + ": OFF");

			SetAlarmsState(_Severity, false);
			m_ActiveAlarms[(int)_Severity] = false;
		}
		else
		{
			//Debug.Log("Alarms Uneffected for: " + _Severity);
		}
	}


	[AServerOnly]
	void OnAtmosphereDecompression(bool _Decompression, bool _Explosive)
	{
		if(_Decompression)
		{
			if(_Explosive)	AddWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Critical);
			else 			AddWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Minor);
		}
		else 
		{
			if(_Explosive) 	RemoveWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Critical);
			else 			RemoveWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Minor);
		}
	}


	[AServerOnly]
	void SetAlarmsState(EWarningSeverity _Severity, bool _State)
	{
		switch(_Severity) 
		{
		case EWarningSeverity.Minor:
			m_CachedFacilityInterface.FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning).ForEach((alarm) =>
     		{
				alarm.GetComponent<CAlarmBehaviour>().SetAlarmActive(_State);
     		});
			break;

		case EWarningSeverity.Major:
			m_CachedFacilityInterface.FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning).ForEach((alarm) =>
		   	{
				alarm.GetComponent<CAlarmBehaviour>().SetAlarmActive(_State);
			});
			break;

		case EWarningSeverity.Critical:
			m_CachedFacilityInterface.FindAccessoriesByType(CAccessoryInterface.EType.Alarm).ForEach((alarm) =>
			{
				alarm.GetComponent<CAlarmBehaviour>().SetAlarmActive(_State);
			});
			break;

		default:
			break;
		}
	}


	bool DoesWarningInstanceExist(EWarningType _Type, EWarningSeverity _Severity)
	{
		return(m_ActiveWarningInstances.Exists(wi => 
		                                     wi.m_WarningSeverity == _Severity && 
		                                     wi.m_WarningType == _Type));
	}


	void OnNetworkVarSync(INetworkVar _cSynedVar)
	{
		// Empty
	}
    */


// Member Fields


    List<TWarningInstance> m_ActiveWarningInstances = new List<TWarningInstance>();

    bool[] m_ActiveAlarms = new bool[(int)EWarningSeverity.MAX];

    CFacilityInterface m_CachedFacilityInterface = null;
    CFacilityAtmosphere m_CachedFacilityAtmosphere = null;

    CShipPowerSystem m_CachedShipPowerSystem = null;

};
