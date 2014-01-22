using UnityEngine;
using System.Collections;

public class CEnemyShip : MonoBehaviour
{
    GameObject target = null;
    CPidController mPidAngleYaw = new CPidController(); // Correction for yaw angle to target.
    CPidController mPidAnglePitch = new CPidController(); // Correction for pitch angle to target.
    CPidController mPidVelocityYaw = new CPidController(); // Correction for yaw velocity to target.
    CPidController mPidVelocityPitch = new CPidController(); // Correction for pitch velocity to target.

	void Start ()
    {
        transform.rotation = Random.rotation;
	}

    void FixedUpdate()
    {
        if (target != null)  // If attacking a target...
        {
            float time = Time.fixedDeltaTime;

            // Get the position of the target in local space of this ship (i.e. relative position).
            Vector3 targetPosition = Quaternion.Inverse(gameObject.transform.rotation) * (target.transform.position - gameObject.transform.position);
            float deltaYaw = Mathf.Atan2(targetPosition.z, targetPosition.x);
            float deltaPitch = Mathf.Atan2(targetPosition.z, targetPosition.y);

            //float torqueYaw = 
        }
    }
	
	void Update ()
    {
        //gameObject.rigidbody.AddTorque(Quaternion.Angle(gameObject.transform.rotation, Quaternion.LookRotation(dirToPlayerShip)));
        //gameObject.rigidbody.AddForce(dirToPlayerShip * 10.0f, ForceMode.VelocityChange);
	}
}
