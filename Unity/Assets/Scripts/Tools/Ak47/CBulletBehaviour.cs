//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CBulletBehaviour.cs
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


public class CBulletBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public void Start()
	{
		GameObject.Destroy(gameObject, 3.0f);
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}


	void OnCollisionEnter(Collision _cCollision)
	{
		if (_cCollision.gameObject.GetComponent<CToolInterface>() == null)
		{
			if (_cCollision.gameObject.GetComponent<CPlayerHealth>() != null)
			{
				if (CNetwork.IsServer)
				{
					_cCollision.gameObject.GetComponent<CPlayerHealth>().ApplyDamage(5.0f);
				}
			}


			//GameObject.Destroy(gameObject);
		}
	}


// Member Fields


};
