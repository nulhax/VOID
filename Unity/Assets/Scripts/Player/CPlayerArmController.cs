using UnityEngine;
using System.Collections;

public class CPlayerArmController : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		gameObject.GetComponent<CPlayerInteractor> ().EventUse += OnUse;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
		
	void OnUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
	{

	}
}
