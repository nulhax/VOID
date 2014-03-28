//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerRegen.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;

public class CPlayerRegen : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(gameObject.GetComponent<CPlayerHealth>().Health != 100.0f)
		{
			// Heal the player
			gameObject.GetComponent<CPlayerHealth>().ApplyHeal((0.01f * Time.deltaTime) / 100.0f);
		}
		else
		{
			// Do nothing
		}
	}	
}
