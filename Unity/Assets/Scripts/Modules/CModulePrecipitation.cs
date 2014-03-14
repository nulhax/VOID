//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModulePrecipitation.cs
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


public class CModulePrecipitation : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public float m_BuildTime = 5.0f;

	private float m_Timer = 0.0f;


	// Member Properties
	
	
	// Member Methods
	void Start()
	{

	}
	
	void Update()
	{
		if(m_Timer != m_BuildTime)
		{
			m_Timer += Time.deltaTime;
			m_Timer = Mathf.Clamp(m_Timer, 0.0f, m_BuildTime);

			renderer.material.SetFloat("_Amount", m_Timer/m_BuildTime);
		}
	}
};
