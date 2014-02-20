//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CActorHealth.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/* Implementation */

public class CPlayerHealth : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


	public delegate void OnPlayerApplyHeal(GameObject _TargetPlayer, float _fHealAmount);
	public delegate void OnPlayerApplyDamage(GameObject _TargetPlayer, float _fDamageAmount);
	public delegate void OnPlayerDeath(GameObject _SourcePlayer);
	public delegate void OnPlayerRevive(GameObject _SourcePlayer);
	

	public event OnPlayerApplyHeal   EventApplyHeal;
	public event OnPlayerApplyDamage EventApplyDamage;
	public event OnPlayerDeath       EventDeath;
	public event OnPlayerRevive      EventRevive;


// Member Properties


	public float HitPoints
	{ 
		get { return (m_fHitPoints.Get()); }
		set { m_fHitPoints.Set(value); }
	}
	

	public bool Alive
	{
		get { return (m_bAlive.Get()); }
		set { m_bAlive.Set(value); }
	}
	

	public float Breath
	{
		get { return(m_fOxygenUseRate.Get()); }
		set { m_fOxygenUseRate.Set(value); }
	}


    public static CPlayerHealth Instance
    {
        get { return (s_cInstance); }
    }

// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fHitPoints = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, k_fMaxHealth);
		m_bAlive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
		m_fOxygenUseRate = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 5.0f);
	}


    [AServerOnly]
    public void ApplyDamage(float _fAmount)
    {
        // If the player's current health, minus the new damage is still greater than 0.0f
        if ((m_fHitPoints.Get() - _fAmount) > 0.0f)
        {
            // Apply the damage normally
            m_fHitPoints.Set(m_fHitPoints.Get() - _fAmount);
        }

        // Else the player's current health, minus the new damage, is 0.0f or negative
        else
        {
            // Set the player's helath to a flat 0.0f
            m_fHitPoints.Set(0.0f);
        }

        // Notify observers about dmagae
        if (EventApplyDamage != null) EventApplyDamage(gameObject, _fAmount);
    }


    [AServerOnly]
    public void ApplyHeal(float _fAmount)
    {
        if((m_fHitPoints.Get() + _fAmount) > k_fMaxHealth)
        {
			m_fHitPoints.Set(k_fMaxHealth);
        }
		else
		{
			m_fHitPoints.Set(m_fHitPoints.Get() + _fAmount);
		}

        // Notify observers about heal
        if (EventApplyHeal != null) EventApplyHeal(gameObject, _fAmount);
    }


    public void Awake()
    {
        s_cInstance = this;
    }


	void Start() 
    {
        // Death audio
		CAudioCue[] audioCues = gameObject.GetComponents<CAudioCue>();

		foreach(CAudioCue cue in audioCues)
		{
			if(cue.m_strCueName == "LaughTrack")
			{
				//m_LaughTrack = 	cue;
			}
		}
	}
		 

    void OnDestroy()
    { 
        // Empty
    }


	void Update() 
	{
        if (CNetwork.IsServer)
        {
            UpdateAtmosphereEffects();
            UpdatePlayerAlive();
        }

		if (CNetwork.IsServer) 
        {
			if (Input.GetKeyDown (KeyCode.Q)) 
            {
				if (HitPoints > 0) 
                {
					ApplyDamage (100);
				} 
                else 
                {
					ApplyHeal (100);	
				}
			}
		}
	}


    [AServerOnly]
    void UpdateAtmosphereEffects()
    {
    }


    [AServerOnly]
	void UpdatePlayerAlive()
	{
        if (Alive)
        {
            // Check player is now dead
            if (HitPoints == 0.0f)
            {
                m_bAlive.Set(false);

                // Notify observers
                if (EventDeath != null) EventDeath(gameObject);
            }
        }
        else
        {
            // Check player is now alive
            if (HitPoints > 0.0f)
            {
                m_bAlive.Set(true);

                // Notify observers
                if (EventRevive != null) EventRevive(gameObject);
            }
        }
	}


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_bAlive)
        {
            // Player is alive
            if (m_bAlive.Get())
            {
                transform.GetComponent<CPlayerGroundMotor>().ReenableInput(this);
                transform.GetComponent<CPlayerHead>().ReenableInput(this);
            }

            // Player is dead
            else
            {
                transform.GetComponent<CPlayerGroundMotor>().DisableInput(this);
                transform.GetComponent<CPlayerHead>().DisableInput(this);
            }
        }
        else  if (_cVarInstance == m_fHitPoints &&
                  m_LaughTrack != null)
        {
            m_LaughTrack.Play(0.5f, false, -1);

            if (CNetwork.IsServer)
            {
                if (m_fHitPoints.Get() <= 0.0f)
                {
                    m_bAlive.Set(false);
                }
                else if (m_fHitPoints.Get() > 0.0f)
                {
                    m_bAlive.Set(true);
                }
            }
        }
    }


    void OnGUI()
    {
        const float kBoxMargin = 10.0f;
        const float kBoxWidth = 200.0f;
        const float kBoxHeight = 22.0f;

        
        if (gameObject == CGamePlayers.SelfActor)
        {
            GUIStyle cStyle = new GUIStyle();
            cStyle.alignment = TextAnchor.UpperLeft;
            cStyle.fontStyle = FontStyle.Bold;

            // Hit points
            GUI.Box(new Rect(Screen.width  - kBoxWidth - kBoxMargin,
                             Screen.height - kBoxHeight - kBoxMargin,
                             kBoxWidth, kBoxHeight),
                             "Health: " + Math.Round(m_fHitPoints.Get(), 2) + "/" + k_fMaxHealth);
		}
    }
	

// Member Fields


	public float k_fMaxHealth = 100.0f;


	CNetworkVar<float> m_fHitPoints;
	CNetworkVar<float> m_fOxygenUseRate;
	CNetworkVar<bool> m_bAlive;


	CAudioCue m_LaughTrack;
    static CPlayerHealth s_cInstance = null;

}
	