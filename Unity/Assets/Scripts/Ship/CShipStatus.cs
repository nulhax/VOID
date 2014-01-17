//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipStatus.cs
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


public class CShipStatus : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


// Member Properties


    public override void InstanceNetworkVars()
    {
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
	{

	}

	void OnGUI()
	{
		float shipSpeed = CGameShips.GalaxyShip.rigidbody.velocity.magnitude;
		Vector3 absShipPos = CGalaxy.instance.AbsoluteCellToAbsolutePoint(CGalaxy.instance.centreCell) + CGameShips.GalaxyShip.transform.position;

		string shipOutput = "";
		shipOutput += string.Format("\tShipSpeed: [{0}] CurrentCell [{1},{2},{3}] ShipAbsPos [{4}] ", 
		                            Math.Round(shipSpeed, 2),
		                            CGalaxy.instance.centreCell.x, CGalaxy.instance.centreCell.y, CGalaxy.instance.centreCell.z,
		                            absShipPos.ToString()); 

		float boxWidth = 700;
		float boxHeight = 40;
		GUI.Label(new Rect(Screen.width / 2 - boxWidth, Screen.height - boxHeight, boxWidth, boxHeight),
		          "ShipMiscInfo\n" + shipOutput);
	}


// Member Fields


};