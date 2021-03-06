﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CActorHealth.cs
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
public class CPlayerHealth : CNetworkMonoBehaviour
{
// Member Types
    // Damage Type
    [Flags]
    public enum HealthChangeSourceType
    {
        None         = 0,

        Fire         = 1,
        Physical     = 2,
        Electrical   = 4,
        Asphyxiation = 8,

        Heal = 16
    }

    // Health State
    public enum HealthState : byte
    {
        INVALID = 0,

        ALIVE,
        DOWNED,
        DEAD,

        MAX
    }


// Member Delegates & Events
    public delegate void EventHealthChanged     (GameObject _TargetPlayer, float _fHealthCurrentValue, float _fHealthPreviousValue);
	public delegate void EventHealthStateChange (GameObject _SourcePlayer, HealthState _eHealthCurrentState, HealthState _eHealthPreviousState);

    public event EventHealthChanged     m_EventHealthChanged;
    public event EventHealthStateChange m_EventHealthStateChanged;

	private int m_PlayerForceDamageSoundIndex = -1;
	private int m_PlayerForceDeathSoundIndex = -1;
	float m_PlayerForceDamageSoundIndex_Time = 0.0f;

// Member Properties
	public float Health
	{
        // Get
		get { return (m_fHealth.Get()); }

        // Set
        [AServerOnly]
        set
        {
            // Local variables
            float fNewHealthValue = value;                         // New health value
            float fPrevHealth     = m_fHealth.Get();               // Current health
            float fHealthDelta    = fNewHealthValue - fPrevHealth; // Delta: New - Old

            // NOTE:
            // If fHealthDelta is ZERO, health is UNCHANGED
            // If fHealthDelta is a POSITIVE number, health INCREASED
            // If fHealthDelta is a NEGATIVE number, health DECREASED

            // If health is not (max and being incremented) and
            // If health is not (min and being decremented) and
            // If health delta is not 0
            if (!( (fPrevHealth  == k_fMaxHealth) && (fHealthDelta > 0.0f) ) &&
                !( (fPrevHealth  == k_fMinHealth) && (fHealthDelta < 0.0f) ) &&
                 (fHealthDelta != 0.0f))
            {
                // If new health value is  between the min and max values
                if ( (fNewHealthValue > k_fMinHealth) && (fNewHealthValue < k_fMaxHealth) )
                {
                    // Assign new health value
                    m_fHealth.Set(fNewHealthValue);
                }

                // Else if new health value is less than or equal to zero
                else if (fNewHealthValue <= 0.0f)
                {
                    // Set health to min health
                    m_fHealth.Set(k_fMinHealth);
                }

                // Else if new health value is greater than or equal to max health
                else if (fNewHealthValue >= k_fMaxHealth)
                {
                    // Set health to max health
                    m_fHealth.Set(k_fMaxHealth);
                }

                // Trigger EventHealthChanged
                if (m_EventHealthChanged != null)
                {
                    m_EventHealthChanged(gameObject, m_fHealth.Get(), fPrevHealth);
                }
            }
        }
    }
	

	public HealthState CurrentHealthState
	{
        // Get
        get { return ((HealthState)m_HealthState.Get()); }

        // Set
        [AServerOnly]
		set { m_HealthState.Set((byte)value); }
	}

    public float MaxHealth
    {
        // Get
        get { return (k_fMaxHealth); }
    }


    public float MinHealth
    {
        // Get
        get { return (k_fMinHealth); }
    }


	public float Breath
	{
        // Get
		get { return (m_fOxygenUseRate.Get()); }

        // Set
		set { m_fOxygenUseRate.Set(value); }
	}


    public float DownedTimer
    {
        // Get
        get { return (m_fTimerDowned.Get()); }

        // Set
        set { m_fTimerDowned.Set(value); }
    }


