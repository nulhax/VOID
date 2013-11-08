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

public class RakNetListFilterQuery : IDisposable {
  private HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal RakNetListFilterQuery(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(RakNetListFilterQuery obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~RakNetListFilterQuery() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          RakNetPINVOKE.delete_RakNetListFilterQuery(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }

    public FilterQuery this[int index]  
    {  
        get   
        {
            return Get((uint)index); // use indexto retrieve and return another value.    
        }  
        set   
        {
            Replace(value, value, (uint)index, "Not used", 0);// use index and value to set the value somewhere.   
        }  
    } 

  public RakNetListFilterQuery() : this(RakNetPINVOKE.new_RakNetListFilterQuery__SWIG_0(), true) {
  }

  public RakNetListFilterQuery(RakNetListFilterQuery original_copy) : this(RakNetPINVOKE.new_RakNetListFilterQuery__SWIG_1(RakNetListFilterQuery.getCPtr(original_copy)), true) {
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public RakNetListFilterQuery CopyData(RakNetListFilterQuery original_copy) {
    RakNetListFilterQuery ret = new RakNetListFilterQuery(RakNetPINVOKE.RakNetListFilterQuery_CopyData(swigCPtr, RakNetListFilterQuery.getCPtr(original_copy)), false);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FilterQuery Get(uint position) {
    FilterQuery ret = new FilterQuery(RakNetPINVOKE.RakNetListFilterQuery_Get(swigCPtr, position), false);
    return ret;
  }

  public void Push(FilterQuery input, string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Push(swigCPtr, FilterQuery.getCPtr(input), file, line);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public FilterQuery Pop() {
    FilterQuery ret = new FilterQuery(RakNetPINVOKE.RakNetListFilterQuery_Pop(swigCPtr), false);
    return ret;
  }

  public void Insert(FilterQuery input, uint position, string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Insert__SWIG_0(swigCPtr, FilterQuery.getCPtr(input), position, file, line);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Insert(FilterQuery input, string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Insert__SWIG_1(swigCPtr, FilterQuery.getCPtr(input), file, line);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Replace(FilterQuery input, FilterQuery filler, uint position, string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Replace__SWIG_0(swigCPtr, FilterQuery.getCPtr(input), FilterQuery.getCPtr(filler), position, file, line);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Replace(FilterQuery input) {
    RakNetPINVOKE.RakNetListFilterQuery_Replace__SWIG_1(swigCPtr, FilterQuery.getCPtr(input));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAtIndex(uint position) {
    RakNetPINVOKE.RakNetListFilterQuery_RemoveAtIndex(swigCPtr, position);
  }

  public void RemoveAtIndexFast(uint position) {
    RakNetPINVOKE.RakNetListFilterQuery_RemoveAtIndexFast(swigCPtr, position);
  }

  public void RemoveFromEnd(uint num) {
    RakNetPINVOKE.RakNetListFilterQuery_RemoveFromEnd__SWIG_0(swigCPtr, num);
  }

  public void RemoveFromEnd() {
    RakNetPINVOKE.RakNetListFilterQuery_RemoveFromEnd__SWIG_1(swigCPtr);
  }

  public uint Size() {
    uint ret = RakNetPINVOKE.RakNetListFilterQuery_Size(swigCPtr);
    return ret;
  }

  public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
  }

  public void Preallocate(uint countNeeded, string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Preallocate(swigCPtr, countNeeded, file, line);
  }

  public void Compress(string file, uint line) {
    RakNetPINVOKE.RakNetListFilterQuery_Compress(swigCPtr, file, line);
  }

}

}
