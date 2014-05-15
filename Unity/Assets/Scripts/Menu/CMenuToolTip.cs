//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CMenuToolTip.cs
//  Description :   Allows buttons to show tool tips next to them on hover.
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;

public class CMenuToolTip : MonoBehaviour {

	public UILabel Label;

	// Use this for initialization
	void Start () 
	{
		OnHover(false);
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	void OnHover(bool _bool)
	{

		if(_bool)
		{
			Label.enabled = true;
		}
		else
		{
			Label.enabled = false;
		}
	}


}
