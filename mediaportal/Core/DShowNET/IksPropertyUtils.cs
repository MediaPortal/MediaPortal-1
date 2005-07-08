using System;
using System.Runtime.InteropServices; 
using MediaPortal.GUI.Library;
namespace DShowNET
{
	/// <summary>
	/// Summary description for IksPropertyUtils.
	/// </summary>
	public class IksPropertyUtils
	{
		protected IBaseFilter captureFilter;

		protected enum KsPropertySupport:uint
		{
			Get=1,
			Set=2
		};


		// used by IKsPropertySet set AMPROPSETID_Pin
		protected enum AmPropertyPin:int
		{
			AMPROPERTY_PIN_CATEGORY,
			AMPROPERTY_PIN_MEDIUM
		} ;
		

		[StructLayout(LayoutKind.Sequential),  ComVisible(false)]
		protected 	struct KSPROPERTY
		{
			Guid    Set;
			int   Id;
			int   Flags;
		};
		[StructLayout(LayoutKind.Sequential), ComVisible(false)]
		protected 	struct KSPROPERTYByte
		{
			public Guid    Set;  //16		0-15
			public int   Id;		 //4		16-19
			public int   Flags;	 //4		20-23
			public int alignment;//4		24-27
			public byte byData;	 //     28-31
		};		

		[StructLayout(LayoutKind.Sequential), ComVisible(false)]
		protected 	struct KSPROPERTYInt
		{
			public Guid    Set;  //16		0-15
			public int   Id;		 //4		16-19
			public int   Flags;	 //4		20-23
			public int alignment;//4		24-27
			public int byData;	 //     28-31
		};		


		[ComVisible(true), ComImport,
			Guid("31EFAC30-515C-11d0-A9AA-00AA0061BE93"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
			public interface IKsPropertySet
		{
			[PreserveSig]
			int RemoteSet([In] ref Guid guidPropSet, 
				[In] uint dwPropID, 
				[In] IntPtr pInstanceData, 
				[In] uint cbInstanceData, 
				[In] IntPtr pPropData, 
				[In] uint cbPropData);
			[PreserveSig]
			int RemoteGet([In] ref Guid guidPropSet, 
				[In] uint dwPropID, 
				[In] IntPtr pInstanceData, 
				[In] uint cbInstanceData, 
				[In]  IntPtr pPropData, 
				[In] uint cbPropData, 
				out uint pcbReturned);
			[PreserveSig]
			int QuerySupported([In] ref Guid guidPropSet, [In] uint dwPropID, out uint pTypeSupport);
		}


		public IksPropertyUtils(IBaseFilter filter)
		{
			captureFilter=filter;
		}

		protected byte GetByteValue(Guid guidPropSet, uint propId)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetByteValue() properySet=null");
				return 0;
			}
			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetByteValue() property is not supported");
				return 0;
			}

			byte returnValue=0;
			KSPROPERTYByte propByte = new KSPROPERTYByte();
			KSPROPERTY prop         = new KSPROPERTY();
			int sizeProperty     = Marshal.SizeOf(prop);
			int sizeByteProperty = Marshal.SizeOf(propByte);

			KSPROPERTYByte newByteValue = new KSPROPERTYByte();
			IntPtr pDataReturned=Marshal.AllocCoTaskMem(100);
			Marshal.StructureToPtr(newByteValue,pDataReturned,true);

			int adress=pDataReturned.ToInt32()+sizeProperty;
			IntPtr ptrData = new IntPtr(adress);
			hr=propertySet.RemoteGet(ref propertyGuid,
				propId,
				ptrData,
				(uint)(sizeByteProperty-sizeProperty), 
				pDataReturned,
				(uint)sizeByteProperty,
				out uiSize);
			if (hr==0 && uiSize==1)
			{
				returnValue=Marshal.ReadByte(ptrData);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
			
			if (hr!=0)
			{
				Log.Write("GetByteValue() failed 0x{0:X}",hr);
			}
			return returnValue;
		}

		protected void SetByteValue(Guid guidPropSet, uint propId, byte byteValue)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			if (propertySet==null) 
			{
				Log.Write("GetByteValue() properySet=null");
				return ;
			}
			uint IsTypeSupported=0;

			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SetByteValue() property is not supported");
				return ;
			}

