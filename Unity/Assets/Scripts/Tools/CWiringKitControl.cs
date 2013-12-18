//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CWiringKitControl.cs
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


public class CWiringKitControl : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public void Start()
	{
		GetComponent<CToolInterface>().EventPrimaryActivate += new CToolInterface.NotifyPrimaryActivate(OnRepair);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		// Empty
	}


	[AServerMethod]
	public void OnRepair(GameObject _cInteractableObject)
	{
		if (_cInteractableObject != null &&
		    _cInteractableObject.GetComponent<CFuseBoxControl>() != null)
		{
			if (_cInteractableObject.GetComponent<CFuseBoxControl>().IsBroken)
			{
				_cInteractableObject.GetComponent<CFuseBoxControl>().Fix();
			}
		}
	}


// Member Fields


};
