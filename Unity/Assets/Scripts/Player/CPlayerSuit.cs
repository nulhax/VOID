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


[RequireComponent(typeof(CModulePower))]
public class CPlayerSuit : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events
	
    public delegate void NotifyEnviormentChange(bool _Breathable);
	
	public event NotifyEnviormentChange EventEnviromentalOxygenChange;


// Member Fields
	private bool m_PreviousVisorDownState = false;
	private CActorAtmosphericConsumer m_AtmosphereConsumer = null;
	
	
	private float k_fOxygenCapacity = 60.0f;
	private float k_fOxygenDepleteRate = 1.0f;
	private float k_fOxygenRefillRate = 10.0f;

	
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


	public static float AirDensityLimit
	{
		get { return(0.3f); }
	}


// Member Functions


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_fOxygen = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, k_fOxygenCapacity);
        m_EnviromentalOxygen = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, true);
	}


	void Start()
	{
		m_AtmosphereConsumer = GetComponent<CActorAtmosphericConsumer>();

		if(CGamePlayers.SelfActor == gameObject)
		{
			EventEnviromentalOxygenChange += OnEnviromentOxygenChange;
		}
	}


	void OnDestroy()
	{
		// Empty
	}

	void Update()
	{
        if(CNetwork.IsServer)
        {
			GameObject currentFacility = gameObject.GetComponent<CActorLocator>().CurrentFacility;

            // Check enviromental oxygen state
			if (currentFacility == null)
            {
                m_EnviromentalOxygen.Set(false);
            }
            else
            {
				if(currentFacility.GetComponent<CFacilityAtmosphere>().Density < AirDensityLimit)
                {
					m_AtmosphereConsumer.SetAtmosphereConsumption(false);
                    m_EnviromentalOxygen.Set(false);
                }
                else
                {
					m_AtmosphereConsumer.SetAtmosphereConsumption(true);
                    m_EnviromentalOxygen.Set(true);
                }
            }

			// If there is no oxygen in the atmosphere
			if(!m_EnviromentalOxygen.Value)
            {
                // Consume oxygen from the suit supply
                float fOxygen = OxygenSupply - k_fOxygenDepleteRate * Time.deltaTime;

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

                    if(OxygenSupply > k_fOxygenCapacity)
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

	[ALocalOnly]
	void OnEnviromentOxygenChange(bool _Breathable)
	{
		CGameHUD.Visor.SetVisorState(!_Breathable);
	}


//    void OnGUI()
//    {
//        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
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
