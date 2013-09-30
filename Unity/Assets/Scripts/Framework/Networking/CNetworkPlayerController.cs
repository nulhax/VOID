//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CNetworkPlayerController.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CNetworkPlayerController : MonoBehaviour
{

// Member Types


    public enum EActionType : byte
    {
        Camera_RotationX,
        Camera_RotationY,
        Camera_RotationZ,

        Actor_MoveForward,
        Actor_MoveForwardStop,
        Actor_MoveBackward,
        Actor_MoveBackwardStop,
        Actor_MoveLeft,
        Actor_MoveLeftStop,
        Actor_MoveRight,
        Actor_MoveRightStop,

        Monitor_Interaction,

        Component_Build,

        Tool_Change,
    }


    public struct TAction
    {
        public TAction(EActionType _eType)
        {
            eType = _eType;
            caParameters = null;
        }

        public TAction(EActionType _eType, object[] _caParameters)
        {
            eType = _eType;
            caParameters = _caParameters;
        }

        public EActionType eType;
        public object[] caParameters;
    }


// Member Functions

    // public:


    public void Start()
    {
    }


    public void OnDestroy()
    {
    }


    public void Update()
    {
        if (CGame.IsConnectedToServer())
        {
            ProcessMovement();
        }
    }


    public void CompileOutboundData(CPacketStream _cTransmissionStream)
    {
        foreach (TAction tAction in m_aQueuedActions)
        {
            _cTransmissionStream.Write((byte)tAction.eType);
        }

        m_aQueuedActions.Clear();
    }


    public bool HasOutboundData()
    {
        return (m_bOutboundData);
    }


    public static void HandlePlayerInboundData(CNetworkPlayer _cPlayer, CPacketStream _cTransmissionStream)
    {
    }


    // protected:


    public void ProcessMovement()
    {
        // Start moving forward
        if ( Input.GetKeyDown(m_eMoveForwardKey) &&
            !Input.GetKey(m_eMoveBackwardsKey))
        {
            m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveForward));
            m_bMovingForwards = true;
        }

        // Start moving backwards
        else if (!Input.GetKey(m_eMoveForwardKey) &&
                  Input.GetKeyDown(m_eMoveBackwardsKey))
        {
            m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveBackward));
            m_bMovingBackwards = true;
        }
        else
        {
            // Stop moving forward
            if (m_bMovingForwards)
            {
                m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveForwardStop));
                m_bMovingForwards = false;
            }

            // Stop moving backwards
            else if (m_bMovingBackwards)
            {
                m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveBackwardStop));
                m_bMovingBackwards = false;
            }
        }


        // Start moving reft
        if ( Input.GetKeyDown(m_eMoveLeftKey) &&
            !Input.GetKey(m_eMoveRightKey))
        {
            m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveLeft));
            m_bMovingLeft = true;
        }

        // Start moving right
        else if (!Input.GetKey(m_eMoveLeftKey) &&
                  Input.GetKeyDown(m_eMoveRightKey))
        {
            m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveRight));
            m_bMovingRight = true;
        }
        else if (m_bMovingLeft ||
                 m_bMovingRight)
        {
            // Stop moving left
            if (m_bMovingLeft)
            {
                m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveLeftStop));
                m_bMovingLeft = false;
            }

            // Stop moving right
            else if (m_bMovingRight)
            {
                m_aQueuedActions.Add(new TAction(EActionType.Actor_MoveRightStop));
                m_bMovingRight = false;
            }
        }
    }


    // private:


// Member Variables

    // protected:


    // private:


    KeyCode m_eMoveForwardKey = KeyCode.W;
    KeyCode m_eMoveBackwardsKey = KeyCode.S;
    KeyCode m_eMoveLeftKey = KeyCode.A;
    KeyCode m_eMoveRightKey = KeyCode.D;


    bool m_bMovingForwards = false;
    bool m_bMovingBackwards = false;
    bool m_bMovingLeft = false;
    bool m_bMovingRight = false;
    bool m_bOutboundData = false;


    List<TAction> m_aQueuedActions = new List<TAction>();


};
