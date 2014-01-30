using UnityEditor;

[CustomEditor(typeof(CActorHealth))]
public class CActorHealthEditor : Editor
{
	static bool foldoutHealth = false;
	static bool foldoutState = false;

	public override void OnInspectorGUI()
	{
		CActorHealth myTarget = (CActorHealth)target;

		foldoutHealth = EditorGUILayout.Foldout(foldoutHealth, "Health");
		if (foldoutHealth)
		{
			myTarget.health_initial = EditorGUILayout.FloatField("Initial Value", myTarget.health_initial);
			myTarget.syncNetworkHealth = EditorGUILayout.Toggle("Sync Network", myTarget.syncNetworkHealth);
			myTarget.destroyOnZeroHealth = EditorGUILayout.Toggle("Destroy On Zero", myTarget.destroyOnZeroHealth);
			myTarget.takeDamageOnImpact = EditorGUILayout.Toggle("Impact Damage", myTarget.takeDamageOnImpact);
		}

		foldoutState = EditorGUILayout.Foldout(foldoutState, "State");
		if (foldoutState)
		{
			int initialState = EditorGUILayout.IntField("Initial Value", myTarget.state_initial); myTarget.state_initial = (byte)(initialState < 0 ? 0 : initialState > 255 ? 255 : initialState);
			myTarget.syncNetworkState = EditorGUILayout.Toggle("Sync Network", myTarget.syncNetworkState);

			int currentStateTransitionLength = myTarget.stateTransitions != null ? myTarget.stateTransitions.Length : 0;
			int newStateTransitionLength = EditorGUILayout.IntField("Transition Count", currentStateTransitionLength);
			if (newStateTransitionLength != currentStateTransitionLength)
			{
				float[] newStateArray = new float[newStateTransitionLength];
				for (int i = 0; i < newStateTransitionLength; ++i)
					newStateArray[i] = (i < currentStateTransitionLength) ? myTarget.stateTransitions[i] : i != 0 ? newStateArray[i - 1] : 0;

				myTarget.stateTransitions = newStateArray;
			}

			for (int i = 0; i < currentStateTransitionLength; ++i)
				myTarget.stateTransitions[i] = EditorGUILayout.FloatField("State " + (i + 1).ToString() + " if health >=", myTarget.stateTransitions[i]);
		}
	}
}