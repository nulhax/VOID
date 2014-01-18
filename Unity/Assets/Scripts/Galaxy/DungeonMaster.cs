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
    public class DynamicEvent
    {
        public DynamicEvent(float _cost, Behaviour _behaviour) { cost = _cost; behaviour = _behaviour; timeEventLastOccurred = 0.0f; }

        public float cost;
        public float timeEventLastOccurred;
        public Behaviour behaviour;
    }

    // Difficulty modifiers are individual factors that influence the overall difficulty.
    // Difficulty modifiers are split into groups.
    // Difficulty modifiers in the same group add together to form the group value, and group values are multiplied together to produce the overall difficulty.
    // E.g. In its own group; a difficulty modifier of -0.5 will halve difficulty, +1.0 will double difficulty, -1.0 will erase difficulty entirely.
    public class DifficultyModifier   // Relative effect on the overall difficulty (-50% makes it half as difficult | +100% makes it twice as difficult).
    {
        public DifficultyModifier() { }
        public DifficultyModifier(float value) { value_internal = value; }
        public DifficultyModifier(uint group) { DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }
        public DifficultyModifier(float value, uint group) { value_internal = value; DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }
        public DifficultyModifier(uint group, float value) { value_internal = value; DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }

        public void AddToGroup(uint group) { DungeonMaster.instance.AddDifficultyModifierToGroup(this, group); }

        public float value { get { return value_internal; } set { value_internal = value; DungeonMaster.instance.DifficultyNeedsUpdating(); } }

        private float value_internal = 0.0f;
    }

    private static DungeonMaster sDM = null;
    public static DungeonMaster instance { get { return sDM; } }

    private System.Collections.Generic.SortedList<uint, System.Collections.Generic.List<DifficultyModifier>> mDifficultyFactors = new System.Collections.Generic.SortedList<uint, System.Collections.Generic.List<DifficultyModifier>>();
    private bool mbDifficultyNeedsUpdating = false;
    private float mfDifficulty_internal = 1.0f;

    private System.Collections.Generic.List<DynamicEvent> mDynamicEvents = new System.Collections.Generic.List<DynamicEvent>();

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
        return; // <- Fuck you
        // Update coinage.
        mfPengar += Time.deltaTime * difficulty;

        // Decide what to do.
        foreach (DynamicEvent dynamicEvent in mDynamicEvents)
        {
            if (mfPengar >= dynamicEvent.cost)
            {
                mfPengar -= dynamicEvent.cost;

                dynamicEvent.behaviour();
                dynamicEvent.timeEventLastOccurred = Time.fixedTime;
            }
        }
	}

    public void AddDynamicEvent(DynamicEvent dynamicEvent)
    {
        mDynamicEvents.Add(dynamicEvent);
    }

    public void AddDifficultyModifierToGroup(DifficultyModifier difficultyModifier, uint group)
    {
        System.Collections.Generic.List<DifficultyModifier> difficultyModifiers;
        if (!mDifficultyFactors.TryGetValue(group, out difficultyModifiers))
        {
            difficultyModifiers = new System.Collections.Generic.List<DifficultyModifier>();
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

        foreach (System.Collections.Generic.KeyValuePair<uint, System.Collections.Generic.List<DifficultyModifier>> factors in mDifficultyFactors)
        {
            float groupDifficulty = 1.0f;
            foreach (DifficultyModifier modifier in factors.Value)
                groupDifficulty += modifier.value;

            if (groupDifficulty < 0.0f)
                groupDifficulty = 0.0f;  // Difficulty can not be negative.

            mfDifficulty_internal *= groupDifficulty;
        }

        mbDifficultyNeedsUpdating = false;
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