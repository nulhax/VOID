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

    [AServerOnly]
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

				// Damagable actors.
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
							damagableActor.health -= damage;
							//Debug.Log(damagableActor.gameObject.name + " took " + damage.ToString() + " damage");
						}
					}
                }

				// Damagable fires.
				CFireHazard[] fireHazards = CGameShips.Ship.GetComponentsInChildren<CFireHazard>();
				foreach (CFireHazard fireHazard in fireHazards)
				{
					if (fireHazard.health.takeDamageOnImpact)
					{
						//Debug.LogWarning(damagableActor.gameObject.ToString() + " can be damaged on impact");
						float actorDistanceToImpact = (fireHazard.gameObject.transform.position - contactPointOnShip).magnitude;
						//Debug.Log(damagableActor.gameObject.ToString() + " was " + actorDistanceToImpact.ToString() + " units from impact");

						if (actorDistanceToImpact < radius)
						{
							float damage = impulse * (1.0f - (actorDistanceToImpact / radius));
							fireHazard.health.health -= damage;
							//Debug.Log(damagableActor.gameObject.name + " took " + damage.ToString() + " damage");
						}
					}
				}
            }
        }
    }


    [AServerOnly]
    public void CreateExplosion(Vector3 _Position, float _fRadius, float _fImpulse)
    {
        // Server check
        if (!CNetwork.IsServer) { return; }

        // Create an array of all hull breach nodes
        CHullBreachNode[] HullBreachNodes = CGameShips.Ship.GetComponentsInChildren<CHullBreachNode>();

        // Create explosion visual effect


        // For each hull breach node
        foreach (CHullBreachNode Node in HullBreachNodes)
        {
            // Local variables
            float fDamage = 0.0f;
            float fDistance = (Node.transform.position - _Position).magnitude;

            // If the hull breach node is within the radius of the explosion
            if (fDistance <= _fRadius)
            {
                // Calculate the amount of damage to inflict to the node
                // Note: Damage scales linearly with proximity to explosion
                fDamage = _fImpulse * (1.0f / fDistance);

                // Damage the node
                Node.GetComponent<CActorHealth>().health -= fDamage;
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
