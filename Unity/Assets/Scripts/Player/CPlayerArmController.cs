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
    bool                    m_bInteracting;

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

    void LateUpdate()
    {
        if (m_bInteracting)
        {
            Ray cameraRay = new Ray(CGameCameras.MainCamera.transform.position, CGameCameras.MainCamera.transform.forward);
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(cameraRay, out hit);

            switch (m_eHoldState)
            {
                case HoldState.NoTool:
                {
                    m_IKController.SetRightHandTarget(hit.point, CGameCameras.MainCamera.transform.rotation * Quaternion.Euler(-90,0,0));
                    m_IKController.RightHandIKWeight = 1.0f;
                    break;
                }
                case HoldState.OneHandedTool:
                {
                    m_IKController.SetLeftHandTarget(hit.point, CGameCameras.MainCamera.transform.rotation * Quaternion.Euler(-90,0,0));
                    m_IKController.LeftHandIKWeight = 1.0f;
                    break;  
                }
                case HoldState.TwoHandedTool:
                {
                    m_IKController.SetLeftHandTarget(hit.point, CGameCameras.MainCamera.transform.rotation * Quaternion.Euler(-90,0,0));
                    m_IKController.LeftHandIKWeight = 1.0f;
                    break;
                }
            }
        }
    }
			
	void OnUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
	{
        if (_bDown)
        {
            m_bInteracting = true;

            switch (m_eHoldState)
            {
                case HoldState.NoTool:
                    {
                        m_IKController.SetRightHandTarget(_cRaycastHit.point, Quaternion.identity);
                        m_IKController.RightHandIKWeight = 1.0f;
                        break;
                    }
                case HoldState.OneHandedTool:
                    {
                        m_IKController.SetLeftHandTarget(_cRaycastHit.point, Quaternion.identity);
                        m_IKController.LeftHandIKWeight = 1.0f;
                        break;	
                    }
                case HoldState.TwoHandedTool:
                    {
                        m_IKController.SetLeftHandTarget(_cRaycastHit.point, Quaternion.identity);
                        m_IKController.LeftHandIKWeight = 1.0f;
                        break;
                    }
            }
        }
        else
        {
            m_bInteracting = false;

            switch (m_eHoldState)
            {
                case HoldState.NoTool:
                {
                    m_IKController.EndRightHandIK();
                    
                    break;
                }
                case HoldState.OneHandedTool:
                {
                    m_IKController.EndLeftHandIK();

                    break;  
                }
                case HoldState.TwoHandedTool:
                {
                    m_IKController.EndLeftHandIK();

                    break;
                }
            }
        }
	}

	void ToolChange(GameObject _tool)
	{

	}
}
