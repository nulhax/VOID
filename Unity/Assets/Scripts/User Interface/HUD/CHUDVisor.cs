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
	public delegate void HandleVisorStateChange();

	public event HandleVisorStateChange EventVisorUp;
	public event HandleVisorStateChange EventVisorDown;


	// Member Fields
	public Transform m_VisorUpTrans = null;
	public Transform m_VisorDownTrans = null;

	public float m_TransitionTime = 0.3f;

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
	public void Start()
	{
		// Start the visor up
		UpdateVisorTransform(m_VisorUpTrans.position, m_VisorUpTrans.rotation);
	}

	public void Update()
	{
		if(m_VisorUIActive)
		{
			UpdateUI();
		}
	}

	public void SetVisorState(bool _Down)
	{
		m_VisorDown = _Down;

		if(_Down)
		{
			this.StopAllCoroutines();
			this.StartCoroutine(InterpolateVisorDown());

			if(EventVisorDown != null)
				EventVisorDown();
		}
		else
		{
			this.StopAllCoroutines();
			this.StartCoroutine(InterpolateVisorUp());

			if(EventVisorUp != null)
				EventVisorUp();
		}

		// Update the indicator
		if(CGamePlayers.SelfActor.GetComponent<CActorLocator>().LastEnteredFacility == null)
		{
			if(!m_ShipIndicator.gameObject.activeSelf)
				m_ShipIndicator.gameObject.SetActive(true);
			
			m_ShipIndicator.Tracker = CGameShips.GalaxyShip.transform;
			m_ShipIndicator.GameCamera = CGameCameras.PlayersHeadCamera.camera;
		}
		else
		{
			if(m_ShipIndicator.gameObject.activeSelf)
				m_ShipIndicator.gameObject.SetActive(false);
		}
	}

	private void UpdateUI()
	{
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
	}

	private void UpdateVisorTransform(Vector3 _Position, Quaternion _Rotation)
	{
		transform.position = _Position;
		transform.rotation = _Rotation;
	}

	private IEnumerator InterpolateVisorDown()
	{
		bool lerping = true;
		while(lerping)
		{
			m_TransitionValue += Time.deltaTime;
			if(m_TransitionValue > m_TransitionTime)
			{
				m_TransitionValue = m_TransitionTime;
				lerping = false;

				// Turn on visor UI
				m_VisorUIActive = true;
			}
			
			float lerpValue = m_TransitionValue/m_TransitionTime;
			
			UpdateVisorTransform(Vector3.Lerp(m_VisorUpTrans.position, m_VisorDownTrans.position, lerpValue),
			                     Quaternion.Slerp(m_VisorUpTrans.rotation, m_VisorDownTrans.rotation, lerpValue));
			
			yield return null;
		}
	}

	private IEnumerator InterpolateVisorUp()
	{
		bool lerping = true;
		while(lerping)
		{
			m_TransitionValue -= Time.deltaTime;
			if(m_TransitionValue < 0.0f)
			{
				m_TransitionValue = 0.0f;
				lerping = false;

				// Turn off visor
				m_VisorUIActive = false;
			}
			
			float lerpValue = m_TransitionValue/m_TransitionTime;
			
			UpdateVisorTransform(Vector3.Lerp(m_VisorUpTrans.position, m_VisorDownTrans.position, lerpValue),
			                     Quaternion.Slerp(m_VisorUpTrans.rotation, m_VisorDownTrans.rotation, lerpValue));
			
			yield return null;
		}
	}
}
