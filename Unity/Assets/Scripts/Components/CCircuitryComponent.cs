//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CWiringComponent.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


[RequireComponent(typeof(CComponentInterface))]
public class CCircuitryComponent : CNetworkMonoBehaviour
{

// Member Types
	
	
// Member Delegates & Events
	
	
// Member Properties


	public List<Transform> ComponentRepairPosition
	{
		get { return(m_RepairPositions);}
	}


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


	void StormDamage()
	{
		gameObject.GetComponent<CActorHealth>().health -= 0.4f;

		if (gameObject.GetComponent<CActorHealth> ().health > 1.0f) 
			Debug.Log ("Circuit Component Health: " + gameObject.GetComponent<CActorHealth> ().health);
	
	}


	void Start()
	{
		// Find all the children which are component transforms
		foreach (Transform child in transform)
		{
			if (child.tag == "ComponentTransform")
				m_RepairPositions.Add(child);
		}

		CGalaxy.instance.gameObject.GetComponent<CGalaxyStorm>().EventDamageComponent += OnEventStormDamage;
	}


	void OnDestroy()
	{
        if (CGalaxy.instance != null)
            CGalaxy.instance.gameObject.GetComponent<CGalaxyStorm>().EventDamageComponent -= OnEventStormDamage;
	}


	void Update()
	{
		// Empty
	}


    void OnEventStormDamage()
    {
        gameObject.GetComponent<CActorHealth>().health -= 0.01f;
        Debug.Log("Circuit Component Health: " + gameObject.GetComponent<CActorHealth>().health);
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		// Empty
	}

// Member Fields


	List<Transform> m_RepairPositions = new List<Transform>();
	bool m_IsLerping = false;


};
