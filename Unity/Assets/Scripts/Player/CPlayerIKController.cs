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
		SetRightTransform,
		SetLeftTransform,
	}
	
	//Member Delegates & Events
	
	
	// Member Properties
	
	public Transform RightHandIKTarget
	{
        set { m_RightHandTarget = value; m_fRightHandLerpTimer = 0; }			 
		get { return (m_RightHandTarget); }
	}

	public Transform LeftHandIKTarget
    {
        set { m_LeftHandTarget = value; m_fLeftHandLerpTimer = 0; }
        get { return (m_LeftHandTarget); }
    }   
	
	public float RightHandIKWeight
	{
		set { m_fRightHandIKWeight = value;  }
        get { return (m_fRightHandIKWeight); }
	}

	public float LeftHandIKWeight
	{
		set { m_fLeftHandIKWeight = value;  }
		get { return (m_fLeftHandIKWeight); }
	}
	
	//Member variables
	Animator m_ThirdPersonAnim;
    CNetworkVar<Vector3>    m_RightHandNetworkedPos;
    CNetworkVar<Vector3>    m_LeftHandNetworkedPos;

	CNetworkVar<Vector3>    m_RightHandNetworkedRot;
	CNetworkVar<Vector3>    m_LeftHandNetworkedRot;

	//Right hand
	float 					m_fRightHandIKWeight;
	Transform				m_RightHandPos;
	Transform				m_RightHandTarget;
	const float 			m_kfRightHandLerpTime = 0.75f;
	float 					m_fRightHandLerpTimer = 0.75f;
 
	//Left hand
	float 					m_fLeftHandIKWeight;
	Transform				m_LeftHandPos;
	Transform				m_LeftHandTarget;	
	const float 			m_kfLeftHandLerpTime = 0.75f;
	float 					m_fLeftHandLerpTimer = 0.75f;		

	//Member Methods
	
	// Use this for initialization
	void Start () 
	{	
		m_ThirdPersonAnim = GetComponent<Animator>();          
   	} 

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_RightHandNetworkedPos = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_RightHandNetworkedRot = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);

		m_LeftHandNetworkedPos = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_LeftHandNetworkedRot = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);   
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{ 
		if(_cSyncedNetworkVar == m_RightHandNetworkedPos)
        {
            RightHandIKTarget.position = m_RightHandNetworkedPos.Get();           
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedPos)
        {
            LeftHandIKTarget.position = m_LeftHandNetworkedPos.Get();          
        }
	} 
	
	public static void SerializeIKTarget(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;
		CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
				
		if (cSelfActor != null && cSelfIKController.m_RightHandTarget != null)
		{
			// Write rotation states
			_cStream.Write((byte)ENetworkAction.SetRightTransform);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.position.x);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.position.y);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.position.z);	      

			_cStream.Write((float)cSelfIKController.m_RightHandTarget.rotation.eulerAngles.x);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.rotation.eulerAngles.y);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.rotation.eulerAngles.z);
		}

		if (cSelfActor != null && cSelfIKController.m_LeftHandTarget != null)
		{
			// Write rotation states
			_cStream.Write((byte)ENetworkAction.SetLeftTransform);
			_cStream.Write((float)cSelfIKController.m_LeftHandTarget.position.x);
			_cStream.Write((float)cSelfIKController.m_LeftHandTarget.position.y);
			_cStream.Write((float)cSelfIKController.m_LeftHandTarget.position.z);	      
			
			_cStream.Write((float)cSelfIKController.m_LeftHandTarget.rotation.eulerAngles.x);
			_cStream.Write((float)cSelfIKController.m_LeftHandTarget.rotation.eulerAngles.y);
			_cStream.Write((float)cSelfIKController.m_LeftHandTarget.rotation.eulerAngles.z);
		}
	}
	
	public static void UnserializeIKTarget(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);
		
		if (cPlayerActor != null)
		{			
			CPlayerIKController cPlayerIKController = cPlayerActor.GetComponent<CPlayerIKController>();
			
			ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
			
			switch (eNetworkAction)
			{
			case ENetworkAction.SetRightTransform:
			{
				Vector3 pos = new Vector3();
				Vector3 rot = new Vector3();

				pos.x = _cStream.ReadFloat();
				pos.y = _cStream.ReadFloat();
				pos.z = _cStream.ReadFloat();

				cPlayerIKController.m_RightHandTarget.position = pos;

				rot.x = _cStream.ReadFloat();
				rot.y = _cStream.ReadFloat();
				rot.z = _cStream.ReadFloat();	

				cPlayerIKController.m_RightHandTarget.rotation = Quaternion.Euler(rot);
				
				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_RightHandTarget.position);              
				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_RightHandTarget.rotation.eulerAngles);
			}
				break;

			case ENetworkAction.SetLeftTransform:
			{
				Vector3 pos = new Vector3();
				Vector3 rot = new Vector3();
				
				pos.x = _cStream.ReadFloat();
				pos.y = _cStream.ReadFloat();
				pos.z = _cStream.ReadFloat();
				
				cPlayerIKController.m_LeftHandTarget.position = pos;
				
				rot.x = _cStream.ReadFloat();
				rot.y = _cStream.ReadFloat();
				rot.z = _cStream.ReadFloat();	
				
				cPlayerIKController.m_LeftHandTarget.rotation = Quaternion.Euler(rot);
				
				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_LeftHandTarget.position);              
				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_LeftHandTarget.rotation.eulerAngles);
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
       	LerpRightHand();             				
		LerpLeftHand();
	}  

    void LerpRightHand()
    {
        if(m_fRightHandLerpTimer < m_kfRightHandLerpTime)
        {
            m_fRightHandLerpTimer += Time.deltaTime;   
			
			Vector3 pos = m_ThirdPersonAnim.GetIKPosition(AvatarIKGoal.RightHand);			
            float LerpFactor = m_fRightHandLerpTimer / m_kfRightHandLerpTime;

			if (m_RightHandPos != null)
				m_RightHandPos.position = Vector3.Lerp(pos, m_RightHandTarget.position, LerpFactor);
        }        
    }

	void LerpLeftHand()
	{
		if(m_fLeftHandLerpTimer < m_kfLeftHandLerpTime)
		{
			m_fLeftHandLerpTimer += Time.deltaTime;   
			
			Vector3 pos = m_ThirdPersonAnim.GetIKPosition(AvatarIKGoal.LeftHand);			
			float LerpFactor = m_fLeftHandLerpTimer / m_kfLeftHandLerpTime;

			if (m_LeftHandPos != null)
				m_LeftHandPos.position = Vector3.Lerp(pos, m_LeftHandTarget.position, LerpFactor);
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

				if (m_RightHandPos != null)
				{
					m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandPos.position);
					m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, m_RightHandPos.rotation);
				}
			}	

			//Left hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_LeftHandTarget != null)
			{
				m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_fLeftHandIKWeight);
				m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_fLeftHandIKWeight);

				if (m_LeftHandPos != null)
				{
					m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.LeftHand, m_LeftHandPos.position);
					m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.LeftHand, m_LeftHandPos.rotation);
				}
			}
		}
	}  
}
