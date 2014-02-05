//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CMiningCockpitBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}


	void Start()
	{
        GetComponent<CActorInteractable>().EventHover += OnHover;
	}


	void OnDestroy()
	{
        GetComponent<CActorInteractable>().EventHover -= OnHover;
	}


	void Update()
	{
	}

    // TEMPORARY //
    //
    // Hover text logic that needs revision. OnGUI + Copy/Paste code = Terribad
    //
    // TEMPORARY //
    bool bShowName = false;
    bool bOnGUIHit = false;
    void OnHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
    {
        bShowName = true;
    }


    public void OnGUI()
    {
        float fScreenCenterX = Screen.width / 2;
        float fScreenCenterY = Screen.height / 2;
        float fWidth = 100.0f;
        float fHeight = 20.0f;
        float fOriginX = fScreenCenterX + 25.0f;
        float fOriginY = fScreenCenterY - 10.0f;

        if (bShowName && !bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Mining Cockpit");
            bOnGUIHit = true;
        }
        else if (bShowName && bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Mining Cockpit");
            bShowName = false;
            bOnGUIHit = false;
        }
    }
    // TEMPORARY //
    //
    // 
    //
    // TEMPORARY //


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
	}


// Member Fields


};
