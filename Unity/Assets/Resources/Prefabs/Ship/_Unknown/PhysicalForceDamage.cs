using UnityEngine;
using System.Collections;

public class PhysicalForceDamage : MonoBehaviour
{
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnCollisionEnter(Collision collision)
	{	
		if (CNetwork.IsServer)
		{
			//Assume player weighs 80 kg
			float fMass = 80;
			//Acceleration will be the relative velocity, as this is essentially an instant change in velocity.
			float fAcceleration =  collision.relativeVelocity.magnitude;
			//f = ma
			float fNetForce	= fMass * fAcceleration;
			
			
			if(fNetForce > m_fForceDamageThreshold)
			{
				CPlayerHealth health = gameObject.GetComponent<CPlayerHealth>();
				
				//Make damage scale exponentially, according to force.
				
				//For reference, a human punch can exert up to 5000 newtons of force. Let's assume that would hurt lots.
				
				
				float fDamage = Mathf.Log(fNetForce, 2);
									
				health.ApplyDamage(Mathf.Clamp(fDamage, 5, 45));
				
				//Debug.Log("Player took damage after being hit by " + collision.gameObject.name);
				//Debug.Log("Force = " + fNetForce.ToString());
				//Debug.Log("Damage = " + fDamage.ToString());
			}
		}
		
	}
	
	float m_fForceDamageThreshold = 700;
}
