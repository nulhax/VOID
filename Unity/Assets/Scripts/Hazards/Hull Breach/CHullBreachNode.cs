using UnityEngine;
using System.Collections;

public class CHullBreachNode : MonoBehaviour
{
	public delegate void OnSetBreached(GameObject gameObject, bool breached);

	public Mesh goodMesh = null;
	public Mesh breachedMesh = null;

	private CFacilityHull parentFacilityHull = null;

	private System.Collections.Generic.List<CHullBreachNode> childBreaches = new System.Collections.Generic.List<CHullBreachNode>();
	uint numChildrenBreached = 0;

	public OnSetBreached EventOnSetBreached;
	public bool breached { get { return breached_internal; } set { if (breached_internal != value) { breached_internal = value; if (EventOnSetBreached != null)EventOnSetBreached(gameObject, value); } } }
	private bool breached_internal = false;

	void Awake()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;

		Transform parentTransform = gameObject.transform.parent;
		if (parentTransform != null)
		{
			CFacilityHull parentHull = parentTransform.GetComponent<CFacilityHull>();
			if (parentHull)
				SetFacilityHull(parentHull);
		}

		foreach (Transform child in transform)
		{
			CHullBreachNode childBreachNode = child.GetComponent<CHullBreachNode>();
			if (childBreachNode != null)
			{
				childBreachNode.EventOnSetBreached += OnChildSetBreached;
				childBreaches.Add(childBreachNode);
			}
		}
	}

	private void SetFacilityHull(CFacilityHull facilityHull)
	{
		parentFacilityHull = facilityHull;

		foreach (Transform child in transform)
		{
			CHullBreachNode childBreachNode = child.GetComponent<CHullBreachNode>();
			if (childBreachNode != null)
				childBreachNode.SetFacilityHull(facilityHull);
		}
	}

	void Start()
	{

	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;

		foreach (Transform child in transform)
		{
			CHullBreachNode childBreachNode = child.GetComponent<CHullBreachNode>();
			if (childBreachNode != null)
				childBreachNode.EventOnSetBreached -= OnChildSetBreached;
		}
	}

	static void OnSetState(GameObject gameObject, byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Hull breach threshold.
				{
					CHullBreachNode hullBreachNode = gameObject.GetComponent<CHullBreachNode>();

					if (!hullBreachNode.breached)	// If the hull was not breached before passing the breach threshold...
					{
						// Breach the hull.
						if (hullBreachNode.childBreaches.Count > 0)	// If this breach is a parent...
						{
							// Remove the models set by children.
							foreach (CHullBreachNode childBreach in hullBreachNode.childBreaches)
							{
								childBreach.GetComponent<MeshFilter>().sharedMesh = null;
								childBreach.GetComponent<MeshCollider>().sharedMesh = null;
							}
						}

						// Set breached mesh model and collider.
						gameObject.GetComponent<MeshFilter>().sharedMesh = hullBreachNode.breachedMesh;
						gameObject.GetComponent<MeshCollider>().sharedMesh = hullBreachNode.breachedMesh;

						// Set breached state.
						hullBreachNode.breached = true;

						// Inform the facility this breach resides in.
						if (hullBreachNode.parentFacilityHull != null)
							hullBreachNode.parentFacilityHull.AddBreach(gameObject);
					}
				}
				break;

			case 2:	// Hull fix threshold.
				{
					CHullBreachNode hullBreachNode = gameObject.GetComponent<CHullBreachNode>();

					if (hullBreachNode.breached)	// If the hull was breached before passing the fix threshold...
					{
						// Fix the breach.
						if (hullBreachNode.childBreaches.Count > 0)	// If this breach is a parent...
						{
							// Fix the children.
							foreach (CHullBreachNode childBreach in hullBreachNode.childBreaches)
							{
								CActorHealth childActorHealth = childBreach.GetComponent<CActorHealth>();
								childActorHealth.health = childActorHealth.health_max;	// Force all children to repair.
							}
						}

						// Set breached mesh model and collider.
						gameObject.GetComponent<MeshFilter>().sharedMesh = hullBreachNode.goodMesh;
						gameObject.GetComponent<MeshCollider>().sharedMesh = hullBreachNode.goodMesh;

						// Set breached state.
						hullBreachNode.breached = false;

						// Inform the facility this breach resides in.
						if (hullBreachNode.parentFacilityHull != null)
							hullBreachNode.parentFacilityHull.RemoveBreach(gameObject);
					}
				}
				break;
		}
	}

	static void OnChildSetBreached(GameObject gameObject, bool breached)
	{
		CHullBreachNode hullBreachNode = gameObject.transform.parent.GetComponent<CHullBreachNode>();

		if (breached)	// If the child is now breached...
		{
			++hullBreachNode.numChildrenBreached;

			if (hullBreachNode.numChildrenBreached >= hullBreachNode.childBreaches.Count)	// If all children are breached...
			{
				CActorHealth hullBreachActorHealth = hullBreachNode.GetComponent<CActorHealth>();
				hullBreachActorHealth.health = hullBreachActorHealth.health_min;	// Force this parent to breach.
			}
		}
		else
			--hullBreachNode.numChildrenBreached;
	}
}