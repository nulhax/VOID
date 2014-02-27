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
//#include <Windows.h>
//#include <iostream>
//#include <stdlib.h>
//#include <time.h>

//using namespace std;

//enum HazardType
//{
//    HAZARD_NONE        = -1,

//    HAZARD_FIRE        =  0,
//    HAZARD_HULL_BREACH =  1,
//    HAZARD_MALFUNCTION =  2,

//    HAZARD_MAX
//};

//float CalculateScaledValue(float _fX, float _fNewMin = 0.0f, float _fNewMax = 100.0f, float _fOldMin = 0.0f, float _fOldMax = 100.0f);
//int GetTotalHazardsOfType(HazardType _eType);
//void TriggerHazard(HazardType _Hazard);

//HazardType HazardUpdate();

//// // // DEBUG // // //
//int TotalFireHazards        = 0;
//int TotalHullBreachHazards  = 0;
//int TotalMalfunctionHazards = 0;
//int TotalHazards            = 0;

//int   Total                 = 100;
//float Difficulty            = Total - 75.0f;
//float WeightedDifficulty    = Difficulty;
//float Remainder             = Total - WeightedDifficulty;
//// // // DEBUG // // //

//int main()
//{
//    // // // WINDOWS // // //
//    srand((unsigned int)time(NULL));
//    // // // WINDOWS // // //

//    TotalFireHazards = rand() % 10;
//    TotalHullBreachHazards = rand() % 10;
//    TotalMalfunctionHazards = rand() % 10;

//    TotalHazards = TotalFireHazards + TotalHullBreachHazards + TotalMalfunctionHazards;

//    DWORD TimePrev  = timeGetTime();;
//    DWORD TimeCurr  = TimePrev;
//    DWORD TimeDelta = 0;
//    DWORD Timer     = 0;

//    while (true)
//    {
//        TimePrev  = TimeCurr;
//        TimeCurr  = timeGetTime();
//        TimeDelta = TimeCurr - TimePrev;

//        Timer += TimeDelta;

//        if (Timer >= 1000)
//        {
//            HazardType Hazard = HazardUpdate();
//            TriggerHazard(Hazard);

//            Timer = 0;
//        }
//    }

////    // // // DEBUG // // //
////    cout << "---------------" << endl;
////    // // // DEBUG // // //
////
////    // // // WORKING // // //
////    float Total              = 100.0f;
////    float Difficulty         = Total - 75.0f;
////    float WeightedDifficulty = Difficulty;
////    float Remainder          = Total - Difficulty;
////
////    float FireHazards   = 125.0f;
////    float HullBreaches  = 30.0f;
////    float Malfuncs      = 55.0f;
////    float TotalHazards  = FireHazards + HullBreaches + Malfuncs;
////    // // // WORKING // // //
////
////    // // // DEBUG // // //
////    cout << "Total:       " << Total      << endl;
////    cout << "Difficulty:  " << Difficulty << endl;
////    cout << "Remainder:   " << Remainder  << endl;
////    cout << endl;
////    // // // DEBUG // // //
////
////    // // // WORKING // // //
////    float RatioTotalFire       = (FireHazards  / TotalHazards) * 100.0f;
////    float RatioTotalHullBreach = (HullBreaches / TotalHazards) * 100.0f;
////    float RatioTotalMalfunc    = (Malfuncs     / TotalHazards) * 100.0f;
////    float RatioTotalTotal      = RatioTotalFire + RatioTotalHullBreach + RatioTotalMalfunc;
////    // // // WORKING // // //
////
////    // // // DEBUG // // //
////    cout << "Fire  Ratio: "  << RatioTotalFire << endl;
////    cout << "HuBr  Ratio: "  << RatioTotalHullBreach << endl;
////    cout << "Malf  Ratio: "  << RatioTotalMalfunc << endl;
////    cout << "Ratio Total: "  << RatioTotalTotal << endl;
////    cout << endl;
////    // // // DEBUG // // //
////
////    // // // WORKING // // //
////    float RatioRemainderFire =       CalculateScaledValue(RatioTotalFire, 0.0f, Remainder);
////    float RatioRemainderHullBreach = CalculateScaledValue(RatioTotalHullBreach, 0.0f, Remainder);
////    float RatioRemainderMalfunc =    CalculateScaledValue(RatioTotalMalfunc, 0.0f, Remainder);
////    float RatioRemainderTotal =      RatioRemainderFire + RatioRemainderHullBreach + RatioRemainderMalfunc;
////    // // // WORKING // // //
////
////    // // // DEBUG // // //
////    cout << "FRem  Ratio: " << RatioRemainderFire << endl;
////    cout << "HRem  Ratio: " << RatioRemainderHullBreach << endl;
////    cout << "MRem  Ratio: " << RatioRemainderMalfunc << endl;
////    cout << "RemR Total:  " << RatioRemainderTotal << endl;
////    cout << endl;
////    // // // DEBUG // // //
////    
////    // // // WORKING // // //
////    float Threshold1 = WeightedDifficulty;
////    float Threshold2 = Threshold1 + RatioRemainderFire;
////    float Threshold3 = Threshold2 + RatioRemainderHullBreach;
////    float Threshold4 = Threshold3 + RatioRemainderMalfunc;
////    // // // WORKING // // //
////
////    // // // DEBUG // // //
////    cout << "Threshold1:  " << Threshold1 << endl;
////    cout << "Threshold2:  " << Threshold2 << endl;
////    cout << "Threshold3:  " << Threshold3 << endl;
////    cout << "Threshold4:  " << Threshold4 << endl;
////    // // // DEBUG // // //
////
////    // // // DEBUG // // //
////    cout << "---------------";
////    // // // DEBUG // // //

