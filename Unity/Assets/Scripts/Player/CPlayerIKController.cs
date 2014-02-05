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
using System.Collections.Generic;

/* Implementation */

public class CPlayerIKController : CNetworkMonoBehaviour
{
	
	//Member Types
	public enum ENetworkAction : byte
	{
		UpdateTarget,
	}

	public enum EToolState
	{
		NoTool,
		SingleHandTool,
		TwoHandTool,
	}

    public enum ERepairState
    {
        Inactive,
        Repairing,
    }
	
	//Member Delegates & Events
	public delegate void NotifyTargetChange(Vector3 _bNewTarget);
	public event NotifyTargetChange EventTargetChange;
	
	// Member Properties
	
	public Vector3 RightHandIKTarget
	{
        set { m_RightHandTarget = value; m_fRightHandLerpTimer = 0;}
		get { return (m_RightHandTarget); }
	}

    public Vector3 LeftHandIKTarget
    {
        set { m_LeftHandTarget = value; m_fLeftHandLerpTimer = 0; }
        get { return (m_LeftHandTarget); }
    }   
	
	//Member variables
	Animator m_ThirdPersonAnim;
    CNetworkVar<int>        m_bHoldState = null;
    CNetworkVar<int>        m_bRepairState = null;
    CNetworkVar<Vector3>    m_RightHandNetworkedTarget;
    CNetworkVar<Vector3>    m_LeftHandNetworkedTarget;

	//Right hand
	float 					m_fRightHandIKWeight;
	public float 			m_fRightHandWeightTarget;
    Vector3					m_RightHandTarget;
	float 					m_fRightHandLerpTime = 0.5f;
	float 					m_fRightHandLerpTimer = 0.0f;	
    Transform               m_RightShoulder;
    Vector3                 m_TargetOffset;
    List<Vector3>           m_RightHandTargetList = new List<Vector3>(); 

    float                   m_fTargetChangeTimer = 0.0f;
    float                   m_fTargetChangeTime = 1.0f;

    int                     m_iTargetIndex;
    int                     m_iTotalTargets;

	//Left hand
	float 					m_fLeftHandIKWeight;
	public float 			m_fLeftHandWeightTarget;
	Vector3					m_LeftHandTarget;
	float 					m_fLeftHandLerpTime = 0.1f;
	float 					m_fLeftHandLerpTimer = 0.0f;		

	EToolState 				m_eToolState;
    ERepairState            m_eRepairState;
	    	
	//Member Methods
	
