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

        // If the timer is greater than the threshold
        if (m_fTimerHazard >= m_fTimerHazardThreshold)
        {
            // Reset timer
            m_fTimerHazard = 0.0f;

            // Update hazard total
            // TODO: Implement method to get number of all currently active hazards
            // m_HazardTotalActive   = ???;

            // Update difficulty modifier
            // TODO: Determine how difficulty will scale as the game progresses
            // m_fModifierDifficulty = ???;

            // Update modifiers for each hazard type
            // m_fModifierAsteroids  = ???;
            // m_fModifierModules    = ???;
            // m_fModifierFire       = ???;
            // m_fModifierDebug      = ???;

            // PSYDO CODE:
            // Get numbers of hazards
            // ushort usNumHazardsTotal = ???;
            // ushort usNumHazardsFire = ???;
            // ushort usNumHazardsHullBreach = ???;
            // ushort usNumHazardsNoAtmo = ???;

            // Get ratio of each hazard type to the total number
            // float fHazardFireRatio = usNumHazardsTotal / usNumHazardsFire;
            // float fHazardHullBreachRatio = usNumHazardsTotal / usNumHazardsHullBreach;
            // float fHazardNoAtmoRatio = usNumHazardsTotal / usNumHazardsNoAtmo;

            // Note all ratios added together should equal 1.0f

            // Random a number between 1 and 100
            // float fNumberRandom = 1-100;

            // Threshold One
            float fThresholdOne   = CalculateScaledValue(m_fModifierDifficulty);

            // Threshold Two
            float fThresholdTwo   = CalculateScaledValue(m_fModifierFires, fThresholdOne);

            // Threshold Three
            float fThresholdThree = CalculateScaledValue(m_fModifierFires, fThresholdTwo);
            
            // Algorithm if logic
            // if (fNumberRandom < first threshold)
            // {
            //      // Do something
            // }
            //
            // else if (fNumberRandom < second threshold)
            // {
            //      // Do something
            // }
            //
            // else if (fNumberRandom < third threshold)
            // {
            //      // Do something
            // }
            //
            // else if (fNumberRandom < fourth threshold)
            // {
            //      // Do something
            // }
            //
            // else
            // {
            //      // Something else
            // }

            // IDEA:
            // Ship hazard biases randomly selected on startup
            // Ship has a bias towards triggering certain types of hazards
            // Ee.g. more fire than other types, or more malfunctions
            // Result would be more hazards of the biased type
            // Fewer hazards of non-biased types
            // Subtley different gameplay from hazard triggers each playthrough

            // Determine what type of hazard to trigger

            // Determine where to trigger the hazard

            // Trigger the hazard
        }
    }


    // DEBUG WORK IN PROGRESS PUT HERE TO SAVE
//    #include <Windows.h>
//#include <iostream>

//using namespace std;

//float CalculateScaledValue(float _fX, float _fNewMin = 0.0f, float _fNewMax = 100.0f, float _fOldMin = 0.0f, float _fOldMax = 100.0f);

//int main()
//{
//    float iTotalHazards = 100;
//    float iFireHazards = 50;
//    float iHullBreaches = 25;
//    float iAtmos = 25;

//    float fDifficultyMod = 0.5f * 100.0f;

//    // Inversion logic needs work
//    // Must convert more common (higher value) hazards to lower percentage ratio

//    float fRatioFire =  (1.0f - (iFireHazards / iTotalHazards)) * 100.0f;
//    float fRatioHullBreach = (1.0f - (iHullBreaches / iTotalHazards)) * 100.0f;
//    float fRatioAtmos = (1.0f - (iAtmos / iTotalHazards)) * 100.0f;
//  //  float fDebug;

//    cout << "Fire ratio: " << fRatioFire << endl << "Hull breach ratio: " << fRatioHullBreach << endl << "Atmo ratio: " << fRatioAtmos << endl << endl;

//    cout << "Threshold One: " << CalculateScaledValue(fDifficultyMod);

//    char cWinHold = NULL;
//    cin >> cWinHold;

//    return (0);
//}

//float CalculateScaledValue(float _fX, float _fNewMin, float _fNewMax, float _fOldMin, float _fOldMax)
//{
//    // Temporary return variable
//    float fReturn = 0.0f;

//    // Scale _fX to fit within new range
//    fReturn = ((((_fX - _fOldMin) * (_fNewMax - _fNewMin)) / (_fOldMax - _fOldMin)) + _fNewMin);

//    // Return
//    return (fReturn);
//}

    private float CalculateScaledValue(float _fX,
                                       float _fNewMin = 0.0f,
                                       float _fNewMax = 100.0f,
                                       float _fOldMin = 0.0f,
                                       float _fOldMax = 100.0f)
    {
        // Temporary return variable
        float fReturn = 0.0f;

        // Scale _fX to fit within new range
        fReturn = ((((_fX - _fOldMin) * (_fNewMax - _fNewMin)) / (_fOldMax - _fOldMin)) + _fNewMin);

        // Return
        return (fReturn);
    }


    [AServerOnly]
    public void DebugUpdateDebugModifier(float _fNewValue)
    {
        // Update the debug spawning modifier
        m_fModifierDebug = _fNewValue;
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
    public float m_fModifierDebug         = 0.0f;
    public float m_fModifierFires         = 0.0f;
    public float m_fModifierModules       = 0.0f;
    public float m_fModifierAsteroids     = 0.0f;
    public float m_fModifierDifficulty    = 0.0f;

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
