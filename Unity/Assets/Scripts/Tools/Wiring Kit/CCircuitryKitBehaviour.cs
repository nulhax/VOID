//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CWiringKitBehaviour.cs
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

[RequireComponent(typeof(CToolInterface))]
public class CCircuitryKitBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public void Start()
	{
		GetComponent<CToolInterface>().EventPrimaryActivate += OnRepair;
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		// Empty
	}


	[AServerOnly]
	public void OnRepair(GameObject _cInteractableObject)
	{
		if (_cInteractableObject != null &&
		    _cInteractableObject.GetComponent<CFuseBoxBehaviour>() != null)
		{
			if (_cInteractableObject.GetComponent<CFuseBoxBehaviour>().IsBroken)
			{
				_cInteractableObject.GetComponent<CFuseBoxBehaviour>().Fix();
			}
		}
	}


// Member Fields


};
