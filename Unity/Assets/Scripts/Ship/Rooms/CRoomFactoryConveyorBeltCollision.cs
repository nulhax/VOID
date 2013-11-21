using UnityEngine;
using System.Collections;

public class CRoomFactoryConveyorBeltCollision : MonoBehaviour
{
	void OnTriggerStay(Collider TriggerObject)
	{
		CToolInterface cToolInterface = TriggerObject.GetComponent<CToolInterface>();
		
		if (cToolInterface)
		{			
			Rigidbody RB = TriggerObject.gameObject.rigidbody;
			
			if (RB)
			{
				RB.useGravity = false;
				RB.velocity = Vector3.zero;
				RB.angularVelocity = Vector3.zero;
				RB.AddForce(transform.right * -2.0f, ForceMode.Impulse);
			}
		}
	}
	
	void OnTriggerExit(Collider TriggerObject)
	{
		CToolInterface cToolInterface = TriggerObject.GetComponent<CToolInterface>();
		
		if (cToolInterface)
		{
			Rigidbody RB = TriggerObject.gameObject.rigidbody;
			
			if (RB)
			{
				RB.velocity = Vector3.zero;
				RB.angularVelocity = Vector3.zero;
				RB.useGravity = true;
			}
		}
	}
	
	void Start (){}
	void Update(){}
}