	// Use this for initialization
	void Start () 
	{		
		m_eToolState = EToolState.NoTool;
        m_eRepairState = ERepairState.Inactive;

		m_ThirdPersonAnim = GetComponent<Animator>();
		EventTargetChange += UpdateTarget;
		
		gameObject.GetComponent<CPlayerInteractor>().EventInteraction += OnPlayerInteraction;
        gameObject.GetComponent<CPlayerBelt>().EventToolPickedup += EquipTool; 
        gameObject.GetComponent<CPlayerBelt>().EventToolDropped += DropTool; 
        gameObject.GetComponent<CPlayerBelt>().EventToolChanged += ChangeTool; 
      
        m_eToolState = EToolState.NoTool;
   	} 

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_RightHandNetworkedTarget = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_LeftHandNetworkedTarget = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
        m_bHoldState = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, 0);
        m_bRepairState = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, 0);
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{       
        if(_cSyncedNetworkVar == m_bHoldState)
        {
            int iToolState = m_bHoldState.Get();

            if((iToolState & (int)EToolState.NoTool) > 0)
            {
                m_eToolState = EToolState.NoTool;
            }
            if((iToolState & (int)EToolState.SingleHandTool) > 0)
            {
                m_eToolState = EToolState.SingleHandTool;
            }
            if((iToolState & (int)EToolState.TwoHandTool) > 0)
            {
                m_eToolState = EToolState.TwoHandTool;
            }

              switch(m_eToolState)
            {
                case EToolState.NoTool:
                {                    
                    gameObject.GetComponent<CThirdPersonAnimController>().IsHoldingTool = false;
                    break;
                }
                case EToolState.SingleHandTool:
                {                    
                    gameObject.GetComponent<CThirdPersonAnimController>().IsHoldingTool = true;
                    break;
                }
                case EToolState.TwoHandTool:
                {                    
                    gameObject.GetComponent<CThirdPersonAnimController>().IsHoldingTool = true;
                    break;
                }
            }
        }

        if(_cSyncedNetworkVar == m_bRepairState)
        {
            int iRepairState = m_bRepairState.Get();
            
            if((iRepairState & (int)ERepairState.Inactive) > 0)
            {
                m_eRepairState = ERepairState.Inactive;
            }
            if((iRepairState & (int)ERepairState.Repairing) > 0)
            {
                m_eRepairState = ERepairState.Repairing;
            }           
        }

        if(_cSyncedNetworkVar == m_RightHandNetworkedTarget)
        {
            if (EventTargetChange != null) EventTargetChange(m_RightHandNetworkedTarget.Get());         
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedTarget)
        {
            if (EventTargetChange != null) EventTargetChange(m_LeftHandNetworkedTarget.Get());         
        }
	} 
	
	public static void SerializeIKTarget(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;
		
		if (cSelfActor != null)
		{
			CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
			
			// Write rotation states
			_cStream.Write((byte)ENetworkAction.UpdateTarget);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.x);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.y);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.z);	
            _cStream.Write((int)cSelfIKController.m_eToolState);
            _cStream.Write((int)cSelfIKController.m_eRepairState);
		}
	}
	
	public static void UnserializeIKTarget(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.FindPlayerActor(_cNetworkPlayer.PlayerId);
		
		if (cPlayerActor != null)
		{			
			CPlayerIKController cPlayerIKController = cPlayerActor.GetComponent<CPlayerIKController>();
			
			ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
			
			switch (eNetworkAction)
			{
			case ENetworkAction.UpdateTarget:
			{
				cPlayerIKController.m_RightHandTarget.x = _cStream.ReadFloat();
				cPlayerIKController.m_RightHandTarget.y = _cStream.ReadFloat();
				cPlayerIKController.m_RightHandTarget.z = _cStream.ReadFloat();	
				
				cPlayerIKController.m_RightHandNetworkedTarget.Set(cPlayerIKController.m_RightHandTarget);

                //Send states to clients                 
                cPlayerIKController.m_bHoldState.Set(_cStream.ReadInt());
                cPlayerIKController.m_bRepairState.Set(_cStream.ReadInt());
			}
				break;
				
			default:
				Debug.LogError(string.Format("Unknown network action ({0})", (byte)eNetworkAction));
				break;
			}
		}		
	}
	
	// Update is called once per frame
	void Update ()
	{	      
        if (m_eRepairState == ERepairState.Repairing && CNetwork.IsServer)
        {
            TargetSwitch();
            LerpToTarget();
        }       
	}

    void EquipTool(CNetworkViewId _ToolId)
    {
        m_eToolState = EToolState.SingleHandTool;      
    }

    void DropTool(CNetworkViewId _ToolId)
    {
       //m_eToolState = EToolState.NoTool;      
    }

    void ChangeTool(CNetworkViewId _ToolId)
    {             
        if (_ToolId.GameObject != null)
        {
            GameObject tool = _ToolId.GameObject;
            m_eToolState = EToolState.SingleHandTool; 
        } 
        else
        {
            m_eToolState = EToolState.NoTool; 
        }
    }

    public void OnPlayerInteraction(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
    {
        switch (_eType)
        {
            case CPlayerInteractor.EInteractionType.PrimaryStart:
            {
                m_RightHandTarget = _cRayHit.point;
                m_fRightHandWeightTarget = 1.0f;
                m_fTargetChangeTimer = 0.0f;

                Transform[] children = _cInteractableObject.GetComponentsInChildren<Transform>();
                foreach(Transform child in children)
                {
                    if(child.name == "Transform" && m_RightHandTargetList.Contains(child.position) == false)
                    {
                        m_RightHandTargetList.Add(child.position);
                        m_iTotalTargets++;
                    }
                }

                m_iTargetIndex = 0;
                m_eRepairState = ERepairState.Repairing;

                break;
            }

            case CPlayerInteractor.EInteractionType.PrimaryEnd:
            {
              // m_RightHandTarget = new Vector3(0,0,0);
              // m_fRightHandWeightTarget = 0.0f;
              // m_eRepairState = ERepairState.Inactive;
                break;
            }
        }
    }
    
    void UpdateTarget(Vector3 _newTarget)
    { 
        RightHandIKTarget = _newTarget;             
    }	

    void TargetSwitch()
    {
        if (m_RightHandTargetList.Count > 0)
        {
            m_fTargetChangeTimer += Time.deltaTime;
            
            if(m_fTargetChangeTimer > m_fTargetChangeTime)
            {
                //Select next target
                if(m_iTargetIndex < m_iTotalTargets - 1)
                {
                    m_iTargetIndex++;
                }
                else
                {
                    m_iTargetIndex = 0;
                }

                //reset timer
                m_fTargetChangeTimer = 0;
              
                //Set new target and lerp vairables
                RightHandIKTarget = m_RightHandTargetList[m_iTargetIndex];      

                m_fRightHandIKWeight = 1.0f;
                m_fRightHandWeightTarget = 1.0f;
                m_fRightHandLerpTimer = 0.0f;

                Debug.Log("Target switched to: " + RightHandIKTarget.ToString());               
            }
        }
    }

    void LerpToTarget()
    {
        if(m_fRightHandIKWeight != m_fRightHandWeightTarget)
        {
            m_fRightHandLerpTimer += Time.deltaTime;            
            float LerpFactor = m_fRightHandLerpTimer / m_fRightHandLerpTime;
            m_fRightHandIKWeight = Mathf.Lerp(m_fRightHandIKWeight, m_fRightHandWeightTarget, LerpFactor);
        }        
    }
	
	void OnAnimatorIK()
	{
		if(m_ThirdPersonAnim) 
		{		
			//Right hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_RightHandTarget != null)
			{
                m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);
                m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);
				
				Quaternion handRotation = Quaternion.LookRotation(m_RightHandTarget - transform.position);
				handRotation *= Quaternion.Euler(0,0,0);

				m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandTarget);
				//m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
			}			
		}
	}  
}
