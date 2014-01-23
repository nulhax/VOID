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


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_fHitPoints = new CNetworkVar<float>(OnNetworkVarSync, k_fMaxHealth);
		m_bAlive = new CNetworkVar<bool>(OnNetworkVarSync, true);
		m_fOxygenUseRate = new CNetworkVar<float>(OnNetworkVarSync, 5.0f);
	}


    [AServerOnly]
    public void ApplyDamage(float _fAmount)
    {
        m_fHitPoints.Set(m_fHitPoints.Get() - _fAmount);

        // Notify observers about dmagae
        if (EventApplyDamage != null) EventApplyDamage(gameObject, _fAmount);
    }


    [AServerOnly]
    public void ApplyHeal(float _fAmount)
    {
        m_fHitPoints.Set(m_fHitPoints.Get() + _fAmount);

        // Notify observers about heal
        if (EventApplyHeal != null) EventApplyHeal(gameObject, _fAmount);
    }

	
	void Start() 
    {
        // Death audio
		AudioCue[] audioCues = gameObject.GetComponents<AudioCue>();

		foreach(AudioCue cue in audioCues)
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
            if (HitPoints < 0.0f)
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
                transform.GetComponent<CPlayerGroundMotor>().UndisableInput(this);
                transform.GetComponent<CPlayerHead>().UndisableInput(this);
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


	const float k_fMaxHealth = 1000.0f;


	CNetworkVar<float> m_fHitPoints;
	CNetworkVar<float> m_fOxygenUseRate;
	CNetworkVar<bool> m_bAlive;


	AudioCue m_LaughTrack;


}
	