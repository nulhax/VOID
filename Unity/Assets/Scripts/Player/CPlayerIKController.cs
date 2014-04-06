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
	
	public Vector3 RightHandIKTargetPos
	{
        set { m_RightHandTargetPos = value; m_fRightHandLerpTimer = 0; }			 
		get { return (m_RightHandTargetPos); }
	}

	public Vector3 LeftHandIKTargetPos
    {
        set { m_LeftHandTargetPos = value; m_fLeftHandLerpTimer = 0; }
        get { return (m_LeftHandTargetPos); }
    }  

	public Quaternion RightHandIKTargetRot
	{
		set { m_RightHandTargetRot = value; }			 
		get { return (m_RightHandTargetRot); }
	}
		
	public Quaternion LeftHandIKTargetRot
	{
		set { m_LeftHandTargetRot = value; }			 
		get { return (m_LeftHandTargetRot); }
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
	Vector3					m_RightHandPos;
	Vector3					m_RightHandTargetPos;
	Quaternion				m_RightHandTargetRot;
	const float 			m_kfRightHandLerpTime = 0.75f;
	float 					m_fRightHandLerpTimer = 0.75f;
 
	//Left hand
	float 					m_fLeftHandIKWeight;
	Vector3					m_LeftHandPos;
	Vector3					m_LeftHandTargetPos;
	Quaternion				m_LeftHandTargetRot;
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
		m_RightHandNetworkedPos = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_RightHandNetworkedRot = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);

		m_LeftHandNetworkedPos = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_LeftHandNetworkedRot = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);   
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{ 
		if(_cSyncedNetworkVar == m_RightHandNetworkedPos)
        {
            RightHandIKTargetPos = m_RightHandNetworkedPos.Get();           
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedPos)
        {
            LeftHandIKTargetPos = m_LeftHandNetworkedPos.Get();          
        }
	} 
	
	public static void SerializeIKTarget(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;
        if (cSelfActor != null)
        {
            CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
    				
            if (cSelfActor != null && cSelfIKController.m_RightHandTargetPos != null)
            {
                // Write rotation states
                _cStream.Write((byte)ENetworkAction.SetRightTransform);
                _cStream.Write((float)cSelfIKController.m_RightHandTargetPos.x);
                _cStream.Write((float)cSelfIKController.m_RightHandTargetPos.y);
                _cStream.Write((float)cSelfIKController.m_RightHandTargetPos.z);	      

                _cStream.Write((float)cSelfIKController.m_RightHandTargetRot.eulerAngles.x);
                _cStream.Write((float)cSelfIKController.m_RightHandTargetRot.eulerAngles.y);
                _cStream.Write((float)cSelfIKController.m_RightHandTargetRot.eulerAngles.z);
            }

            if (cSelfActor != null && cSelfIKController.m_LeftHandTargetPos != null)
            {
                // Write rotation states
                _cStream.Write((byte)ENetworkAction.SetLeftTransform);
                _cStream.Write((float)cSelfIKController.m_LeftHandTargetPos.x);
                _cStream.Write((float)cSelfIKController.m_LeftHandTargetPos.y);
                _cStream.Write((float)cSelfIKController.m_LeftHandTargetPos.z);	      
    			
                _cStream.Write((float)cSelfIKController.m_LeftHandTargetRot.eulerAngles.x);
                _cStream.Write((float)cSelfIKController.m_LeftHandTargetRot.eulerAngles.y);
                _cStream.Write((float)cSelfIKController.m_LeftHandTargetRot.eulerAngles.z);
            }
        }
	}
	
	public static void UnserializeIKTarget(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);
		
		if (cPlayerActor != null)
		{			
			CPlayerIKController cPlayerIKController = cPlayerActor.GetComponent<CPlayerIKController>();
			
			ENetworkAction eNetworkAction = (ENetworkAction)_cStream.Read<byte>();
			
			switch (eNetworkAction)
			{
			case ENetworkAction.SetRightTransform:
			{
				Vector3 pos = new Vector3();
				Vector3 rot = new Vector3();

				pos.x = _cStream.Read<float>();
				pos.y = _cStream.Read<float>();
				pos.z = _cStream.Read<float>();

				cPlayerIKController.m_RightHandTargetPos = pos;

				rot.x = _cStream.Read<float>();
				rot.y = _cStream.Read<float>();
				rot.z = _cStream.Read<float>();	

				cPlayerIKController.m_RightHandTargetRot = Quaternion.Euler(rot);
				
				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_RightHandTargetPos);              
				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_RightHandTargetRot.eulerAngles);
			}
				break;

			case ENetworkAction.SetLeftTransform:
			{
				Vector3 pos = new Vector3();
				Vector3 rot = new Vector3();
				
				pos.x = _cStream.Read<float>();
				pos.y = _cStream.Read<float>();
				pos.z = _cStream.Read<float>();
				
				cPlayerIKController.m_LeftHandTargetPos = pos;
				
				rot.x = _cStream.Read<float>();
				rot.y = _cStream.Read<float>();
				rot.z = _cStream.Read<float>();	
				
				cPlayerIKController.m_LeftHandTargetRot = Quaternion.Euler(rot);
				
				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_LeftHandTargetPos);              
				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_LeftHandTargetRot.eulerAngles);
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

			m_RightHandPos = Vector3.Lerp(pos, m_RightHandTargetPos, LerpFactor);
        }        
    }

	void LerpLeftHand()
	{
		if(m_fLeftHandLerpTimer < m_kfLeftHandLerpTime)
		{
			m_fLeftHandLerpTimer += Time.deltaTime;   
			
			Vector3 pos = m_ThirdPersonAnim.GetIKPosition(AvatarIKGoal.LeftHand);			
			float LerpFactor = m_fLeftHandLerpTimer / m_kfLeftHandLerpTime;

			m_LeftHandPos = Vector3.Lerp(pos, m_LeftHandTargetPos, LerpFactor);
		}        
	}
	
	void OnAnimatorIK()
	{
		if(m_ThirdPersonAnim) 
		{		
			//Right hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_RightHandTargetPos != null)
			{
                m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);
                m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);

				if (m_RightHandPos != null)
				{
					m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandPos);
					m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, m_RightHandTargetRot);
				}
			}	

			//Left hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_LeftHandTargetPos != null)
			{
				m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_fLeftHandIKWeight);
				m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_fLeftHandIKWeight);

				if (m_LeftHandPos != null)
				{
					m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.LeftHand, m_LeftHandPos);
					m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.LeftHand, m_LeftHandTargetRot);
				}
			}
		}
	}  
}
