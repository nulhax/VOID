//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomFactoryToolConveyor.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Implementation */
public class CFactoryToolConveyor : CNetworkMonoBehaviour
{
	// Member Types
	
	// Member Delegates & Events
	
	// Member Properties
	
	// Member Functions
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar){}
	
	public void Start(){}
	public void OnDestroy(){}
	public void Update(){}
	void OnNetworkVarSync(INetworkVar _cVarInstance){}
	
	// Member Fields
	CNetworkVar<float> m_fPowerConsumption;
};