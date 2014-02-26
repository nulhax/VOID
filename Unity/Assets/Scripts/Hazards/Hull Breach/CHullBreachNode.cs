using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CActorAtmosphericConsumer))]
public class CHullBreachNode : MonoBehaviour
{
	public delegate void OnSetBreached(bool breached);

	public Mesh goodMesh = null;
	public Mesh breachedMesh = null;

	private CFacilityHull parentFacilityHull = null;

	private System.Collections.Generic.List<CHullBreachNode> childBreaches = new System.Collections.Generic.List<CHullBreachNode>();
	uint numChildrenBreached = 0;

	public OnSetBreached EventOnSetBreached;
	public bool breached { get { return breached_internal; } set { if (breached_internal != value) { breached_internal = value; if (EventOnSetBreached != null)EventOnSetBreached(value); } } }
	private bool breached_internal = false;

	private int audioClipIndex = -1;

	void Awake()
	{
		GetComponent<CActorAtmosphericConsumer>().AtmosphericConsumptionRate = 100.0f;

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

		// Add components at runtime instead of updating all the prefabs.
		{
			// CAudioCue
			CAudioCue audioCue = gameObject.AddComponent<CAudioCue>();
			audioClipIndex = audioCue.AddSound("Audio/HullBreach", 0.0f, 0.0f, true);
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

	void OnSetState(byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Hull breach threshold.
				if (!breached)	// If the hull was not breached before passing the breach threshold...
				{
					GetComponent<CAudioCue>().Play(transform, 1.0f, true, audioClipIndex);

					if(particleSystem != null)
						particleSystem.Play();

					// Breach the hull.
					if (childBreaches.Count > 0)	// If this breach is a parent...
					{
						// Remove the models set by children.
						foreach (CHullBreachNode childBreach in childBreaches)
						{
							childBreach.GetComponent<MeshFilter>().sharedMesh = null;
							childBreach.GetComponent<MeshCollider>().sharedMesh = null;
						}
					}

					// Set breached mesh model and collider.
					GetComponent<MeshFilter>().sharedMesh = breachedMesh;
					GetComponent<MeshCollider>().sharedMesh = breachedMesh;

					// Set breached state.
					breached = true;

					// Consume atmosphere.
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(true);

					// Inform the facility this breach resides in.
					if (parentFacilityHull != null)
						parentFacilityHull.AddBreach(gameObject);
				}
				break;

			case 2:	// Hull fix threshold.
				if (breached)	// If the hull was breached before passing the fix threshold...
				{
					GetComponent<CAudioCue>().StopAllSound();

					if(particleSystem != null)
						particleSystem.Stop();

					// Fix the breach.
					if (childBreaches.Count > 0)	// If this breach is a parent...
					{
						// Fix the children.
						foreach (CHullBreachNode childBreach in childBreaches)
						{
							CActorHealth childActorHealth = childBreach.GetComponent<CActorHealth>();
							childActorHealth.health = childActorHealth.health_max;	// Force all children to repair.
						}
					}

					// Set breached mesh model and collider.
					GetComponent<MeshFilter>().sharedMesh = goodMesh;
					GetComponent<MeshCollider>().sharedMesh = goodMesh;

					// Set breached state.
					breached = false;

					// Stop consuming atmosphere.
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(false);

					// Inform the facility this breach resides in.
					if (parentFacilityHull != null)
						parentFacilityHull.RemoveBreach(gameObject);
				}
				break;
		}
	}

	void OnChildSetBreached(bool breached)
	{
		//CHullBreachNode hullBreachNode = transform.parent.GetComponent<CHullBreachNode>();

		if (breached)	// If the child is now breached...
		{
			++numChildrenBreached;

			if (numChildrenBreached >= childBreaches.Count)	// If all children are breached...
			{
				CActorHealth hullBreachActorHealth = GetComponent<CActorHealth>();
				hullBreachActorHealth.health = hullBreachActorHealth.health_min;	// Force this parent to breach.
			}
		}
		else
			--numChildrenBreached;
	}
}