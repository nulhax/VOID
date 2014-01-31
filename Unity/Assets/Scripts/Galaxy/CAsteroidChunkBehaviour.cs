//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CAsteroidChunkBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public bool IsAlive
	{
		get { return (m_bAlive.Get()); }
	}


	public bool IsHighlighted
	{
		get { return (m_bHighlighted.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bHighlighted = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync);
		m_bAlive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
	}


	[AServerOnly]
	public void SetHighlighted(bool _bHighlighted)
	{
		m_iHighlightRefCount += (_bHighlighted) ? 1 : -1;

		if (m_iHighlightRefCount > 0)
		{
			m_bHighlighted.Set(true);
		}
		else
		{
			m_bHighlighted.Set(false);
		}
	}


	[AServerOnly]
	public void DecrementHealth(float _fAmount)
	{
		m_fHealth -= _fAmount;

		if (IsAlive &&
		    m_fHealth < 0.0f)
		{
			m_bAlive.Set(false);
		}
	}


	void Start()
	{
		// Empty
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (m_bHighlighted == _cSyncedVar)
		{
			if (IsHighlighted)
			{
				if (!GetComponent<FracturedChunk>().FracturedObjectSource.HasDetachedChunks())
				{
					renderer.enabled = true;
				}

				foreach (Material cMaterial in renderer.materials)
				{
					cMaterial.color = Color.red;
				}
			}
			else
			{
				if (!GetComponent<FracturedChunk>().FracturedObjectSource.HasDetachedChunks())
				{
					renderer.enabled = false;
				}

				foreach (Material cMaterial in renderer.materials)
				{
					cMaterial.color = Color.white;
				}
			}
		}

		if (_cSyncedVar == m_bAlive)
		{
			if (!IsAlive)
			{
				GetComponent<FracturedChunk>().DetachFromObject(false);
			}
		}
	}


// Member Fields


	CNetworkVar<bool> m_bHighlighted = null;
	CNetworkVar<bool> m_bAlive = null;


	float m_fHealth = 10.0f;


	int m_iHighlightRefCount = 0;


};
