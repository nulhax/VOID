//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHazardSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// NOTES:
// Consider modifying time between hazard triggers
// based upon game difficulty and current hazard total

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */
public class CHazardSystem : MonoBehaviour
{
// Member Types


// Member Delegates & Events
    public delegate void OnHazardTriggered(/*Hazard information goes here*/);

    public event OnHazardTriggered EventHazardTriggered;

// Member Properties


// Member Functions
    public void Awake() { }


    public void Start()
    {
        // TODO: Data-drive / .ini / .xml the initial data
        m_fModifierDifficulty = 0.0f;
        m_fModifierAsteroids  = 0.0f;
        m_fModifierFires      = 0.0f;
        m_fModifierModules    = 0.0f;
        m_fModifierDebug      = 0.0f;
    }


    public void Update()
    {
        // Increment timer
        m_fTimerHazard += Time.deltaTime;

        // Update hazard total
        // TODO: Implement method to get number of all currently active hazards
        // m_HazardTotalActive = ???;

        // Update difficulty modifier
        // TODO: Determine how difficulty will scale as the game progresses
        // m_fModifierDifficulty = ???;

        // If the timer is greater than the threshold
        if (m_fTimerHazard >= m_fTimerHazardThreshold)
        {
            
        }
    }


    [AServerOnly]
    public void DebugTriggerHazard()
    {
        // Set the hazard timer to the timer threshold
        m_fTimerHazard = m_fTimerHazardThreshold;
    }


    // Unused methods
    public void OnDestroy(){}

// Member Fields
    // Algorithm modifiers
    public float m_fModifierDifficulty    = 0.0f;
    public float m_fModifierAsteroids     = 0.0f;
    public float m_fModifierFires         = 0.0f;
    public float m_fModifierModules       = 0.0f;
    public float m_fModifierDebug         = 0.0f;

    // Timers
    private float m_fTimerHazard          = 0.0f;
    private float m_fTimerHazardThreshold = 0.0f;

    // Data containers
    private byte m_HazardTotalActive      = 0;

    // Hazard containers
    // Asteroid container/equivilent
    // Fire container/equivilent
    // Module container/equivilent
};
