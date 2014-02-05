//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLiquidComponent.cs
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
public class CFluidComponent : CNetworkMonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Properties
	public List<Transform> RatchetRepairPosition
	{
		get { return(m_RepairPositions);}
	}


    public float CurrentHealth
    {
        get { return (m_CurrentHealth); }
    }

	
	// Member Methods
	// Do the functionality in the on break. This will start when the eventcomponentbreak is triggered
	void OnBreak(CComponentInterface _Sender)
	{
		// TODO: swap between fixed to broken
		
	}
	

	// Do the functionality in the onfix. This will start when the eventcomponentfix is triggered
	void OnFix(CComponentInterface _Sender)
	{
		//TODO swap between broken to fixed
		
	}
	

	void OnHealthChange(CComponentInterface _Sender, CActorHealth _SenderHealth)
	{
		m_CurrentHealth = _SenderHealth.health;
		m_PreviousHealth = _SenderHealth.health_previous;
		float maxHealth = _SenderHealth.health_initial;
		
		transform.FindChild("Model").renderer.material.color = Color.Lerp(Color.red, Color.green, m_CurrentHealth / maxHealth);	
	}
	

	void Start()
	{
		// Find all the children which are component transforms
		foreach(Transform child in transform)
		{
			if(child.tag == "ComponentTransform")
				m_RepairPositions.Add(child);
		}

        transform.FindChild("Model").renderer.material.color = Color.Lerp(Color.red, Color.green, GetComponent<CActorHealth>().health / GetComponent<CActorHealth>().health_initial);

		// Register events created in the inherited class CComponentInterface
		// This will call onbreak or onfix when the even is triggered.
		gameObject.GetComponent<CComponentInterface>().EventComponentBreak += OnBreak;
		gameObject.GetComponent<CComponentInterface>().EventComponentFix   += OnFix;
		gameObject.GetComponent<CComponentInterface>().EventHealthChange   += OnHealthChange;

       // GetComponent<CActorInteractable>().EventHover += OnHover;
	}
	

	void OnDestroy()
	{
        gameObject.GetComponent<CComponentInterface>().EventComponentBreak -= OnBreak;
        gameObject.GetComponent<CComponentInterface>().EventComponentFix   -= OnFix;
        gameObject.GetComponent<CComponentInterface>().EventHealthChange   -= OnHealthChange;

       // GetComponent<CActorInteractable>().EventHover -= OnHover;
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
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Fluid Component");
            bOnGUIHit = true;
        }
        else if (bShowName && bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Fluid Component");
            bShowName = false;
            bOnGUIHit = false;
        }
    }
    // TEMPORARY //
    //
    // 
    //
    // TEMPORARY //
	
	
	void Update()
	{
		
	}
	

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		
	}
	

	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	

	// Member Fields
	private List<Transform> m_RepairPositions = new List<Transform>();
	private float m_CurrentHealth = 0.0f;
	private float m_PreviousHealth = 0.0f;
	
	private bool m_IsLerping = false;
};
