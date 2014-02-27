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
	private bool m_VisorDown = false;
	private bool m_VisorUIActive = false;


	// Member Properties
	public bool IsVisorDown
	{
		get { return(m_VisorDown); }
	}
	

	// Member Methods
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

//		// Turn off ship indicator
//		if(m_ShipIndicator.gameObject.activeSelf)
//			m_ShipIndicator.gameObject.SetActive(false);
	}
}
