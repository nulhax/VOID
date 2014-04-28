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
	
	public Vector3 RightHandIKPos
	{
        set { m_RightHandPos = value; }			 
        get { return (m_RightHandPos); }
	}   

	public Vector3 LeftHandIKPos
    {
        set { m_LeftHandPos = value; }
        get { return (m_LeftHandPos); }
    } 
   
	public Quaternion RightHandIKRot
	{
		set { m_RightHandRot = value; }			 
        get { return (m_RightHandRot); }
	}
		
	public Quaternion LeftHandIKRot
	{
		set { m_LeftHandRot = value; }			 
        get { return (m_LeftHandRot); }
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
	
    public float RightHandIKWeightTarget
    {
        set { m_fRightIKTargetWeight = value; m_fRightHandWeightLerpTimer = 0.0f; }
        get { return (m_fRightIKTargetWeight); }
    }
    
    public float LeftHandIKWeightTarget
    {
        set { m_fLeftIKTargetWeight = value; m_fLeftHandWeightLerpTimer = 0.0f; }
        get { return (m_fLeftIKTargetWeight); }
    }

	//Member variables
	Animator m_ThirdPersonAnim;
    CNetworkVar<Vector3>    m_RightHandNetworkedPos;
    CNetworkVar<Vector3>    m_LeftHandNetworkedPos;

	CNetworkVar<Vector3>    m_RightHandNetworkedRot;
	CNetworkVar<Vector3>    m_LeftHandNetworkedRot;

	//Right hand
	float 					m_fRightHandIKWeight = 0;
    float                   m_fRightIKTargetWeight = 0;
	Vector3					m_RightHandPos;
    Vector3                 m_RightHandTargetPos;
    Quaternion              m_RightHandRot;

	const float 			m_kfRightHandWeightLerpTime = 0.25f;
	float 					m_fRightHandWeightLerpTimer = 0.0f;   

	//Left hand
	float 					m_fLeftHandIKWeight = 0;
    float                   m_fLeftIKTargetWeight = 0;
	Vector3					m_LeftHandPos;
    Vector3                 m_LeftHandTargetPos;
    Quaternion              m_LeftHandRot;

	const float 			m_kfLeftHandWeightLerpTime = 0.25f;
	float 					m_fLeftHandWeightLerpTimer = 0.0f;		

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
            RightHandIKPos = m_RightHandNetworkedPos.Get();           
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedPos)
        {
            LeftHandIKPos = m_LeftHandNetworkedPos.Get();          
        }
	} 
	
	public static void SerializeIKTarget(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;
        if (cSelfActor != null)
        {
            CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
    				
            if (cSelfActor != null && cSelfIKController.m_RightHandPos != null)
            {
                // Write rotation states
                _cStream.Write((byte)ENetworkAction.SetRightTransform);
                _cStream.Write((float)cSelfIKController.m_RightHandPos.x);
                _cStream.Write((float)cSelfIKController.m_RightHandPos.y);
                _cStream.Write((float)cSelfIKController.m_RightHandPos.z);	      

                _cStream.Write((float)cSelfIKController.m_RightHandRot.eulerAngles.x);
                _cStream.Write((float)cSelfIKController.m_RightHandRot.eulerAngles.y);
                _cStream.Write((float)cSelfIKController.m_RightHandRot.eulerAngles.z);
            }

            if (cSelfActor != null && cSelfIKController.m_LeftHandPos != null)
            {
                // Write rotation states
                _cStream.Write((byte)ENetworkAction.SetLeftTransform);
                _cStream.Write((float)cSelfIKController.m_LeftHandPos.x);
                _cStream.Write((float)cSelfIKController.m_LeftHandPos.y);
                _cStream.Write((float)cSelfIKController.m_LeftHandPos.z);	      
    			
                _cStream.Write((float)cSelfIKController.m_LeftHandRot.eulerAngles.x);
                _cStream.Write((float)cSelfIKController.m_LeftHandRot.eulerAngles.y);
                _cStream.Write((float)cSelfIKController.m_LeftHandRot.eulerAngles.z);
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

				cPlayerIKController.m_RightHandPos = pos;

				rot.x = _cStream.Read<float>();
				rot.y = _cStream.Read<float>();
				rot.z = _cStream.Read<float>();	

				cPlayerIKController.m_RightHandRot = Quaternion.Euler(rot);
				
				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_RightHandPos);              
				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_RightHandRot.eulerAngles);
			}
				break;

			case ENetworkAction.SetLeftTransform:
			{
				Vector3 pos = new Vector3();
				Vector3 rot = new Vector3();
				
				pos.x = _cStream.Read<float>();
				pos.y = _cStream.Read<float>();
				pos.z = _cStream.Read<float>();
				
				cPlayerIKController.m_LeftHandPos = pos;
				
				rot.x = _cStream.Read<float>();
				rot.y = _cStream.Read<float>();
				rot.z = _cStream.Read<float>();	
				
				cPlayerIKController.m_LeftHandRot = Quaternion.Euler(rot);
				
				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_LeftHandPos);              
				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_LeftHandRot.eulerAngles);
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
       	LerpRightHandWeight(); 
        LerpLeftHandWeight();
	}  

    void LerpRightHandWeight()
    {
        if (m_fRightHandWeightLerpTimer < m_kfRightHandWeightLerpTime)
        {
            m_fRightHandWeightLerpTimer += Time.deltaTime;   
            
            float LerpFactor = m_fRightHandWeightLerpTimer / m_kfRightHandWeightLerpTime;
            
            RightHandIKWeight = Mathf.Lerp(RightHandIKWeight, m_fRightIKTargetWeight, LerpFactor);
        }      
    }   

	void LerpLeftHandWeight()
	{
        if (m_fLeftHandWeightLerpTimer < m_kfLeftHandWeightLerpTime)
        {
            m_fLeftHandWeightLerpTimer += Time.deltaTime;   
            
            float LerpFactor = m_fLeftHandWeightLerpTimer / m_kfLeftHandWeightLerpTime;
            
            LeftHandIKWeight = Mathf.Lerp(LeftHandIKWeight, m_fLeftIKTargetWeight, LerpFactor);
        }     
	}
	
    public void SetRightHandTarget(Vector3 _position, Quaternion _rotation)
    {
        RightHandIKPos = _position;
        RightHandIKRot = _rotation;
        
        m_fRightIKTargetWeight = 1;
        
        m_fRightHandWeightLerpTimer = 0.0f;
    }
    
    public void EndRightHandIK()
    {
       	m_fRightIKTargetWeight = 0;	    
	   	m_fRightHandWeightLerpTimer = 0.0f;      
    }
    
    public void SetLeftHandTarget(Vector3 _position, Quaternion _rotation)
    {
        LeftHandIKPos = _position;
        LeftHandIKRot = _rotation;        
       
        m_fLeftIKTargetWeight = 1;
        
        m_fLeftHandWeightLerpTimer = 0.0f;
    }
    
    public void EndLeftHandIK()
    {        
	    m_fLeftIKTargetWeight = 0;	    
	    m_fLeftHandWeightLerpTimer = 0.0f;        
    }

	void OnAnimatorIK()
	{
		if(m_ThirdPersonAnim) 
		{		
			//Right hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_RightHandPos != null)
			{
				m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);
				m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);

				if (m_RightHandPos != null)
				{
					m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandPos);
					m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, m_RightHandRot);
				}
			}	

			//Left hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_LeftHandPos != null)
			{
				m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_fLeftHandIKWeight);
				m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_fLeftHandIKWeight);

				if (m_LeftHandPos != null)
				{
					m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.LeftHand, m_LeftHandPos);
					m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.LeftHand, m_LeftHandRot);
				}
			}
		}
	}  
}
