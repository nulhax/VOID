//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   DynamicEventShipHazard.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Instructions: Add Additional Ship Hazards
//
// Note: All steps can be found by pressing using [Ctrl] + [F]
//
// Step 1: Add a new element to the Enumerated type 'EHazardType'.
//
// Step 2: Add a new member variable for containing the hazard's quantity.
//
// Step 3: Add logic to 'get' the total number of that hazard currently within the game.
//
// Step 4: Create a new static class inside the 'Dynamic Events/Ship Hazards' folder
//         that contains trigger logic for the new hazard.
//
// Step 5: Add an additional case to the switch statement to call the added static trigger function.
//
// Step 6: Add an additional case to the switch statement for returning the current total of the new hazard.


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class DynamicEventShipHazard : MonoBehaviour
{
    // Member Types
    // Step 1:
    public enum EHazardType
    {
        NONE = -1,

        MALFUNCTION,

        MAX
    };

    struct HazardInfo
    {
        public EHazardType eType;
        public float fRatio;
        public uint uiTotal;
    };


    // Member Delegates & Events


    // Member Properties
    public float Difficulty
    {
        // Get the current difficulty
        get { return (m_fDifficulty); }

        // Set the current difficulty
        [AServerOnly]
        set
        {
            // Server check
            // Save value
            if (CNetwork.IsServer) { m_fDifficulty = value; }
        }
    }


    // Member Functions
    void UpdateHazardTotals()
    {
        // Reset all hazard totals
        m_iTotalHazards = 0;
        m_iTotalHazardsMalfunction = 0;

        // Malfunction
        // Create an array of module interfaces
        CModuleInterface[] ArrayModules = CGameShips.Ship.GetComponentsInChildren<CModuleInterface>();

        // For each module on the ship
        foreach (CModuleInterface ModInt in ArrayModules)
        {
            // Create a local variable for holding the current component interfaces
            CComponentInterface[] LocalCompInterface = ModInt.GetComponentsInChildren<CComponentInterface>();

            // For each component in the module
            foreach (CComponentInterface CompInt in LocalCompInterface)
            {
                // Increment the total malfunctions counter
                ++m_iTotalHazardsMalfunction;
            }
        }

        // Step 3:

        // Update total
        m_iTotalHazards = m_iTotalHazardsMalfunction;
    }


    void TriggerHazardOfType(EHazardType _eType)
    {
        // Switch on the hazard parameter
        switch (_eType)
        {
            // Trigger nothing
            case EHazardType.NONE:
            {
                // Note: This makes hazards more likely to trigger
                //       each time no hazard is triggered

                // Increase the difficulty modifier by 10%
                m_fDifficultyWeighted -= m_fDifficulty * 0.1f;

                // Return
                return;
            }

            // Malfunction
            case EHazardType.MALFUNCTION:
            {
                // Trigger a malfunction
                ShipHazardMalfunction.Trigger();

                // Reset the weighted difficulty
                m_fDifficultyWeighted = m_fDifficulty;

                // Return
                return;
            }

            // Step 5:

            // Default
            default:
            {
                // Report error
                Debug.LogError("Unhandled hazard type: " + _eType.ToString());

                // Return
                return;
            }
        }
    }


    int GetTotalHazardsOfType(EHazardType _eType)
    {
        // Switch on the hazard parameter
        switch (_eType)
        {
            // Add additional hazard info here

            // Malfunction
            case EHazardType.MALFUNCTION:
            {
                // Set return value
                return (m_iTotalHazardsMalfunction);
            }

            // Step 6:

            // Default
            default:
            {
                // Report error
                Debug.LogError("Unhandled hazard type: " + _eType.ToString());

                // Return
                return (0);
            }
        }
    }


    void Start()
    {
        // Set initial values for member data
        m_iTotal              = 100;
        m_fDifficulty         = m_iTotal - 25.0f;
        m_fDifficultyWeighted = Difficulty;
        m_fRemainder          = m_iTotal - m_fDifficultyWeighted;
    }


    public void Trigger(EHazardType _eBiasedType = EHazardType.NONE)
    {
        // Quick return case
        if (DisableRandomHazards) { return; }

        // Update current hazard totals
        UpdateHazardTotals();

        // Determine what hazard to trigger
        // Trigger a hazard of the returned type
        TriggerHazardOfType(UpdateHazardTrigger());
    }


    EHazardType UpdateHazardTrigger()
    {
        // If there are no hazards
        if (m_iTotalHazards == 0)
        {
            // Generate a random value to determine if a hazard is triggered
            if ((float)(Random.value * 100.0f) <= m_fRemainder)
            {
                // Return a random hazard type
                return ((EHazardType)((Random.value * 100.0f) % (int)EHazardType.MAX));
            }

            // Return none
            return (EHazardType.NONE);
        }
        
        // Local variables
        int iUniqueHazardTypes = (int)EHazardType.MAX;

        // Local containers
        List<float>      HazardRatios      = new List<float>();
        List<HazardInfo> HazardTotals      = new List<HazardInfo>();
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
            // ((HazardsOfThisType / (TotalHazards)) * 100.0f

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
        return (CalculateHazardTrigger(HazardInformation));
    }


    EHazardType CalculateHazardTrigger(List<HazardInfo> _Info)
    {
        // Local variables
        List<float> fThresholds         = new List<float>();
        float fHazardRandomTriggerValue = -1.0f;

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
                // If the 'next' element exists
                // Note: Container index starts at 0
                if ((i + 1) < fThresholds.Count)
                {
                    // If the random trigger value is greater than or equal to the current threshold and
                    // If the random trigger value is less than the next threshold
                    if ((fHazardRandomTriggerValue >= fThresholds.ElementAt(i)) &&
                        (fHazardRandomTriggerValue <  fThresholds.ElementAt(i + 1)))
                    {
                        // Return the current type
                        return ((EHazardType)i);
                    }
                }

                else
                {
                    // If the random trigger value is greater than or equal to the current threshold
                    if (fHazardRandomTriggerValue >= fThresholds.ElementAt(i))
                    {
                        // Return the current type
                        return ((EHazardType)i - 1);
                    }
                }
            }
        }

        // Default return
        return (EHazardType.NONE);
    }


    List<HazardInfo> OrderTotals(List<HazardInfo> _Info) // SMALLEST -> LARGEST
    {
        // Create a new array of equal size
        HazardInfo[] HazardInfoArray = new HazardInfo[_Info.Count];

        // Copy to the new array
        _Info.CopyTo(HazardInfoArray);

        // Order and return
        return (HazardInfoArray.OrderBy((item) => item.uiTotal).ToList());
    }


    List<float> OrderRatios(List<float> _Info) // LARGEST -> SMALLEST
    {
        // Create a new array of equal size
        float[] FloatArray = new float[_Info.Count];

        // Copy to the new array
        _Info.CopyTo(FloatArray);

        // Order and return
        return (FloatArray.OrderByDescending((item) => item).ToList());
    }


    List<HazardInfo> OrderMerged(List<HazardInfo> _Info) //  SMALLEST - LARGEST
    {
        // Create a new array of equal size
        HazardInfo[] HazardInfoArray = new HazardInfo[_Info.Count];

        // Copy to the new array
        _Info.CopyTo(HazardInfoArray);

        // Order and return
        return (HazardInfoArray.OrderBy((item) => (int)item.eType).ToList());
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
            Debug.LogError("Total count and Ratio count are not equal");
        }
    }


    float CalculateScaledValue(float _fValue, float _fNewMin, float _fNewMax, float _fOldMin, float _fOldMax)
    {
        // Scale _fValue to fit within the new range
        return ((((_fValue - _fOldMin) * (_fNewMax - _fNewMin)) / (_fOldMax - _fOldMin)) + _fNewMin);
    }


    // Dungeon Master Dynamic Event Methods
    public DynamicEventShipHazard()
    {
        // On construction, sign up as an event to the dungeon master
        DungeonMaster.instance.AddDynamicEvent(new DungeonMaster.DynamicEvent(Cost, Behaviour));
    }


    public void Cost(out float _Cost)
    {
        // Local variables
        if (m_TimeLastUpdate == float.PositiveInfinity) { m_TimeLastUpdate = Time.time; }

        float TimeCurrent = Time.time;
        float TimeDelta = TimeCurrent - m_TimeLastUpdate;

        // Update update time
        m_TimeLastUpdate = TimeCurrent;

        // Update decay cost
        // If decay would be less than zero, set to zero
        if ((m_CostDecay - TimeDelta) < 0.0f) { m_CostDecay = 0.0f; }

        // Else deduct current delta
        else { m_CostDecay -= TimeDelta; }

        // Return the weighted cost
        _Cost = m_CostBase + m_CostDecay;
    }


    public void Behaviour()
    {
        // Temporarily increase the cost of the dynamic event
        m_CostDecay += m_CostUseIncrease;

        // Trigger a ship hazard
        Trigger();
    }


    // Unused Functions
    void Awake() { }
    void Update() { }
    void OnDestroy() { }


    // Member Fields
    int   m_iTotal;
    float m_fDifficulty;
    float m_fDifficultyWeighted;
    float m_fRemainder;

    // Step 2:
    int m_iTotalHazards;
    int m_iTotalHazardsMalfunction;

    const float m_CostBase  = 50.0f;
    float m_CostUseIncrease = 25.0f;
    float m_CostDecay       = 0.0f;
    float m_TimeLastUpdate  = float.PositiveInfinity;

    // Debug
    public bool DisableRandomHazards = false;
}