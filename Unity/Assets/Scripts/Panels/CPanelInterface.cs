//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPanelInterface.cs
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


public class CPanelInterface : MonoBehaviour
{

// Member Types


	public enum EType
	{
		INVALID,
		FuseBox,
		AirConditioning
	}


// Member Delegates & Events


// Member Properties


	public EType PanelType
	{
		get { return (m_eType); }
	}


// Member Functions


	public void Awake()
	{
		gameObject.AddComponent<CInteractableObject>();
		gameObject.AddComponent<CNetworkView>();
	}


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}


// Member Fields


	public EType m_eType = EType.INVALID;


};
