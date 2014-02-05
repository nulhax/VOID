using UnityEngine;
using System.Collections;

public class CHullBreachNode : MonoBehaviour
{
	public Mesh goodMesh = null;
	public Mesh breachedMesh = null;
	public CHullBreachNode[] childBreaches = new CHullBreachNode[0];

	private bool breached = false;

	void Awake()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
	}

	void Start()
	{

	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
	}

	static void OnSetState(GameObject gameObject, byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Hull breached.
				{
					CHullBreachNode hullBreachNode = gameObject.GetComponent<CHullBreachNode>();

					if (!hullBreachNode.breached)
					{
						CGameShips.Ship.transform.position = Vector3.zero;
						gameObject.GetComponent<MeshFilter>().sharedMesh = hullBreachNode.breachedMesh;
						gameObject.GetComponent<MeshCollider>().sharedMesh = hullBreachNode.breachedMesh;
						
						Transform parentTransform = gameObject.transform.parent;
						if(parentTransform != null)
						{
							CFacilityHull parentHull = parentTransform.GetComponent<CFacilityHull>();
							if (parentHull)
								parentHull.AddBreach(gameObject);
						}
					}
				}
				break;

			case 2:	// Hull fixed.
				{
					CHullBreachNode hullBreachNode = gameObject.GetComponent<CHullBreachNode>();

					if (hullBreachNode.breached)
					{
						gameObject.GetComponent<MeshFilter>().sharedMesh = hullBreachNode.goodMesh;
						gameObject.GetComponent<MeshCollider>().sharedMesh = hullBreachNode.goodMesh;

						Transform parentTransform = gameObject.transform.parent;
						if (parentTransform != null)
						{
							CFacilityHull parentHull = parentTransform.GetComponent<CFacilityHull>();
							if (parentHull)
								parentHull.RemoveBreach(gameObject);
						}
					}
				}
				break;
		}
	}
}