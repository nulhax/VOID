//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModuleEngineBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CSmallEngineAnim : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public Transform m_OuterRing = null;
	public Transform m_MiddleRing = null;
	public Transform m_InnerRing = null;

	public float m_AverageSpeed = 180.0f; // Deg per second

	private float m_CurrentSpeed = 0.0f;

	private float m_VarianceTimer1 = 0.0f;
	private float m_VarianceTimer2 = 0.0f;
	private float m_VarianceTimer3 = 0.0f;


	// Member Properties
	
	
	// Member Methods
	void Start()
	{
		m_CurrentSpeed = m_AverageSpeed;
	}
	
	
	void Update()
	{
		m_VarianceTimer1 += Time.deltaTime * m_CurrentSpeed * Mathf.Deg2Rad * 0.75f;
		m_VarianceTimer2 += Time.deltaTime * m_CurrentSpeed * Mathf.Deg2Rad * 0.5f;
		m_VarianceTimer3 += Time.deltaTime * m_CurrentSpeed * Mathf.Deg2Rad * 0.25f;

		// Outer ring
		float angle = m_CurrentSpeed * 0.5f * Time.deltaTime;
		Vector3 axis = new Vector3(Mathf.Sin(m_VarianceTimer3), Mathf.Cos(m_VarianceTimer3), 0.0f).normalized;
		m_OuterRing.transform.Rotate(axis, angle);

		// Middle ring
		angle = m_CurrentSpeed * Time.deltaTime;
		axis = new Vector3(Mathf.Cos(m_VarianceTimer2), Mathf.Sin(m_VarianceTimer2), 0.0f).normalized;
		m_MiddleRing.transform.Rotate(axis, angle);

		// Inner ring
		angle = m_CurrentSpeed * 2.0f * Time.deltaTime;
		axis = new Vector3(Mathf.Sin(m_VarianceTimer1), -Mathf.Cos(m_VarianceTimer1), 0.0f).normalized;
		m_InnerRing.transform.Rotate(axis, angle);
	}
};