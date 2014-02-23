
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

public class CActorPowerConsumer : MonoBehaviour
{
	public delegate void NotifyConsumptionState(bool consuming);

	// Member Fields
	public event NotifyConsumptionState EventConsumptionToggle;
	private CFacilityPower m_ParentFacility = null;
	public float m_InitialConsumptionRate = 1.0f;
	[HideInInspector] public float m_CurrentConsumptionRate = 0.0f;

	// Member Methods
	public void Awake()
	{
		for (Transform parentTransform = transform.parent; parentTransform != null; parentTransform = parentTransform.parent)
		{
			CFacilityPower parentFacility = parentTransform.GetComponent<CFacilityPower>();
			if (parentFacility != null)
			{
				m_ParentFacility = parentFacility;
				m_ParentFacility.EventFacilityPowerActivated += FacilityActivate;
				m_ParentFacility.EventFacilityPowerDeactivated += FacilityDeactivate;
				break;
			}
		}
	}

	public void OnDestroy()
	{
		if (m_ParentFacility != null)
		{
			m_ParentFacility.EventFacilityPowerActivated -= FacilityActivate;
			m_ParentFacility.EventFacilityPowerDeactivated -= FacilityDeactivate;
		}
	}

	[AServerOnly]
	public void FacilityActivate(GameObject sender)
	{
		m_CurrentConsumptionRate = m_InitialConsumptionRate;
		if (EventConsumptionToggle != null)
			EventConsumptionToggle(true);
	}

	[AServerOnly]
	public void FacilityDeactivate(GameObject sender)
	{
		m_CurrentConsumptionRate = 0.0f;
		if (EventConsumptionToggle != null)
			EventConsumptionToggle(false);
	}
}