    public static CPlayerHealth Instance
    {
        // Get
        get { return (s_cInstance); }
    }


// Member Functions
    public void Awake()
    {
		s_cInstance = this;

		CAudioCue audioCue = GetComponent<CAudioCue>();
		if (audioCue == null)
			audioCue = gameObject.AddComponent<CAudioCue>();

		m_EventHealthChanged += OnHealthChange;
		m_EventHealthStateChanged += OnHealthStateChange;
	}


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        //                                              Type   Callback          Initial Vlaue
		m_fHealth        = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, k_fMaxHealth);
        m_HealthState    = _cRegistrar.CreateReliableNetworkVar<byte> (OnNetworkVarSync, (byte)HealthState.ALIVE);
		m_fOxygenUseRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 5.0f);
        m_fTimerDowned   = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    [AServerOnly]
    private void UpdateHealthState()
    {
        // Set an invalid initial previous health state
        HealthState PrevHealthState = HealthState.INVALID;

        // Switch on the current health state
        switch (CurrentHealthState)
        {
            // Alive
            case HealthState.ALIVE:
            {
                // If the player's health is the minimum health
                if (Health == k_fMinHealth)
                {
                    // Change player's state to downed
                    PrevHealthState    = CurrentHealthState;
                    CurrentHealthState = HealthState.DOWNED;
                }

                // Break switch
                break;
            }

            // Dead
            case HealthState.DEAD:
            {
                // If the player's health is not the minimum health
                if (!(Health == k_fMinHealth))
                {
                    // Change player's state to downed
                    PrevHealthState    = CurrentHealthState;
                    CurrentHealthState = HealthState.ALIVE;
                }

                // Break switch
                break;
            }

            // Downed
            case HealthState.DOWNED:
            {
                // If downed timer is equal to or greater than the max downed timer duration
                if (fTimerDowned >= k_fTimerDownedMaxDuration)
                {
                    // Change player's state to dead
                    PrevHealthState    = CurrentHealthState;
                    CurrentHealthState = HealthState.DEAD;

                    // Reset downed timer
                    fTimerDowned = 0.0f;
                }

                // Break switch
                break;
            }

            // Default
            default:
            {
                // Log the current health state as an error
                Debug.LogError("Health state: " + CurrentHealthState.ToString());

                // Break switch
                break;
            }
        }

        // If the previous health state is valid
        // And previous health state is not the same as the current health state
        if ( (PrevHealthState != HealthState.INVALID) && (PrevHealthState != HealthState.MAX) &&
             (PrevHealthState != CurrentHealthState))
        {
            // Trigger EventHealthStateChanged
            if (m_EventHealthStateChanged != null)
            {
                m_EventHealthStateChanged(gameObject, CurrentHealthState, PrevHealthState);
            }
        }
    }
	
    [AServerOnly]
    private void UpdateHealthStateDowned()
    {
        if (CurrentHealthState == HealthState.DOWNED)
        {
            fTimerDowned += Time.deltaTime;

            if (fTimerDowned >= k_fTimerDownedMaxDuration)
            {
                if (CNetwork.IsServer)
                {
                    UpdateHealthState();
                }
            }
        }
    }


    [AServerOnly]
    public void ApplyDamage(float _fValue)
    {
        Health -= _fValue;
    }


    [AServerOnly]
    public void ApplyHeal(float _fValue)
    {
        Health += _fValue;
    }


	void Update()
	{
        UpdateHealthStateDowned();

        if (CNetwork.IsServer)
            UpdateAtmosphereEffects();

		if(CGamePlayers.SelfActor != gameObject)
			return;

        if (!m_bVisorHeatlhLightOn &&
            Health < 20.0f)
        {
            GetComponent<CPlayerInterface>().Model.GetComponent<CPlayerSkeleton>().Visor.renderer.material.SetFloat("_EmissivePowerB", 1.2f);
            m_bVisorHeatlhLightOn = true;
        }
        else if (m_bVisorHeatlhLightOn &&
                 Health > 20.0f)
        {
            GetComponent<CPlayerInterface>().Model.GetComponent<CPlayerSkeleton>().Visor.renderer.material.SetFloat("_EmissivePowerB", 0.0f);
            m_bVisorHeatlhLightOn = false;
        }
	}


    bool m_bVisorHeatlhLightOn = false;


    [AServerOnly]
	void UpdateAtmosphereEffects() { }

	private void OnHealthChange(GameObject _TargetPlayer, float _fHealthCurrentValue, float _fHealthPreviousValue)
	{
		if (gameObject != _TargetPlayer) Debug.LogError("CPlayerHealth→OnHealthChange is not returning the correct player!");
		if (_fHealthCurrentValue == _fHealthPreviousValue) Debug.LogError("CPlayerHealth→OnHealthChange is being called despite no change!");

		// Play ouchies.
		if (_fHealthCurrentValue < _fHealthPreviousValue && m_PlayerForceDamageSoundIndex_Time <= Time.time && CurrentHealthState != HealthState.DOWNED)
		{
			m_PlayerForceDamageSoundIndex_Time = Time.time + 0.5f;

			if(_TargetPlayer == CGamePlayers.SelfActor)
			{
				foreach(CAudioCue cue in GetComponents<CAudioCue>())
				{
					if(cue.m_strCueName == "PersonalSFX")
					{
						cue.Play(transform, 1.0f, false, -1, 2, 4);
					}
				}
			}
			else
			{
				foreach(CAudioCue cue in GetComponents<CAudioCue>())
				{
					if(cue.m_strCueName == "PlayerSFX")
					{
						cue.Play(transform, 1.0f, false, -1, 10, 12);
					}
				}
			}
		}
	}

	private void OnHealthStateChange(GameObject _SourcePlayer, HealthState _eHealthCurrentState, HealthState _eHealthPreviousState)
	{
		if (gameObject != _SourcePlayer) Debug.LogError("CPlayerHealth→OnHealthStateChange is not returning the correct player!");
		if (_eHealthCurrentState == _eHealthPreviousState) Debug.LogError("CPlayerHealth→OnHealthStateChange is being called despite no change!");

		// Play ooies.
        if (_eHealthCurrentState == HealthState.DOWNED)
        {
			if(_SourcePlayer == CGamePlayers.SelfActor)
			{
				foreach(CAudioCue cue in GetComponents<CAudioCue>())
				{
					if(cue.m_strCueName == "PersonalSFX")
					{
           				cue.Play( 1.0f, false, 1);
					}
				}
			}
			else
			{
				foreach(CAudioCue cue in GetComponents<CAudioCue>())
				{
					if(cue.m_strCueName == "PlayerSFX")
					{
						cue.Play(transform, 1.0f, false, 8);
					}
				}
			}
        }

	}

    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // If the updated network var was the health state
        if (_cVarInstance == m_fHealth)
        {
            if (CNetwork.IsServer)
            {
                // Update health states
                UpdateHealthState();
            }

            // Switch on the current health state
            switch (CurrentHealthState)
            {
                // Alive
                case HealthState.ALIVE:
                {
                    // Enable input
                    transform.GetComponent<CPlayerMotor>().EnableInput(this);
                    transform.GetComponent<CPlayerHead>().EnableInput(this);

                    // Break switch
                    break;
                }

                // Dead
                case HealthState.DEAD:
                {
                    // Break switch
                    break;
                }

                // Downed
                case HealthState.DOWNED:
                {
                    // Disable input
                    transform.GetComponent<CPlayerMotor>().DisableInput(this);
                    transform.GetComponent<CPlayerHead>().DisableInput(this);

                    // Break switch
                    break;
                }

                // Default
                default:
                {
                    // Log the current health state as an error
                    Debug.LogError("Health state: " + CurrentHealthState.ToString());

                    // Break switch
                    break;
                }
            }
        }
    }


    [ALocalOnly]
    void OnGUI()
    {
		if(!CGameHUD.IsOnGUIEnabled)
			return;

        const float kBoxMargin = 10.0f;
        const float kBoxWidth = 200.0f;
        const float kBoxHeight = 22.0f;


        if (CCursorControl.IsCursorLocked && 
            gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            GUIStyle cStyle = new GUIStyle();
            cStyle.alignment = TextAnchor.UpperLeft;
            cStyle.fontStyle = FontStyle.Bold;

            // Hit points
            GUI.Box(new Rect(Screen.width  - kBoxWidth - kBoxMargin,
                             Screen.height - kBoxHeight - kBoxMargin,
                             kBoxWidth, kBoxHeight),
                             "Health: " + Math.Round(m_fHealth.Get(), 2) + "/" + k_fMaxHealth);
		}
    }


    // Unused Functions
    void Start(){}
    void OnDestroy(){}
	

// Member Fields
	public const float k_fMaxHealth              = 100.0f;
    public const float k_fMinHealth              = 0.0f;
    public const float k_fInitHealth             = k_fMaxHealth;
    public const float k_fTimerDownedMaxDuration = 5.0f;

    float fTimerDowned = 0.0f;
	CNetworkVar<float> m_fHealth;
	CNetworkVar<float> m_fOxygenUseRate;
	CNetworkVar<byte> m_HealthState;

    static CPlayerHealth s_cInstance = null;

    CNetworkVar<float> m_fTimerDowned;
}