#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using MediaPortal.GUI.Library;

namespace DShowNET
{
  /// <summary>
  /// Summary description for IksPropertyUtils.
  /// </summary>
  public class IksPropertyUtils2
  {
    protected IBaseFilter captureFilter;


    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    protected struct KSPROPERTY
    {
      public Guid Set;
      public int Id;
      public int Flags;
    } ;


    [ComImport,
     Guid("31EFAC30-515C-11d0-A9AA-00AA0061BE93"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKsPropertySet
    {
      [PreserveSig]
      int Set(
        [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
        [In] int dwPropID,
        [In] IntPtr pInstanceData,
        [In] int cbInstanceData,
        [In] IntPtr pPropData,
        [In] int cbPropData
        );

      [PreserveSig]
      int Get(
        [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
        [In] int dwPropID,
        [In] IntPtr pInstanceData,
        [In] int cbInstanceData,
        [In, Out] IntPtr pPropData,
        [In] int cbPropData,
        [Out] out int pcbReturned
        );

      [PreserveSig]
      int QuerySupported(
        [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
        [In] int dwPropID,
        [Out] out KSPropertySupport pTypeSupport
        );
    }


    public IksPropertyUtils2(IBaseFilter filter)
    {
      captureFilter = filter;
    }


    protected int GetIntValue(Guid guidPropSet, int propId)
    {
      int returnValue = 0;
      IKsPropertySet propertySet = (IKsPropertySet) captureFilter;
      KSPropertySupport IsTypeSupported = 0;
      int iSize;
      if (propertySet == null)
      {
        Log.Info("GetIntValue() properySet=null");
        return 0;
      }
      int hr = propertySet.QuerySupported(guidPropSet, propId, out IsTypeSupported);
      if (hr != 0 && ((int) IsTypeSupported & (int) KSPropertySupport.Get) == 0)
      {
        Log.Info("KS Test: GetIntValue() property is not supported");
        return 0;
      }
      else
      {
        Log.Info("Ks Test: GetIntValue() property is supported");
      }


      IntPtr pDataReturned = Marshal.AllocCoTaskMem(4);
      hr = propertySet.Get(guidPropSet, propId, IntPtr.Zero, 0, pDataReturned, 4, out iSize);

      Log.Info("Ks Test: hr - 0x{0:X}", hr);
      Log.Info("Ks Test: usize = {0}", iSize);

      if (hr == 0 && iSize == 4)
      {
        returnValue = Marshal.ReadInt32(pDataReturned);
        Log.Info("Ks Test  returnvalue - {0}", returnValue);
      }
      if (hr == 0 && iSize == 1)
      {
        returnValue = Marshal.ReadByte(pDataReturned);
        Log.Info("KS Test read byte - prop - {0}, value - {1}", propId, returnValue);
      }
      Marshal.FreeCoTaskMem(pDataReturned);
      return returnValue;
    }

    protected void SetIntValue(Guid guidPropSet, int propId, int intValue)
    {
      IKsPropertySet propertySet = (IKsPropertySet) captureFilter;

      if (propertySet == null)
      {
        Log.Info("Ks Test: SetIntValue() properySet=null");
        return;
      }
      KSPropertySupport IsTypeSupported = 0;

      int hr = propertySet.QuerySupported(guidPropSet, propId, out IsTypeSupported);
      if (hr != 0 && ((int) IsTypeSupported & (int) KSPropertySupport.Set) == 0)
      {
        Log.Info("Ks Test: SetIntValue() property is not supported");
        return;
      }
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(4);
      Marshal.WriteInt32(pDataReturned, intValue);
      hr = propertySet.Set(guidPropSet, propId, IntPtr.Zero, 0, pDataReturned, 4);
      if (hr != 0)
      {
        Log.Info("Ks Test: SetIntValue() failed 0x{0:X}", hr);
      }
      else
      {
        Log.Info("Ks Test: Set PropID - {0}, to {1}", propId, intValue);
      }
      Marshal.FreeCoTaskMem(pDataReturned);
    }

    protected string GetString(Guid guidPropSet, uint propId)
    {
      Guid propertyGuid = guidPropSet;
      IKsPropertySet propertySet = captureFilter as IKsPropertySet;
      string returnedText = string.Empty;
      //   uint IsTypeSupported=0;
      //   uint uiSize;
      //  if (propertySet==null) 
      //  {
      //    Log.Info("GetString() properySet=null");
      //    return string.Empty;
      //  }
      //  int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
      //  if (hr!=0 && (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
      //  {
      //    Log.Info("GetString() property is not supported");
      //    return string.Empty;
      //  }

      //  IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
      //  
      //  hr=propertySet.Get(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,100,out uiSize);
      //  if (hr==0)
      //  {
      //    returnedText=Marshal.PtrToStringAnsi(pDataReturned,(int)uiSize);
      //  }
      //  Marshal.FreeCoTaskMem(pDataReturned);
      return returnedText;
    }


    protected object GetStructure(Guid guidPropSet, int propId, Type structureType, int structsize)
    {
      //Guid propertyGuid=guidPropSet;
      IKsPropertySet propertySet = (IKsPropertySet) captureFilter;
      object objReturned = null;
      KSPropertySupport IsTypeSupported = 0;
      int iSize;
      if (propertySet == null)
      {
        Log.Info("GetStructure() properySet=null");
        return null;
      }
      int hr = propertySet.QuerySupported(guidPropSet, propId, out IsTypeSupported);
      Log.Info("IKS: typesupported - {0}", IsTypeSupported);
      if (hr != 0 && ((int) IsTypeSupported & (int) KSPropertySupport.Get) == 0)
      {
        Log.Info("GetStructure is not supported");
        return null;
      }

      //hr = propertySet.Get(ref propertyGuid, propId, IntPtr.Zero, 0, IntPtr.Zero, 0, out uiSize);
      //Log.Info("IVAC: needed uisize - {0} ", uiSize);
      //Log.Info("IVAC: hr - 0x{0:X}", hr);
      // 
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(1000);

      //Log.Info("IVAC: 0x{0:X}", hr);
      hr = propertySet.Get(guidPropSet, propId, IntPtr.Zero, 0, pDataReturned, structsize, out iSize);
      if (hr == 0)
      {
        Log.Info("Ks Test: Returned struct size succeeded - uisize - {0}", iSize);
        objReturned = Marshal.PtrToStructure(pDataReturned, structureType);
      }
      else
      {
        Log.Info("Ks Test: PropID - {0}", propId);
        Log.Info("Ks Test: Returned size - uisize - {0}", iSize);
        Log.Info("Ks Test: GetStructure() failed 0x{0:X}", hr);
      }
      Marshal.FreeCoTaskMem(pDataReturned);
      return objReturned;
    }

    protected virtual void SetStructure(Guid guidPropSet, uint propId, Type structureType, object structValue)
    {
      Guid propertyGuid = guidPropSet;
      IKsPropertySet propertySet = captureFilter as IKsPropertySet;
      KSPropertySupport IsTypeSupported = 0;
      if (propertySet == null)
      {
        Log.Info("SetStructure() properySet=null");
        return;
      }

      int hr = propertySet.QuerySupported(propertyGuid, (int) propId, out IsTypeSupported);
      if (hr != 0 && ((int) IsTypeSupported & (int) KSPropertySupport.Set) == 0)
      {
        Log.Info("GetString() GetStructure is not supported");
        return;
      }

      int iSize = Marshal.SizeOf(structureType);
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(iSize);
      Marshal.StructureToPtr(structValue, pDataReturned, true);
      hr = propertySet.Set(propertyGuid, (int) propId, pDataReturned, iSize, pDataReturned, iSize);
      if (hr != 0)
      {
        Log.Info("SetStructure() failed 0x{0:X}", hr);
      }
      Marshal.FreeCoTaskMem(pDataReturned);
    }


    /// <summary>
    /// Checks if the card specified supports getting/setting properties using the IKsPropertySet interface
    /// </summary>
    /// <returns>
    /// true:		IKsPropertySet is supported
    /// false:	IKsPropertySet is not supported
    /// </returns>
    public bool SupportsProperties
    {
      get
      {
        IKsPropertySet propertySet = captureFilter as IKsPropertySet;
        if (propertySet == null)
        {
          return false;
        }
        return true;
      }
    }
  }
}