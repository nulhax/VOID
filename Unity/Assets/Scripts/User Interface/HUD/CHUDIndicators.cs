//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDRoot.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CHUDIndicators : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CHUDLocator m_ShipIndicator = null;
	
	
	// Member Properties
	

	// Member Methods
	private void Start()
	{
		// Register events for UI activation from the visor
		CGameHUD.Visor.EventVisorHUDDeactivated += OnDeactivateHUD;
	}

	private void Update()
	{
		if(CGameHUD.HUD3D.IsHUDActive)
		{
			UpdateIndicators();
		}
	}

	private void UpdateIndicators()
	{
		// Turn on and update the ship indicator
		if(!CGameCameras.IsObserverInsideShip)
		{
			if(!m_ShipIndicator.gameObject.activeSelf)
				m_ShipIndicator.gameObject.SetActive(true);
			
			m_ShipIndicator.Target = CGameShips.GalaxyShip.transform;
		}
		else
		{
			// Turn off ship indicator
			if(m_ShipIndicator.gameObject.activeSelf)
				m_ShipIndicator.gameObject.SetActive(false);
		}
	}

	private void OnDeactivateHUD()
	{
		// Turn off ship indicator
		if(m_ShipIndicator.gameObject.activeSelf)
			m_ShipIndicator.gameObject.SetActive(false);
	}
}
