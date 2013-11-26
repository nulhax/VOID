//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNetworkInterpolatedObject.cs
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


public class CNetworkInterpolatedObject : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
		m_fInterpolationTimer += Time.deltaTime;


		if (m_fInterpolationTimer >= 1.0f)
		{
			m_fInterpolationTimer -= 1.0f;
		}


		int iHistoryElement = (int)((float)CNetworkConnection.k_uiOutboundRate * m_fInterpolationTimer);
		m_vaPositions[iHistoryElement] = transform.position;
		m_vaRotations[iHistoryElement] = transform.rotation;
	}


// Member Fields


	float m_fInterpolationTimer = 0.0f;


	Vector3[] m_vaPositions = new Vector3[CNetworkConnection.k_uiOutboundRate];
	Quaternion[] m_vaRotations = new Quaternion[CNetworkConnection.k_uiOutboundRate];


};
