using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CActorHealth))]
public class CHullBreachNode : MonoBehaviour
{
	public Mesh goodMesh = null;
	public Mesh breachedMesh = null;

	void Awake()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
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
					gameObject.GetComponent<MeshFilter>().sharedMesh = hullBreachNode.breachedMesh;
					gameObject.GetComponent<MeshCollider>().sharedMesh = hullBreachNode.breachedMesh;
				}
				break;

			case 2:	// Hull fixed.
				{
					CHullBreachNode hullBreachNode = gameObject.GetComponent<CHullBreachNode>();
					gameObject.GetComponent<MeshFilter>().sharedMesh = hullBreachNode.goodMesh;
					gameObject.GetComponent<MeshCollider>().sharedMesh = hullBreachNode.goodMesh;
				}
				break;
		}
	}
}