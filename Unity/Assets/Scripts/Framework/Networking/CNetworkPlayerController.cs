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
using System.Reflection;

/* Implementation */


public class CNetworkPlayerController : MonoBehaviour
{

// Member Types


    delegate void CompilePlayerControllerOutput(CPacketStream _cStream);
    delegate void ProcessPlayerControllerInput(CNetworkPlayer _cNetworkPlayer, CPacketStream _cStream);


    public enum ETarget : byte
    {
        INVALID,

        ActorMotor,

        MAX
    }


// Member Functions

    // public:


    static CNetworkPlayerController()
    {
        s_mCompileDelegates.Add(ETarget.ActorMotor, CActorMotor.CompilePlayerControllerOutput);
        s_mProcessDelegates.Add(ETarget.ActorMotor, CActorMotor.ProcessPlayerControllerInput);
    }


    public void Start()
    {
        ClearOutboundData();
    }


    public void OnDestroy()
    {
    }


    public void Update()
    {
        CPacketStream cTargetStream = new CPacketStream();


        for (ETarget i = ETarget.INVALID + 1; i < ETarget.MAX; ++i)
        {
            s_mCompileDelegates[i](cTargetStream);


            if (cTargetStream.GetSize() > 0)
            {
                PacketStream.Write((byte)i);
                PacketStream.Write((byte)cTargetStream.GetSize());
                PacketStream.Write(cTargetStream);


                cTargetStream.Clear();
            }
        }
    }


    public bool HasOutboundData()
    {
        return (PacketStream.GetSize() > 1);
    }


    public void ClearOutboundData()
    {
        PacketStream.Clear();
        PacketStream.Write((byte)CNetworkServer.EPacketId.PlayerController);
    }


    public CPacketStream PacketStream
    {
        get { return (m_cPacketStream); }
    }


    public static void ProcessNetworkInboundData(CNetworkPlayer _cPlayer, CPacketStream _cStream)
    {
        while (_cStream.HasUnreadData())
        {
            ETarget eTarget = (ETarget)_cStream.ReadByte();
            byte bSize = _cStream.ReadByte();
            byte[] baData = _cStream.ReadBytes(bSize);


            CPacketStream cTargetStream = new CPacketStream();
            cTargetStream.Write(baData);


            s_mProcessDelegates[eTarget](_cPlayer, cTargetStream);
        }
    }


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


    CPacketStream m_cPacketStream = new CPacketStream();


    static Dictionary<ETarget, CompilePlayerControllerOutput> s_mCompileDelegates = new Dictionary<ETarget, CompilePlayerControllerOutput>();
    static Dictionary<ETarget, ProcessPlayerControllerInput> s_mProcessDelegates = new Dictionary<ETarget, ProcessPlayerControllerInput>();


};
