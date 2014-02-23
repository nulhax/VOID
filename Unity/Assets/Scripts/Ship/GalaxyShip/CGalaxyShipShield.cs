//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  Martin Ponichtera, Scott Emery
//  Mail        :  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CGalaxyShipShield : CNetworkMonoBehaviour 
{
	// Member Types
	public enum EShieldState
	{
		INVALID,
		
		PoweredUp,
		PoweredDown,
		Reacting,
		Charging,
		
		MAX
	}
	
	// Member Fields
	public GameObject m_Shield = null;
	
	private float m_ShieldPower = 100.0f;
	private const float m_MaxShieldPower = 100.0f;

	// Take this from the module to determine active
	private bool m_Active = true;

	private EShieldState m_ShieldState = EShieldState.PoweredDown;
	 
	private const float cShipRechargeRate = 1.0f;
	
	// Member Properies

	// Member Methods


	void Start()
	{
//		gameObject.GetComponentInChildren<CShieldEventHandler>().EventShieldCollider += ShipShieldCollider;
//		gameObject.GetComponentInChildren<CShieldEventHandler>().EventShieldDamage += ShipShieldDamage;
//		gameObject.GetComponentInChildren<CShieldEventHandler>().EventShieldRecharge += ShieldRecharge;
	}

	public void UpdateShieldBounds(Mesh _ShieldMesh)
	{	
		m_Shield.GetComponent<MeshFilter>().mesh = _ShieldMesh;

		m_Shield.GetComponent<MeshCollider>().sharedMesh = null;
		m_Shield.GetComponent<MeshCollider>().sharedMesh = _ShieldMesh;
	}

	void ShipShieldCollider(Collider _Collider)
	{
		if(_Collider.gameObject.tag == "Asteroid")
		{
			if(_Collider.gameObject.tag == "Asteroid")
			{
				Debug.Log("Collider entered Trigger as asteroid");

				// Take the ship velocity and add it to the asteroid velocity
				//TODO: Make this betterer
				Vector3 Move = CGameShips.GalaxyShip.rigidbody.velocity;

				_Collider.gameObject.transform.rigidbody.velocity = Move;
			}
		}
	}

	void ShipShieldDamage(Collider _Collider)
	{
		if(CNetwork.IsServer)
		{
			float fDamage = _Collider.rigidbody.mass / 10.0f;
			if(m_ShieldPower <= 0.0f)
			{
				// Set health to zero
				m_ShieldPower = 0.0f;

				// Let shield activity be false
				m_Active = false;
			}
			else
			{
				m_ShieldPower -= fDamage;
			}

			Debug.Log ("Shield Health is at: " + m_ShieldPower);
		}
	}

	void ShieldRecharge()
	{
		if(CNetwork.IsServer)
		{
			if(m_Active == true)
			{
				// As long as the shield power isn't high than max power increment power
				if(m_ShieldPower < m_MaxShieldPower)
				{
					m_ShieldPower += cShipRechargeRate * Time.deltaTime;

					Debug.Log ("Shield Health: " + m_ShieldPower);
				}
			}
			else
			{
				// When the shield is not active, drain the shield power. 
				m_ShieldPower -= cShipRechargeRate * Time.deltaTime;
			}
		}
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		
	}
	
	
	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	void OnGui()
	{

	}

}
