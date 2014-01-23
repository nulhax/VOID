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
	
	//Member Delegates & Events
	public delegate void NotifyTargetChange(Vector3 _bNewTarget);
	public event NotifyTargetChange EventTargetChange;
	
	// Member Properties
	
	public Vector3 RightHandIKTarget
	{
		set { m_RightHandTarget = value; m_fLerpTimer = 0; }
		get { return (m_RightHandTarget); }
	}	
	
	//Member variables
	Animator m_ThirdPersonAnim;
	
	float m_fRightHandIKWeight;
	public float m_fRightHandWeightTarget;
	Vector3 m_RightHandTarget;

	bool m_bDisableRightHandWeighting = false;
	float m_fLerpTime = 0.1f;
	float m_fLerpTimer = 0.0f;
	
	CNetworkVar<Vector3> m_NetworkedTarget;
	
	//Member Methods
	
	// Use this for initialization
	void Start () 
	{		
		m_ThirdPersonAnim = GetComponent<Animator>();
		EventTargetChange += UpdateTarget;
		
		gameObject.GetComponent<CPlayerInteractor>().EventInteraction += OnPlayerInteraction;
	}
	
	public void OnPlayerInteraction(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
	{
		switch (_eType) 
		{

			case CPlayerInteractor.EInteractionType.Hover:
			{
				if(!m_bDisableRightHandWeighting)
				{
					m_fRightHandWeightTarget = 0.3f;
					RightHandIKTarget = _cRayHit.point;					
				}				
				else
				{
					RightHandIKTarget = _cRayHit.point;					
				}

				break;
			}
				
			case CPlayerInteractor.EInteractionType.PrimaryStart:
			{
				m_bDisableRightHandWeighting = true;
				m_fRightHandWeightTarget = 1;
				RightHandIKTarget = _cRayHit.point;
				break;
			}

			case CPlayerInteractor.EInteractionType.PrimaryEnd:
			{
				m_bDisableRightHandWeighting = false;
								
				break;
			}
		}
	}
	
	public override void InstanceNetworkVars()
	{
		m_NetworkedTarget = new CNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
	}
	
	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(CGamePlayers.SelfActor != gameObject)
		{
			
			if (EventTargetChange != null) EventTargetChange(m_NetworkedTarget.Get());
			
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
			// Retrieve my actor motor
			CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
			
			// Write movement and rotation states
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
			// Retrieve player actor motor
			CPlayerIKController cPlayerIKController = cPlayerActor.GetComponent<CPlayerIKController>();
			
			ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
			
			switch (eNetworkAction)
			{
			case ENetworkAction.UpdateTarget:
			{
				cPlayerIKController.m_RightHandTarget.x = _cStream.ReadFloat();
				cPlayerIKController.m_RightHandTarget.y = _cStream.ReadFloat();
				cPlayerIKController.m_RightHandTarget.z = _cStream.ReadFloat();	
				
				cPlayerIKController.m_NetworkedTarget.Set(cPlayerIKController.m_RightHandTarget);					
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
		LerpToTarget();		
	}
	
	void LerpToTarget()
	{
		if(m_fRightHandIKWeight != m_fRightHandWeightTarget)
		{
			m_fLerpTimer += Time.deltaTime;
			
			float LerpFactor = m_fLerpTimer / m_fLerpTime;
			m_fRightHandIKWeight = Mathf.Lerp(m_fRightHandIKWeight, m_fRightHandWeightTarget, LerpFactor);
		}
		else
		{
			if(!m_bDisableRightHandWeighting)
			{
				m_fLerpTimer = 0.0f;
				m_fRightHandWeightTarget = 0.0f;
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
				m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandTarget);
				m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
			}			
		}
	}
}
