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


public class CDoorBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum EEventType
    {
        OpenStart,
        Opened,
        CloseStart,
        Closed
    }


// Member Delegates & Events


    public delegate void HandleDoorOpenStart(CDoorBehaviour _cSenderDoorBehaviour, EEventType _eEventType);
    public event HandleDoorOpenStart EventOpenStart;


    public delegate void HandleDoorOpened(CDoorBehaviour _cSenderDoorBehaviour, EEventType _eEventType);
    public event HandleDoorOpened EventOpened;


    public delegate void HandleDoorCloseStart(CDoorBehaviour _cSenderDoorBehaviour, EEventType _eEventType);
    public event HandleDoorCloseStart EventCloseStart;


    public delegate void HandleDoorClosed(CDoorBehaviour _cSenderDoorBehaviour, EEventType _eEventType);
    public event HandleDoorClosed EventClosed;


// Member Properties


    public CExpansionPortBehaviour GetParentExpansionPortBehaviour
    {
        get { return (m_caParentExpansionPortBehaviours[0]); }
    }


    public CExpansionPortBehaviour GetNeighbourExpansionPortBehaviour
    {
        get { return (m_caParentExpansionPortBehaviours[1]); }
    }


    public float OpenedPercent
    {
        get { return (m_fMotorTimer / m_fOpenCloseInterval); }
    }


    public bool IsOpened
    {
        get { return (m_cOpened.Get()); }
    }


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_cOpened = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
    }


    [AServerOnly]
    public void SetOpened(bool _bOpened)
    {
        m_cOpened.Set(_bOpened);
    }


    public void RegisterParentExpansionPort(CExpansionPortBehaviour _cExpansionPortBehaviour, int _iParentId)
    {
        m_caParentExpansionPortBehaviours[_iParentId] = _cExpansionPortBehaviour;

        if (_iParentId == 0)
        {
            GetParentExpansionPortBehaviour.AttachedDuiDoorControl.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>().EventClickOpenDoor += OnEventDuiDoorControlClick;
            GetParentExpansionPortBehaviour.AttachedDuiDoorControl.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>().EventClickCloseDoor += OnEventDuiDoorControlClick;
        }
    }


	void Start()
	{
        m_vClosedPosition = transform.position;
        m_vOpenedPosition = m_vClosedPosition + new Vector3(0.0f, 2.5f, 0.0f);
	}


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        if (m_fMotorTimer < m_fOpenCloseInterval)
        {
            m_fMotorTimer += Time.deltaTime;

            if (IsOpened)
            {
                transform.position = Vector3.Lerp(m_vClosedPosition, m_vOpenedPosition, m_fMotorTimer);
            }
            else
            {
                transform.position = Vector3.Lerp(m_vOpenedPosition, m_vClosedPosition, m_fMotorTimer);
            }

            if (m_fMotorTimer > m_fOpenCloseInterval)
            {
                if (IsOpened)
                {
                    if (EventOpened != null) EventOpened(this, EEventType.Opened);
                }
                else
                {
                    if (EventClosed != null) EventClosed(this, EEventType.Closed);
                }
            }
        }
	}


    void OnEventDuiDoorControlClick(CDuiFacilityDoorBehaviour.EButton _eButton)
    {
        switch (_eButton)
        {
            case CDuiFacilityDoorBehaviour.EButton.OpenDoor:
                SetOpened(true);
                break;

            case CDuiFacilityDoorBehaviour.EButton.CloseDoor:
                SetOpened(false);
                break;

            default:
                Debug.LogError("Unknown dui facility door behaviour button. " + _eButton);
                break;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (m_cOpened == _cSyncedVar)
        {
            if (m_cOpened.Get())
            {
                if (EventOpenStart != null) EventOpenStart(this, EEventType.OpenStart);
            }
            else
            {
                if (EventCloseStart != null) EventCloseStart(this, EEventType.CloseStart);
            }

            m_fMotorTimer = 0.0f;
        }
    }


// Member Fields


    CNetworkVar<bool> m_cOpened = null;


    Vector3 m_vClosedPosition;
    Vector3 m_vOpenedPosition;


    float m_fMotorTimer         = 0.0f;
    float m_fOpenCloseInterval  = 1.0f;


    CExpansionPortBehaviour[] m_caParentExpansionPortBehaviours = new CExpansionPortBehaviour[2];


};
