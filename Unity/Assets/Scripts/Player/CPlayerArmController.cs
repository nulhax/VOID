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
    public delegate void DisableToolRotation(bool _bUseHeadRotation);
    public event DisableToolRotation EventDisableToolRotation;
		
	// Member Properties

	//Member variables
	HoldState				m_eHoldState;
    HoldState               m_ePreviousHoldState;
	CPlayerIKController		m_IKController;
    bool                    m_bInteracting;
	bool 					m_bRighthanded;
    const float             m_kfInteractionDistance = 2.0f;
    GameObject              m_heldTool = null;

    Vector3 m_vInitialToolEquipedPosition;
    Vector3 m_vToolEquipedPosition;
    Vector3 m_vToolUnequipedPosition;
    Transform m_EquipTransform;

    float m_fLateralDeviation = 0.3f;
    float m_fVerticalDeviation = 0.4f;    
    float m_fLateralRotationThreshold = 60.0f;
    float m_fVerticalRotationThreshold = 45.0f;   

    ushort m_MovementState;

    CPlayerBelt.ESwitchToolState m_eSwitchToolState;

	static int m_iNumVelocities = 5;
	//Right Hand velocities
	Vector3[] m_aRightHandVelocities = new Vector3[m_iNumVelocities];
	int m_iRightHandVelocityIndex = 0;
	//Left Hand velocities
	Vector3[] m_aLeftHandVelocities = new Vector3[m_iNumVelocities];
	Vector3 m_vPreviousPosition = new Vector3();
	int m_iLeftHandVelocityIndex = 0;

	// Use this for initialization
	void Start () 
	{
		gameObject.GetComponent<CPlayerInteractor> ().EventPrimary += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventSecondary += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventUse += OnUse;
		gameObject.GetComponent<CPlayerInteractor> ().EventTargetChange += OnTargetChange;

		gameObject.GetComponent<CPlayerBelt> ().EventEquipedToolChanged += OnToolChange;
        gameObject.GetComponent<CPlayerBelt> ().EventSwitchStateChanged += OnSwitchStateChange;

		m_eHoldState = HoldState.NoTool;

		m_IKController = gameObject.GetComponent<CPlayerIKController>();

        m_vToolEquipedPosition = GetComponent<CPlayerInterface>().Model.transform.FindChild("ToolActive").transform.localPosition;
        m_vInitialToolEquipedPosition = m_vToolEquipedPosition;
        
        m_vToolUnequipedPosition = GetComponent<CPlayerInterface>().Model.transform.FindChild("ToolDeactive").transform.localPosition;

        m_EquipTransform = GetComponent<CPlayerInterface>().Model.transform.FindChild("ToolActive").transform;

        gameObject.GetComponent<CPlayerMotor>().EventInputStatesChange += NotifyMovementStateChange;
	}

    void NotifyMovementStateChange(ushort _usPreviousStates, ushort _usNewSates)
    {
        if (m_heldTool == null)
        {
            return;
        }

        m_MovementState = _usNewSates;        
        bool bRunForward;
        bool bRunBack;
        bool bSprint;              
        
        bRunForward     =   ((m_MovementState & (uint)CPlayerMotor.EInputState.Forward)     > 0) ? true : false;   
        bRunBack        =   ((m_MovementState & (uint)CPlayerMotor.EInputState.Backward)    > 0) ? true : false;         
        bSprint         =   ((m_MovementState & (uint)CPlayerMotor.EInputState.Run)         > 0) ? true : false;  


        if (bRunForward || bRunBack && !bSprint)
        {
            m_IKController.RightHandIKWeightTarget = 0.5f; 
            if(m_eHoldState == HoldState.TwoHandedTool)
            {
                m_IKController.LeftHandIKWeight = 1.0f;
            }
            if(EventDisableToolRotation != null)EventDisableToolRotation(true);
        }
        if (bRunForward && bSprint)
        {
            m_IKController.RightHandIKWeightTarget = 0.0f;
            if(m_eHoldState == HoldState.TwoHandedTool)
            {
                m_IKController.LeftHandIKWeight = 0.0f;
            }

            if(EventDisableToolRotation != null)EventDisableToolRotation(false);           
        } 
        if(!bRunForward && !bSprint)
        {
            m_IKController.RightHandIKWeightTarget = 1.0f; 

            if(m_eHoldState == HoldState.TwoHandedTool)
            {
                m_IKController.LeftHandIKWeight = 1.0f;
            }

            if(EventDisableToolRotation != null)EventDisableToolRotation(true);          
        }
    }   

    void Update()
    {
        if (m_bInteracting)
        {
            GameObject head = GetComponent<CPlayerHead>().Head;
            Ray headRay = new Ray(head.transform.position, head.transform.forward);
            RaycastHit _RaycastHit = new RaycastHit();

            Debug.DrawLine(head.transform.position, head.transform.position + head.transform.forward * 5);

            Physics.Raycast(headRay, out _RaycastHit, 5.0f, 1 << CGameCameras.MainCamera.layer);

			Quaternion qHandRotation = transform.rotation;
			Vector3 vHandRotation = qHandRotation.eulerAngles;
			vHandRotation.x = -45;
            Vector3 vCamRotation = head.transform.rotation.eulerAngles;
			vHandRotation.y = vCamRotation.y;

			qHandRotation = Quaternion.Euler(vHandRotation);

			Vector3 handPos = _RaycastHit.point;
				
		    switch (m_eHoldState)
            {
                case HoldState.NoTool:
                {
					m_IKController.SetRightHandTarget(handPos,qHandRotation, false);
                    m_EquipTransform.position = handPos;
                    break;
                }
                case HoldState.OneHandedTool:
                {
					m_IKController.SetLeftHandTarget(handPos, qHandRotation, false); 
                    m_EquipTransform.position = handPos;
                    break;  
                }
                case HoldState.TwoHandedTool:
                {
					m_IKController.SetLeftHandTarget(handPos, qHandRotation, false);                    
                    m_EquipTransform.position = handPos;
                    break;
                }
            }
        }

        if (m_eHoldState == HoldState.TwoHandedTool)
        {
            GetComponent<CPlayerMotor>().m_bRealignBodyWithHead = true;
        }       
    }

    void FixedUpdate()
    {
        if (m_heldTool != null) 
        {
            //Get variables from current tool's orientation script
            CToolOrientation cToolOrientation = m_heldTool.GetComponent<CToolOrientation>();

            if(gameObject == CGamePlayers.SelfActor)
            {
                m_vToolEquipedPosition = cToolOrientation.FirstPersonPosition;
            }
            else
            {
                m_vToolEquipedPosition = cToolOrientation.ThirdPersonPosition;
            }

            m_vInitialToolEquipedPosition = m_vToolEquipedPosition;
            
            m_fLateralDeviation = cToolOrientation.LateralDeviation;
            m_fVerticalDeviation = cToolOrientation.VerticalDeviation;
            
            UpdateVerticalToolPositioning();
            UpdateLateralToolPositioning();

            if(m_eSwitchToolState == CPlayerBelt.ESwitchToolState.INVALID)
            {
                m_EquipTransform.localPosition = m_vToolEquipedPosition;			   
            }

            m_EquipTransform.rotation = m_heldTool.GetComponent<CToolInterface>().m_RightHandPos.transform.rotation;
            m_heldTool.GetComponent<CToolOrientation>().ModifiedPosition = m_vToolEquipedPosition;
        }

        //Handle placement of hands
        if (m_heldTool != null)
        {
            switch (m_eHoldState)
            {
                case HoldState.OneHandedTool:
                {
                    Vector3 rightHandIKPos = GetComponent<Animator>().GetIKPosition(AvatarIKGoal.RightHand);
						
					//Determine average hand velocity
					Vector3 currentRightHandVelocity = rigidbody.GetPointVelocity(rightHandIKPos);					
					currentRightHandVelocity = GetRightHandAverageVelocity(currentRightHandVelocity);
					GameObject head = GetComponent<CPlayerHead>().Head;

					Vector3 rightHandPos = m_EquipTransform.position + currentRightHandVelocity * Time.deltaTime;
				    m_IKController.RightHandIKPos = rightHandPos;
					m_IKController.RightHandIKRot = head.transform.rotation * Quaternion.Euler(m_heldTool.GetComponent<CToolOrientation>().RightHandRotation);
                    
                    break;  
                }
                case HoldState.TwoHandedTool:
                {
					GameObject head = GetComponent<CPlayerHead>().Head;
                    Vector3 rightHandIKPos = GetComponent<Animator>().GetIKPosition(AvatarIKGoal.RightHand);
					Vector3 rightHandPos = m_EquipTransform.position + rigidbody.GetPointVelocity(rightHandIKPos) * Time.fixedDeltaTime;                    
                    m_IKController.RightHandIKPos = rightHandPos;
					m_IKController.RightHandIKRot = head.transform.rotation * Quaternion.Euler(m_heldTool.GetComponent<CToolOrientation>().RightHandRotation);
                
					if(!m_bInteracting)
					{
	                    //Offhand                    				
						Vector3 leftHandPos = m_heldTool.GetComponent<CToolInterface>().m_LeftHandPos.transform.position;
						//Vector3 averageVelocity = GetLeftHandAverageVelocity(leftHandPos);
						Vector3 allTheVelocities = rigidbody.GetPointVelocity(leftHandPos) + rigidbody.angularVelocity;

						m_IKController.LeftHandIKPos = leftHandPos + allTheVelocities * Time.deltaTime;
	                    m_IKController.LeftHandIKWeight = 1.0f;
						m_IKController.LeftHandIKRot = head.transform.rotation * Quaternion.Euler(m_heldTool.GetComponent<CToolOrientation>().LeftHandRotation);             					
					}
                    break;
                }
            }
        }       
    }

    void UpdateVerticalToolPositioning()
    {
        float headPitch = GetComponent<CPlayerHead>().RemoteHeadEulerX;
        if(headPitch > 180 && headPitch < 360)
        {
            headPitch -= 360;
        }
        float minRotation = -m_fVerticalRotationThreshold;
        float maxRotation = m_fVerticalRotationThreshold;
        
        float scale = maxRotation - minRotation;
        headPitch += scale / 2;
        float lerpFactor = headPitch / (scale);
        
        Vector3 maxPositionY = m_vInitialToolEquipedPosition;
        maxPositionY.y -= m_fVerticalDeviation;
        
        Vector3 minPositionY = m_vInitialToolEquipedPosition;
        minPositionY.y += m_fVerticalDeviation;
        
        Vector3 newOffset =  Vector3.Lerp(minPositionY, maxPositionY, lerpFactor);
        m_vToolEquipedPosition.y = newOffset.y;
        //m_heldTool.transform.localPosition = m_vToolEquipedPosition;
    }
    
    void UpdateLateralToolPositioning()
    {
        float offSet = GetComponent<CPlayerHead>().RemoteHeadEulerY;
        if(offSet > 180 && offSet < 360)
        {
            offSet -= 360;
        }
             
        float minRotation = -m_fLateralRotationThreshold;
        float maxRotation = m_fLateralRotationThreshold;
        
        float scale = maxRotation - minRotation;
        offSet += scale / 2;
        float lerpFactor = offSet / (scale);
        
        Vector3 maxPositionX = m_vInitialToolEquipedPosition;
        maxPositionX.x += m_fLateralDeviation;
        
        Vector3 minPositionX = m_vInitialToolEquipedPosition;
        minPositionX.x -= m_fLateralDeviation;
        
        Vector3 newOffset =  Vector3.Lerp(minPositionX, maxPositionX, lerpFactor);
        m_vToolEquipedPosition.x = newOffset.x;
        //m_heldTool.transform.localPosition = m_vToolEquipedPosition;
    }
			
	void OnUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
	{
		//Check that interaction is valid and the player is not interacting with a module
		if (_bDown && _cActorInteractable.GetComponent<CDUIConsole>() != null &&
		    _cRaycastHit.distance < m_kfInteractionDistance)
        {
            m_bInteracting = true;

			//Play Sound
			foreach(CAudioCue cue in GetComponents<CAudioCue>())
			{
				if(cue.m_strCueName == "PlayerSFX")
				{
					cue.Play(transform, 1.0f, false, 12);
				}
			}
           
            switch (m_eHoldState)
            {
                case HoldState.NoTool:
                    {	
                        m_IKController.SetRightHandTarget(_cRaycastHit.point, GetComponent<CPlayerHead>().HeadRotation, true);											                                                
                        break;
                    }
                case HoldState.OneHandedTool:
                    {
                        m_IKController.SetLeftHandTarget(_cRaycastHit.point, GetComponent<CPlayerHead>().HeadRotation, true);                     
                        break;	
                    }
                case HoldState.TwoHandedTool:
                    {
                        m_IKController.SetLeftHandTarget(_cRaycastHit.point, GetComponent<CPlayerHead>().HeadRotation, true);                       						
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

    void LowerBothArms()
    {
        m_bInteracting = false;

        m_IKController.EndRightHandIK(); 
        m_IKController.EndLeftHandIK();     
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
            CToolInterface toolInterface = _Tool.GetComponent<CToolInterface> ();
            GetComponent<CAudioCue>().Play(1.0f,false,7);

            switch (toolInterface.m_eToolCategory)
			{
				case CToolInterface.EToolCategory.OneHanded:
				{  
                    EndInteraction();

                    m_eHoldState = HoldState.OneHandedTool;  
                    m_IKController.RightHandIKWeightTarget = 1.0f;
                   
					break;
				}

				case CToolInterface.EToolCategory.TwoHanded:
				{
                    EndInteraction();

					m_eHoldState = HoldState.TwoHandedTool;
                    m_IKController.RightHandIKWeightTarget = 1.0f;
                    m_IKController.LeftHandIKWeightTarget = 1.0f;
                   
					break;
				}
			}
		} 
		else 
		{
            LowerBothArms();      
            m_eHoldState = HoldState.NoTool;
            m_heldTool = null;
		}
	}

    void OnSwitchStateChange(CPlayerBelt.ESwitchToolState _newState)
    {
        m_eSwitchToolState = _newState; 
    }

	Vector3 GetRightHandAverageVelocity(Vector3 _currentVelocity)
	{
		m_aRightHandVelocities[m_iRightHandVelocityIndex] = _currentVelocity;
		
		if(m_iRightHandVelocityIndex < m_iNumVelocities - 1)
		{
			m_iRightHandVelocityIndex++;
		}
		else
		{
			m_iRightHandVelocityIndex = 0;
		}
		
		Vector3 rightHandAverageVelocity = new Vector3();
		foreach(Vector3 velocity in m_aRightHandVelocities)
		{
			rightHandAverageVelocity += velocity;
		}
		
		rightHandAverageVelocity /= m_iNumVelocities;

		return(rightHandAverageVelocity);
	}

	Vector3 GetLeftHandAverageVelocity(Vector3 _currentPosition)
	{

		Vector3 currentVelocity = _currentPosition - m_vPreviousPosition;
		m_vPreviousPosition = _currentPosition;

		m_aLeftHandVelocities[m_iLeftHandVelocityIndex] = currentVelocity;
		
		if(m_iLeftHandVelocityIndex < m_iNumVelocities - 1)
		{
			m_iLeftHandVelocityIndex++;
		}
		else
		{
			m_iLeftHandVelocityIndex = 0;
		}
		
		Vector3 leftHandAverageVelocity = new Vector3();
		foreach(Vector3 velocity in m_aLeftHandVelocities)
		{
			leftHandAverageVelocity += velocity;
		}
		
		leftHandAverageVelocity /= m_iNumVelocities;

		return(leftHandAverageVelocity);
	}
}
