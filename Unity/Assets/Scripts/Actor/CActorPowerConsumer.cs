
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

public class CActorPowerConsumer : MonoBehaviour
{
	public delegate void NotifyPowerConsumptionState();

	// Member Fields
	public event NotifyPowerConsumptionState EventConsumptionToggle;
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

	[AServerOnly]
	public void FacilityActivate(GameObject sender)
	{
		m_CurrentConsumptionRate = m_InitialConsumptionRate;
	}

	[AServerOnly]
	public void FacilityDeactivate(GameObject sender)
	{
		m_CurrentConsumptionRate = 0.0f;
	}

	[AServerOnly]
	public void InsufficientPower(GameObject _Sender)
	{
		if (EventConsumptionToggle != null)
			EventConsumptionToggle();

		m_CurrentConsumptionRate = 0.0f;
	}
}
