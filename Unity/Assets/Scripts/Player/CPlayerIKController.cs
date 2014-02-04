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
	
	//Member Delegates & Events
	public delegate void NotifyTargetChange(Vector3 _bNewTarget);
	public event NotifyTargetChange EventTargetChange;
	
	// Member Properties
	
	public Vector3 RightHandIKTarget
	{
		set { m_RightHandTarget = value; m_fRightHandLerpTimer = 0; }
		get { return (m_RightHandTarget); }
	}

    public Vector3 LeftHandIKTarget
    {
        set { m_LeftHandTarget = value; m_fLeftHandLerpTimer = 0; }
        get { return (m_LeftHandTarget); }
    }   
	
	//Member variables
	Animator m_ThirdPersonAnim;

	//Right hand
	float 					m_fRightHandIKWeight;
	public float 			m_fRightHandWeightTarget;
    Vector3					m_RightHandTarget;
	bool 					m_bDisableRightHandWeighting = false;
	float 					m_fRightHandLerpTime = 0.1f;
	float 					m_fRightHandLerpTimer = 0.0f;	
    Transform               m_RightShoulder;
	CNetworkVar<Vector3> 	m_RightHandNetworkedTarget;

	//Left hand
	float 					m_fLeftHandIKWeight;
	public float 			m_fLeftHandWeightTarget;
	Vector3					m_LeftHandTarget;
	bool 					m_bDisableLeftHandWeighting = false;
	float 					m_fLeftHandLerpTime = 0.1f;
	float 					m_fLeftHandLerpTimer = 0.0f;	
	CNetworkVar<Vector3> 	m_LeftHandNetworkedTarget;

	EToolState 				m_eToolState;
	Transform 				m_equippedTool;
	
	//Member Methods
	
	// Use this for initialization
	void Start () 
	{		
		m_eToolState = EToolState.NoTool;

		m_ThirdPersonAnim = GetComponent<Animator>();
		EventTargetChange += UpdateTarget;
		
		gameObject.GetComponent<CPlayerInteractor>().EventInteraction += OnPlayerInteraction;
		gameObject.GetComponent<CPlayerBelt>().EventEquipTool += EquipTool;     
      
        //Find players right shoulder
        Transform[]children = gameObject.GetComponentsInChildren<Transform>();
        foreach(Transform child in children)
        {
            if(child.name == "RightShoulder")
            {
                m_RightShoulder = child;
            }
        }     
	} 

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_RightHandNetworkedTarget = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_LeftHandNetworkedTarget = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
	}

	[AServerOnly]
	void EquipTool(GameObject _Tool)
	{
        if(_Tool != null)
		m_equippedTool = _Tool.transform;
        gameObject.GetComponent<CThirdPersonAnimController>().IsHoldingTool = true;
	}
	
	public void OnPlayerInteraction(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
	{
       
	}
    	
	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(CGamePlayers.SelfActor != gameObject)
		{			
			if (EventTargetChange != null) EventTargetChange(m_RightHandNetworkedTarget.Get());			
		}
	}
	
	void UpdateTarget(Vector3 _newTarget)
	{ 
        RightHandIKTarget = _newTarget;				
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
      
	}
	
	void LerpToTarget()
	{
		if(m_fRightHandIKWeight != m_fRightHandWeightTarget)
		{
			m_fRightHandLerpTimer += Time.deltaTime;
			
			float LerpFactor = m_fRightHandLerpTimer / m_fRightHandLerpTime;
			m_fRightHandIKWeight = Mathf.Lerp(m_fRightHandIKWeight, m_fRightHandWeightTarget, LerpFactor);
		}
		else
		{
			if(!m_bDisableRightHandWeighting)
			{
				m_fRightHandLerpTimer = 0.0f;
			}
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
				m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
			}			
		}
	}  
}
