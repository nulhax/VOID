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
    HoldState               m_ePreviousHoldState;
	CPlayerIKController		m_IKController;
    bool                    m_bInteracting;
	bool 					m_bRighthanded;
    const float             m_kfInteractionDistance = 1.5f;
    GameObject              m_heldTool = null;

	// Use this for initialization
	void Start () 
	{
		gameObject.GetComponent<CPlayerInteractor> ().EventPrimary += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventSecondary += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventUse += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventTargetChange += OnTargetChange;

		gameObject.GetComponent<CPlayerBelt> ().EventEquipedToolChanged += OnToolChange;

		m_eHoldState = HoldState.NoTool;

		m_IKController = gameObject.GetComponent<CPlayerIKController>();
	}

    void Update()
    {
        if (m_bInteracting)
        {
            Ray cameraRay = new Ray(CGameCameras.MainCamera.transform.position, CGameCameras.MainCamera.transform.forward);
            RaycastHit _RaycastHit = new RaycastHit();
            
            Physics.Raycast(cameraRay, out _RaycastHit, 5.0f, 1 << CGameCameras.MainCamera.layer);
            
            switch (m_eHoldState)
            {
                case HoldState.NoTool:
                {
                    m_IKController.SetRightHandTarget(_RaycastHit.point, CGameCameras.MainCamera.transform.rotation);
                    m_IKController.RightHandIKWeight = 1.0f;
                    break;
                }
                case HoldState.OneHandedTool:
                {
                    m_IKController.SetLeftHandTarget(_RaycastHit.point, CGameCameras.MainCamera.transform.rotation);
                    m_IKController.LeftHandIKWeight = 1.0f;
                    break;  
                }
                case HoldState.TwoHandedTool:
                {
                    m_IKController.SetLeftHandTarget(_RaycastHit.point, CGameCameras.MainCamera.transform.rotation);
                    m_IKController.LeftHandIKWeight = 1.0f;
                    break;
                }
            }
        }
    }

    void FixedUpdate()
    {
        //Handle placement of hands
        if (m_heldTool != null)
        {
            switch (m_eHoldState)
            {
                case HoldState.OneHandedTool:
                {
                    Vector3 rightHandPos = m_heldTool.GetComponent<CToolInterface>().m_RightHandPos.transform.position;

                    m_IKController.RightHandIKPos = rightHandPos + rigidbody.GetPointVelocity(rightHandPos) * Time.fixedDeltaTime;
                    m_IKController.RightHandIKRot = m_heldTool.GetComponent<CToolInterface>().m_RightHandPos.transform.rotation;
                    m_IKController.RightHandIKWeight = 1.0f;
                    break;  
                }
                case HoldState.TwoHandedTool:
                {
                    Vector3 rightHandPos = m_heldTool.GetComponent<CToolInterface>().m_RightHandPos.transform.position;
                    Vector3 leftHandPos = m_heldTool.GetComponent<CToolInterface>().m_LeftHandPos.transform.position;

                    m_IKController.RightHandIKPos = rightHandPos + rigidbody.GetPointVelocity(rightHandPos) * Time.fixedDeltaTime;
                    m_IKController.RightHandIKRot = m_heldTool.GetComponent<CToolInterface>().m_RightHandPos.transform.rotation;

                    m_IKController.LeftHandIKPos = leftHandPos + rigidbody.GetPointVelocity(leftHandPos) * Time.fixedDeltaTime;
                    m_IKController.LeftHandIKRot = m_heldTool.GetComponent<CToolInterface>().m_LeftHandPos.transform.rotation;

                    m_IKController.RightHandIKWeight = 1.0f;
                    m_IKController.LeftHandIKWeight = 1.0f;

                    break;
                }
            }
        }
    }
			
	void OnUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
	{
		//Check that interaction is valid and the player is not interacting with a module
		if (_bDown && _cActorInteractable.GetComponent<CDUIConsole>() != null &&
		    _cRaycastHit.distance < m_kfInteractionDistance)
        {
            m_bInteracting = true;
           
            switch (m_eHoldState)
            {
                case HoldState.NoTool:
                    {	
						m_IKController.SetRightHandTarget(_cRaycastHit.point, CGameCameras.MainCamera.transform.rotation);
						m_IKController.RightHandIKWeight = 1.0f;
						
                        break;
                    }
                case HoldState.OneHandedTool:
                    {
						m_IKController.SetLeftHandTarget(_cRaycastHit.point, CGameCameras.MainCamera.transform.rotation);
                        m_IKController.LeftHandIKWeight = 1.0f;
                        break;	
                    }
                case HoldState.TwoHandedTool:
                    {
						m_IKController.SetLeftHandTarget(_cRaycastHit.point, CGameCameras.MainCamera.transform.rotation);
                        m_IKController.LeftHandIKWeight = 1.0f;
						
                        break;
                    }
            }
        }
        else if(_cRaycastHit.distance < m_kfInteractionDistance)
        {
			EndInteraction();
        }
	}

	void OnTargetChange(GameObject _cOldTargetObject,  GameObject _CNewTargetObject, RaycastHit _cRaycastHit)
	{
		if (m_bInteracting) 
		{
			EndInteraction ();
		}
	}

	void EndInteraction()
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

	void OnToolChange(GameObject _Tool)
	{
		if (_Tool != null) 
		{
            m_heldTool = _Tool;

			switch (_Tool.GetComponent<CToolInterface> ().m_eToolCategory)
			{
				case CToolInterface.EToolCategory.OneHanded:
				{
                    m_eHoldState = HoldState.OneHandedTool;                   
                    EndInteraction();
					break;
				}

				case CToolInterface.EToolCategory.TwoHanded:
				{
					m_eHoldState = HoldState.TwoHandedTool;
                    EndInteraction();
					break;
				}
			}
		} 
		else 
		{
			m_eHoldState = HoldState.NoTool;
            m_heldTool = null;
            EndInteraction();
		}
	}
}
