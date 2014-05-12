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


public class CMissileProjectileBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteDestroy");
        _cRegistrar.RegisterRpc(this, "RemoteInitNoTarget");
        _cRegistrar.RegisterRpc(this, "RemoteInitWithTarget");
    }


    [ANetworkRpc]
    public void RemoteInitNoTarget(Vector3 _vStartPosition, Vector3 _vStartEuler)
    {
        rigidbody.transform.position = _vStartPosition;
        rigidbody.transform.eulerAngles = _vStartEuler;

        rigidbody.AddRelativeForce(new Vector3(0.0f, 0.0f, m_fMoveSpeed));
    }


    [ANetworkRpc]
    public void RemoteInitWithTarget(Vector3 _vStartPosition, Vector3 _vStartEuler, TNetworkViewId _cTargetViewId, Vector3 _tTargetOffset)
    {
        rigidbody.transform.position = _vStartPosition;
        rigidbody.transform.eulerAngles = _vStartEuler;

        m_tTargetViewId = _cTargetViewId;
        m_tTargetOffset = _tTargetOffset;

        rigidbody.AddRelativeForce(new Vector3(0.0f, 0.0f, m_fMoveSpeed));
    }


	void Start()
	{
        // Empty
	}


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        if (CNetwork.IsServer)
        {
            UpdateLifeTimer();
        }
	}


    [AServerOnly]
    void UpdateLifeTimer()
    {
        m_fLifeTimer += Time.deltaTime;

        if (!m_bDestroyed &&
             m_fLifeTimer > m_fLifeDuration)
        {
            // Destory without hitting target
            InvokeRpcAll("RemoteDestroy", (TNetworkViewId)null);
        }
    }


    void FixedUpdate()
    {
        if (!m_bDestroyed)
        {
            UpdateTargetSeeking();
            UpdatePower();
        }
    }


    void UpdateTargetSeeking()
    {
        if (m_tTargetViewId == null ||
            m_tTargetViewId.GameObject == null)
            return;

        Quaternion qTargetRotation = Quaternion.LookRotation(m_tTargetViewId.GameObject.transform.position + m_tTargetOffset - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, qTargetRotation, m_fRotationSpeed * Time.fixedDeltaTime);
    }


    void UpdatePower()
    {
        float fPower = m_fLifeTimer;

        if (fPower < 1.5f)
        {
            fPower /= 2.0f;
        }
        else
        {
            fPower = 1.5f;
        }

        rigidbody.velocity = (transform.rotation * Vector3.forward) * m_fMoveSpeed * fPower;
    }


    [ANetworkRpc]
    void RemoteDestroy(TNetworkViewId _tHitTarget)
    {
        if (m_bDestroyed)
            return;

        GameObject cHitParticles = Resources.Load(CNetwork.Factory.GetRegisteredPrefabFile(CGameRegistrator.ENetworkPrefab.MissileHitParticles), typeof(GameObject)) as GameObject;
        cHitParticles = GameObject.Instantiate(cHitParticles, transform.position, Quaternion.identity) as GameObject;

        if (_tHitTarget != null)
        {
            cHitParticles.transform.parent = _tHitTarget.GameObject.transform;
        }

        Destroy(gameObject);

        m_bDestroyed = true;
    }


    [AServerOnly]
    void OnTriggerEnter(Collider _cCollider)
    {
        if (!m_bDestroyed &&
            CNetwork.IsServer)
        {
            if (_cCollider.GetComponent<CEnemyShip>() != null)
            {
                // Destroy with hit target
                InvokeRpcAll("RemoteDestroy", _cCollider.gameObject.GetComponent<CNetworkView>().ViewId);

                _cCollider.gameObject.GetComponent<CActorHealth>().health -= 70.0f;
            }
        }
    }


// Member Fields


    public float m_fLifeDuration = 5.0f;
    public float m_fMoveSpeed = 10.0f;
    public float m_fRotationSpeed = 20.0f;


    TNetworkViewId m_tTargetViewId = null;
    Vector3 m_tTargetOffset = Vector3.zero;

    float m_fLifeTimer = 0.0f;

    bool m_bDestroyed = false;


};
