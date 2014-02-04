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


public class CPlayerSuit : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void NotifyVisorChange(CPlayerSuit _cPlayerSuit, bool _bDown);
    public event NotifyVisorChange EventVisorChange;


// Member Properties


    public float OxygenSupply
    {
        get { return (m_fOxygen.Get()); }
    }


    public bool IsVisorDown
    {
        get { return (m_bVisorDown.Get()); }
    }


// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fOxygen = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, k_fOxygenCapacity);
        m_bVisorDown = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}


	void Start()
	{
        m_cVisorTexture = Resources.Load("Textures/GUI/CrackedVisor", typeof(Texture2D)) as Texture2D;
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
        if (CNetwork.IsServer)
        {
            GameObject cFacilityObject = GetComponent<CPlayerLocator>().ContainingFacility;

            // Control visor state
            if (cFacilityObject == null)
            {
                m_bVisorDown.Set(true);
            }
            else
            {
                if (cFacilityObject.GetComponent<CFacilityAtmosphere>().AtmospherePercentage < 25.0f)
                {
                    m_bVisorDown.Set(true);
                }
                else
                {
                    m_bVisorDown.Set(false);
                }
            }

            if (IsVisorDown)
            {
                // Consume oxygen
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

                if (fOxygen == 0.0f)
                {
                    GetComponent<CPlayerHealth>().ApplyDamage(10.0f * Time.deltaTime);
                }
            }
            else
            {
                // Refill oxygen
                if (OxygenSupply != k_fOxygenCapacity)
                {
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
        if (_cSyncedVar == m_bVisorDown)
        {
            if (m_bVisorDown.Get())
            {
                
            }
            else
            {

            }

            if (EventVisorChange != null) EventVisorChange(this, m_bVisorDown.Get());
        }
    }


    void OnGUI()
    {
        if (gameObject == CGamePlayers.SelfActor)
        {
            if (IsVisorDown)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_cVisorTexture, ScaleMode.StretchToFill, true);
            }

            // Hit points
            GUI.Box(new Rect(Screen.width - 160,
                             Screen.height - 100,
                             150.0f, 56.0f),
                             "[Suit]\n" +
                             "Visor Down: " + (IsVisorDown ? "True" : "False") + "\n" +
                             "Oxygen Supply: " + Math.Round(OxygenSupply, 1).ToString());

            GUIStyle cStyle = new GUIStyle();
            cStyle.fontSize = 40;

            if (OxygenSupply == 0.0f)
            {
                cStyle.normal.textColor = Color.red;

                GUI.Label(new Rect(Screen.width / 2 - 290, Screen.height - 100, 576, 100),
                          "CRITICAL: OXYGEN DEPLETED!", cStyle);
            }
            else if (OxygenSupply < k_fOxygenCapacity * 0.20f)
            {
                cStyle.normal.textColor = new Color(1.0f, 1.0f / 156.0f, 0.0f);

                GUI.Label(new Rect(Screen.width / 2 - 270, Screen.height - 100, 576, 100),
                          "Critical: Low Oxygen!", cStyle);
            }
            else if (OxygenSupply < k_fOxygenCapacity * 0.40f)
            {
                cStyle.normal.textColor = Color.yellow;

                GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height - 100, 576, 100),
                          "Warning: Low Oxygen", cStyle);
            }
        }
    }

    [AServerOnly]
    public void AddOxygen(float _OxygenAmount)
    {
        m_fOxygen.Set(_OxygenAmount + m_fOxygen.Get());
    }


// Member Fields


    const float k_fOxygenCapacity = 60.0f;
    const float k_fOxygenDepleteRate = 1.0f;
    const float k_fOxygenRefillRate = 5.0f;


    CNetworkVar<float> m_fOxygen = null;
    CNetworkVar<bool>  m_bVisorDown = null;


    Texture2D m_cVisorTexture = null;
};