//    // // // WINDOWS // // //
//    char cWinHold = NULL;
//    cin >> cWinHold;
//    // // // WINDOWS // // //

//    return (0);
//}

//void TriggerHazard(HazardType _Hazard)
//{
//    switch (_Hazard)
//    {
//    case HAZARD_NONE:
//        {
//            cout << "No hazard triggered, adjusting weightings" << endl << endl;
//            WeightedDifficulty -= Difficulty * 0.1f;
//            Remainder           = Total - WeightedDifficulty;
//            break;
//        }
//    case HAZARD_FIRE:
//        {
//            cout << "Triggering Hazard: " << _Hazard << " - Fire" << endl;
//            cout << "Total Fire Hazards: " << TotalFireHazards << endl << endl;
//            WeightedDifficulty = Difficulty;
//            Remainder          = Total - WeightedDifficulty;
//            ++TotalFireHazards;
//            break;
//        }

//    case HAZARD_HULL_BREACH:
//        {
//            cout << "Triggering Hazard: " << _Hazard << " - Hull Breach" << endl;
//            cout << "Total Hull Breach Hazards: " << TotalHullBreachHazards << endl << endl;
//            WeightedDifficulty = Difficulty;
//            Remainder          = Total - WeightedDifficulty;
//            ++TotalHullBreachHazards;
//            break;
//        }

//    case HAZARD_MALFUNCTION:
//        {
//            cout << "Triggering Hazard: " << _Hazard << " - Malfunction" << endl;
//            cout << "Total Malfunction Hazards: " << TotalMalfunctionHazards << endl << endl;
//            WeightedDifficulty = Difficulty;
//            Remainder          = Total - WeightedDifficulty;
//            ++TotalMalfunctionHazards;
//            break;
//        }
//    }
//}

//HazardType HazardUpdate()
//{
//    // Local Types
//    struct HazardInfo
//    {
//        float      fHazardRatio;
//        float      fHazardRatioNormalised;
//        float      fHazardRatioFinal;
//        int        iNumHazards;
//        HazardType eHazardType;
//    };

//    // Hazard return value
//    HazardType eHazard = HAZARD_NONE;

//    // Generate random number
//    const int HazardRandomValue = rand() % Total;
//    cout << "Random Value: " << HazardRandomValue << endl;

//    // Get number of unique hazard types
//    // DEBUG: Default
//    const int UniqueHazardTypes = HAZARD_MAX;
////    cout << "Unique hazard types: " << UniqueHazardTypes << endl;

//    // Threshold container
//    float Thresholds[UniqueHazardTypes + 1];//      = new float[UniqueHazardTypes + 1];
////    cout << "Thresholds: " << (sizeof(Thresholds) / sizeof(float)) << endl;

//    Thresholds[0] = WeightedDifficulty;

