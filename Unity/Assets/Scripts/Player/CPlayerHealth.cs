//  Auckland
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
    public enum DamageType
    {
        None         = 0,
        Fire         = 1,
        Physical     = 2,
        Electrical   = 4,
        Asphyxiation = 8
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
    public delegate void OnHealthChanged     (GameObject _TargetPlayer, float _fHealthCurrentValue, float _fHealthPreviousValue);
	public delegate void OnHealthStateChange (GameObject _SourcePlayer, HealthState _eHealthCurrentState, HealthState _eHealthPreviousState);

    public event OnHealthChanged     EventHealthChanged;
    public event OnHealthStateChange EventHealthStateChanged;


// Member Properties
	public float Health
	{
        // Get
		get { return (m_fHealth.Get()); }

        // Set
        set
        {
            // Local variables
            float fNewHealthValue = value;                   // New health value
            float fPrevHealth     = m_fHealth.Get();         // Current health
            float fHealthDelta    = value - m_fHealth.Get(); // Delta: New - Old

            // NOTE:
            // If fHealthDelta is ZERO, health is UNCHANGED
            // If fHealthDelta is a POSITIVE number, health INCREASED
            // If fHealthDelta is a NEGATIVE number, health DECREASED

            // TODO: Consider moving the below code into seperate functions.
            //       Note that doing so will require calling m_fHealth.Set()
            //       outside of Health's set method.

            // If health changed
            if (fHealthDelta != 0.0f)
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
                EventHealthChanged(gameObject, m_fHealth.Get(), fPrevHealth);
            }
        }
    }
	

	public HealthState CurrentHealthState
	{
        // Get
        get { return ((HealthState)m_HealthState.Get()); }

        // Set
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


    public static CPlayerHealth Instance
    {
        // Get
        get { return (s_cInstance); }
    }


// Member Functions
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        //                                              Type   Callback          Initial Vlaue
		m_fHealth        = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, k_fMaxHealth);
        m_HealthState    = _cRegistrar.CreateNetworkVar<byte> (OnNetworkVarSync, (byte)HealthState.INVALID);
		m_fOxygenUseRate = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 5.0f);
	}

    private void UpdateHealthState(GameObject _TargetPlayer, float _fHealthCurrentValue, float _fHealthPreviousValue)
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
                if (m_fHealth.Get() == k_fMinHealth)
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
                if (!(m_fHealth.Get() == k_fMinHealth))
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
                // Increment the downed timer
                fTimerDowned += Time.deltaTime;

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
             (PrevHealthState != CurrentHealthState) )
        {
            // Trigger EventHealthStateChanged
            EventHealthStateChanged(gameObject, CurrentHealthState, PrevHealthState);
        }
    }


    private void UpdateHealthStateDowned()
    {
        if (CurrentHealthState == HealthState.DOWNED)
        {
            UpdateHealthState(null, 0.0f, 0.0f);
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


    public void Awake()
    {
        s_cInstance = this;
        EventHealthChanged += UpdateHealthState;
    }


	void Start() 
    {
        if (Health == MaxHealth)
        {
            CurrentHealthState = HealthState.ALIVE;
        }
	}
		 

    void OnDestroy()
    {
        EventHealthChanged -= UpdateHealthState;
    }


	void Update()
	{
        // Update the downed timer
        UpdateHealthStateDowned();

        if (CNetwork.IsServer)
        {
            UpdateAtmosphereEffects();
        }

		if (CNetwork.IsServer) 
        {
			if (Input.GetKeyDown (KeyCode.Q)) 
            {
				if (Health > 0) 
                {
					ApplyDamage (100);
				}                
			}
		}
	}

    [AServerOnly]
    void UpdateAtmosphereEffects() { }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // If the updated network var was the health state
        if (_cVarInstance == m_HealthState)
        {
            // Switch on the current health state
            switch (CurrentHealthState)
            {
                // Alive
                case HealthState.ALIVE:
                {
                    // Enable input
                    transform.GetComponent<CPlayerGroundMotor>().ReenableInput(this);
                    transform.GetComponent<CPlayerHead>().ReenableInput(this);

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
                    transform.GetComponent<CPlayerGroundMotor>().DisableInput(this);
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
                             "Health: " + Math.Round(m_fHealth.Get(), 2) + "/" + k_fMaxHealth);
		}
    }
	

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
}
	