			KSPROPERTYByte KsProperty  = new KSPROPERTYByte ();
			KsProperty.byData=byteValue;
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
			Marshal.StructureToPtr(KsProperty, pDataReturned,false);
			hr=propertySet.RemoteSet(ref propertyGuid,
				propId,
				pDataReturned,
				1, 
				pDataReturned,
				(uint)Marshal.SizeOf(KsProperty) );
			Marshal.FreeCoTaskMem(pDataReturned);
			
			if (hr!=0)
			{
				Log.Write("SetByteValue() failed 0x{0:X}",hr);
			}
		}

		protected int GetIntValue(Guid guidPropSet, uint propId)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetIntValue() properySet=null");
				return 0;
			}
			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetIntValue() property is not supported");
				return 0;
			}
      
			int returnValue=0;
			KSPROPERTYInt propInt = new KSPROPERTYInt();
			KSPROPERTY prop         = new KSPROPERTY();
			int sizeProperty     = Marshal.SizeOf(prop);
			int sizeIntProperty = Marshal.SizeOf(propInt);

			KSPROPERTYInt newIntValue = new KSPROPERTYInt();
			IntPtr pDataReturned=Marshal.AllocCoTaskMem(100);
			Marshal.StructureToPtr(newIntValue,pDataReturned,true);

			int adress=pDataReturned.ToInt32()+sizeProperty;
			IntPtr ptrData = new IntPtr(adress);
			hr=propertySet.RemoteGet(ref propertyGuid,
				propId,
				ptrData,
				(uint)(sizeIntProperty-sizeProperty), 
				pDataReturned,
				(uint)sizeIntProperty,
				out uiSize);


			if (hr==0 && uiSize==4)
			{
				returnValue=Marshal.ReadInt32(pDataReturned);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
			return returnValue;
		}

		protected void SetIntValue(Guid guidPropSet, uint propId, int intValue)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			if (propertySet==null) 
			{
				Log.Write("SetIntValue() properySet=null");
				return ;
			}
			uint IsTypeSupported=0;

			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SetIntValue() property is not supported");
				return ;
			}
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
			Marshal.WriteInt32(pDataReturned, intValue);
			hr=propertySet.RemoteSet(ref propertyGuid,propId,pDataReturned,4, pDataReturned,4);
			if (hr!=0)
			{
				Log.Write("SetIntValue() failed 0x{0:X}",hr);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
		}

		protected string GetString(Guid guidPropSet, uint propId)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetString() properySet=null");
				return String.Empty;
			}
			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetString() property is not supported");
				return String.Empty;
			}

			IntPtr pDataReturned = Marshal.AllocCoTaskMem(100);
			string returnedText=String.Empty;
			hr=propertySet.RemoteGet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,100,out uiSize);
			if (hr==0)
			{
				returnedText=Marshal.PtrToStringAnsi(pDataReturned,(int)uiSize);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
			return returnedText;
		}


		protected object GetStructure(Guid guidPropSet, uint propId, System.Type structureType)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			uint uiSize;
			if (propertySet==null) 
			{
				Log.Write("GetStructure() properySet=null");
				return null;
			}
			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
			{
				Log.Write("GetString() GetStructure is not supported");
				return null;
			}

			object objReturned=null;
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(1000);
			hr=propertySet.RemoteGet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,1000,out uiSize);
			if (hr==0)
			{
				objReturned=Marshal.PtrToStructure(pDataReturned, structureType);
			}
			else
			{
				Log.Write("GetStructure() failed 0x{0:X}",hr);
			}
			Marshal.FreeCoTaskMem(pDataReturned);
			return objReturned;
		}

		protected virtual void SetStructure(Guid guidPropSet, uint propId, System.Type structureType, object structValue)
		{
			Guid propertyGuid=guidPropSet;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			if (propertySet==null) 
			{
				Log.Write("SetStructure() properySet=null");
				return ;
			}

			int hr=propertySet.QuerySupported( ref propertyGuid, propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("GetString() GetStructure is not supported");
				return ;
			}

			int iSize=Marshal.SizeOf(structureType);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(iSize);
			Marshal.StructureToPtr(structValue,pDataReturned,true);
			hr=propertySet.RemoteSet(ref propertyGuid,propId,pDataReturned,(uint)iSize, pDataReturned,(uint)iSize );
			if (hr!=0)
			{
				Log.Write("SetStructure() failed 0x{0:X}",hr);
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
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				return true;
			}
		}


	}
}
