using UnityEngine;
using System.Collections;

public class DungeonMaster : MonoBehaviour
{
    public enum EDifficultyModifier : uint
    {
        // Level 1
        TotalDistanceTravelled, // +(√km)%
        TotalShipWorth, // Up to 100,000 nanites - +0% to +50%
        RandomFluctuation,  // -cos(0)*0.25 → (beginning at -25%, and oscillating up to +25%)
        // Level 2
        ShipDamage, // -0% to -50%
        // Highest level
        DifficultyChoice,   // -50% easy | 0% normal | 50% hard | variable from -100% to +∞%

        MAX
    }

    private class CDifficultyModifier { public float value = 0.0f;}  // Relative effect on the overall difficulty (-50% makes it half as difficult | +100% makes it twice as difficult).

    private CDifficultyModifier[] mDifficultyModifiers = new CDifficultyModifier[(uint)EDifficultyModifier.MAX];
    private System.Collections.Generic.List<System.Collections.Generic.List<CDifficultyModifier>> mDifficultyFactors = new System.Collections.Generic.List<System.Collections.Generic.List<CDifficultyModifier>>();
    private bool mbDifficultyNeedsUpdating = false;
    private float mfDifficulty_internal;
    private float mfMyntslag = 0.0f;

    public float mDifficulty { get { if (mbDifficultyNeedsUpdating)UpdateDifficulty(); return mfDifficulty_internal; } }

    public DungeonMaster()
    {
        // Create difficulty modifiers.
        for (uint x = 0; x < (uint)EDifficultyModifier.MAX; ++x)
            mDifficultyModifiers[x] = new CDifficultyModifier();

        // Level 1:
        System.Collections.Generic.List<CDifficultyModifier> level1 = new System.Collections.Generic.List<CDifficultyModifier>();
        level1.Add(mDifficultyModifiers[(uint)EDifficultyModifier.TotalDistanceTravelled]);
        level1.Add(mDifficultyModifiers[(uint)EDifficultyModifier.TotalShipWorth]);
        level1.Add(mDifficultyModifiers[(uint)EDifficultyModifier.RandomFluctuation]);
        mDifficultyFactors.Add(level1);

        // Level 2:
        System.Collections.Generic.List<CDifficultyModifier> level2 = new System.Collections.Generic.List<CDifficultyModifier>();
        level2.Add(mDifficultyModifiers[(uint)EDifficultyModifier.ShipDamage]);
        mDifficultyFactors.Add(level2);

        // Level 3:
        System.Collections.Generic.List<CDifficultyModifier> level3 = new System.Collections.Generic.List<CDifficultyModifier>();
        level3.Add(mDifficultyModifiers[(uint)EDifficultyModifier.DifficultyChoice]);
        mDifficultyFactors.Add(level3);
    }

	void Start()
    {
        // Initialise difficulty modifiers.
        SetDifficultyModifier(EDifficultyModifier.DifficultyChoice, -0.5f);
        // Etc...
        // Etc...
        // Etc...
	}
	
	void Update()
    {
	    // Update difficulty modifiers.
        SetDifficultyModifier(EDifficultyModifier.RandomFluctuation, -0.25f + Mathf.PingPong(Time.time, 0.5f));

        // Update coinage.
        mfMyntslag += Time.deltaTime * mDifficulty;

        // Decide what to do.
        while (mfMyntslag >= 2.0f)
        {
            mfMyntslag -= 2.0f;

            CGalaxy.SCellPos parentAbsoluteCell = CGalaxy.instance.PointToAbsoluteCell(CGame.GalaxyShip.transform.position);
            Vector3 pos = (CGame.GalaxyShip.transform.position - CGalaxy.instance.RelativeCellCentrePoint(parentAbsoluteCell - CGalaxy.instance.centreCell)) + Random.onUnitSphere * CGalaxy.instance.cellRadius/*Fog end*/;
            CGalaxy.instance.LoadGubbin(new CGalaxy.SGubbinMeta((CGame.ENetworkRegisteredPrefab)Random.Range((ushort)CGame.ENetworkRegisteredPrefab.Asteroid_FIRST, (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_LAST + 1), parentAbsoluteCell, Random.Range(10.0f, 30.0f), pos, Random.rotationUniform, (CGame.GalaxyShip.transform.position - pos).normalized * 100.0f, Random.onUnitSphere * 50.0f, 0.125f, true, true));
        }
	}

    void SetDifficultyModifier(EDifficultyModifier difficultyModifier, float value)
    {
        mDifficultyModifiers[(uint)difficultyModifier].value = value;
        mbDifficultyNeedsUpdating = true;
    }

    float GetDifficultyModifier(EDifficultyModifier difficultyModifier) { return mDifficultyModifiers[(uint)difficultyModifier].value; }
    
    void UpdateDifficulty()
    {
        mfDifficulty_internal = 1.0f;

        foreach (System.Collections.Generic.List<CDifficultyModifier> factors in mDifficultyFactors)
        {
            float difficultyModifier = 1.0f;
            foreach (CDifficultyModifier modifier in factors)
                difficultyModifier += modifier.value;

            if (difficultyModifier < 0.0f)
                difficultyModifier = 0.0f;

            mfDifficulty_internal *= difficultyModifier;
        }

        mbDifficultyNeedsUpdating = false;
        //Debug.Log("Difficulty set to: " + Mathf.RoundToInt((mfDifficulty_internal*100)).ToString() + "%");
    }
}
