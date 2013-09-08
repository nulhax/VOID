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

public class UDPProxyServerResultHandler : IDisposable {
  private HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UDPProxyServerResultHandler(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(UDPProxyServerResultHandler obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~UDPProxyServerResultHandler() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          RakNetPINVOKE.delete_UDPProxyServerResultHandler(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }

  public UDPProxyServerResultHandler() : this(RakNetPINVOKE.new_UDPProxyServerResultHandler(), true) {
    SwigDirectorConnect();
  }

  public virtual void OnLoginSuccess(RakString usedPassword, UDPProxyServer proxyServerPlugin) {
    RakNetPINVOKE.UDPProxyServerResultHandler_OnLoginSuccess(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void OnAlreadyLoggedIn(RakString usedPassword, UDPProxyServer proxyServerPlugin) {
    RakNetPINVOKE.UDPProxyServerResultHandler_OnAlreadyLoggedIn(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void OnNoPasswordSet(RakString usedPassword, UDPProxyServer proxyServerPlugin) {
    RakNetPINVOKE.UDPProxyServerResultHandler_OnNoPasswordSet(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void OnWrongPassword(RakString usedPassword, UDPProxyServer proxyServerPlugin) {
    RakNetPINVOKE.UDPProxyServerResultHandler_OnWrongPassword(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  private void SwigDirectorConnect() {
    if (SwigDerivedClassHasMethod("OnLoginSuccess", swigMethodTypes0))
      swigDelegate0 = new SwigDelegateUDPProxyServerResultHandler_0(SwigDirectorOnLoginSuccess);
    if (SwigDerivedClassHasMethod("OnAlreadyLoggedIn", swigMethodTypes1))
      swigDelegate1 = new SwigDelegateUDPProxyServerResultHandler_1(SwigDirectorOnAlreadyLoggedIn);
    if (SwigDerivedClassHasMethod("OnNoPasswordSet", swigMethodTypes2))
      swigDelegate2 = new SwigDelegateUDPProxyServerResultHandler_2(SwigDirectorOnNoPasswordSet);
    if (SwigDerivedClassHasMethod("OnWrongPassword", swigMethodTypes3))
      swigDelegate3 = new SwigDelegateUDPProxyServerResultHandler_3(SwigDirectorOnWrongPassword);
    RakNetPINVOKE.UDPProxyServerResultHandler_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3);
  }

  private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes) {
    System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, methodTypes, null);
    bool hasDerivedMethod = methodInfo.DeclaringType.IsSubclassOf(typeof(UDPProxyServerResultHandler));
    return hasDerivedMethod;
  }

  private void SwigDirectorOnLoginSuccess(IntPtr usedPassword, IntPtr proxyServerPlugin) {
    OnLoginSuccess(new RakString(usedPassword, false), (proxyServerPlugin == IntPtr.Zero) ? null : new UDPProxyServer(proxyServerPlugin, false));
  }

  private void SwigDirectorOnAlreadyLoggedIn(IntPtr usedPassword, IntPtr proxyServerPlugin) {
    OnAlreadyLoggedIn(new RakString(usedPassword, false), (proxyServerPlugin == IntPtr.Zero) ? null : new UDPProxyServer(proxyServerPlugin, false));
  }

  private void SwigDirectorOnNoPasswordSet(IntPtr usedPassword, IntPtr proxyServerPlugin) {
    OnNoPasswordSet(new RakString(usedPassword, false), (proxyServerPlugin == IntPtr.Zero) ? null : new UDPProxyServer(proxyServerPlugin, false));
  }

  private void SwigDirectorOnWrongPassword(IntPtr usedPassword, IntPtr proxyServerPlugin) {
    OnWrongPassword(new RakString(usedPassword, false), (proxyServerPlugin == IntPtr.Zero) ? null : new UDPProxyServer(proxyServerPlugin, false));
  }

  public delegate void SwigDelegateUDPProxyServerResultHandler_0(IntPtr usedPassword, IntPtr proxyServerPlugin);
  public delegate void SwigDelegateUDPProxyServerResultHandler_1(IntPtr usedPassword, IntPtr proxyServerPlugin);
  public delegate void SwigDelegateUDPProxyServerResultHandler_2(IntPtr usedPassword, IntPtr proxyServerPlugin);
  public delegate void SwigDelegateUDPProxyServerResultHandler_3(IntPtr usedPassword, IntPtr proxyServerPlugin);

  private SwigDelegateUDPProxyServerResultHandler_0 swigDelegate0;
  private SwigDelegateUDPProxyServerResultHandler_1 swigDelegate1;
  private SwigDelegateUDPProxyServerResultHandler_2 swigDelegate2;
  private SwigDelegateUDPProxyServerResultHandler_3 swigDelegate3;

  private static Type[] swigMethodTypes0 = new Type[] { typeof(RakString), typeof(UDPProxyServer) };
  private static Type[] swigMethodTypes1 = new Type[] { typeof(RakString), typeof(UDPProxyServer) };
  private static Type[] swigMethodTypes2 = new Type[] { typeof(RakString), typeof(UDPProxyServer) };
  private static Type[] swigMethodTypes3 = new Type[] { typeof(RakString), typeof(UDPProxyServer) };
}

}