//    // Hazard info container
//    HazardInfo * pHazardInfo = new HazardInfo[UniqueHazardTypes];

//    // Hazard total containers
//    int * pHazardTotalsContainer = new int[UniqueHazardTypes];
//    int iTotalHazards = 0;

//    for (short i = 0; i < UniqueHazardTypes; ++i)
//    {
//        // Get the number of active hazards of the current type
//        // DEBUG: Default
//        pHazardTotalsContainer[i] = GetTotalHazardsOfType((HazardType)i);

// //       cout << "Total hazards of type " << i << ": " << pHazardTotalsContainer[i] << endl;

//        iTotalHazards += pHazardTotalsContainer[i];
//    }

////    cout << "Total hazards: " << iTotalHazards << endl;

// //   cout << endl << endl;
//    for (int i = 0; i < UniqueHazardTypes; ++i)
//    {
//        // Set the current hazard's type
//        pHazardInfo[i].eHazardType  = (HazardType)i;
// //       cout << "Current hazard type: " << (HazardType)i << endl;

//        // Get the number of active hazards of the current type
//        // DEBUG: Default
//        int iTotalHazardsOfThisType = pHazardTotalsContainer[i];
//        cout << "Total hazards of this type: " << iTotalHazardsOfThisType << endl;

//        // Determine the ratio of this hazard type to the total number of hazards
//        pHazardInfo[i].fHazardRatio = ((float)pHazardTotalsContainer[i]  / (float)iTotalHazards) * 100.0f;
//        cout << "Current hazard ratio: " << pHazardInfo[i].fHazardRatio << endl;

//        // Normalise the ratio to within current range
//        pHazardInfo[i].fHazardRatioNormalised = CalculateScaledValue(pHazardInfo[i].fHazardRatio, 0.0f, Remainder);
//        cout << "Normalized current hazard ratio: " << pHazardInfo[i].fHazardRatioNormalised << endl;

//        // Set next threshold
//        // Note: First threshold is set by difficulty, not hazards
////        Thresholds[i + 1] = Thresholds[i] + pHazardInfo[i].fHazardRatioNormalised;
//    //    cout << "Threshold: " << Thresholds[i + 1] << endl;
//    }

//    for (int i = 0; i < UniqueHazardTypes / 2; ++i)
//    {
//        int mod2 = UniqueHazardTypes - i - 1;

//        pHazardInfo[i].fHazardRatioFinal = pHazardInfo[mod2].fHazardRatioNormalised;
//        pHazardInfo[mod2].fHazardRatioFinal = pHazardInfo[i].fHazardRatioNormalised;
//    }

//    for (int i = 0; i < UniqueHazardTypes; ++i)
//    {
//        cout << "Final ratio " << i << ": " << pHazardInfo[i].fHazardRatioFinal << endl;
//        Thresholds[i + 1] = Thresholds[i] + pHazardInfo[i].fHazardRatioFinal;
//    }

////    cout << endl << endl;
//    for (short i = 0; i < UniqueHazardTypes + 1; ++i)
//    {
//        cout << "Threshold " << i << ": " << Thresholds[i] << endl;
//    }

//    for (int i = 0; i < UniqueHazardTypes; ++i)
//    {
//        if ((HazardRandomValue >= Thresholds[i]) && (HazardRandomValue < Thresholds[i + 1]))
//        {
//            eHazard = (HazardType)(i);
//        }
//    }

//    return (eHazard);
//}

//int GetTotalHazardsOfType(HazardType _eType)
//{
//    int iReturn = 0;

//    switch (_eType)
//    {
//    case HAZARD_FIRE:
//        {
//            iReturn = TotalFireHazards;
//            break;
//        }

//    case HAZARD_HULL_BREACH:
//        {
//            iReturn = TotalHullBreachHazards;
//            break;
//        }

//    case HAZARD_MALFUNCTION:
//        {
//            iReturn = TotalMalfunctionHazards;
//            break;
//        }
//    }

//    return (iReturn);
//}

//float CalculateScaledValue(float _fX, float _fNewMin, float _fNewMax, float _fOldMin, float _fOldMax)
//{
//    // Temporary return variable
//    float fReturn = 0.0f;

//    // Scale _fX to fit within the new range
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
