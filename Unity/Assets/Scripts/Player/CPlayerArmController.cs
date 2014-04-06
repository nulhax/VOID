//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerIKController
//  Description :   Controls player IK, allowing a rigged character to place body parts at specified locations
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */

public class CPlayerArmController : MonoBehaviour 
{
	//Member Types
	enum HoldState
	{
		Invalid,

		NoTool,
		OneHandedTool,
		TwoHandedTool,

		Max,
	}

	//Member Delegates & Events
		
	// Member Properties

	//Member variables
	HoldState				m_eHoldState;
	CPlayerIKController		m_IKController;

	// Use this for initialization
	void Start () 
	{
		gameObject.GetComponent<CPlayerInteractor> ().EventPrimary += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventSecondary += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventUse += OnUse;
		gameObject.GetComponent<CPlayerBelt>().EventToolDropped += ToolChange;
		gameObject.GetComponent<CPlayerBelt>().EventToolPickedup += ToolChange;

		m_eHoldState = HoldState.NoTool;

		m_IKController = gameObject.GetComponent<CPlayerIKController>();
	}
			
	void OnUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
	{
		switch(m_eHoldState)
		{
			case HoldState.NoTool:
			{
				m_IKController.RightHandIKTargetPos = _cRaycastHit.point;
				m_IKController.RightHandIKWeight = 1.0f;
				break;
			}
			case HoldState.OneHandedTool:
			{
				m_IKController.LeftHandIKTargetPos = _cRaycastHit.point;
				m_IKController.LeftHandIKWeight = 1.0f;
				break;	
			}
			case HoldState.TwoHandedTool:
			{
				m_IKController.LeftHandIKTargetPos = _cRaycastHit.point;
				m_IKController.LeftHandIKWeight = 1.0f;
				break;
			}
		}
	}

	void ToolChange(GameObject _tool)
	{

	}
}
