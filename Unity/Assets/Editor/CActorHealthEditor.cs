using UnityEditor;

[CustomEditor(typeof(CActorHealth))]
public class CActorHealthEditor : Editor
{
	static bool foldoutHealth = false;
	static bool foldoutState = false;

	//SerializedProperty syncNetworkHealth;
	//SerializedProperty destroyOnZeroHealth;
	//SerializedProperty takeDamageOnImpact;
	//SerializedProperty syncNetworkState;
	//SerializedProperty health_initial;
	//SerializedProperty state_initial;
	//SerializedProperty stateTransitions;
	//
	//void OnEnable()
	//{
	//	syncNetworkHealth = serializedObject.FindProperty("syncNetworkHealth");
	//	destroyOnZeroHealth = serializedObject.FindProperty("destroyOnZeroHealth");
	//	takeDamageOnImpact = serializedObject.FindProperty("takeDamageOnImpact");
	//	syncNetworkState = serializedObject.FindProperty("syncNetworkState");
	//	health_initial = serializedObject.FindProperty("health_initial");
	//	state_initial = serializedObject.FindProperty("state_initial");
	//	stateTransitions = serializedObject.FindProperty("stateTransitions");
	//}

	public override void OnInspectorGUI()
	{
		//serializedObject.Update();

		CActorHealth myTarget = (CActorHealth)target;

		foldoutHealth = EditorGUILayout.Foldout(foldoutHealth, "Health");
		if (foldoutHealth)
		{
			//EditorGUILayout.IntSlider(health_initial, 0, 100, new UnityEngine.GUIContent("Health"));
			//EditorGUILayout.FloatField(health_initial, new UnityEngine.GUIContent("Initial Health"));

			myTarget.health_initial = EditorGUILayout.FloatField("Initial Value", myTarget.health_initial);
			myTarget.health_max = EditorGUILayout.FloatField("Max Value", myTarget.health_max);
			myTarget.health_min = EditorGUILayout.FloatField("Min Value", myTarget.health_min);
			EditorGUILayout.LabelField("Current value\t\t\t  " + myTarget.health);
			myTarget.syncNetworkHealth = EditorGUILayout.Toggle("Sync Network", myTarget.syncNetworkHealth);
			myTarget.destroyOnZeroHealth = EditorGUILayout.Toggle("Destroy On Zero", myTarget.destroyOnZeroHealth);
			myTarget.takeDamageOnImpact = EditorGUILayout.Toggle("Impact Damage", myTarget.takeDamageOnImpact);
		}

		foldoutState = EditorGUILayout.Foldout(foldoutState, "State");
		if (foldoutState)
		{
			int initialState = EditorGUILayout.IntField("Initial Value", myTarget.state_initial); myTarget.state_initial = (byte)(initialState < 0 ? 0 : initialState > 255 ? 255 : initialState);
			EditorGUILayout.LabelField("Current value\t\t\t  " + myTarget.state);
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

			for (int i = 0; i < newStateTransitionLength; ++i)
				myTarget.stateTransitions[i] = EditorGUILayout.FloatField("State " + (i + 1).ToString() + " if health >=", myTarget.stateTransitions[i]);
		}

		myTarget.callEventsOnStart = EditorGUILayout.Toggle("Call Events On Start", myTarget.callEventsOnStart);

		if(myTarget.syncNetworkState || myTarget.syncNetworkHealth)
		{
			float result = EditorGUILayout.FloatField("Syncs Per Second", 1.0f / myTarget.timeBetweenNetworkSyncs);
			myTarget.timeBetweenNetworkSyncs = result <= 0.0f ? float.PositiveInfinity : 1.0f / result;
		}

		//serializedObject.ApplyModifiedProperties();
	}
}