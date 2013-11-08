/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 2.0.10
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace RakNet {

using System;
using System.Runtime.InteropServices;

public class LogCommandParser : CommandParserInterface {
  private HandleRef swigCPtr;

  internal LogCommandParser(IntPtr cPtr, bool cMemoryOwn) : base(RakNetPINVOKE.LogCommandParser_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(LogCommandParser obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~LogCommandParser() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          RakNetPINVOKE.delete_LogCommandParser(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public static LogCommandParser GetInstance() {
    IntPtr cPtr = RakNetPINVOKE.LogCommandParser_GetInstance();
    LogCommandParser ret = (cPtr == IntPtr.Zero) ? null : new LogCommandParser(cPtr, false);
    return ret;
  }

  public static void DestroyInstance(LogCommandParser i) {
    RakNetPINVOKE.LogCommandParser_DestroyInstance(LogCommandParser.getCPtr(i));
  }

  public LogCommandParser() : this(RakNetPINVOKE.new_LogCommandParser(), true) {
  }

  public override bool OnCommand(string command, uint numParameters, string[] parameterList, TransportInterface transport, SystemAddress systemAddress, string originalString) {
    bool ret = RakNetPINVOKE.LogCommandParser_OnCommand(swigCPtr, command, numParameters, parameterList, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress), originalString);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override string GetName() {
    string ret = RakNetPINVOKE.LogCommandParser_GetName(swigCPtr);
    return ret;
  }

  public override void SendHelp(TransportInterface transport, SystemAddress systemAddress) {
    RakNetPINVOKE.LogCommandParser_SendHelp(swigCPtr, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddChannel(string channelName) {
    RakNetPINVOKE.LogCommandParser_AddChannel(swigCPtr, channelName);
  }

  public void WriteLog(string channelName, string format) {
    RakNetPINVOKE.LogCommandParser_WriteLog(swigCPtr, channelName, format);
  }

  public override void OnNewIncomingConnection(SystemAddress systemAddress, TransportInterface transport) {
    RakNetPINVOKE.LogCommandParser_OnNewIncomingConnection(swigCPtr, SystemAddress.getCPtr(systemAddress), TransportInterface.getCPtr(transport));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public override void OnConnectionLost(SystemAddress systemAddress, TransportInterface transport) {
    RakNetPINVOKE.LogCommandParser_OnConnectionLost(swigCPtr, SystemAddress.getCPtr(systemAddress), TransportInterface.getCPtr(transport));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public override void OnTransportChange(TransportInterface transport) {
    RakNetPINVOKE.LogCommandParser_OnTransportChange(swigCPtr, TransportInterface.getCPtr(transport));
  }

}

}
