/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DShowNET
{


	//IMPDST
	[ComVisible(true), ComImport,
	Guid("FB1EF498-2C7D-4fed-B2AA-B8F9E199F074"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IStreamAnalyzer
	{
		[PreserveSig]
		int put_MediaType(IntPtr pmt);

		[PreserveSig]
		int get_MediaType(IntPtr pmt);

		[PreserveSig]
		int get_IPin (IntPtr pPin);

		[PreserveSig]
		int get_State (IntPtr state);

		[PreserveSig]
		int GetChannelCount(ref UInt16 count);
		
		[PreserveSig]
		int GetChannel(UInt16 chNumber,IntPtr ptr);

		[PreserveSig]
		int GetCISize(ref UInt16 len);

		[PreserveSig]
		int ResetParser();
		
		[PreserveSig]
		int ResetPids();
		
		[PreserveSig]
		int SetPMTProgramNumber(int prg);

		[PreserveSig]
		int GetPMTData(IntPtr pmt);
		
		[PreserveSig]
		int IsChannelReady(int chNum);

		[PreserveSig]
		int UseATSC(byte yesNo);
		
		[PreserveSig]
		int IsATSCUsed(out bool yesNo);

		[PreserveSig]
		int GrabEPG();

		[PreserveSig]
		int IsEPGReady(out bool yesNo);

		[PreserveSig]
		int GetEPGChannelCount([Out] out uint channelCount);

		[PreserveSig]
		int GetEPGEventCount([In] uint channel, [Out] out uint eventCount);

		[PreserveSig]
		int GetEPGChannel( [In] uint channel,  [In,Out] ref UInt16 networkId,  [In,Out] ref UInt16 transportid, [In,Out] ref UInt16 service_id  );

		[PreserveSig]
		int GetEPGEvent( [In] uint channel,  [In] uint eventid, [Out] out uint date, [Out] out uint time, [Out] out uint duration,  out   IntPtr eventstr,  out   IntPtr text, out IntPtr genre    );
	}


  [ComVisible(true), ComImport,
  Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface ICaptureGraphBuilder2
  {
    [PreserveSig]
    int SetFiltergraph( [In] IGraphBuilder pfg );

    [PreserveSig]
    int GetFiltergraph( [Out] out IGraphBuilder ppfg );

    [PreserveSig]
    int SetOutputFileName(
      [In]											ref Guid		pType,
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpstrFile,
      [Out]										out IBaseFilter		ppbf,
      [Out]										out IFileSinkFilter	ppSink );

    [PreserveSig]
    int FindInterface(
      [MarshalAs(UnmanagedType.LPArray)] Guid[] pCategory,
      [MarshalAs(UnmanagedType.LPArray)] Guid[] pType,
      [In]											IBaseFilter		pbf,
      [In]											ref Guid		riid,
      [Out, MarshalAs(UnmanagedType.IUnknown) ]		out	object		ppint );

    [PreserveSig]
    int RenderStream( 
      [MarshalAs(UnmanagedType.LPArray)] Guid[] pCategory,
      [MarshalAs(UnmanagedType.LPArray)] Guid[] pType,
      [In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
      [In] IBaseFilter pfCompressor,
      [In] IBaseFilter pfRenderer );

    [PreserveSig]
    int ControlStream(
      [In]										ref Guid			pCategory,
      [In]										ref Guid			pType,
      [In]											IBaseFilter		pFilter,
      [In]											IntPtr			pstart,
      [In]											IntPtr			pstop,
      [In]											short			wStartCookie,
      [In]											short			wStopCookie );

    [PreserveSig]
    int AllocCapFile(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpstrFile,
      [In]											long			dwlSize );

    [PreserveSig]
    int CopyCaptureFile(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpwstrOld,
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpwstrNew,
      [In]											int							fAllowEscAbort,
      [In]											IAMCopyCaptureFileProgress	pFilter );


    [PreserveSig]
    int FindPin(
      [In, MarshalAs(UnmanagedType.IUnknown)]		object			pSource,			// IUnknown
      [In]											int				pindir,					//PIN_DIRECTION
      [In]										ref Guid			pCategory,		//Category
      [In]										ref Guid			pType,				//Major type
      [In, MarshalAs(UnmanagedType.Bool) ]			bool			fUnconnected,
      [In]											int				num,
      [Out]										out IPin			ppPin );
  }





  [ComVisible(true), ComImport,
  Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IGraphBuilder
  {
    #region "IFilterGraph Methods"
    [PreserveSig]
    int AddFilter(
      [In] IBaseFilter pFilter,
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			pName );

    [PreserveSig]
    int RemoveFilter( [In] IBaseFilter pFilter );

    [PreserveSig]
    int EnumFilters( [Out] out IEnumFilters ppEnum );

    [PreserveSig]
    int FindFilterByName(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			pName,
      [Out]										out IBaseFilter		ppFilter );

    [PreserveSig]
    int ConnectDirect( [In] IPin ppinOut, [In] IPin ppinIn,
      [In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaType	pmt );

    [PreserveSig]
    int Reconnect( [In] IPin ppin );

    [PreserveSig]
    int Disconnect( [In] IPin ppin );

    [PreserveSig]
    int SetDefaultSyncSource();
    #endregion

    [PreserveSig]
    int Connect( [In] IPin ppinOut, [In] IPin ppinIn );

    [PreserveSig]
    int Render( [In] IPin ppinOut );

    [PreserveSig]
    int RenderFile(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrFile,
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrPlayList );

    [PreserveSig]
    int AddSourceFilter(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrFileName,
      [In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwstrFilterName,
      [Out]										out IBaseFilter		ppFilter );

    [PreserveSig]
    int SetLogFile( IntPtr hFile );

    [PreserveSig]
    int Abort();

    [PreserveSig]
    int ShouldOperationContinue();
  }







  // ---------------------------------------------------------------------------------------


  [ComVisible(true), ComImport,
  Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IFileSinkFilter
  {
    [PreserveSig]
    int SetFileName(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string		pszFileName,
      [In] ref			AMMediaType	pmt );
	
    [PreserveSig]
    int GetCurFile(
      [Out, MarshalAs(UnmanagedType.LPWStr) ]		out	string		pszFileName,
      [Out, MarshalAs(UnmanagedType.LPStruct) ]		AMMediaType pmt );
  }

  [ComVisible(true), ComImport,
  Guid("00855B90-CE1B-11d0-BD4F-00A0C911CE86"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IFileSinkFilter2
  {
    [PreserveSig]
    int SetFileName(
      [In, MarshalAs(UnmanagedType.LPWStr)]			string		pszFileName,
      [In] IntPtr ptrMedia);
	
    [PreserveSig]
    int GetCurFile(
      [Out, MarshalAs(UnmanagedType.LPWStr) ]		out	string		pszFileName,
      [Out, MarshalAs(UnmanagedType.LPStruct) ]		AMMediaType pmt );

    [PreserveSig]
    int SetMode( [In] int dwFlags );

    [PreserveSig]
    int GetMode( [Out] out int dwFlags );

  }


  // Interface to control the AVI mux
  [ComVisible(true), ComImport,
  Guid("5ACD6AA0-F482-11ce-8B67-00AA00A3F1A6"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IConfigAviMux 
  {

    // control whether the AVI mux adjusts the frame rate or audio
    // sampling rate for drift when the file is closed. -1 to disables
    // this behavior.
    [PreserveSig]
    int SetMasterStream([In] int iStream);

    [PreserveSig]
    int GetMasterStream([Out] out int pStream);

    // control whether the AVI mux writes out an idx1 index chunk for
    // compatibility with older AVI players.
    [PreserveSig]
    int SetOutputCompatibilityIndex([In]  bool fOldIndex);
    
    [PreserveSig]
    int GetOutputCompatibilityIndex([Out] out bool pfOldIndex);
  }

  public enum AviInterleaveMode :int
  {
    // uninterleaved - samples written out in the order they
    // arrive.
    INTERLEAVE_NONE=0,
    // approximate interleaving with less overhead for video
    // capture
    INTERLEAVE_CAPTURE,
    // full, precise interleaving. slower.
    INTERLEAVE_FULL,
    // samples written out in the order they arrive. writes are
    // buffered
    INTERLEAVE_NONE_BUFFERED
  }

  // Interface to control interleaving of different streams in one file
  [ComVisible(true), ComImport,
  Guid("BEE3D220-157B-11d0-BD23-00A0C911CE86"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IConfigInterleaving 
  {
    [PreserveSig]
    int put_Mode([In] AviInterleaveMode mode);

    [PreserveSig]
    int get_Mode([Out] out AviInterleaveMode pMode);

    [PreserveSig]
    int put_Interleaving([In] ref long prtInterleave,[In] ref long prtPreroll);

    [PreserveSig]
    int get_Interleaving([Out] long prtInterleave,[Out] long prtPreroll);
  }

  [ComVisible(true), ComImport,
  Guid("45086030-F7E4-486a-B504-826BB5792A3B"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IConfigAsfWriter 
  {
    [PreserveSig]
    int ConfigureFilterUsingProfileId([In] int dwProfileId);

    [PreserveSig]
    int GetCurrentProfileId([Out] out int pdwProfileId);

    [PreserveSig]
    int ConfigureFilterUsingProfileGuid([In] Guid GuidProfile);

    [PreserveSig]
    int GetCurrentProfileGuid([Out] out Guid pProfileGuid);

    [PreserveSig]
    int ConfigureFilterUsingProfile([In] IntPtr pProfile);

    [PreserveSig]
    int GetCurrentProfile([Out] out IntPtr ppProfile);

    [PreserveSig]
    int SetIndexMode( [In]  bool bIndexFile );

    [PreserveSig]
    int GetIndexMode( [Out] out bool pbIndexFile );
  }

  // ---------------------------------------------------------------------------------------

  [ComVisible(true), ComImport,
  Guid("670d1d20-a068-11d0-b3f0-00aa003761c5"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMCopyCaptureFileProgress
  {
    [PreserveSig]
    int Progress( int iProgress );
  }








  // ---------------------------------------------------------------------------------------

  [ComVisible(true), ComImport,
  Guid("e46a9787-2b71-444d-a4b5-1fab7b708d6a"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IVideoFrameStep
  {
    [PreserveSig]
    int Step( int dwFrames,
      [In, MarshalAs(UnmanagedType.IUnknown)]			object			pStepObject );

    [PreserveSig]
    int CanStep( int bMultiple,
      [In, MarshalAs(UnmanagedType.IUnknown)]			object			pStepObject );

    [PreserveSig]
    int CancelStep();
  }








  // ---------------------------------------------------------------------------------------
  [ComVisible(true), ComImport,
  Guid("C6E13380-30AC-11d0-A18C-00A0C9118956"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMCrossbar
  {
    [PreserveSig]
    int get_PinCounts(out int outputPinCount, out int inputPinCount);

    [PreserveSig]
    int CanRoute(int outputPinIndex,int  inputPinIndex);

    [PreserveSig]
    int Route(int outputPinIndex,int  inputPinIndex);

    [PreserveSig]
    int get_IsRoutedTo(int outputPinIndex,out int inputPinIndex);

    [PreserveSig]
    int get_CrossbarPinInfo(bool isInputPin, int pinIndex, out int pinIndexRelated, out PhysicalConnectorType physicalType);
  }	

  [ComVisible(true), ComImport,
  Guid("C6E13340-30AC-11d0-A18C-00A0C9118956"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMStreamConfig
  {
    [PreserveSig]
    int SetFormat(
      [In, MarshalAs(UnmanagedType.LPStruct)]			AMMediaTypeClass	pmt );

    [PreserveSig]
    int GetFormat(
      [Out, MarshalAs(UnmanagedType.LPStruct)]	out	AMMediaTypeClass	pmt );

    [PreserveSig]
    int GetNumberOfCapabilities( out int piCount, out int piSize );

    [PreserveSig]
    int GetStreamCaps( int iIndex,
      [Out, MarshalAs(UnmanagedType.LPStruct)]	out AMMediaType	ppmt,
      IntPtr		pSCC );
  }














  // =============================================================================
  //											TUNER
  // =============================================================================

  [ComVisible(false)]
  public enum AMTunerSubChannel
  {
    NoTune		= -2,	// AMTUNER_SUBCHAN_NO_TUNE : don't tune
    Default		= -1	// AMTUNER_SUBCHAN_DEFAULT : use default sub chan
  }

  [ComVisible(false)]
  public enum AMTunerSignalStrength
  {
    NA				= -1,	// AMTUNER_HASNOSIGNALSTRENGTH : cannot indicate signal strength
    NoSignal		= 0,	// AMTUNER_NOSIGNAL : no signal available
    SignalPresent	= 1		// AMTUNER_SIGNALPRESENT : signal present
  }

  [Flags, ComVisible(false)]
  public enum AMTunerModeType
  {
    Default		= 0x0000,	// AMTUNER_MODE_DEFAULT : default tuner mode
    TV			= 0x0001,	// AMTUNER_MODE_TV : tv
    FMRadio		= 0x0002,	// AMTUNER_MODE_FM_RADIO : fm radio
    AMRadio		= 0x0004,	// AMTUNER_MODE_AM_RADIO : am radio
    Dss			= 0x0008	// AMTUNER_MODE_DSS : dss
  }

  [ComVisible(false)]
  public enum AMTunerEventType
  {
    Changed		= 0x0001,	// AMTUNER_EVENT_CHANGED : status changed
  }


  // ---------------------------------------------------------------------------------------

  [ComVisible(true), ComImport,
  Guid("211A8761-03AC-11d1-8D13-00AA00BD8339"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMTuner
  {
    [PreserveSig]
    int put_Channel( int lChannel, AMTunerSubChannel lVideoSubChannel, AMTunerSubChannel lAudioSubChannel );

    [PreserveSig]
    int get_Channel( out int plChannel, out int plVideoSubChannel, out int plAudioSubChannel );
    
    [PreserveSig]
    int ChannelMinMax( out int lChannelMin, out int lChannelMax );
        
    [PreserveSig]
    int put_CountryCode( int lCountryCode );

    [PreserveSig]
    int get_CountryCode( out int plCountryCode );

    [PreserveSig]
    int put_TuningSpace( int lTuningSpace );

    [PreserveSig]
    int get_TuningSpace( out int plTuningSpace );

    [PreserveSig]
    int Logon( IntPtr hCurrentUser );
    
    [PreserveSig]
    int Logout();

    [PreserveSig]
    int SignalPresent( out AMTunerSignalStrength plSignalStrength );
    
    [PreserveSig]
    int put_Mode( AMTunerModeType lMode );

    [PreserveSig]
    int get_Mode( out AMTunerModeType plMode );
    
    [PreserveSig]
    int GetAvailableModes( out AMTunerModeType plModes );
    
    [PreserveSig]
    int RegisterNotificationCallBack( IAMTunerNotification pNotify, AMTunerEventType lEvents );

    [PreserveSig]
    int UnRegisterNotificationCallBack( IAMTunerNotification pNotify );
  }


  // ---------------------------------------------------------------------------------------

  [ComVisible(true), ComImport,
  Guid("211A8760-03AC-11d1-8D13-00AA00BD8339"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMTunerNotification
  {
    [PreserveSig]
    int OnEvent( AMTunerEventType Event );
  }


  // ---------------------------------------------------------------------------------------
  [Flags, ComVisible(false)]
  public enum AnalogVideoStandard
  {
    None		= 0x00000000,  // This is a digital sensor
    NTSC_M		= 0x00000001,  //        75 IRE Setup
    NTSC_M_J	= 0x00000002,  // Japan,  0 IRE Setup
    NTSC_433	= 0x00000004,
    PAL_B		= 0x00000010,
    PAL_D		= 0x00000020,
    PAL_G		= 0x00000040,
    PAL_H		= 0x00000080,
    PAL_I		= 0x00000100,
    PAL_M		= 0x00000200,
    PAL_N		= 0x00000400,
    PAL_60		= 0x00000800,
    SECAM_B		= 0x00001000,
    SECAM_D		= 0x00002000,
    SECAM_G		= 0x00004000,
    SECAM_H		= 0x00008000,
    SECAM_K		= 0x00010000,
    SECAM_K1	= 0x00020000,
    SECAM_L		= 0x00040000,
    SECAM_L1	= 0x00080000,
    PAL_N_COMBO	= 0x00100000	// Argentina

  }

  [ComVisible(false)]
  public enum TunerInputType:int
  {
    Cable=0,
    Antenna=1
  }
  public enum VideoProcAmpFlags:int
  {
    Auto   = 0x0001,
    Manual = 0x0002
  }

  public enum VideoProcAmpProperty:int
  {
    Brightness,
    Contrast,
    Hue,
    Saturation,
    Sharpness,
    Gamma,
    ColorEnable,
    WhiteBalance,
    BacklightCompensation,
    Gain
  } 

  // ---------------------------------------------------------------------------------------
  [ComVisible(true), ComImport,
  Guid("C6E13360-30AC-11d0-A18C-00A0C9118956"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMVideoProcAmp 
  {
    // Returns min, max, step size, and default values
    int GetRange(
      [In] VideoProcAmpProperty Property,         // Which property to query
      [Out] out int pMin,          // Range minimum
      [Out] out int pMax,          // Range maxumum
      [Out] out int pSteppingDelta,// Step size
      [Out] out int pDefault,      // Default value
      [Out] out VideoProcAmpFlags pCapsFlags     // VideoProcAmpFlags

      );

    // Set a VideoProcAmp property
    int Set(
      [In]  VideoProcAmpProperty Property,        // VideoProcAmpProperty
      [In]  int lValue,          // Value to set
      [In]  VideoProcAmpFlags Flags            // VideoProcAmp_Flags_*

      );

    // Get a VideoProcAmp property
    int Get(
      [In]  VideoProcAmpProperty Property,        // VideoProcAmpProperty
      [Out] out int lValue,        // Current value
      [Out] out VideoProcAmpFlags Flags          // VideoProcAmp_Flags_*
      );
  }
  // ---------------------------------------------------------------------------------------

  [ComVisible(true), ComImport,
  Guid("211A8766-03AC-11d1-8D13-00AA00BD8339"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMTVTuner
  {

    #region "IAMTuner Methods"
    [PreserveSig]
    int put_Channel( int lChannel, AMTunerSubChannel lVideoSubChannel, AMTunerSubChannel lAudioSubChannel );

    [PreserveSig]
    int get_Channel( out int plChannel, out int plVideoSubChannel, out int plAudioSubChannel );
	    
    [PreserveSig]
    int ChannelMinMax( out int lChannelMin, out int lChannelMax );
	        
    [PreserveSig]
    int put_CountryCode( int lCountryCode );

    [PreserveSig]
    int get_CountryCode( out int plCountryCode );

    [PreserveSig]
    int put_TuningSpace( int lTuningSpace );

    [PreserveSig]
    int get_TuningSpace( out int plTuningSpace );

    [PreserveSig]
    int Logon( IntPtr hCurrentUser );
	    
    [PreserveSig]
    int Logout();

    [PreserveSig]
    int SignalPresent( out AMTunerSignalStrength plSignalStrength );
	    
    [PreserveSig]
    int put_Mode( AMTunerModeType lMode );

    [PreserveSig]
    int get_Mode( out AMTunerModeType plMode );
	    
    [PreserveSig]
    int GetAvailableModes( out AMTunerModeType plModes );
	    
    [PreserveSig]
    int RegisterNotificationCallBack( IAMTunerNotification pNotify, AMTunerEventType lEvents );

    [PreserveSig]
    int UnRegisterNotificationCallBack( IAMTunerNotification pNotify );
    #endregion

    [PreserveSig]
    int get_AvailableTVFormats( out AnalogVideoStandard lAnalogVideoStandard );

    [PreserveSig]
    int get_TVFormat( out AnalogVideoStandard lAnalogVideoStandard );
    
    [PreserveSig]
    int AutoTune( int lChannel, out int plFoundSignal );
    
    [PreserveSig]
    int StoreAutoTune();
    
    [PreserveSig]
    int get_NumInputConnections( out int plNumInputConnections );
    
    [PreserveSig]
    int put_InputType( int lIndex, TunerInputType inputType );
    
    [PreserveSig]
    int get_InputType( int lIndex, out TunerInputType inputType );

    [PreserveSig]
    int put_ConnectInput( int lIndex );
    
    [PreserveSig]
    int get_ConnectInput( out int lIndex );

    [PreserveSig]
    int get_VideoFrequency( out int lFreq );

    [PreserveSig]
    int get_AudioFrequency( out int lFreq );
  }

  // ---------------------------------------------------------------------------------------
  [ComVisible(true), ComImport,
  Guid("54C39221-8380-11d0-B3F0-00AA003761C5"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMAudioInputMixer
  {
    // This interface is only supported by the input pins, not the filter
    // If disabled, this channel will not be mixed in as part of the
    // recorded signal.
    int put_Enable (
      [In] bool fEnable);	// TRUE=enable FALSE=disable

    //Is this channel enabled?
    int get_Enable (
      [Out] out bool pfEnable);

    // When set to mono mode, making a stereo recording of this channel
    // will have both channels contain the same data... a mixture of the
    // left and right signals
    int put_Mono (
      [In] bool fMono);	// TRUE=mono FALSE=multi channel

    //all channels combined into a mono signal?
    int get_Mono (
      [Out] out bool pfMono);

    // !!! WILL CARDS BE ABLE TO BOOST THE GAIN?
    //Set the record level for this channel
    int put_MixLevel (
      [In] double Level);	// 0 = off, 1 = full (unity?) volume
    // AMF_AUTOMATICGAIN, if supported,
    // means automatic

    //Get the record level for this channel
    int get_MixLevel (
      [Out] out double pLevel);

    // For instance, when panned full left, and you make a stereo recording
    // of this channel, you will record a silent right channel.
    int put_Pan (
      [In] double Pan);	// -1 = full left, 0 = centre, 1 = right

    //Get the pan for this channel
    int get_Pan (
      [Out] out double pPan);

    // Boosts the bass of low volume signals before they are recorded
    // to compensate for the fact that your ear has trouble hearing quiet
    // bass sounds
    int put_Loudness (
      [In] bool fLoudness);// TRUE=on FALSE=off

    int get_Loudness (
      [Out] out bool pfLoudness);

    // boosts or cuts the treble of the signal before it's recorded by
    // a certain amount of dB
    int put_Treble (
      [In] double Treble); // gain in dB (-ve = attenuate)

    //Get the treble EQ for this channel
    int get_Treble (
      [Out] out double pTreble);

    // This is the maximum value allowed in put_Treble.  ie 6.0 means
    // any value between -6.0 and 6.0 is allowed
    int get_TrebleRange (
      [Out] out double pRange); // largest value allowed

    // boosts or cuts the bass of the signal before it's recorded by
    // a certain amount of dB
    int put_Bass (
      [In] double Bass); // gain in dB (-ve = attenuate)

    // Get the bass EQ for this channel
    int get_Bass (
      [Out] out double pBass);

    // This is the maximum value allowed in put_Bass.  ie 6.0 means
    // any value between -6.0 and 6.0 is allowed
    int get_BassRange (
      [Out] out double pRange); // largest value allowed
  }

  // ---------------------------------------------------------------------------------------
  public enum VfwCompressDialogs
  {
    Config = 0x01,
    About =  0x02,
    QueryConfig = 0x04,
    QueryAbout =  0x08
  }

  // ---------------------------------------------------------------------------------------
  [ComVisible(true), ComImport,
  Guid("D8D715A3-6E5E-11D0-B3F0-00AA003761C5"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMVfwCompressDialogs
  {
    [PreserveSig]
      // Bring up a dialog for this codec
    int ShowDialog(
      [In]  VfwCompressDialogs	iDialog,
      [In]  IntPtr				hwnd );

    // Calls ICGetState and gives you the result
    int GetState(
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pState,
      ref int pcbState );

    // Calls ICSetState
    int SetState(
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pState,
      [In] int cbState );

    // Send a codec specific message
    int SendDriverMessage(
      int uMsg,
      long dw1,
      long dw2 );
  }


  // ---------------------------------------------------------------------------------------
  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public class VideoStreamConfigCaps		// Video_STREAM_CONFIG_CAPS
  {
    public Guid			Guid;
    public AnalogVideoStandard	    VideoStandard;
    public Size			InputSize;
    public Size			MinCroppingSize;
    public Size			MaxCroppingSize;
    public int			CropGranularityX;
    public int			CropGranularityY;
    public int			CropAlignX;
    public int			CropAlignY;
    public Size			MinOutputSize;
    public Size			MaxOutputSize;
    public int			OutputGranularityX;
    public int			OutputGranularityY;
    public int			StretchTapsX;
    public int			StretchTapsY;
    public int			ShrinkTapsX;
    public int			ShrinkTapsY;
    public long			MinFrameInterval;
    public long			MaxFrameInterval;
    public int			MinBitsPerSecond;
    public int			MaxBitsPerSecond;
  }

  // ---------------------------------------------------------------------------------------
  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public class AudioStreamConfigCaps  // AUDIO_STREAM_CONFIG_CAPS
  {
    public Guid	Guid;
    public int	MinimumChannels;
    public int	MaximumChannels;
    public int	ChannelsGranularity;
    public int	MinimumBitsPerSample;
    public int	MaximumBitsPerSample;
    public int	BitsPerSampleGranularity;
    public int	MinimumSampleFrequency;
    public int	MaximumSampleFrequency;
    public int	SampleFrequencyGranularity;
  }
  public enum AMOVERLAYFX :uint
  {
    None=  0x00000000,
    MIRRORLEFTRIGHT =  0x00000002,
    MIRRORUPDOWN =     0x00000004,
    DEINTERLACE =      0x00000008
  };

  [ComVisible(true), ComImport,
  Guid("62fae250-7e65-4460-bfc9-6398b322073c"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMOverlayFX 
  {
    [PreserveSig]
    int QueryOverlayFXCaps([Out] out uint lpdwOverlayFXCaps);

    [PreserveSig]
    int SetOverlayFX([In] uint dwOverlayFX);

    [PreserveSig]
    int GetOverlayFX([Out] out uint lpdwOverlayFX);
  };	



  public enum VMRDeinterlacePrefs
  {
    DeinterlacePref_NextBest = 0x01,
    DeinterlacePref_BOB = 0x02,
    DeinterlacePref_Weave = 0x04,
    DeinterlacePref_Mask = 0x07
  } ;

  public enum VMRDeinterlaceTech
  {
    DeinterlaceTech_Unknown                = 0x0000,
    DeinterlaceTech_BOBLineReplicate       = 0x0001,
    DeinterlaceTech_BOBVerticalStretch     = 0x0002,
    DeinterlaceTech_MedianFiltering        = 0x0004,
    DeinterlaceTech_EdgeFiltering          = 0x0010,
    DeinterlaceTech_FieldAdaptive          = 0x0020,
    DeinterlaceTech_PixelAdaptive          = 0x0040,
    DeinterlaceTech_MotionVectorSteered      = 0x0080
  } ;


  [StructLayout(LayoutKind.Sequential), ComVisible(false)]  
  public struct VMRFrequency 
  {
    public uint dwNumerator;
    public uint dwDenominator;
  };

  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public struct VMRVideoDesc 
  {
    public uint               dwSize;
    public uint               dwSampleWidth;
    public uint               dwSampleHeight;
    public bool                SingleFieldPerSample;
    public uint               dwFourCC;
    public VMRFrequency        InputSampleFreq;
    public VMRFrequency        OutputFrameFreq;
  };

  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public struct VMRDeinterlaceCaps 
  {
    uint               dwSize;
    uint               dwNumPreviousOutputFrames;
    uint               dwNumForwardRefSamples;
    uint               dwNumBackwardRefSamples;
    VMRDeinterlaceTech  DeinterlaceTechnology;
  } ;

  [ComVisible(true), ComImport,
  Guid("bb057577-0db8-4e6a-87a7-1a8c9a505a0f"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IVMRDeinterlaceControl 
  {
    [PreserveSig]
    int GetNumberOfDeinterlaceModes(
      [In] VMRVideoDesc lpVideoDescription,
      [In, Out] ref uint lpdwNumDeinterlaceModes,
      [Out] out Guid lpDeinterlaceModes
      );

    [PreserveSig]
    int GetDeinterlaceModeCaps(
      [In] ref Guid lpDeinterlaceMode,
      [In] VMRVideoDesc lpVideoDescription,
      [In, Out] ref VMRDeinterlaceCaps lpDeinterlaceCaps
      );

    [PreserveSig]
    int GetDeinterlaceMode(
      [In] uint dwStreamID,
      [Out] out Guid lpDeinterlaceMode  // returns GUID_NULL if SetDeinterlaceMode
      );                              // has not been called yet.

    [PreserveSig]
    int SetDeinterlaceMode(
      [In] uint dwStreamID,          // use 0xFFFFFFFF to set mode for all streams
      [In] ref Guid lpDeinterlaceMode   // GUID_NULL == turn deinterlacing off
      );


    [PreserveSig]
    int GetDeinterlacePrefs(
      [Out] out uint lpdwDeinterlacePrefs
      );

    [PreserveSig]
    int SetDeinterlacePrefs(
      [In] uint dwDeinterlacePrefs
      );

    [PreserveSig]
    int GetActualDeinterlaceMode(
      [In] uint dwStreamID,
      [Out] out Guid lpDeinterlaceMode
      );

  }

  [StructLayout(LayoutKind.Sequential), ComVisible(true)]
  public struct VMR9Frequency 
  {
    public uint dwNumerator;
    public uint dwDenominator;
  } ;

  public enum VMR9_SampleFormat :int
  {
    VMR9_SampleReserved      = 1,
    VMR9_SampleProgressiveFrame = 2,
    VMR9_SampleFieldInterleavedEvenFirst = 3,
    VMR9_SampleFieldInterleavedOddFirst = 4,
    VMR9_SampleFieldSingleEven = 5,
    VMR9_SampleFieldSingleOdd = 6,
  } ;

  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public struct VMR9VideoDesc 
  {
    public uint                dwSize;
    public uint                dwSampleWidth;
    public uint                dwSampleHeight;
    public VMR9_SampleFormat   SampleFormat;
    public uint                dwFourCC;
    public VMR9Frequency       InputSampleFreq;
    public VMR9Frequency       OutputFrameFreq;
  } ;


  [StructLayout(LayoutKind.Sequential), ComVisible(true)]
  public struct VMR9DeinterlaceCaps 
  {
    public uint               dwSize;
    public uint               dwNumPreviousOutputFrames;
    public uint               dwNumForwardRefSamples;
    public uint               dwNumBackwardRefSamples;
    public VMR9DeinterlaceTech DeinterlaceTechnology;
  } ;






  [ComVisible(true), ComImport,
  Guid("a215fb8d-13c2-4f7f-993c-003d6271a459"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IVMRDeinterlaceControl9 
  {
    [PreserveSig]
    int GetNumberOfDeinterlaceModes(
      [In] ref VMR9VideoDesc desc,
      [In, Out] ref uint lpdwNumDeinterlaceModes,
      [In, Out ] IntPtr guids
      );

    [PreserveSig]
    int GetDeinterlaceModeCaps(
      [In] ref Guid lpDeinterlaceMode,
      [In] VMR9VideoDesc lpVideoDescription,
      [In, Out] ref VMR9DeinterlaceCaps lpDeinterlaceCaps
      );

    [PreserveSig]
    int GetDeinterlaceMode(
      [In] uint dwStreamID,
      [Out] out Guid lpDeinterlaceMode  // returns GUID_NULL if SetDeinterlaceMode
      );                              // has not been called yet.

    [PreserveSig]
    int SetDeinterlaceMode(
      [In] uint dwStreamID,          // use 0xFFFFFFFF to set mode for all streams
      [In] ref Guid lpDeinterlaceMode   // GUID_NULL == turn deinterlacing off
      );


    [PreserveSig]
    int GetDeinterlacePrefs(
      [Out] out uint lpdwDeinterlacePrefs
      );

    [PreserveSig]
    int SetDeinterlacePrefs(
      [In] uint dwDeinterlacePrefs
      );

    [PreserveSig]
    int GetActualDeinterlaceMode(
      [In] uint dwStreamID,
      [Out] out Guid lpDeinterlaceMode
      );

  }

  
  [ComVisible(true), ComImport,
  Guid("C6E13350-30AC-11d0-A18C-00A0C9118956"),
  InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
  public interface IAMAnalogVideoDecoder 
  {
    [PreserveSig]
    int get_AvailableTVFormats([Out] [MarshalAs(UnmanagedType.LPArray)] int[] lAnalogVideoStandards);

    [PreserveSig]
    int put_TVFormat([In] AnalogVideoStandard AnalogVideoStandard);

    [PreserveSig]
    int get_TVFormat([Out] out AnalogVideoStandard  plAnalogVideoStandard);

    [PreserveSig]
    int get_HorizontalLocked ([Out] int plLocked);

    [PreserveSig]
    int put_VCRHorizontalLocking ([In] int lVCRHorizontalLocking);

    [PreserveSig]
    int get_VCRHorizontalLocking ([Out] out int plVCRHorizontalLocking);

    [PreserveSig]
    int get_NumberOfLines ([Out] out int plNumberOfLines);

    [PreserveSig]
    int put_OutputEnable ([In] int lOutputEnable);

    [PreserveSig]
    int get_OutputEnable ([Out] out int plOutputEnable);
  }


} // namespace DShowNET
