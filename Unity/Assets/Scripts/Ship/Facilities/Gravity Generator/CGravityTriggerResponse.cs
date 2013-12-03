//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGravityTrigger.cs
//  Description :   Class script for the gravity trigger
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;

// Implementation
public class CGravityTriggerResponse : CNetworkMonoBehaviour
{
	// Member Data
	CNetworkVar<float> m_fTriggerRadius;
	SphereCollider m_Trigger;
	
	// Member Properties
	float TriggerRadius { get { return (m_fTriggerRadius.Get()); } set { TriggerRadius = value; } }
	
	// Member Functions
	public override void InstanceNetworkVars()
	{
		m_fTriggerRadius = new CNetworkVar<float>(OnNetworkVarSync);
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_fTriggerRadius)
		{
			// Update scale (radius) using trigger radius
			m_Trigger.radius = TriggerRadius;
		}
	}
	
	[AServerMethod]
	void UpdateTriggerRadius()
	{
		// Calculate radius
		m_fTriggerRadius.Set(m_fTriggerRadius.Get() + 1.0f);
	}	
	
	void Update()
	{
		
	}
	
	void Start()
	{
		m_Trigger = transform.GetComponent<SphereCollider>();
	}
}