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


public class CHUDHealth : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public TweenAlpha m_HealthOverlayTween = null;


	// Member Properties

	
	// Member Methods
	public void Start()
	{

	}
	
	public void Update()
	{
		UpdateOverlay();
	}
	
	private void UpdateOverlay()
	{
		if (CNetwork.IsConnectedToServer) 
		{
			// Get the player oxygen supplu
			float health = CGamePlayers.SelfActor.GetComponent<CPlayerHealth> ().HitPoints;
			float maxHealth = CGamePlayers.SelfActor.GetComponent<CPlayerHealth> ().k_fMaxHealth;

			// Calculate the value ratio
			float value = 1.0f - health / maxHealth;
			float alphaTweenRange = value * 0.5f;

			// Update the bar
			m_HealthOverlayTween.to = value;
			m_HealthOverlayTween.from = value - alphaTweenRange;
			m_HealthOverlayTween.duration = 0.5f;
		}
	}
}
