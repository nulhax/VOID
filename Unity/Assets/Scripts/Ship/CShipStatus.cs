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


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
	{

	}

	void OnGUI()
	{
        return;
		float shipSpeed = CGameShips.GalaxyShip.rigidbody.velocity.magnitude;
		Vector3 absShipPos = CGalaxy.instance.RelativePointToAbsolutePoint(CGameShips.GalaxyShip.transform.position);
		CGalaxy.SCellPos shipCellPos = CGalaxy.instance.RelativePointToAbsoluteCell(CGameShips.GalaxyShip.transform.position);

		string shipOutput = "";
		shipOutput += string.Format("\tShipSpeed: [{0}] CurrentCell [{1},{2},{3}] ShipAbsPos [{4}] ",
									shipSpeed.ToString("F2"),
									shipCellPos.x, shipCellPos.y, shipCellPos.z,
		                            absShipPos.ToString("F2")); 

		float boxWidth = 700;
		float boxHeight = 40;
		GUI.Label(new Rect(Screen.width / 2 - boxWidth, Screen.height - boxHeight, boxWidth, boxHeight),
		          "ShipMiscInfo\n" + shipOutput);
	}


// Member Fields


};