//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CShipDamageOnCollision : MonoBehaviour
{
	struct SDebugVisual
	{
		public Vector3 pointOfImpact;
		public float radius;
		public float expireTime;

		public SDebugVisual(Vector3 _PointOfImpact, float _Radius, float _ExpireTime) { pointOfImpact = _PointOfImpact; radius = _Radius; expireTime = _ExpireTime; }
	}

    public float impulseToRadius = 1.0f;

	System.Collections.Generic.List<SDebugVisual> m_DebugVisuals = new System.Collections.Generic.List<SDebugVisual>();

    void OnCollisionEnter(Collision collision)
    {
        if (CNetwork.IsServer)
        {
            float force = 0.0f;
            float impulse = 0.0f;
            if (collision.transform.rigidbody != null)
            {
                force = collision.relativeVelocity.magnitude * (collision.transform.rigidbody.mass / rigidbody.mass);
                impulse = (collision.rigidbody.mass * collision.relativeVelocity / (rigidbody.mass + collision.transform.rigidbody.mass)).magnitude;
            }
            else
                Debug.LogError("Put a Rigidbody on " + collision.transform.gameObject.name + " else there is no force in impacts.");

            float radius = impulse * impulseToRadius;
            Debug.LogWarning("Impulse: " + impulse.ToString() + "\nRadius: " + radius.ToString() + " units\nForce: " + force.ToString() + " newtons");

            // Find all damagable actors within radius and apply damage.
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 contactPointOnShip = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationPos(contact.point);
				m_DebugVisuals.Add(new SDebugVisual(contactPointOnShip, radius, Time.time + 1.0f));

                CActorHealth[] damagableActors = CGameShips.Ship.GetComponentsInChildren<CActorHealth>();
				foreach (CActorHealth damagableActor in damagableActors)
                {
					if (damagableActor.takeDamageOnImpact)
					{
						//Debug.LogWarning(damagableActor.gameObject.ToString() + " can be damaged on impact");
						float actorDistanceToImpact = (damagableActor.gameObject.transform.position - contactPointOnShip).magnitude;
						//Debug.Log(damagableActor.gameObject.ToString() + " was " + actorDistanceToImpact.ToString() + " units from impact");

						if (actorDistanceToImpact < radius)
						{
							float damage = impulse * (1.0f - (actorDistanceToImpact / radius));
							damagableActor.gameObject.GetComponent<CActorHealth>().health -= damage;
							Debug.Log(damagableActor.gameObject.name + " took " + damage.ToString() + " damage");
						}
					}
                }
            }
        }
    }

	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		for (int i = 0; i < m_DebugVisuals.Count; i++)
		{
			if (m_DebugVisuals[i].expireTime >= Time.time)
			{
				m_DebugVisuals.RemoveAt(i);
				--i;
				continue;
			}
			else
				Gizmos.DrawSphere(m_DebugVisuals[i].pointOfImpact, m_DebugVisuals[i].radius);
		}
	}
}
