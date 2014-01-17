using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
public class CActorNetworkSyncronized : CNetworkMonoBehaviour 
{
	// Member Types


	// Member Delegates and Events


	// Member Fields
	private CNetworkVar<Vector3> m_Position = null;
	private CNetworkVar<Vector3> m_EulerAngles = null;
	
	public bool m_SyncPosition = true;
	public bool m_SyncRotation = true;


	// Member Properties		


	// Member Methods
	public void Start() 
	{
		// Set rigidbody to kinematic on the client
		if(!CNetwork.IsServer && rigidbody != null)
		{
			rigidbody.isKinematic = true;
		}
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			SyncTransform();
		}
	}

	public override void InstanceNetworkVars()
	{
		m_Position = new CNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
		m_EulerAngles = new CNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
	}

	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		if(!CNetwork.IsServer)
		{
			// Position
			if (_rSender == m_Position && m_SyncPosition)
			{
				transform.position = m_Position.Get();
			}
			
			// Rotation
			else if (_rSender == m_EulerAngles && m_SyncRotation)
			{	
				transform.eulerAngles = m_EulerAngles.Get();
			}
		}
	}

	private void SyncTransform()
	{
		m_Position.Set(transform.position);
		m_EulerAngles.Set(transform.eulerAngles);
	}
}
