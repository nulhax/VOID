//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CGalaxyShipShield : MonoBehaviour 
{
	// Member Types
	public enum EShieldState
	{
		INVALID,
		
		PoweredUp,
		PoweredDown,
		Reacting,
		Charging,
		
		MAX
	}
	
	// Member Fields
	public GameObject m_Shield = null;
	
	private float m_ShieldPower;
	private float m_MaxShieldPower;
	
	private bool m_Active = true;
	private EShieldState m_ShieldState = EShieldState.PoweredDown;
	
	// Member Properies
	
	
	// Member Methods
	public void UpdateShieldBounds(Bounds _ShieldBounds)
	{	
		Vector3 scale = Vector3.zero;
		scale.x = _ShieldBounds.size.x / Mathf.Sqrt(2.0f) * 2.0f;
		scale.z = _ShieldBounds.size.z / Mathf.Sqrt(2.0f) * 2.0f;
		scale.y = _ShieldBounds.size.y * 2.0f;
		
		// Set as the scale of the object and increase the scale a little
		m_Shield.transform.localScale = scale * 1.1f;
		m_Shield.transform.localPosition = _ShieldBounds.center;
	}
}
