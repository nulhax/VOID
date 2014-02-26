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


public class CHUDVisor : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public UILabel m_O2Value = null;
	public UIProgressBar m_02Bar = null;
	public UILabel m_Status = null;

	public CHUDLocator m_ShipIndicator = null;

	private bool m_VisorDown = false;
	private bool m_VisorUIActive = false;
	private float m_TransitionValue = 0.0f;


	// Member Properties
	public bool IsVisorDown
	{
		get {return(m_VisorDown);}
	}
	
	// Member Methods
	public void Update()
	{
		UpdateUI();
	}

	public void SetVisorState(bool _Down)
	{
		if(_Down && !m_VisorDown)
		{
			animation.CrossFade("VisorDown");
 		}
		else if(!_Down && m_VisorDown)
		{
			animation.CrossFade("VisorUp");
		}

		m_VisorDown = _Down;
	}

	private void UpdateUI()
	{
        if (CGamePlayers.SelfActor == null)
        {
            return;
        }

		// Get the player oxygen supplu
		float oxygenSupply = CGamePlayers.SelfActor.GetComponent<CPlayerSuit>().OxygenSupply;
		float oxygenCapacity = CGamePlayers.SelfActor.GetComponent<CPlayerSuit>().OxygenCapacity;
		
		// Calculate the value ratio
		float value = oxygenSupply/oxygenCapacity;
		
		// Update the bar
		CDUIUtilites.LerpBarColor(value, m_02Bar);
		m_02Bar.value = value;
		
		// Update the label
		m_O2Value.color = CDUIUtilites.LerpColor(value);
		m_O2Value.text = Mathf.RoundToInt(oxygenSupply) + " / " + Mathf.RoundToInt(oxygenCapacity);

		// Update the status label
		if(value == 0.0f)
		{
			m_Status.color = Color.red;
			m_Status.text = "CRITICAL:\nOXYGEN DEPLETED!";

			m_Status.GetComponent<UITweener>().enabled = true;
		}
		else if(value < 0.20f)
		{
			m_Status.color = Color.red;
			m_Status.text = "Critical:\nLow Oxygen!";

			m_Status.GetComponent<UITweener>().enabled = true;
		}
		else if(value < 0.50f)
		{
			m_Status.color = Color.yellow;
			m_Status.text = "Warning:\nLow Oxygen!";

			if(m_Status.GetComponent<UITweener>().enabled)
			{
				m_Status.GetComponent<UITweener>().ResetToBeginning();
				m_Status.GetComponent<UITweener>().enabled = false;
			}
		}
		else
		{
			m_Status.text = "";

			if(m_Status.GetComponent<UITweener>().enabled)
			{
				m_Status.GetComponent<UITweener>().ResetToBeginning();
				m_Status.GetComponent<UITweener>().enabled = false;
			}
		}

		// Turn on and update the indicator
		if(CGamePlayers.SelfActor.GetComponent<CActorLocator>().LastEnteredFacility == null && m_VisorUIActive)
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

	private void ActivateUI()
	{
		// Turn on visor UI
		m_VisorUIActive = true;

		// Play the tweeners
		gameObject.GetComponent<UIPlayTween>().playDirection = AnimationOrTween.Direction.Forward;
		gameObject.GetComponent<UIPlayTween>().Play(true);
	}

	private void DeactivateUI()
	{
		// Turn off visor
		m_VisorUIActive = false;

		// Play the tweeners
		gameObject.GetComponent<UIPlayTween>().playDirection = AnimationOrTween.Direction.Reverse;
		gameObject.GetComponent<UIPlayTween>().Play(true);

		// Turn off ship indicator
		if(m_ShipIndicator.gameObject.activeSelf)
			m_ShipIndicator.gameObject.SetActive(false);
	}
}
