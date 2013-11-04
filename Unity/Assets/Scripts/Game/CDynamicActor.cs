
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CDynamicActor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;

public class CDynamicActor : CNetworkMonoBehaviour 
{
	
// Member Fields
    protected CNetworkVar<float> m_cPositionX    = null;
    protected CNetworkVar<float> m_cPositionY    = null;
    protected CNetworkVar<float> m_cPositionZ    = null;
	
    protected CNetworkVar<float> m_EulerAngleX    = null;
    protected CNetworkVar<float> m_EulerAngleY    = null;
    protected CNetworkVar<float> m_EulerAngleZ    = null;
	
	
// Member Properties	
	public Vector3 Position
    {
        set 
		{ 
			m_cPositionX.Set(value.x); m_cPositionY.Set(value.y); m_cPositionZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cPositionX.Get(), m_cPositionY.Get(), m_cPositionZ.Get())); 
		}
    }
	
	public Vector3 EulerAngles
    {
        set 
		{ 
			m_EulerAngleX.Set(value.x); m_EulerAngleY.Set(value.y); m_EulerAngleZ.Set(value.z);
		}
        get 
		{ 
			return (new Vector3(m_EulerAngleX.Get(), m_EulerAngleY.Get(), m_EulerAngleZ.Get())); 
		}
    }

// Member Methods
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			SyncTransform();
		}
	}
	
    public override void InstanceNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_EulerAngleX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_EulerAngleY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_EulerAngleZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		if(!CNetwork.IsServer)
		{
			// Position
	        if (_rSender == m_cPositionX || _rSender == m_cPositionY || _rSender == m_cPositionZ)
			{
				transform.position = Position;
			}
			
			// Rotation
	        else if (_rSender == m_EulerAngleX || _rSender == m_EulerAngleY || _rSender == m_EulerAngleZ)
	        {	
	            transform.eulerAngles = EulerAngles;
	        }
		}
	}
	
	protected void SyncTransform()
	{
		Position = transform.position;
		EulerAngles = transform.eulerAngles;
	}
}
