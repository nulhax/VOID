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
	private TweenAlpha m_HealthOverlayTween = null;


	// Member Properties

	
	// Member Methods
	public void Start()
	{
		// Cache the health tween
		m_HealthOverlayTween = GetComponent<TweenAlpha>();

		// Set the health amcors as screen size
		UISprite sprite = GetComponent<UISprite>();
		sprite.leftAnchor.absolute = -Screen.width / 2;
		sprite.rightAnchor.absolute = Screen.width / 2;
		sprite.bottomAnchor.absolute = -Screen.height / 2;
		sprite.topAnchor.absolute = Screen.height / 2;
	}
	
	public void Update()
	{
		if(CGamePlayers.SelfActor != null)
		{
			UpdateOverlay();
		}
	}
	
	private void UpdateOverlay()
	{
        // Get the player oxygen supplu
        float health = CGamePlayers.SelfActor.GetComponent<CPlayerHealth>().Health;
        float maxHealth = CGamePlayers.SelfActor.GetComponent<CPlayerHealth>().MaxHealth;
		
        // Calculate the value ratio
        float value = 1.0f - health / maxHealth;
        float alphaTweenRange = value * 0.5f;

        // Update the bar
        m_HealthOverlayTween.to = value;
        m_HealthOverlayTween.from = value - alphaTweenRange;
        m_HealthOverlayTween.duration = 0.5f;
	}
}
