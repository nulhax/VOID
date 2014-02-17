//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIUtilites.cs
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


public class CDUIUtilites 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	
	
	// Member Properties

	
	// Member Methods
	public static Color LerpColor(float _Value)
	{
		Color fromColor = _Value > 0.5f ? Color.yellow : Color.red;
		Color toColor = _Value > 0.5f ? Color.cyan : Color.yellow;
		float value = _Value > 0.5f ? (_Value - 0.5f) / 0.5f : _Value / 0.5f;
		return(Color.Lerp(fromColor, toColor, value));
	}

	public static void LerpBarColor(float _Value, UIProgressBar _Bar)
	{
		_Bar.backgroundWidget.color = LerpColor(_Value) * 0.8f;
		_Bar.foregroundWidget.color = LerpColor(_Value);
	}
}
