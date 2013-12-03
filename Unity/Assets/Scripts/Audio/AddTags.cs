using UnityEngine;
using System.Collections;

public class AddTags : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		int i =0;
		Transform[] children = gameObject.GetComponentsInChildren<Transform>();
		
		foreach(Transform child in children)
		{
			child.tag = "Listener";
		}		
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
