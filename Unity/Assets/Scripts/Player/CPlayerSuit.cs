//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerSuit.cs
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


[RequireComponent(typeof(CActorPowerConsumer))]
public class CPlayerSuit : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events
	
    public delegate void NotifyEnviormentChange(bool _Breathable);
	
	public event NotifyEnviormentChange EventEnviromentalOxygenChange;


// Member Fields
	
	public CHUDVisor m_Visor = null;


	private bool m_VisorDownState = false;
	private CActorAtmosphericConsumer m_AtmosphereConsumer = null;
	
	
	private float k_fOxygenCapacity = 600.0f;
	private float k_fOxygenDepleteRate = 1.0f;
	private float k_fOxygenRefillRate = 10.0f;


	private CHUDVisor m_CachedVisor = null;

	
	CNetworkVar<float> m_fOxygen = null;
	CNetworkVar<bool>  m_EnviromentalOxygen = null;



// Member Properties


    public float OxygenSupply
    {
        get { return (m_fOxygen.Value); }
    }


	public float OxygenCapacity
	{
		get { return (k_fOxygenCapacity); }
	}


    public bool IsOxygenAbsent
    {
		get { return (!m_EnviromentalOxygen.Value); }
    }


// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fOxygen = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, k_fOxygenCapacity);
        m_EnviromentalOxygen = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
	}


	void Start()
	{
		m_AtmosphereConsumer = GetComponent<CActorAtmosphericConsumer>();

		if(CGamePlayers.SelfActor == gameObject)
			EventEnviromentalOxygenChange += OnEnviromentOxygenChange;

		CUserInput.SubscribeInputChange(CUserInput.EInput.Visor, OnEventInput);

		m_CachedVisor = CHUD3D.Visor;
	}


	void OnDestroy()
	{
		// Empty
	}

	void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
	{
		if(_eInput == CUserInput.EInput.Visor && _bDown)
		{
			// Toggle between up/down visor
			m_CachedVisor.SetVisorState(!CHUD3D.Visor.IsVisorDown);
		}
	}

	void Update()
	{
        if (CNetwork.IsServer)
        {
			GameObject currentFacility = gameObject.GetComponent<CActorLocator>().LastEnteredFacility;

            // Control visor state
			if (currentFacility == null)
            {
                m_EnviromentalOxygen.Set(false);
            }
            else
            {
				if (currentFacility.GetComponent<CFacilityAtmosphere>().AtmospherePercentage < 25.0f)
                {
					m_AtmosphereConsumer.SetAtmosphereConsumption(false);
                    m_EnviromentalOxygen.Set(false);
                }
                else
                {
					// Set the origninal consumption rate
					m_AtmosphereConsumer.AtmosphericConsumptionRate = m_AtmosphereConsumer.InitialAtmosphericConsumptionRate;

					m_AtmosphereConsumer.SetAtmosphereConsumption(true);
                    m_EnviromentalOxygen.Set(true);
                }
            }

			if(!m_EnviromentalOxygen.Value)
            {
                // Consume oxygen
                float fOxygen = OxygenSupply - k_fOxygenDepleteRate * Time.deltaTime;

				if(!m_CachedVisor.IsVisorDown)
					fOxygen = OxygenSupply;

                if (fOxygen < 0.0f)
                {
                    fOxygen = 0.0f;
                }
                else if (fOxygen > k_fOxygenCapacity)
                {
                    fOxygen = k_fOxygenCapacity;
                }

                m_fOxygen.Set(fOxygen);

				if(fOxygen == 0.0f)
				{
					GetComponent<CPlayerHealth>().ApplyDamage(10.0f * Time.deltaTime);
				}
				else if(!CHUD3D.Visor.IsVisorDown)
				{
					GetComponent<CPlayerHealth>().ApplyDamage(500.0f * Time.deltaTime);
				}
            }
            else
            {
                // Refill oxygen
                if (OxygenSupply != k_fOxygenCapacity)
                {
					// Set the updated consumption rate
					m_AtmosphereConsumer.AtmosphericConsumptionRate = k_fOxygenRefillRate;

					// Add to current oxygen to suit
					m_fOxygen.Set(OxygenSupply + k_fOxygenRefillRate * Time.deltaTime);

                    if (OxygenSupply > k_fOxygenCapacity)
                    {
                        m_fOxygen.Set(k_fOxygenCapacity);
                    }
                }
            }
        }
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_EnviromentalOxygen)
        {
            if (EventEnviromentalOxygenChange != null) 
				EventEnviromentalOxygenChange(m_EnviromentalOxygen.Value);
        }
    }

	[AClientOnly]
	void OnEnviromentOxygenChange(bool _Breathable)
	{
		if(!_Breathable)
		{
			// Cache the last visor state
			m_VisorDownState = m_CachedVisor.IsVisorDown;
			m_CachedVisor.SetVisorState(true);
		}
		else
		{
			m_CachedVisor.SetVisorState(m_VisorDownState);
		}
	}


//    void OnGUI()
//    {
//        if (gameObject == CGamePlayers.SelfActor)
//        {
////            if (IsVisorDown)
////            {
////				if(!CGameCameras.IsOculusRiftActive)
////               		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_cVisorTexture, ScaleMode.StretchToFill, true);
////				else
////					GUI.DrawTexture(new Rect(0, 0, Screen.width * 0.5f, Screen.height), m_cVisorTexture, ScaleMode.StretchToFill, true);
////					GUI.DrawTexture(new Rect(Screen.width * 0.5f, 0, Screen.width * 0.5f, Screen.height), m_cVisorTexture, ScaleMode.StretchToFill, true);
////            }
//
//            // Hit points
//            GUI.Box(new Rect(Screen.width - 160,
//                             Screen.height - 100,
//                             150.0f, 56.0f),
//                             "[Suit]\n" +
//                             "Visor Down: " + (IsVisorDown ? "True" : "False") + "\n" +
//                             "Oxygen Supply: " + Math.Round(OxygenSupply, 1).ToString());
//
//            GUIStyle cStyle = new GUIStyle();
//            cStyle.fontSize = 40;
//
//            if (OxygenSupply == 0.0f)
//            {
//                cStyle.normal.textColor = Color.red;
//
//                GUI.Label(new Rect(Screen.width / 2 - 290, Screen.height - 100, 576, 100),
//                          "CRITICAL: OXYGEN DEPLETED!", cStyle);
//            }
//            else if (OxygenSupply < k_fOxygenCapacity * 0.20f)
//            {
//                cStyle.normal.textColor = new Color(1.0f, 1.0f / 156.0f, 0.0f);
//
//                GUI.Label(new Rect(Screen.width / 2 - 270, Screen.height - 100, 576, 100),
//                          "Critical: Low Oxygen!", cStyle);
//            }
//            else if (OxygenSupply < k_fOxygenCapacity * 0.40f)
//            {
//                cStyle.normal.textColor = Color.yellow;
//
//                GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height - 100, 576, 100),
//                          "Warning: Low Oxygen", cStyle);
//            }
//        }
//    }

    [AServerOnly]
    public void AddOxygen(float _OxygenAmount)
    {
        m_fOxygen.Set(_OxygenAmount + m_fOxygen.Get());
    }
};
