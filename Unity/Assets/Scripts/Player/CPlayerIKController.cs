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
        SetAllIKStates,
		SetRightHandStates,
		SetLeftHandStates,
	}
	
	//Member Delegates & Events
	
	
	// Member Properties
	
	public Vector3 RightHandIKPos
	{
        set { m_RightHandPos = value;   m_bRightHandModified = true;}			 
        get { return (m_RightHandPos);  }
	}   

	public Vector3 LeftHandIKPos
    {
        set { m_LeftHandPos = value;    m_bLeftHandModified = true; }
        get { return (m_LeftHandPos);   }
    } 
   
	public Quaternion RightHandIKRot
	{
        set { m_RightHandRot = value;   m_bRightHandModified = true;}			 
        get { return (m_RightHandRot);  }
	}
		
	public Quaternion LeftHandIKRot
	{
        set { m_LeftHandRot = value;    }			 
        get { return (m_LeftHandRot);   }
	}
	
	public float RightHandIKWeight
	{
        set { m_fRightHandIKWeight = value;  m_bRightHandModified = true;}
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

	CNetworkVar<float>		m_RightHandNetworkedWeight;
	CNetworkVar<float>		m_LeftHandNetworkedWeight;

	//Right hand
	float 					m_fRightHandIKWeight = 0;
    float                   m_fRightIKTargetWeight = 0;
    Vector3					m_RightHandPos = Vector3.zero;
    Vector3                 m_RightHandTargetPos = Vector3.zero;
    Quaternion              m_RightHandRot = Quaternion.identity;

	const float 			m_kfRightHandWeightLerpTime = 0.25f;
	float 					m_fRightHandWeightLerpTimer = 0.0f; 

    bool                    m_bRightHandModified = false;

	//Left hand
	float 					m_fLeftHandIKWeight = 0;
    float                   m_fLeftIKTargetWeight = 0;
    Vector3					m_LeftHandPos = Vector3.zero;
    Vector3                 m_LeftHandTargetPos = Vector3.zero;
    Quaternion              m_LeftHandRot = Quaternion.identity;

	const float 			m_kfLeftHandWeightLerpTime = 0.25f;
	float 					m_fLeftHandWeightLerpTimer = 0.0f;	

    bool                    m_bLeftHandModified = false;

    static CNetworkStream m_IKNetworkStream = new CNetworkStream();

    //Member Methods
	
	// Use this for initialization
	void Start () 
	{	
		m_ThirdPersonAnim = GetComponent<Animator>();          
   	} 

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_RightHandNetworkedPos = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync, Vector3.zero);
        m_RightHandNetworkedRot = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync, Vector3.zero);

		m_LeftHandNetworkedPos = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync, Vector3.zero);
        m_LeftHandNetworkedRot = _cRegistrar.CreateReliableNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync, Vector3.zero);   

		m_RightHandNetworkedWeight 	= _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync,0);
		m_LeftHandNetworkedWeight 	= _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync,0); 
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{ 
		if(_cSyncedNetworkVar == m_RightHandNetworkedPos)
        {
            RightHandIKPos = m_RightHandNetworkedPos.Get();
            Debug.Log("Pos set");
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedPos)
        {
            LeftHandIKPos = m_LeftHandNetworkedPos.Get();            
        }
        if(_cSyncedNetworkVar == m_RightHandNetworkedRot)
        {
            RightHandIKRot = Quaternion.Euler(m_RightHandNetworkedRot.Get());           
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedRot)
        {
            LeftHandIKRot = Quaternion.Euler(m_LeftHandNetworkedRot.Get());                      
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedPos)
        {
            LeftHandIKPos = m_LeftHandNetworkedPos.Get();          
        }
		if (_cSyncedNetworkVar == m_RightHandNetworkedWeight) 
		{
            RightHandIKWeight = m_RightHandNetworkedWeight.Get();
		}
        if (_cSyncedNetworkVar == m_LeftHandNetworkedWeight) 
        {
            LeftHandIKWeight = m_LeftHandNetworkedWeight.Get();
        }
	} 
	
	public static void SerializeIKTarget(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;
        if (cSelfActor != null)
        {
            CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
    				
            _cStream.Write(m_IKNetworkStream);
            m_IKNetworkStream.Clear();
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
                case ENetworkAction.SetAllIKStates:
                {
                    Debug.Log("Pos received on server");

                    //Right hand states
                    Vector3 pos = new Vector3();
                    Vector3 rot = new Vector3();
                    float IKWeight = 0;
                    
                    pos.x = _cStream.Read<float>();
                    pos.y = _cStream.Read<float>();
                    pos.z = _cStream.Read<float>();
                    
                    cPlayerIKController.m_RightHandPos = pos;
                    
                    rot.x = _cStream.Read<float>();
                    rot.y = _cStream.Read<float>();
                    rot.z = _cStream.Read<float>(); 
                    
                    cPlayerIKController.m_RightHandRot = Quaternion.Euler(rot);
                    
                    IKWeight = _cStream.Read<float>();  
                    cPlayerIKController.RightHandIKWeight = IKWeight;
                    
                    cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_RightHandPos);              
                    cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_RightHandRot.eulerAngles);
                    
                    cPlayerIKController.m_RightHandNetworkedWeight.Set(cPlayerIKController.RightHandIKWeight);  

                    //Left hand states
                    pos = new Vector3();
                    rot = new Vector3();
                    IKWeight = 0;

                    pos.x = _cStream.Read<float>();
                    pos.y = _cStream.Read<float>();
                    pos.z = _cStream.Read<float>();
                    
                    cPlayerIKController.m_LeftHandPos = pos;
                    
                    rot.x = _cStream.Read<float>();
                    rot.y = _cStream.Read<float>();
                    rot.z = _cStream.Read<float>(); 
                    
                    cPlayerIKController.m_LeftHandRot = Quaternion.Euler(rot);
                    
                    IKWeight = _cStream.Read<float>();  
                    cPlayerIKController.LeftHandIKWeight = IKWeight;
                    
                    cPlayerIKController.m_LeftHandNetworkedPos.Set(cPlayerIKController.m_LeftHandPos);              
                    cPlayerIKController.m_LeftHandNetworkedRot.Set(cPlayerIKController.m_LeftHandRot.eulerAngles);
                    
                    cPlayerIKController.m_LeftHandNetworkedWeight.Set(cPlayerIKController.LeftHandIKWeight);        
                    break;
                }

    			case ENetworkAction.SetRightHandStates:
    			{
    				Vector3 pos = new Vector3();
    				Vector3 rot = new Vector3();
    				float IKWeight;

    				pos.x = _cStream.Read<float>();
    				pos.y = _cStream.Read<float>();
    				pos.z = _cStream.Read<float>();

    				cPlayerIKController.m_RightHandPos = pos;

    				rot.x = _cStream.Read<float>();
    				rot.y = _cStream.Read<float>();
    				rot.z = _cStream.Read<float>();	

    				cPlayerIKController.m_RightHandRot = Quaternion.Euler(rot);

    				IKWeight = _cStream.Read<float>();	
    				cPlayerIKController.RightHandIKWeight = IKWeight;

    				cPlayerIKController.m_RightHandNetworkedPos.Set(cPlayerIKController.m_RightHandPos);              
    				cPlayerIKController.m_RightHandNetworkedRot.Set(cPlayerIKController.m_RightHandRot.eulerAngles);

    				cPlayerIKController.m_RightHandNetworkedWeight.Set(cPlayerIKController.RightHandIKWeight);            
                    break;
    			}		

    			case ENetworkAction.SetLeftHandStates:
    			{                   
    				Vector3 pos = new Vector3();
    				Vector3 rot = new Vector3();
    				float IKWeight;
    				
    				pos.x = _cStream.Read<float>();
    				pos.y = _cStream.Read<float>();
    				pos.z = _cStream.Read<float>();
    				
    				cPlayerIKController.m_LeftHandPos = pos;
    				
    				rot.x = _cStream.Read<float>();
    				rot.y = _cStream.Read<float>();
    				rot.z = _cStream.Read<float>();	
    				
    				cPlayerIKController.m_LeftHandRot = Quaternion.Euler(rot);

    				IKWeight = _cStream.Read<float>();	
    				cPlayerIKController.LeftHandIKWeight = IKWeight;
    				
    				cPlayerIKController.m_LeftHandNetworkedPos.Set(cPlayerIKController.m_LeftHandPos);              
    				cPlayerIKController.m_LeftHandNetworkedRot.Set(cPlayerIKController.m_LeftHandRot.eulerAngles);

    				cPlayerIKController.m_LeftHandNetworkedWeight.Set(cPlayerIKController.LeftHandIKWeight);            
                    break;
    			}			
    			default:
				Debug.LogError(string.Format("Unknown network action ({0})", (byte)eNetworkAction));
				break;
			}
		}		
	}  
	
	// Update is called once per frame
	void Update ()
	{ 	
        if (CGamePlayers.SelfActor != gameObject) return;        

        LerpRightHandWeight(); 
        LerpLeftHandWeight();

        if (m_bLeftHandModified && m_bRightHandModified)
        {             
            //Right hand states
            m_IKNetworkStream.Write((byte)ENetworkAction.SetAllIKStates);
            m_IKNetworkStream.Write((float)m_RightHandPos.x);
            m_IKNetworkStream.Write((float)m_RightHandPos.y);
            m_IKNetworkStream.Write((float)m_RightHandPos.z);          

            m_IKNetworkStream.Write((float)m_RightHandRot.eulerAngles.x);
            m_IKNetworkStream.Write((float)m_RightHandRot.eulerAngles.y);
            m_IKNetworkStream.Write((float)m_RightHandRot.eulerAngles.z);
                          
            m_IKNetworkStream.Write((float)m_fRightHandIKWeight);       
        
            //left hand states
            m_IKNetworkStream.Write((float)m_LeftHandPos.x);
            m_IKNetworkStream.Write((float)m_LeftHandPos.y);
            m_IKNetworkStream.Write((float)m_LeftHandPos.z);       
          
            m_IKNetworkStream.Write((float)m_LeftHandRot.eulerAngles.x);
            m_IKNetworkStream.Write((float)m_LeftHandRot.eulerAngles.y);
            m_IKNetworkStream.Write((float)m_LeftHandRot.eulerAngles.z);

            m_IKNetworkStream.Write((float)m_fLeftHandIKWeight);          
        }
        else if (m_bRightHandModified)
        {       
            //Right hand states
            m_IKNetworkStream.Write((byte)ENetworkAction.SetRightHandStates);
            m_IKNetworkStream.Write((float)m_RightHandPos.x);
            m_IKNetworkStream.Write((float)m_RightHandPos.y);
            m_IKNetworkStream.Write((float)m_RightHandPos.z);          

            m_IKNetworkStream.Write((float)m_RightHandRot.eulerAngles.x);
            m_IKNetworkStream.Write((float)m_RightHandRot.eulerAngles.y);
            m_IKNetworkStream.Write((float)m_RightHandRot.eulerAngles.z);
                          
            m_IKNetworkStream.Write((float)m_fRightHandIKWeight);        
        } 
        else if (m_bLeftHandModified)
        {
            //left hand states
            m_IKNetworkStream.Write((byte)ENetworkAction.SetLeftHandStates);
            m_IKNetworkStream.Write((float)m_LeftHandPos.x);
            m_IKNetworkStream.Write((float)m_LeftHandPos.y);
            m_IKNetworkStream.Write((float)m_LeftHandPos.z);       
          
            m_IKNetworkStream.Write((float)m_LeftHandRot.eulerAngles.x);
            m_IKNetworkStream.Write((float)m_LeftHandRot.eulerAngles.y);
            m_IKNetworkStream.Write((float)m_LeftHandRot.eulerAngles.z);

            m_IKNetworkStream.Write((float)m_fLeftHandIKWeight);     
        }

        m_bLeftHandModified = false;
        m_bRightHandModified = false;
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
	
    public void SetRightHandTarget(Vector3 _position, Quaternion _rotation, bool _bResetTimer)
    {
        RightHandIKPos = _position;
        RightHandIKRot = _rotation;
        
        m_fRightIKTargetWeight = 1;

        if (_bResetTimer)
        {
            m_fRightHandWeightLerpTimer = 0.0f;
        }
    }
    
    public void EndRightHandIK()
    {
       	m_fRightIKTargetWeight = 0;	    
	   	m_fRightHandWeightLerpTimer = 0.0f;      
    }
    
    public void SetLeftHandTarget(Vector3 _position, Quaternion _rotation, bool _bResetTimer)
    {
        LeftHandIKPos = _position;
        LeftHandIKRot = _rotation;        
       
        m_fLeftIKTargetWeight = 1;

        if(_bResetTimer)
        {
            m_fLeftHandWeightLerpTimer = 0.0f;
        }
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
