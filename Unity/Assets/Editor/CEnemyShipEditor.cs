using UnityEditor;

[CustomEditor(typeof(CEnemyShip))]
public class CEnemyShipEditor : Editor
{
	public override void OnInspectorGUI()
	{
		CEnemyShip myTarget = (CEnemyShip)target;

		EditorGUILayout.LabelField("State: " + myTarget.debug_StateName);
		EditorGUILayout.LabelField("Expires: " + myTarget.mTimeout.ToString("F2"));
		EditorGUILayout.LabelField("Targeting: " + (myTarget.mTarget_InternalSource != null ? myTarget.mTarget_InternalSource.name : "no target") + " for " + (myTarget.mTargetExpireTime - RealTime.time).ToString("F2") + " seconds");
		//EditorGUILayout.LabelField(
	}
}