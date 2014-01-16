using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CShipDamageOnCollision : MonoBehaviour
{
    public float impulseToRadius = 1.0f;
	void Start ()
    {
	    
	}
	
	void Update ()
    {
	    
	}

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

            // Find all components within radius and apply damage.
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 contactPointOnShip = CGame.ShipGalaxySimulator.GetGalaxyToSimulationPos(contact.point);

                CActorBreakable[] breakableActors = CGame.Ship.GetComponentsInChildren<CActorBreakable>();
                foreach (CActorBreakable breakableActor in breakableActors)
                {
                    float actorDistanceToImpact = (breakableActor.gameObject.transform.position - contactPointOnShip).magnitude;

                    if (actorDistanceToImpact < radius)
                    {
                        float damage = impulse * (1.0f - (actorDistanceToImpact / radius));
                        breakableActor.gameObject.GetComponent<CActorHealth>().health -= damage;
                        Debug.LogWarning(breakableActor.gameObject.ToString() + " is " + actorDistanceToImpact.ToString() + " units away from impact and took " + damage.ToString() + " damage");
                    }
                    else
                        Debug.LogWarning(breakableActor.gameObject.ToString() + " is " + actorDistanceToImpact.ToString() + " units away from impact and avoided damage");
                }
            }
        }
    }
}
