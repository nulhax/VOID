using UnityEngine;
using System.Collections;

public class CExit : MonoBehaviour {
	public void Quit()
	{
		Application.Quit();
		
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
