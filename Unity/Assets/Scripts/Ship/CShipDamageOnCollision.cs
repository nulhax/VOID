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
    public float impulseToRadius = 1.0f;

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
            //Debug.LogWarning("Impulse: " + impulse.ToString() + "\nRadius: " + radius.ToString() + " units\nForce: " + force.ToString() + " newtons");

            // Find all damagable actors within radius and apply damage.
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 contactPointOnShip = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationPos(contact.point);

                CActorHealth[] damagableActors = CGameShips.Ship.GetComponentsInChildren<CActorHealth>();
				foreach (CActorHealth damagableActor in damagableActors)
                {
					if (damagableActor.takeDamageOnImpact)
					{
						float actorDistanceToImpact = (damagableActor.gameObject.transform.position - contactPointOnShip).magnitude;

						if (actorDistanceToImpact < radius)
						{
							float damage = impulse * (1.0f - (actorDistanceToImpact / radius));
							damagableActor.gameObject.GetComponent<CActorHealth>().health -= damage;
						}
					}
                }
            }
        }
    }
}
