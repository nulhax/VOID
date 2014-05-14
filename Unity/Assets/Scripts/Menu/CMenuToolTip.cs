using UnityEngine;
using System.Collections;

public class CMenuToolTip : MonoBehaviour {

	public UILabel Label;

	// Use this for initialization
	void Start () 
	{
		OnHover(false);
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	void OnHover(bool _bool)
	{
		Label.enabled = false; 

		if(_bool)
		{
			Label.enabled = true;
		}
		else
		{
			Label.enabled = false;
		}
	}


}
