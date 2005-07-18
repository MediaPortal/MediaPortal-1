using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Mixer
{
	internal class MixerNativeMethods
	{
		#region Constructors

		private MixerNativeMethods()
		{
		}

		#endregion Constructors

		#region Methods

		[DllImport("winmm.dll")] 
		public static extern MixerError mixerClose(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern MixerError mixerGetControlDetailsA(IntPtr handle, ref MixerControlDetails mixerControlDetails, int fdwDetails);

//		[DllImport("winmm.dll")] 
//		public static extern int mixerGetDevCapsA(int uMxId, MIXERCAPS pmxcaps, int cbmxcaps); 

//		[DllImport("winmm.dll")] 
//		public static extern int mixerGetID(IntPtr handle, int pumxID, int fdwId);

		[DllImport("winmm.dll")] 
		public static extern MixerError mixerGetLineControlsA(IntPtr handle, ref MixerLineControls mixerLineControls, MixerLineControlFlags flags);

		[DllImport("winmm.dll")]
		public static extern MixerError mixerGetLineInfoA(IntPtr handle, ref MixerLine mixerLine, MixerLineFlags flags);
 
//		[DllImport("winmm.dll")]
//		public static extern int mixerGetNumDevs();

		[DllImport("winmm.dll")]
		public static extern MixerError mixerMessage(int hmx, int uMsg, int dwParam1, int dwParam2); 

		[DllImport("winmm.dll")] 
		public static extern MixerError mixerOpen(ref IntPtr handle, int index, IntPtr callbackWindowHandle, int dwInstance, MixerFlags flags); 
	
		[DllImport("winmm.dll")] 
		public static extern MixerError mixerOpen(ref IntPtr handle, int index, MixerCallback callback, int dwInstance, MixerFlags flags); 

		[DllImport("winmm.dll")] 
		public static extern MixerError mixerSetControlDetails(IntPtr handle, ref MixerControlDetails mixerControlDetails, int fdwDetails); 

		#endregion Methods

		#region Structures

		public struct MixerControl
		{
			public int						Size;
			public int						ControlId;
			public MixerControlType			ControlType;
			public int						fdwControl;
			public int						MultipleItems;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)]
			public string					ShortName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
			public string					Name;

			public int						Minimum;
			public int						Maximum;

			[MarshalAs(UnmanagedType.U4, SizeConst=10)] 
			public int						Reserved;
		}

		public struct MixerControlDetails
		{ 
			public int						Size;
			public int						ControlId;
			public int						Channels;
			public int						Item;
			public int						DataSize;
			public IntPtr					Data;
		}

		public struct MixerLine
		{
			public int						Size; 
			public int						Destination; 
			public int						Source; 
			public int						LineId; 
			public MixerLineStatusFlags		Status; 
			public int						dwUser; 
			public MixerComponentType		ComponentType; 
			public int						Channels; 
			public int						Connections; 
			public int						Controls; 

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)] 
			public string					ShortName; 

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)] 
			public string					Name; 
			
			public MixerLineTargetType		Type; 
			public int						DeviceId;
			public short					ManufacturerId; 
			public short					ProductId; 
			public int						DriverVersion;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] 
			public string					ProductName; 
		}

		public struct MixerLineControls
		{ 
			public int						Size; 
			public int						LineId;
			public uint						ControlType;
			public int						Controls;
			public int						DataSize;
			public IntPtr					Data; 
		}

		#endregion Structures
	}
}
