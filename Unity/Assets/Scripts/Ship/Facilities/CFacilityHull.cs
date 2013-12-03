
//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityHull.cs
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


public class CFacilityHull : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events
	
	public delegate void NotifyBreached();
	public event NotifyBreached EventBreached;

// Member Properties
	
	public bool IsBreached
	{
		get { return(true); }
	}
	 
	
	public override void InstanceNetworkVars()
    {

    }
// Member Methods


	public void Start()
	{
		if(EventBreached != null)
		{
			EventBreached();
		}
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
    }


// Member Fields


};
