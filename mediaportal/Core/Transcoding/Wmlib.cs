#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#region DirectX.Capture
// ------------------------------------------------------------------
// DirectX.Capture
//
// History:
// 2006-March-20 HV - created
//
// Copyright (C) 2006 Hans Vosman
//
// This is a subset of API declarations, enumerations, structures and
// interfaces that are needed for the DirectX.Capture class example.
// The source of information is the Windows Media SDK and the internet.
// At http://cvs.sourceforge.net/viewcvs.py/directshownet/windowsmedialib
// you can find the best and most complete C# Windows Media library that
// is currently available ... Because I could not find an official and
// good quality library, this one is used.
// ------------------------------------------------------------------
#endregion

using System;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace MediaPortal.Core.Transcoding
{
	#region API Declares
	sealed public class WMLib
	{
		/// <summary>
		/// Windows Media create profile manager
		/// </summary>
		/// <param name="ppProfileManager"></param>
		/// <returns></returns>
		[DllImport("WMVCore.dll")]
		public static extern int WMCreateProfileManager(out IWMProfileManager ppProfileManager);
	}
	#endregion

	#region Declarations
	/// <summary>
	/// Windows media version number
	/// </summary>
	public enum WMVersion
	{
		/// <summary> Version 4 </summary>
		V4_0  = 0x00040000,
		/// <summary> Version 7 </summary>
		V7_0  = 0x00070000,
		/// <summary> Version 8 </summary>
		V8_0  = 0x00080000,
		/// <summary> Version 9 </summary>
		V9_0  = 0x00090000
	}

	/// <summary>
	/// From unnamed enum
	/// </summary>
	[Flags]
	public enum WriteFlags
	{
		None = 0,
		CleanPoint	= 0x1,
		DisContinuity	= 0x2,
		DataLoss	= 0x4
	}

	/// <summary>
	/// From WMT_ATTR_DATATYPE
	/// </summary>
	public enum AttrDataType
	{
		DWORD   = 0,
		STRING  = 1,
		BINARY  = 2,
		BOOL    = 3,
		QWORD   = 4,
		WORD    = 5,
		GUID    = 6
	}

	/// <summary>
	/// From WM_WRITER_STATISTICS
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct WriterStatistics
	{
		public long qwSampleCount;
		public long qwByteCount;
		public long qwDroppedSampleCount;
		public long qwDroppedByteCount;
		public int dwCurrentBitrate;
		public int dwAverageBitrate;
		public int dwExpectedBitrate;
		public int dwCurrentSampleRate;
		public int dwAverageSampleRate;
		public int dwExpectedSampleRate;
	}

  /// <summary>
  /// The WMVIDEOINFOHEADER structure describes the bitmap and color information for a video image.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct WMVIDEOINFOHEADER
  {
    public DsRect SrcRect;
    public DsRect TargetRect;
    public int BitRate;
    public int BitErrorRate;
    public long AvgTimePerFrame;
    public BitmapInfoHeader BmiHeader;
  }
	#endregion

	#region Interfaces
	/// <summary>
	/// IConfigAsfWriter (needed for DShowNET version)
	/// </summary>
	[Guid("45086030-F7E4-486a-B504-826BB5792A3B"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IConfigAsfWriter
	{
		/// Obsolete?
		[PreserveSig]
		int ConfigureFilterUsingProfileId([In] int dwProfileId);

		/// Obsolete?
		[PreserveSig] 
		int GetCurrentProfileId([Out] out int pdwProfileId);

		/// <summary>
		///  Configure filter using profile guid
		/// </summary>
		/// <param name="guidProfile"></param>
		/// <returns></returns>
		[PreserveSig]
		int ConfigureFilterUsingProfileGuid([In, MarshalAs(UnmanagedType.LPStruct)] Guid guidProfile);

		/// <summary>
		/// Get current profile guid
		/// </summary>
		/// <param name="pProfileGuid"></param>
		/// <returns></returns>
		[PreserveSig]
		int GetCurrentProfileGuid([Out] out Guid pProfileGuid);

		/// <summary>
		/// Configure filter using profile
		/// </summary>
		/// <param name="pProfile"></param>
		/// <returns></returns>
		[PreserveSig]
		int ConfigureFilterUsingProfile([In] IWMProfile pProfile);

		/// <summary>
		/// Get current profile
		/// </summary>
		/// <param name="ppProfile"></param>
		/// <returns></returns>
		[PreserveSig]
		int GetCurrentProfile([Out] out IWMProfile ppProfile);

		/// <summary>
		/// Set index mode
		/// </summary>
		/// <param name="bIndexFile"></param>
		/// <returns></returns>
		[PreserveSig]
		int SetIndexMode([In, MarshalAs(UnmanagedType.Bool)] bool bIndexFile);

		/// <summary>
		/// Get index mode
		/// </summary>
		/// <param name="pbIndexFile"></param>
		/// <returns></returns>
		[PreserveSig]
		int GetIndexMode([Out, MarshalAs(UnmanagedType.Bool)] out bool pbIndexFile);
	}

  [StructLayout(LayoutKind.Sequential)]
  public class WmMediaType
  {
    public Guid majortype;
    public Guid subtype;
    [MarshalAs(UnmanagedType.Bool)]
    public bool bFixedSizeSamples;
    [MarshalAs(UnmanagedType.Bool)]
    public bool bTemporalCompression;
    public uint lSampleSize;
    public Guid formattype;
    public IntPtr pUnk;
    public uint cbFormat;
    public IntPtr pbFormat;
  }

	/// <summary>
	/// IWMProfileManager interface
	/// </summary>
	[Guid("D16679F2-6CA0-472D-8D31-2F5D55AEE155"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWMProfileManager
	{
		/// <summary> CreateEmptyProfile </summary>
		[PreserveSig]
		int CreateEmptyProfile(
			[In] WMVersion dwVersion,
			out IWMProfile ppProfile
			);

		/// <summary> Load profile by ID </summary>
		[PreserveSig]
		int LoadProfileByID(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidProfile,
			out IWMProfile ppProfile
			);

		/// <summary> Load profile by data </summary>
		[PreserveSig]
		int LoadProfileByData(
			[In] string pwszProfile,
			out IWMProfile ppProfile
			);

		/// <summary> save profile </summary>
		[PreserveSig]
		int SaveProfile(
			[In] IWMProfile pIWMProfile,
			[In] string pwszProfile,
			ref int pdwLength
			);

		/// <summary> Get system profile count </summary>
		[PreserveSig]
		int GetSystemProfileCount(
			out int pcProfiles
			);

		/// <summary> Load system profile </summary>
		[PreserveSig]
		int LoadSystemProfile(
			[In] int dwProfileIndex,
			out IWMProfile ppProfile
			);
	}

	/// <summary>
	/// IWMProfileManager2 interface
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("7A924E51-73C1-494D-8019-23D37ED9B89A")]
	public interface IWMProfileManager2 : IWMProfileManager
	{
		#region IWMProfileManager Methods

		/// <summary> Create empty profile </summary>
		[PreserveSig]
		new int CreateEmptyProfile(
			[In] WMVersion dwVersion,
			out IWMProfile ppProfile
			);

		/// <summary> Load profile by ID </summary>
		[PreserveSig]
		new int LoadProfileByID(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidProfile,
			out IWMProfile ppProfile
			);

		/// <summary> Load profile by data </summary>
		[PreserveSig]
		new int LoadProfileByData(
			[In] string pwszProfile,
			out IWMProfile ppProfile
			);

		/// <summary> Save profile </summary>
		[PreserveSig]
		new int SaveProfile(
			[In] IWMProfile pIWMProfile,
			[In] string pwszProfile,
			ref int pdwLength
			);

		/// <summary> Get system profile count </summary>
		[PreserveSig]
		new int GetSystemProfileCount(
			out int pcProfiles
			);

		/// <summary> Load system profile </summary>
		[PreserveSig]
		new int LoadSystemProfile(
			[In] int dwProfileIndex,
			out IWMProfile ppProfile
			);

		#endregion

		/// <summary> Get system profile version </summary>
		[PreserveSig]
		int GetSystemProfileVersion(
			out WMVersion pdwVersion
			);

		/// <summary> Set system profile version </summary>
		[PreserveSig]
		int SetSystemProfileVersion(
			WMVersion dwVersion
			);
	}

	/// <summary>
	/// IWMProfileManagerLanguage interface
	/// </summary>
	[Guid("BA4DCC78-7EE0-4AB8-B27A-DBCE8BC51454"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWMProfileManagerLanguage
	{
		/// <summary> Get user language ID, 0x409 = US, 0x413 = NL </summary>
		[PreserveSig]
		int GetUserLanguageID(
			out short wLangID
			);

		/// <summary> Set user language ID </summary>
		[PreserveSig]
		int SetUserLanguageID(
			short wLangID
			);
	}

	/// <summary>
	/// IWMStreamConfig interface
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("96406BDC-2B2B-11D3-B36B-00C04F6108FF")]
	public interface IWMStreamConfig
	{
		/// <summary> Get stream type </summary>
		[PreserveSig]
		int GetStreamType(
			out Guid pguidStreamType
			);

		/// <summary> Get stream number </summary>
		[PreserveSig]
		int GetStreamNumber(
			out short pwStreamNum
			);

		/// <summary> Set stream number </summary>
		[PreserveSig]
		int SetStreamNumber(
			[In] short wStreamNum
			);

		/// <summary> Get stream name </summary>
		[PreserveSig]
		int GetStreamName(
			[Out] StringBuilder pwszStreamName,
			ref short pcchStreamName
			);

		/// <summary> Set stream name </summary>
		[PreserveSig]
		int SetStreamName(
			[In] string pwszStreamName
			);

		/// <summary> Get connection name </summary>
		[PreserveSig]
		int GetConnectionName(
			[Out] StringBuilder pwszInputName,
			ref short pcchInputName
			);

		/// <summary> Set connection name </summary>
		[PreserveSig]
		int SetConnectionName(
			[In] string pwszInputName
			);

		/// <summary> Get bitrate </summary>
		[PreserveSig]
		int GetBitrate(
			out int pdwBitrate
			);

		/// <summary> Set bitrate </summary>
		[PreserveSig]
		int SetBitrate(
			[In] int pdwBitrate
			);

		/// <summary> Get buffer window </summary>
		[PreserveSig]
		int GetBufferWindow(
			out int pmsBufferWindow
			);

		/// <summary> Set buffer window </summary>
		[PreserveSig]
		int SetBufferWindow(
			[In] int msBufferWindow
			);
	}

	/// <summary>
	/// IWMProfile interface
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("96406BDB-2B2B-11D3-B36B-00C04F6108FF")]
	public interface IWMProfile
	{
		/// <summary> Get version </summary>
		[PreserveSig]
		int GetVersion(
			out WMVersion pdwVersion
			);

		/// <summary> Get name </summary>
		[PreserveSig]
		int GetName(
			[Out] StringBuilder pwszName,
			ref int pcchName
			);

		/// <summary> Set name </summary>
		[PreserveSig]
		int SetName(
			[In] string pwszName
			);

		/// <summary> Get description </summary>
		[PreserveSig]
		int GetDescription(
			[Out] StringBuilder pwszDescription,
			ref int pcchDescription
			);

		/// <summary> Set description </summary>
		[PreserveSig]
		int SetDescription(
			[In] string pwszDescription
			);

		/// <summary> Get stream count </summary>
		[PreserveSig]
		int GetStreamCount(
			out int pcStreams
			);

		/// <summary> Get stream </summary>
		[PreserveSig]
		int GetStream(
			[In] int dwStreamIndex,
			out IWMStreamConfig ppConfig
			);

		/// <summary> Get stream by number </summary>
		[PreserveSig]
		int GetStreamByNumber(
			[In] short wStreamNum,
			out IWMStreamConfig ppConfig
			);

		/// <summary> Remove stream </summary>
		[PreserveSig]
		int RemoveStream(
			[In] IWMStreamConfig pConfig
			);

		/// <summary> Remove stream by number </summary>
		[PreserveSig]
		int RemoveStreamByNumber(
			[In] short wStreamNum
			);

		/// <summary> Add stream </summary>
		[PreserveSig]
		int AddStream(
			[In] IWMStreamConfig pConfig
			);

		/// <summary> Reconfigure stream </summary>
		[PreserveSig]
		int ReconfigStream(
			[In] IWMStreamConfig pConfig
			);

		/// <summary> Create new stream </summary>
		[PreserveSig]
		int CreateNewStream(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidStreamType,
			out IWMStreamConfig ppConfig
			);

		/// <summary> Get mutual exclusion count </summary>
		[PreserveSig]
		int GetMutualExclusionCount(
			out int pcME
			);

		/// <summary> Get mutual exclusion </summary>
		[PreserveSig]
		int GetMutualExclusion(
			[In] int dwMEIndex,
			out IWMMutualExclusion ppME
			);

		/// <summary> Remove mutual exclusion </summary>
		[PreserveSig]
		int RemoveMutualExclusion(
			[In] IWMMutualExclusion pME
			);

		/// <summary> Add mutual exclusion </summary>
		[PreserveSig]
		int AddMutualExclusion(
			[In] IWMMutualExclusion pME
			);

		/// <summary> Create new mutual exclusion </summary>
		[PreserveSig]
		int CreateNewMutualExclusion(
			out IWMMutualExclusion ppME
			);
	}

	/// <summary>
	/// IWMProfile2 interface
	/// </summary>
	[Guid("07E72D33-D94E-4BE7-8843-60AE5FF7E5F5"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWMProfile2 : IWMProfile
	{
		#region IWMProfile Methods

		/// <summary>  </summary>
		[PreserveSig]
		new int GetVersion(
			out WMVersion pdwVersion
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetName(
			[Out] StringBuilder pwszName,
			ref int pcchName
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int SetName(
			[In] string pwszName
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetDescription(
			[Out] StringBuilder pwszDescription,
			ref int pcchDescription
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int SetDescription(
			[In] string pwszDescription
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetStreamCount(
			out int pcStreams
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetStream(
			[In] int dwStreamIndex,
			out IWMStreamConfig ppConfig
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetStreamByNumber(
			[In] short wStreamNum,
			out IWMStreamConfig ppConfig
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int RemoveStream(
			[In] IWMStreamConfig pConfig
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int RemoveStreamByNumber(
			[In] short wStreamNum
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int AddStream(
			[In] IWMStreamConfig pConfig
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int ReconfigStream(
			[In] IWMStreamConfig pConfig
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int CreateNewStream(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidStreamType,
			out IWMStreamConfig ppConfig
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetMutualExclusionCount(
			out int pcME
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int GetMutualExclusion(
			[In] int dwMEIndex,
			out IWMMutualExclusion ppME
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int RemoveMutualExclusion(
			[In] IWMMutualExclusion pME
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int AddMutualExclusion(
			[In] IWMMutualExclusion pME
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int CreateNewMutualExclusion(
			out IWMMutualExclusion ppME
			);

		#endregion

		/// <summary> Get profile ID </summary>
		[PreserveSig]
		int GetProfileID(
			out Guid pguidID
			);
	}

	/// <summary>
	/// IWMMutualExclusion interface
	/// </summary>
	[Guid("96406BDE-2B2B-11D3-B36B-00C04F6108FF"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWMMutualExclusion : IWMStreamList
	{
		#region IWMStreamList Methods

		/// <summary>  </summary>
		[PreserveSig]
		new int GetStreams(
			out short [] pwStreamNumArray,
			ref short pcStreams
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int AddStream(
			[In] short wStreamNum
			);

		/// <summary>  </summary>
		[PreserveSig]
		new int RemoveStream(
			[In] short wStreamNum
			);

		#endregion

		/// <summary> Get type </summary>
		[PreserveSig]
		int GetType(
			out Guid pguidType
			);

		/// <summary> Set type </summary>
		[PreserveSig]
		int SetType(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidType
			);
	}

	/// <summary>
	/// IWMStreamList interface
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("96406BDD-2B2B-11D3-B36B-00C04F6108FF")]
	public interface IWMStreamList
	{
		/// <summary> Get streams </summary>
		[PreserveSig]
		int GetStreams(
			out short [] pwStreamNumArray,
			ref short pcStreams
			);

		/// <summary> Add stream </summary>
		[PreserveSig]
		int AddStream(
			[In] short wStreamNum
			);

		/// <summary> Remove stream </summary>
		[PreserveSig]
		int RemoveStream(
			[In] short wStreamNum
			);
	}

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
  Guid("96406BCE-2B2B-11d3-B36B-00C04F6108FF")]
  public interface IWMMediaProps
  {
    int GetType(
        [Out] out Guid pguidType);

    int GetMediaType(
        [Out] IntPtr pType,
        [In, Out] ref uint pcbType);

    int SetMediaType(
        [In] WmMediaType pType);
  }


  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
  Guid("96406BD5-2B2B-11d3-B36B-00C04F6108FF")]
  public interface IWMInputMediaProps : IWMMediaProps
  {
    [PreserveSig]
    new int GetType(
        [Out] out Guid pguidType
      );

    [PreserveSig]
    new int GetMediaType(
        [Out] IntPtr pType,
        [In, Out] ref uint pcbType
      );

    [PreserveSig]
    new int SetMediaType(
        [In] WmMediaType pType
      );

    [PreserveSig]
    int GetConnectionName(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
        [In, Out] ref int pcchName
      );

    int GetGroupName(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
        [In, Out] ref int pcchName
      );
  };
  #endregion

  sealed public class WMData
	{
		// Declarations Windows Media Profiles
		// 20 version 4.0 formats (not used anymore???)
		private static readonly Guid WMProfile_V40_DialUpMBR = new Guid("fd7f47f1-72a6-45a4-80f0-3aecefc32c07");
		private static readonly Guid WMProfile_V40_IntranetMBR        = new Guid("82cd3321-a94a-4ffc-9c2b-092c10ca16e7");
		private static readonly Guid WMProfile_V40_2856100MBR         = new Guid("5a1c2206-dc5e-4186-beb2-4c5a994b132e");
		private static readonly Guid WMProfile_V40_6VoiceAudio        = new Guid("D508978A-11A0-4d15-B0DA-ACDC99D4F890");
		private static readonly Guid WMProfile_V40_16AMRadio          = new Guid("0f4be81f-d57d-41e1-b2e3-2fad986bfec2");
		private static readonly Guid WMProfile_V40_288FMRadioMono     = new Guid("7fa57fc8-6ea4-4645-8abf-b6e5a8f814a1");
		private static readonly Guid WMProfile_V40_288FMRadioStereo   = new Guid("22fcf466-aa40-431f-a289-06d0ea1a1e40");
		private static readonly Guid WMProfile_V40_56DialUpStereo     = new Guid("e8026f87-e905-4594-a3c7-00d00041d1d9");
		private static readonly Guid WMProfile_V40_64Audio            = new Guid("4820b3f7-cbec-41dc-9391-78598714c8e5");
		private static readonly Guid WMProfile_V40_96Audio            = new Guid("0efa0ee3-9e64-41e2-837f-3c0038f327ba");
		private static readonly Guid WMProfile_V40_128Audio           = new Guid("93ddbe12-13dc-4e32-a35e-40378e34279a");
		private static readonly Guid WMProfile_V40_288VideoVoice      = new Guid("bb2bc274-0eb6-4da9-b550-ecf7f2b9948f");
		private static readonly Guid WMProfile_V40_288VideoAudio      = new Guid("ac617f2d-6cbe-4e84-8e9a-ce151a12a354");
		private static readonly Guid WMProfile_V40_288VideoWebServer  = new Guid("abf2f00d-d555-4815-94ce-8275f3a70bfe");
		private static readonly Guid WMProfile_V40_56DialUpVideo      = new Guid("e21713bb-652f-4dab-99de-71e04400270f");
		private static readonly Guid WMProfile_V40_56DialUpVideoWebServer = new Guid("b756ff10-520f-4749-a399-b780e2fc9250");
		private static readonly Guid WMProfile_V40_100Video           = new Guid("8f99ddd8-6684-456b-a0a3-33e1316895f0");
		private static readonly Guid WMProfile_V40_250Video           = new Guid("541841c3-9339-4f7b-9a22-b11540894e42");
		private static readonly Guid WMProfile_V40_512Video           = new Guid("70440e6d-c4ef-4f84-8cd0-d5c28686e784");
		private static readonly Guid WMProfile_V40_1MBVideo           = new Guid("b4482a4c-cc17-4b07-a94e-9818d5e0f13f");
		private static readonly Guid WMProfile_V40_3MBVideo           = new Guid("55374ac0-309b-4396-b88f-e6e292113f28");

		// 26 version 7.0 formats
		private static readonly Guid WMProfile_V70_DialUpMBR          = new Guid("5B16E74B-4068-45b5-B80E-7BF8C80D2C2F");
		private static readonly Guid WMProfile_V70_IntranetMBR        = new Guid("045880DC-34B6-4ca9-A326-73557ED143F3");
		private static readonly Guid WMProfile_V70_2856100MBR         = new Guid("07DF7A25-3FE2-4a5b-8B1E-348B0721CA70");
		private static readonly Guid WMProfile_V70_288VideoVoice      = new Guid("B952F38E-7DBC-4533-A9CA-B00B1C6E9800");
		private static readonly Guid WMProfile_V70_288VideoAudio      = new Guid("58BBA0EE-896A-4948-9953-85B736F83947");
		private static readonly Guid WMProfile_V70_288VideoWebServer  = new Guid("70A32E2B-E2DF-4ebd-9105-D9CA194A2D50");
		private static readonly Guid WMProfile_V70_56VideoWebServer   = new Guid("DEF99E40-57BC-4ab3-B2D1-B6E3CAF64257");
		private static readonly Guid WMProfile_V70_64VideoISDN        = new Guid("C2B7A7E9-7B8E-4992-A1A1-068217A3B311");
		private static readonly Guid WMProfile_V70_100Video           = new Guid("D9F3C932-5EA9-4c6d-89B4-2686E515426E");
		private static readonly Guid WMProfile_V70_256Video           = new Guid("AFE69B3A-403F-4a1b-8007-0E21CFB3DF84");
		private static readonly Guid WMProfile_V70_384Video           = new Guid("F3D45FBB-8782-44df-97C6-8678E2F9B13D");
		private static readonly Guid WMProfile_V70_768Video           = new Guid("0326EBB6-F76E-4964-B0DB-E729978D35EE");
		private static readonly Guid WMProfile_V70_1500Video          = new Guid("0B89164A-5490-4686-9E37-5A80884E5146");
		private static readonly Guid WMProfile_V70_2000Video          = new Guid("AA980124-BF10-4e4f-9AFD-4329A7395CFF");
		private static readonly Guid WMProfile_V70_700FilmContentVideo= new Guid("7A747920-2449-4d76-99CB-FDB0C90484D4");
		private static readonly Guid WMProfile_V70_1500FilmContentVideo = new Guid("F6A5F6DF-EE3F-434c-A433-523CE55F516B");
		private static readonly Guid WMProfile_V70_6VoiceAudio        = new Guid("EABA9FBF-B64F-49b3-AA0C-73FBDD150AD0");
		private static readonly Guid WMProfile_V70_288FMRadioMono     = new Guid("C012A833-A03B-44a5-96DC-ED95CC65582D");
		private static readonly Guid WMProfile_V70_288FMRadioStereo   = new Guid("E96D67C9-1A39-4dc4-B900-B1184DC83620");
		private static readonly Guid WMProfile_V70_56DialUpStereo     = new Guid("674EE767-0949-4fac-875E-F4C9C292013B");
		private static readonly Guid WMProfile_V70_64AudioISDN        = new Guid("91DEA458-9D60-4212-9C59-D40919C939E4");
		private static readonly Guid WMProfile_V70_64Audio            = new Guid("B29CFFC6-F131-41db-B5E8-99D8B0B945F4");
		private static readonly Guid WMProfile_V70_96Audio            = new Guid("A9D4B819-16CC-4a59-9F37-693DBB0302D6");
		private static readonly Guid WMProfile_V70_128Audio           = new Guid("C64CF5DA-DF45-40d3-8027-DE698D68DC66");
		private static readonly Guid WMProfile_V70_225VideoPDA        = new Guid("F55EA573-4C02-42b5-9026-A8260C438A9F");
		private static readonly Guid WMProfile_V70_150VideoPDA        = new Guid("0F472967-E3C6-4797-9694-F0304C5E2F17");

		// 27 version 8.0 formats
		private static readonly Guid WMProfile_V80_255VideoPDA        = new Guid("FEEDBCDF-3FAC-4c93-AC0D-47941EC72C0B");
		private static readonly Guid WMProfile_V80_150VideoPDA        = new Guid("AEE16DFA-2C14-4a2f-AD3F-A3034031784F");
		private static readonly Guid WMProfile_V80_28856VideoMBR      = new Guid("D66920C4-C21F-4ec8-A0B4-95CF2BD57FC4");
		private static readonly Guid WMProfile_V80_100768VideoMBR     = new Guid("5BDB5A0E-979E-47d3-9596-73B386392A55");
		private static readonly Guid WMProfile_V80_288100VideoMBR     = new Guid("D8722C69-2419-4b36-B4E0-6E17B60564E5");
		private static readonly Guid WMProfile_V80_288Video           = new Guid("3DF678D9-1352-4186-BBF8-74F0C19B6AE2");
		private static readonly Guid WMProfile_V80_56Video            = new Guid("254E8A96-2612-405c-8039-F0BF725CED7D");
		private static readonly Guid WMProfile_V80_100Video           = new Guid("A2E300B4-C2D4-4fc0-B5DD-ECBD948DC0DF");
		private static readonly Guid WMProfile_V80_256Video           = new Guid("BBC75500-33D2-4466-B86B-122B201CC9AE");
		private static readonly Guid WMProfile_V80_384Video           = new Guid("29B00C2B-09A9-48bd-AD09-CDAE117D1DA7");
		private static readonly Guid WMProfile_V80_768Video           = new Guid("74D01102-E71A-4820-8F0D-13D2EC1E4872");
		private static readonly Guid WMProfile_V80_700NTSCVideo       = new Guid("C8C2985F-E5D9-4538-9E23-9B21BF78F745");
		private static readonly Guid WMProfile_V80_1400NTSCVideo      = new Guid("931D1BEE-617A-4bcd-9905-CCD0786683EE");
		private static readonly Guid WMProfile_V80_384PALVideo        = new Guid("9227C692-AE62-4f72-A7EA-736062D0E21E");
		/// <summary> Windows Media Video 8 for Broadband (PAL, 700 Kbps) </summary>
		public static readonly Guid WMProfile_V80_700PALVideo        = new Guid("EC298949-639B-45e2-96FD-4AB32D5919C2");
		// Windows Media Audio 8 for Dial-up Modem (Mono, 28.8 Kbps)
		public static readonly Guid WMProfile_V80_288MonoAudio       = new Guid("7EA3126D-E1BA-4716-89AF-F65CEE0C0C67");
		// Windows Media Audio 8 for Dial-up Modem (FM Radio Stereo, 28.8 Kbps)
		public static readonly Guid WMProfile_V80_288StereoAudio     = new Guid("7E4CAB5C-35DC-45bb-A7C0-19B28070D0CC");
		// Windows Media Audio 8 for Dial-up Modem (32 Kbps)
		public static readonly Guid WMProfile_V80_32StereoAudio      = new Guid("60907F9F-B352-47e5-B210-0EF1F47E9F9D");
		// Windows Media Audio 8 for Dial-up Modem (Near CD quality, 48 Kbps)
		public static readonly Guid WMProfile_V80_48StereoAudio      = new Guid("5EE06BE5-492B-480a-8A8F-12F373ECF9D4");
		/// <summary> Windows Media Audio 8 for Dial-up Modem (CD quality, 64 Kbps) </summary>
		public static readonly Guid WMProfile_V80_64StereoAudio      = new Guid("09BB5BC4-3176-457f-8DD6-3CD919123E2D");
		// Windows Media Audio 8 for Dial-up Modem (CD quality, 64 Kbps)
		public static readonly Guid WMProfile_V80_96StereoAudio      = new Guid("1FC81930-61F2-436f-9D33-349F2A1C0F10");
		// Windows Media Audio 8 for ISDN (Better than CD quality, 96 Kbps)
		public static readonly Guid WMProfile_V80_128StereoAudio     = new Guid("407B9450-8BDC-4ee5-88B8-6F527BD941F2");
		private static readonly Guid WMProfile_V80_288VideoOnly       = new Guid("8C45B4C7-4AEB-4f78-A5EC-88420B9DADEF");
		/// <summary> Windows Media Video 8 for Dial-up Modem (No audio, 56 Kbps) </summary>
		public static readonly Guid WMProfile_V80_56VideoOnly = new Guid(0x6E2A6955, 0x81DF, 0x4943, 0xBA, 0x50, 0x68, 0xA9, 0x86, 0xA7, 0x08, 0xF6);
		private static readonly Guid WMProfile_V80_FAIRVBRVideo       = new Guid("3510A862-5850-4886-835F-D78EC6A64042");
		private static readonly Guid WMProfile_V80_HIGHVBRVideo       = new Guid("0F10D9D3-3B04-4fb0-A3D3-88D4AC854ACC");
		/// <summary> Windows Media Video 8 for Broadband (PAL, 700 Kbps) </summary>
		public static readonly Guid WMProfile_V80_BESTVBRVideo       = new Guid("048439BA-309C-440e-9CB4-3DCCA3756423");
		// End of declarations Windows Media profiles
	}
}
