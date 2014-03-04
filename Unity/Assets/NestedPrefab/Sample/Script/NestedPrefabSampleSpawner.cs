using UnityEngine;
using System.Collections;

[AddComponentMenu("NestedPrefabSample/NestedPrefabSampleSpawner")]
// Sample class used to spawn a hierarchical prefab with the special instantiate method
public class NestedPrefabSampleSpawner : MonoBehaviour 
{
	// The prefab to spawn	
	public GameObject prefabToSpawn;
		
	// Update is called once per frame
	private void Update() 
	{
		// If the mouse is clicked
		if(Input.GetMouseButtonUp(0))
		{
			// Spawn the prefab at the spawner position
			HierarchicalPrefabUtility.Instantiate(prefabToSpawn, transform.position, transform.rotation);
		}
	}
}
