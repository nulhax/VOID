//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CInteriorTrigger.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CExitTrigger : MonoBehaviour 
{
// Member Types


// Member Delegates & Events
	
		
// Member Fields
	
	
// Member Properties
	
	
// Member Methods


	void OnTriggerExit(Collider _cOther)
	{
		if(_cOther.rigidbody != null)
		{
			GameObject cActor = _cOther.gameObject;
			CActorBoardable cBoardableActor = cActor.GetComponent<CActorBoardable>();

			if(cBoardableActor != null)
			{
                Debug.Log(_cOther.rigidbody);

				// Ensure the actor is not onboard any other facility before disembarking
				// If not onboard within another facility, disembark the actor
                if (cActor.GetComponent<CActorLocator>().IsInShip)
				{
					// Set the disembarking state
					cBoardableActor.DisembarkActor();
				}
			}
		}
	}


}
