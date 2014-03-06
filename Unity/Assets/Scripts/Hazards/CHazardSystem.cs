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


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CHazardSystem : MonoBehaviour
{
// Member Types
    enum EHazardType
    {
        INVALID     = -2,
                    
        NONE        = -1,
        FIRE,
        HULL_BREACH,
        MALFUNCTION,

        MAX
    };

    struct HazardInfo
    {
        public EHazardType eType;
        public float       fRatio;
        public uint        uiTotal;
    };


// Member Delegates & Events


// Member Properties
    public float Difficulty
    {
        get { return (m_fDifficulty); }

        set
        {
            if (CNetwork.IsServer)
            {
                m_fDifficulty = value;
            }
        }
    }


// Member Functions
    void Start()
    {
        // Set initial values for member data
        m_iTotal                       = 100;
        m_fDifficulty                  = m_iTotal - 75.0f;
        m_fDifficultyWeighted          = Difficulty;
        m_fRemainder                   = m_iTotal - m_fDifficultyWeighted;
        
        m_iTotalHazardsFire            = 0;
        m_iTotalHazardsHullBreach      = 0;
        m_iTotalHazardsMalfunction     = 0;
        m_iTotalHazards                = 0;
        
        m_fHazardTriggerTimer          = 0.0f;
        m_fHazardTriggerTimerThreshold = 1.0f;
    }


    void Update()
    {
        // Update the timer
        m_fHazardTriggerTimer += Time.deltaTime;

        // If the timer equals or exceeds the threshold
        if (m_fHazardTriggerTimer >= m_fHazardTriggerTimerThreshold)
        {
            // Reset the timer
            m_fHazardTriggerTimer = 0.0f;

            // Determine what hazard to trigger
            EHazardType eHazardTypeToTrigger = UpdateHazardTrigger();

            // Trigger a hazard of the returned type
            TriggerHazardOfType(eHazardTypeToTrigger);
        }
    }


    EHazardType UpdateHazardTrigger()
    {
        // Hazard return value
        EHazardType eHazardReturn = EHazardType.NONE;

        // If there are no hazards
        if (m_iTotalHazards == 0)
        {
            // Generate a random trigger value
            eHazardReturn = (EHazardType)((Random.value * 100.0f) % (int)EHazardType.MAX);
        }

        // Else
        else
        {
            // Local variables
            int iUniqueHazardTypes = (int)EHazardType.MAX;

            // Local containers
            List<float> HazardRatios = new List<float>();
            List<HazardInfo> HazardTotals = new List<HazardInfo>();
            List<HazardInfo> HazardInformation = new List<HazardInfo>();        

            // For each hazard type
            for (int i = 0; i < iUniqueHazardTypes; ++i)
            {
                // Get the number of hazards of this type
                int iHazardsOfThisType = GetTotalHazardsOfType((EHazardType)i);

                // Create a new 'hazard'
                HazardInfo NewHazard = new HazardInfo();
                NewHazard.eType = (EHazardType)i;
                NewHazard.uiTotal = (uint)iHazardsOfThisType;

                // Add the new hazard into the totals container
                HazardTotals.Add(NewHazard);

                // Calculate the ratio of this hazard to all hazards
                // ((float)iHazardsOfThisType / (float)m_iTotalHazards)) * 100.0f

                // Normalise the ratio from hazards down to fit within the remainder
                float fNormalisedRatio = 0.0f;
                if (m_iTotalHazards != 0)
                {
                    // Normalise the ratio to with the range of the remainder
                    fNormalisedRatio = CalculateScaledValue(((float)iHazardsOfThisType / (float)m_iTotalHazards) * 100.0f,
                                                            0.0f,
                                                            m_fRemainder,
                                                            0.0f,
                                                            100.0f);
                }

                // Add the normalised ratio to the ratio list
                HazardRatios.Add(fNormalisedRatio);
            }

            // Order the totals: SMALLEST -> LARGEST
            HazardTotals = OrderTotals(HazardTotals);

            // Order the ratios: LARGEST -> SMALLEST
            HazardRatios = OrderRatios(HazardRatios);

            // Merge ratios and totals
            // [1] -> [1] <- [1]
            // [0] -> [0] <- [0]
            // [2] -> [2] <- [2]
            MergeRatiosWithTotals(HazardInformation, HazardTotals, HazardRatios);

            // Order the merged ratios and totals by their type
            // [0]
            // [1]
            // [2]
            HazardInformation = OrderMerged(HazardInformation);

            // Calculate the type of hazard to trigger
            eHazardReturn = CalculateHazardTrigger(HazardInformation);
        }

        // Return
        return (eHazardReturn);
    }


    EHazardType CalculateHazardTrigger(List<HazardInfo> _Info)
    {
        // Local variables
        List<float> fThresholds = new List<float>();
        float fHazardRandomTriggerValue = -1.0f;
        EHazardType eReturnHazard = EHazardType.NONE;

        // Generate random trigger value to two decimal places
        fHazardRandomTriggerValue = (float)(Random.value * 100.0f);

        // If the random trigger value is not below the first thrshold
        if (!(fHazardRandomTriggerValue < m_fDifficultyWeighted))
        {
            // Set first threshold as the weighted difficulty
            fThresholds.Add(m_fDifficultyWeighted);

            // For each element of the info container
            foreach (HazardInfo Info in _Info)
            {
                // Add a new threshold equal to the previous threshold plus the new ratio
                fThresholds.Add(fThresholds.Last() + Info.fRatio);
            }

            // Iterate through each threshold
            for (int i = 0; i < fThresholds.Count; ++i)
            {
                // If the random trigger value is greater than or equal to the current threshold and
                // If the random trigger value is less than the next threshold
                if ((fHazardRandomTriggerValue >= fThresholds.ElementAt(i)) && (fHazardRandomTriggerValue < fThresholds.ElementAt(i + 1)))
                {
                    // Set the hazard to trigger as the current type
                    eReturnHazard = (EHazardType)i;

                    // Break the loop to prevent redundant processing
                    break;
                }
            }
        }

        // Return
        return (eReturnHazard);
    }


    List<HazardInfo> OrderTotals(List<HazardInfo> _Info) // SMALLEST -> LARGEST
    {
        HazardInfo[] HazardInfoArray = new HazardInfo[_Info.Count];
        _Info.CopyTo(HazardInfoArray);
        _Info = HazardInfoArray.OrderBy((item) => item.uiTotal).ToList();

        return (_Info);
    }


    List<float> OrderRatios(List<float> _Info) // LARGEST -> SMALLEST
    {
        float[] FloatArray = new float[_Info.Count];
        _Info.CopyTo(FloatArray);
        _Info = FloatArray.OrderByDescending((item) => item).ToList();

        return (_Info);
    }


    List<HazardInfo> OrderMerged(List<HazardInfo> _Info) //  SMALLEST - LARGEST
    {
        HazardInfo[] HazardInfoArray = new HazardInfo[_Info.Count];
        _Info.CopyTo(HazardInfoArray);
        _Info = HazardInfoArray.OrderBy((item) => (int)item.eType).ToList();

        return (_Info);
    }


    void MergeRatiosWithTotals(List<HazardInfo> _Result, List<HazardInfo> _Totals, List<float> _Ratios)
    {
        // Clear the result container
        _Result.Clear();

        // If both containers contain equal numbers of elements
        if (_Totals.Count == _Ratios.Count)
        {
            // For each element in both lists
            for (int i = 0; i < _Totals.Count; ++i)
            {
                // Merge the two sets of information into a single object
                HazardInfo Info;
                Info.eType   = _Totals.ElementAt(i).eType;
                Info.uiTotal = _Totals.ElementAt(i).uiTotal;
                Info.fRatio  = _Ratios.ElementAt(i);

                // Add the merged object into the result list
                _Result.Add(Info);
            }
        }

        else
        {
            // Report error
            Debug.LogError("Total and Ratio count is not equal");
        }
    }


    void TriggerHazardOfType(EHazardType _eType)
    {
        // Switch on the hazard parameter
        switch (_eType)
        {
            // Trigger nothing
            case EHazardType.NONE:
                {
                    // Note: This makes hazards more likely to trigger each
                    //       time the timer expires and no hazard is triggered

                   // Debug.Log("Hazard Triggered: None");

                    // Increase the difficulty modifier by 10%
                    m_fDifficultyWeighted -= m_fDifficulty * 0.1f;

                    // Break
                    break;
                }

            // Trigger fire
            case EHazardType.FIRE:
                {
                    // Trigger a fire
                    CFireHazard[] shipFireNodes = CGameShips.Ship.GetComponentsInChildren<CFireHazard>();
                    m_iTotalHazardsFire = shipFireNodes.Length;

                    int i = (int)(Random.value * 100.0f) % m_iTotalHazardsFire;
                    shipFireNodes[i].GetComponent<CActorHealth>().health = 0;

                    // TODO: Optimise

                    Debug.Log("Hazard Triggered: Fire");

                    // Reset the weighted difficulty
                    m_fDifficultyWeighted = m_fDifficulty;

                    // Break
                    break;
                }

            // Trigger hull breach
            case EHazardType.HULL_BREACH:
                {
                    // Trigger a hull breach
                    ++m_iTotalHazardsHullBreach;

                    //Debug.Log("Hazard Triggered: Hull Breach");

                    // Reset the weighted difficulty
                    m_fDifficultyWeighted = m_fDifficulty;

                    // Break
                    break;
                }

            // Trigger malfunction
            case EHazardType.MALFUNCTION:
                {
                    // Trigger a malfunction
                    ++m_iTotalHazardsMalfunction;

                    //Debug.Log("Hazard Triggered: Malfunction");

                    // Reset the weighted difficulty
                    m_fDifficultyWeighted = m_fDifficulty;

                    // Break
                    break;
                }

            // Default
            default:
                {
                    // Report error
                    Debug.LogError("Invalid hazard type");

                    // Break
                    break;
                }
        }

        // Update the remainder to include any changes
        m_fRemainder = m_iTotal - m_fDifficultyWeighted;

        // Update the total number of hazards
        m_iTotalHazards = m_iTotalHazardsFire + m_iTotalHazardsHullBreach + m_iTotalHazardsMalfunction;
    }


    int GetTotalHazardsOfType(EHazardType _eType)
    {
        // Temporary return variable
        int iReturn = 0;

        // Switch on the hazard parameter
        switch (_eType)
        {
            // Fire
            case EHazardType.FIRE:
                {
                    // Set return value
                    iReturn = m_iTotalHazardsFire;

                    // Break
                    break;
                }

            // Hull Breach
            case EHazardType.HULL_BREACH:
                {
                    // Set return value
                    iReturn = m_iTotalHazardsHullBreach;

                    // Break
                    break;
                }

            // Malfunction
            case EHazardType.MALFUNCTION:
                {
                    // Set return value
                    iReturn = m_iTotalHazardsMalfunction;

                    // Break
                    break;
                }

            // Default
            default:
                {
                    // Report error
                    Debug.LogError("Invalid hazard type");

                    // Break
                    break;
                }
        }

        // Return
        return (iReturn);
    }


    float CalculateScaledValue(float _fValue, float _fNewMin, float _fNewMax, float _fOldMin, float _fOldMax)
    {
        // Temporary return variable
        float fReturn = 0.0f;

        // Scale _fValue to fit within the new range
        fReturn = ((((_fValue - _fOldMin) * (_fNewMax - _fNewMin)) / (_fOldMax - _fOldMin)) + _fNewMin);

        // Return
        return (fReturn);
    }


// Unused Functions
    void Awake() { }
    void OnDestroy() { }


// Member Fields
    int m_iTotal;
    float m_fDifficulty;
    float m_fDifficultyWeighted;
    float m_fRemainder;

    float m_fHazardTriggerTimer;
    float m_fHazardTriggerTimerThreshold;

    int m_iTotalHazardsFire;
    int m_iTotalHazardsHullBreach;
    int m_iTotalHazardsMalfunction;
    int m_iTotalHazards;
}