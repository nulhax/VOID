using UnityEngine;
using System.Collections;

public class CPanelSwitch : MonoBehaviour {

	public GameObject currentPanelObject;
	public GameObject nextPanelObject;
	
	void OnClick() 
	{
		NGUITools.SetActive(nextPanelObject, true);
		NGUITools.SetActive(currentPanelObject, false);
	}
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
