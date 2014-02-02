//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CScannerBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CScannerBehaviour : CNetworkMonoBehaviour
{
		
// Member Types


// Member Delegates & Events


// Member Properties
	
	
// Member Functions
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}
	
// Member Fields
	CNetworkVar<float> m_fRadius;
}
