using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CEnemyShip))]
public class CEnemyShipEditor : Editor
{
	public override void OnInspectorGUI()
	{
		CEnemyShip myTarget = (CEnemyShip)target;

		EditorGUILayout.LabelField("State: " + myTarget.debug_StateName);
		EditorGUILayout.LabelField("Expires: " + myTarget.mTimeout1.ToString("F2"));
		EditorGUILayout.LabelField("Targeting: " + (myTarget.mTarget_InternalSource != null ? myTarget.mTarget_InternalSource.name : "no target") + " for " + myTarget.mTargetExpireTime.ToString("F2") + " seconds");
		EditorGUILayout.LabelField((myTarget.mFaceTarget ? "T" : "Not t") + "urning to | "  + (myTarget.mTargetWithinViewCone ? "W" : "Not w") + "ithin view cone");
		EditorGUILayout.LabelField((myTarget.mFollowTarget ? "F" : "Not f") + "ollowing | " + (myTarget.mTargetWithinViewSphere ? "W" : "Not w") + "ithin view sphere");
		EditorGUILayout.LabelField((myTarget.mTargetVisible ? "Can" : "Can't") + " see target");
	}
}