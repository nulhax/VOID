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
    public enum HealthState
    {
        HEALTH_INVALID = -1,

        HEALTH_ALIVE,
        HEALTH_DOWNED,
        HEALTH_DEAD,

        HEALTH_MAX
    }


// Member Delegates & Events
    // Health Value
    public delegate void OnHealthChanged(GameObject _TargetPlayer, float _fHealthCurrentValue, float _fHealthPreviousValue);

    // Health State
	public delegate void OnHealthStateChange (GameObject _SourcePlayer, HealthState _eHealthCurrentState, HealthState _eHealthPreviousState);
	
    // Health Value
    public event OnHealthChanged EventHealthChanged;

    // Health State
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
            float fCurrentHealth  = m_fHealth.Get();         // Current health
            float fHealthDelta    = value - m_fHealth.Get(); // Delta: New - Old

            // NOTE:
            // If fHealthDelta is ZERO, health is UNCHANGED
            // If fHealthDelta is a POSITIVE number, health INCREASED
            // If fHealthDelta is a NEGATIVE number, health DECREASED

            // If health is attempting to increase
            if (fHealthDelta > 0.0f)
            {
                // If new health value is below the max health
                if (fNewHealthValue < k_fMaxHealth)
                {
                    // Trigger EventHealthIncreased
                    if (EventHealthIncreased != null) { EventHealthIncreased(gameObject, fHealthDelta); }

                    // Assign the new health value

                }
            }

            // If health is attempting to decrease
            else if (fHealthDelta < 0.0f)
            {

            }







            // If fHealthDelta is not 0 (health has changed)
            // And fHealthDelta is less than 0 (health increased) and health is not currently at max health (health can increase)
            // Or fHealthDelta is greater than 0 (health decreased) and health is not currently 0 (health can decrease)
            if ((fHealthDelta != 0.0f) && ((fHealthDelta < 0.0f && (m_fHealth.Get() != k_fMaxHealth)) || (fHealthDelta > 0.0f && (m_fHealth.Get() != 0.0f))))
            {
                // If old value minus new value is a positive number
                if (fHealthDelta > 0.0f)
                {
                    // Trigger EventHealthDecreased
                    if (EventHealthDecreased != null) { EventHealthDecreased(gameObject, fHealthDelta); }
                }

                // If old value minus new value is a negative number
                else if (fHealthDelta < 0.0f)
                {
                    // Trigger EventHealthIncreased
                    if (EventHealthIncreased != null) { EventHealthIncreased(gameObject, fHealthDelta); }
                }

                // If the new health value is 0 or above, and less than the max health
                if ((value >= 0.0f) && (value <= k_fMaxHealth))
                {
                    // Assign the new health value
                    m_fHealth.Set(value);
                }

                // If the new health value is less than 0
                else if (value < 0.0f)
                {
                    // Set health to 0
                    m_fHealth.Set(0.0f);
                }

                // If the new health is above the max health
                else if (value > k_fMaxHealth)
                {
                    // Set health to max health
                    m_fHealth.Set(k_fMaxHealth);
                }
            }
        }
	}
	

	public HealthState CurrentHealthState
	{
        // Get
        get
        {
            // Convert the CNetworkVar 'm_HealthState' to a standard type
            // Convert standard type to a string
            // Convert byte string to enum type
            // Return converted enum type
            //          Type   String->Enum           Type      CNetworkVar->Standard Type->String
            return ((HealthState)Enum.Parse(typeof(HealthState), m_HealthState.Get().ToString()));

            // TODO: Test shortcut logic
            // TODO: Use Enum.IsDefined() to avoid exception cases
        }

        // Set
		set
        {
            // Convert value to string
            // Convert string to byte
            // Convert byte and save to CNetworkVar
            m_HealthState.Set(byte.Parse(value.ToString()));

            // TODO: Exception checking
        }
	}
	

	public float Breath
	{
        // Get
		get { return(m_fOxygenUseRate.Get()); }

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
        m_HealthState    = _cRegistrar.CreateNetworkVar<byte> (OnNetworkVarSync, byte.Parse(HealthState.HEALTH_INVALID.ToString()));
		m_fOxygenUseRate = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 5.0f);
	}


    [AServerOnly]
    public void ApplyDamage(float _fValue)
    {
        // Set new health value
        m_fHealth.Set(m_fHealth.Get() - _fValue);
    }


    [AServerOnly]
    public void ApplyHeal(float _fAmount)
    {
        // Set new health value
        if((m_fHealth.Get() + _fAmount) <= k_fMaxHealth)
        {
            m_fHealth.Set(m_fHealth.Get() + _fAmount);
        }
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
            if (Health == 0.0f)
            {
                m_HealthState.Set(false);

                // Notify observers
                if (EventHealthDead != null) EventHealthDead(gameObject);
            }
        }
        else
        {
            // Check player is now alive
            if (Health > 0.0f)
            {
                m_HealthState.Set(true);

                // Notify observers
                if (EventHealthRevived != null) EventHealthRevived(gameObject);
            }
        }
	}


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_HealthState)
        {
            // Player is alive
            if (m_HealthState.Get())
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
        else  if (_cVarInstance == m_fHealth &&
                  m_LaughTrack != null)
        {
            m_LaughTrack.Play(0.5f, false, -1);

            if (CNetwork.IsServer)
            {
                if (m_fHealth.Get() <= 0.0f)
                {
                    m_HealthState.Set(false);
                }
                else if (m_fHealth.Get() > 0.0f)
                {
                    m_HealthState.Set(true);
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


	const float k_fMaxHealth = 100.0f;


	CNetworkVar<float> m_fHealth;
	CNetworkVar<float> m_fOxygenUseRate;
	CNetworkVar<byte> m_HealthState;

    static CPlayerHealth s_cInstance = null;
}
	