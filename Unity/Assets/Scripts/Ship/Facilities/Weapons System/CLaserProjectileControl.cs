//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLaserProjectileControl.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CLaserProjectileControl : MonoBehaviour
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
		transform.position = transform.position + transform.forward * 40.0f * Time.deltaTime;
	}


// Member Fields


};
