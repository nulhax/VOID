using UnityEngine;
using System.Collections;

public class DungeonMaster : MonoBehaviour
{
    //public enum EDifficultyModifier : uint
    //{
    //    // Group
    //    TotalDistanceTravelled, // +(√km)%
    //    TotalShipWorth, // Up to 100,000 nanites - +0% to +50%
    //    RandomFluctuation,  // -cos(0)*0.25 → (beginning at -25%, and oscillating up to +25%)
    //    // Group
    //    ShipDamage, // -0% to -50%
    //    // Group
    //    DifficultyChoice,   // -50% easy | 0% normal | 50% hard | variable from -100% to +∞%

    //    MAX
    //}

    public delegate void Behaviour();
    public struct SDynamicEvent
    {
        public SDynamicEvent(float _cost, Behaviour _behaviour) { cost = _cost; behaviour = _behaviour; timeEventLastOccurred = 0.0f; }

        float cost;
        float timeEventLastOccurred;
        Behaviour behaviour;
    }

    // Difficulty modifiers are individual factors that influence the overall difficulty.
    // Difficulty modifiers are split into groups.
    // Difficulty modifiers in the same group add together to form the group value, and group values are multiplied together to produce the overall difficulty.
    // E.g. In its own group; a difficulty modifier of -0.5 will halve difficulty, +1.0 will double difficulty, -1.0 will erase difficulty entirely.
    public class CDifficultyModifier   // Relative effect on the overall difficulty (-50% makes it half as difficult | +100% makes it twice as difficult).
    {
        public CDifficultyModifier() { }
        public CDifficultyModifier(float value) { value_internal = value; }
        public CDifficultyModifier(uint group) { DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }
        public CDifficultyModifier(float value, uint group) { value_internal = value; DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }
        public CDifficultyModifier(uint group, float value) { value_internal = value; DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }

        public void AddToGroup(uint group) { DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }

        public float value { get { return value_internal; } set { value_internal = value; DungeonMaster.instance.DifficultyNeedsUpdating(); } }

        private float value_internal = 0.0f;
    }

    private static DungeonMaster sDM = null;
    public static DungeonMaster instance { get { return sDM; } }
    private System.Collections.Generic.SortedList<uint, System.Collections.Generic.List<CDifficultyModifier>> mDifficultyFactors = new System.Collections.Generic.SortedList<uint, System.Collections.Generic.List<CDifficultyModifier>>();
    private bool mbDifficultyNeedsUpdating = false;
    private float mfDifficulty_internal = 1.0f;

    private System.Collections.Generic.List<SDynamicEvent> mDynamicEvents = new System.Collections.Generic.List<SDynamicEvent>();

    private float mfPengar = 0.0f;

    public float difficulty { get { if (mbDifficultyNeedsUpdating)UpdateDifficulty(); return mfDifficulty_internal; } }

    public DungeonMaster()
    {
        sDM = this;
    }

	void Start()
    {

	}
	
	void Update()
    {
        // Update coinage.
        mfPengar += Time.deltaTime * difficulty;

        // Decide what to do.
        while (mfPengar >= 2.0f)
        {
            mfPengar -= 2.0f;

            CGalaxy.SCellPos parentAbsoluteCell = CGalaxy.instance.RelativePointToAbsoluteCell(CGame.GalaxyShip.transform.position);
            Vector3 pos = (CGame.GalaxyShip.transform.position - CGalaxy.instance.RelativeCellToRelativePoint(parentAbsoluteCell - CGalaxy.instance.centreCell)) + Random.onUnitSphere * CGalaxy.instance.cellRadius/*Fog end*/;
            CGalaxy.instance.LoadGubbin(new CGalaxy.SGubbinMeta((CGame.ENetworkRegisteredPrefab)Random.Range((ushort)CGame.ENetworkRegisteredPrefab.Asteroid_FIRST, (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_LAST + 1), parentAbsoluteCell, Random.Range(10.0f, 30.0f), pos, Random.rotationUniform, (CGame.GalaxyShip.transform.position - pos).normalized * 100.0f, Random.onUnitSphere * 50.0f, 0.125f, true, true));
        }
	}

    public void AddDynamicEvent(SDynamicEvent dynamicEvent)
    {

    }

    public void AddDifficultyModifierToGroup(CDifficultyModifier difficultyModifier, uint group)
    {
        System.Collections.Generic.List<CDifficultyModifier> difficultyModifiers;
        if (!mDifficultyFactors.TryGetValue(group, out difficultyModifiers))
        {
            difficultyModifiers = new System.Collections.Generic.List<CDifficultyModifier>();
            mDifficultyFactors.Add(group, difficultyModifiers);
        }

        difficultyModifiers.Add(difficultyModifier);

        mbDifficultyNeedsUpdating = true;
    }

    public void DifficultyNeedsUpdating()
    {
        mbDifficultyNeedsUpdating = true;
    }
    
    void UpdateDifficulty()
    {
        mfDifficulty_internal = 1.0f;

        foreach (System.Collections.Generic.KeyValuePair<uint, System.Collections.Generic.List<CDifficultyModifier>> factors in mDifficultyFactors)
        {
            float groupDifficulty = 1.0f;
            foreach (CDifficultyModifier modifier in factors.Value)
                groupDifficulty += modifier.value;

            if (groupDifficulty < 0.0f)
                groupDifficulty = 0.0f;  // Difficulty can not be negative.

            mfDifficulty_internal *= groupDifficulty;
        }

        mbDifficultyNeedsUpdating = false;
        //Debug.Log("Difficulty set to: " + Mathf.RoundToInt((mfDifficulty_internal*100)).ToString() + "%");
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleRight;
        style.fontStyle = FontStyle.Bold;
        float boxWidth = 0.1f;
        float boxHeight = 0.06f;
        GUI.Box(new Rect(Screen.width - Screen.width * boxWidth, Screen.height * 0.5f, Screen.width * boxWidth, Screen.height * boxHeight), "Difficulty: " + Mathf.RoundToInt((mfDifficulty_internal * 100)).ToString() + "%\nPengar: " + mfPengar.ToString("N1"));
    }
}