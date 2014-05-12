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
    private delegate void HandleWarningInstanceEvent(TWarningInstance _WarningInstance);
    private event HandleWarningInstanceEvent EventWarningEventStarted;
    private event HandleWarningInstanceEvent EventWarningEventEnded;


	// Member Fields
	List<TWarningInstance> m_ActiveWarningInstances = new List<TWarningInstance>();
	
	bool[] m_ActiveAlarms = new bool[(int)EWarningSeverity.MAX];

	CFacilityInterface m_FacilityInterface = null;
	CFacilityAtmosphere m_FacilityAtmosphere = null;
	CFacilityTiles m_FacilityTiles = null;


	// Member Properties


	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{

	}
	
	private void OnNetworkVarSync(INetworkVar _cSynedVar)
	{
		// Empty
	}
   
	private void Start()
	{
		if(!CNetwork.IsServer)
			return;

		m_FacilityInterface = gameObject.GetComponent<CFacilityInterface>();
		m_FacilityAtmosphere = gameObject.GetComponent<CFacilityAtmosphere>();
		m_FacilityTiles = gameObject.GetComponent<CFacilityTiles>();
	}

	private void Update()
	{
		if(!CNetwork.IsServer)
			return;

		CheckAtmosphericConditions();

		UpdateAlarmAnimation();
	}

	private void UpdateAlarmAnimation()
	{
		EWarningSeverity highestSeverity = EWarningSeverity.INVALID;
		for(int i = (int)EWarningSeverity.INVALID + 1; i < (int)EWarningSeverity.MAX; ++i)
		{
			if(!m_ActiveAlarms[i])
				continue;

			highestSeverity = (EWarningSeverity)i;
		}

		if(highestSeverity == EWarningSeverity.INVALID)
			return;

		Color alarmColor = GetAlarmColor(highestSeverity);
		float alarmColorPower = Mathf.Sign(Mathf.Sin(Time.time * 5.0f)) * 10.0f;
		if(alarmColorPower < 0.0f)
			alarmColorPower = 0.0f;

		foreach(CTileInterface tileInterface in m_FacilityTiles.InteriorTiles)
		{
			CTile interiorWall = tileInterface.GetTile(CTile.EType.Interior_Wall);
			if(interiorWall != null && interiorWall.m_TileObject != null)
			{
				foreach(Renderer tileRenderer in interiorWall.m_TileObject.GetComponentsInChildren<Renderer>())
				{
					foreach(Material tileMaterial in tileRenderer.materials)
					{
						tileMaterial.SetColor("_EmissiveColorR", alarmColor);
						tileMaterial.SetFloat("_EmissivePowerR", alarmColorPower);
					}
				}
			}
		}
	}

	private Color GetAlarmColor(EWarningSeverity _WarningSeverity)
	{
		Color color = Color.black;
		switch(_WarningSeverity)
		{
		case EWarningSeverity.Minor: color = Color.white; break;
		case EWarningSeverity.Major: color = Color.yellow; break;
		case EWarningSeverity.Critical: color = Color.red; break;
		}

		return(color);
	}
	
	[AServerOnly]
	private void CheckAtmosphericConditions()
	{
		// Detirmine the warning severity
		EWarningSeverity warningSeverity = EWarningSeverity.INVALID;
		if(m_FacilityAtmosphere.Density < 1.0f && m_FacilityAtmosphere.Density > 0.3f)
		{
			warningSeverity = EWarningSeverity.Major;
		}
		else if(m_FacilityAtmosphere.Density < 0.3f)
		{
			warningSeverity = EWarningSeverity.Critical;
		}

		// Remove other warning instances
		if(warningSeverity != EWarningSeverity.Major &&
		   DoesWarningInstanceExist(EWarningType.Atmosphere, EWarningSeverity.Major))
		{
			RemoveWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Major);
		}
		if(warningSeverity != EWarningSeverity.Critical &&
		        DoesWarningInstanceExist(EWarningType.Atmosphere, EWarningSeverity.Critical))
		{
			RemoveWarningInstance(EWarningType.Atmosphere, EWarningSeverity.Critical);
		}

		// Add the current wanting instance
		if(warningSeverity != EWarningSeverity.INVALID)
			AddWarningInstance(EWarningType.Atmosphere, warningSeverity);
	}

	[AServerOnly]
	private void AddWarningInstance(EWarningType _Type, EWarningSeverity _Severity)
	{
		// Add the warning instance
		m_ActiveWarningInstances.Add(new TWarningInstance(_Type, _Severity));

		// Activate the alarm for this severity level if not currently active
		if(!m_ActiveAlarms[(int)_Severity])
		{
			m_ActiveAlarms[(int)_Severity] = true;
		}
	}
	
	[AServerOnly]
	private void RemoveWarningInstance(EWarningType _Type, EWarningSeverity _Severity)
	{
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
			m_ActiveAlarms[(int)_Severity] = false;
		}
	}
	
	private bool DoesWarningInstanceExist(EWarningType _Type, EWarningSeverity _Severity)
	{
		return(m_ActiveWarningInstances.Exists(wi => 
		                                     wi.m_WarningSeverity == _Severity && 
		                                     wi.m_WarningType == _Type));
	}
};
