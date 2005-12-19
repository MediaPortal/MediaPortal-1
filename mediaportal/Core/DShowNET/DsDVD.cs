/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Runtime.InteropServices;

namespace DShowNET.Dvd
{


// =================================================================================================
//											DVD GRAPH
// =================================================================================================

[Flags]
public enum DvdGraphFlags		:int // AM_DVD_GRAPH_FLAGS
{
	Default			= 0x00000000,
	HwDecPrefer		= 0x00000001,		// AM_DVD_HWDEC_PREFER
	HwDecOnly		= 0x00000002,		// AM_DVD_HWDEC_ONLY
	SwDecPrefer		= 0x00000004,		// AM_DVD_SWDEC_PREFER
	SwDecOnly		= 0x00000008,		// AM_DVD_SWDEC_ONLY
	NoVpe			= 0x00000100,		// AM_DVD_NOVPE
  VMR9Only= 0x800
}

[Flags]
public enum DvdStreamFlags		// AM_DVD_STREAM_FLAGS
{
	None		= 0x00000000,
	Video		= 0x00000001,		// AM_DVD_STREAM_VIDEO
	Audio		= 0x00000002,		// AM_DVD_STREAM_AUDIO
	SubPic		= 0x00000004		// AM_DVD_STREAM_SUBPIC
}


	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdRenderStatus		//  AM_DVD_RENDERSTATUS
{
	public int		vpeStatus;
	public bool		volInvalid;
	public bool		volUnknown;
	public bool		noLine21In;
	public bool		noLine21Out;
	public int				numStreams;
	public int				numStreamsFailed;
	public DvdStreamFlags	failedStreams;
}


// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("FCC152B6-F372-11d0-8E00-00C04FD7C08B"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IDvdGraphBuilder
{
		[PreserveSig]
	int GetFiltergraph(
		[Out]										out IGraphBuilder		ppGB );

		[PreserveSig]
	int GetDvdInterface(
		[In]										ref	Guid	riid,
		[Out, MarshalAs(UnmanagedType.IUnknown) ]	out	object	ppvIF );

		[PreserveSig]
    int RenderDvdVideoVolume(
      string			lpcwszPathName,
      DvdGraphFlags	dwFlags,
      out DvdRenderStatus	pStatus );
    //int RenderDvdVideoVolume(
		//[In, MarshalAs(UnmanagedType.LPWStr)]			string			lpcwszPathName,
		//												DvdGraphFlags	dwFlags,
		//[Out]										out DvdRenderStatus	pStatus );
}

















// =================================================================================================
//											DVD CONTROL
// =================================================================================================

[Flags]
public enum DvdCmdFlags		// DVD_CMD_FLAGS
{
	None			= 0x00000000,		// DVD_CMD_FLAG_None
	Flush			= 0x00000001,		// DVD_CMD_FLAG_Flush
	SendEvt			= 0x00000002,		// DVD_CMD_FLAG_SendEvents
	Block			= 0x00000004,		// DVD_CMD_FLAG_Block
	StartWRendered	= 0x00000008,		// DVD_CMD_FLAG_StartWhenRendered
	EndARendered	= 0x00000010		// DVD_CMD_FLAG_EndAfterRendered
}


	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdTimeCode		//  DVD_HMSF_TIMECODE
{
	public byte		bHours;
	public byte		bMinutes;
	public byte		bSeconds;
	public byte		bFrames;
}

public enum DvdMenuID		// DVD_MENU_ID
{
	Title		= 2,		// DVD_MENU_Title
	Root		= 3,		// DVD_MENU_Root
	Subpicture	= 4,		// DVD_MENU_Subpicture
	Audio		= 5,		// DVD_MENU_Audio
	Angle		= 6,		// DVD_MENU_Angle
	Chapter		= 7			// DVD_MENU_Chapter
}


public enum DvdRelButton		// DVD_RELATIVE_BUTTON
{
	Upper		= 1,		// DVD_Relative_Upper
	Lower		= 2,		// DVD_Relative_Lower
	Left		= 3,		// DVD_Relative_Left
	Right		= 4			// DVD_Relative_Right
}


public enum DvdOptionFlag		// DVD_OPTION_FLAG
{
	ResetOnStop					= 1,		// DVD_ResetOnStop
	NotifyParentalLevelChange	= 2,		// DVD_NotifyParentalLevelChange
	HmsfTimeCodeEvt				= 3			// DVD_HMSF_TimeCodeEvents
}


public enum DvdAudioLangExt		// DVD_AUDIO_LANG_EXT
{
	NotSpecified			= 0,		// DVD_AUD_EXT_NotSpecified
	Captions				= 1,		// DVD_AUD_EXT_Captions
	VisuallyImpaired		= 2,		// DVD_AUD_EXT_VisuallyImpaired
	DirectorComments1		= 3,		// DVD_AUD_EXT_DirectorComments1
	DirectorComments2		= 4			// DVD_AUD_EXT_DirectorComments2
}

public enum DvdSubPicLangExt		// DVD_SUBPICTURE_LANG_EXT
{
	NotSpecified			= 0,		// DVD_SP_EXT_NotSpecified
	CaptionNormal			= 1,		// DVD_SP_EXT_Caption_Normal
	CaptionBig				= 2,		// DVD_SP_EXT_Caption_Big
	CaptionChildren			= 3,		// DVD_SP_EXT_Caption_Children
	ClosedNormal			= 5,		// DVD_SP_EXT_CC_Normal
	ClosedBig				= 6,		// DVD_SP_EXT_CC_Big
	ClosedChildren			= 7,		// DVD_SP_EXT_CC_Children
	Forced					= 9,		// DVD_SP_EXT_Forced
	DirectorCmtNormal		= 13,		// DVD_SP_EXT_DirectorComments_Normal
	DirectorCmtBig			= 14,		// DVD_SP_EXT_DirectorComments_Big
	DirectorCmtChildren		= 15,		// DVD_SP_EXT_DirectorComments_Children
}




// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("33BC7430-EEC0-11D2-8201-00A0C9D74842"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IDvdControl2
{
		[PreserveSig]
	int PlayTitle( int ulTitle, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayChapterInTitle( int ulTitle, int ulChapter, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayAtTimeInTitle( int ulTitle, [In] ref DvdTimeCode pStartTime, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int Stop();

		[PreserveSig]
	int ReturnFromSubmenu( DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayAtTime( [In] ref DvdTimeCode pTime, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayChapter( int ulChapter, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayPrevChapter( DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int ReplayChapter( DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayNextChapter( DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayForwards( double dSpeed, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayBackwards( double dSpeed, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int ShowMenu( DvdMenuID MenuID, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int Resume( DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SelectRelativeButton( DvdRelButton buttonDir );

		[PreserveSig]
	int ActivateButton();

		[PreserveSig]
	int SelectButton( int ulButton );

		[PreserveSig]
	int SelectAndActivateButton( int ulButton );

		[PreserveSig]
	int StillOff();

		[PreserveSig]
	int Pause(
		[In, MarshalAs(UnmanagedType.Bool)]				bool	bState );

		[PreserveSig]
	int SelectAudioStream( int ulAudio, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SelectSubpictureStream( int ulSubPicture, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SetSubpictureState(
		[In, MarshalAs(UnmanagedType.Bool)]				bool		bState,
														DvdCmdFlags	dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SelectAngle( int ulAngle, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SelectParentalLevel( int ulParentalLevel );

		[PreserveSig]
	int SelectParentalCountry( byte[] bCountry );

		[PreserveSig]
	int SelectKaraokeAudioPresentationMode( int ulMode );

		[PreserveSig]
	int SelectVideoModePreference( int ulPreferredDisplayMode );

		[PreserveSig]
	int SetDVDDirectory(
		[In]			string		pszwPath );

		[PreserveSig]
	int ActivateAtPosition( DsPOINT point);

		[PreserveSig]
	int SelectAtPosition( DsPOINT point );

		[PreserveSig]
	int PlayChaptersAutoStop( int ulTitle, int ulChapter, int ulChaptersToPlay, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int AcceptParentalLevelChange(
		[In, MarshalAs(UnmanagedType.Bool)]				bool		bAccept );

		[PreserveSig]
	int SetOption( DvdOptionFlag flag,
		[In, MarshalAs(UnmanagedType.Bool)]				bool		fState );

		[PreserveSig]
	int SetState( IDvdState pState, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int PlayPeriodInTitleAutoStop( int ulTitle,
		[In]										ref DvdTimeCode pStartTime,
		[In]										ref DvdTimeCode pEndTime,
														DvdCmdFlags	dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SetGPRM( int ulIndex, short wValue, DvdCmdFlags dwFlags,
		[Out]											OptIDvdCmd	ppCmd );

		[PreserveSig]
	int SelectDefaultMenuLanguage( int Language );

		[PreserveSig]
	int SelectDefaultAudioLanguage( int Language, DvdAudioLangExt audioExtension );

		[PreserveSig]
	int SelectDefaultSubpictureLanguage( int Language, DvdSubPicLangExt subpictureExtension );
}






// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("5a4a97e4-94ee-4a55-9751-74b5643aa27d"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IDvdCmd
{
		[PreserveSig]
	int WaitForStart();
		[PreserveSig]
	int WaitForEnd();
}




// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("86303d6d-1c4a-4087-ab42-f711167048ef"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IDvdState
{
		[PreserveSig]
	int GetDiscID( [Out] out long pullUniqueID );
		[PreserveSig]
	int GetParentalLevel( [Out] out int pulParentalLevel );
}











// =================================================================================================
//											DVD INFO
// =================================================================================================


public enum DvdDomain		// DVD_DOMAIN
{
	FirstPlay			= 1,		// DVD_DOMAIN_FirstPlay
	VideoManagerMenu	= 2,		// DVD_DOMAIN_VideoManagerMenu
	VideoTitleSetMenu	= 3,		// DVD_DOMAIN_VideoTitleSetMenu
	Title				= 4,		// DVD_DOMAIN_Title
	Stop				= 5			// DVD_DOMAIN_Stop
}


// ---------------------------------------------------------------------------------------

public enum DvdVideoCompress	// DVD_VIDEO_COMPRESSION
{
	Other		= 0,		// DVD_VideoCompression_Other
	Mpeg1		= 1,		// DVD_VideoCompression_MPEG1
	Mpeg2		= 2			// DVD_VideoCompression_MPEG2
}



// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdPlayLocation		// DVD_PLAYBACK_LOCATION2
{
	public int			TitleNum;
	public int			ChapterNum;
	public DvdTimeCode	timeCode;
	public int			TimeCodeFlags;
}

// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdMenuAttr		// DVD_MenuAttributes
{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst=8 )]
	public bool[]			compatibleRegion;
	public DvdVideoAttr		videoAt;			// DVD_VideoAttributes

	public bool				audioPresent;
	public DvdAudioAttr		audioAt;			// DVD_AudioAttributes

	public bool				subPicPresent;
	public DvdSubPicAttr	subPicAt;			// DVD_SubpictureAttributes
}


// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdVideoAttr		// DVD_VideoAttributes
{
	public bool		panscanPermitted;
	public bool		letterboxPermitted;
	public int		aspectX;
	public int		aspectY;
	public int		frameRate;
	public int		frameHeight;
	public DvdVideoCompress	compression;
	public bool		line21Field1InGOP;
	public bool		line21Field2InGOP;
	public int		sourceResolutionX;
	public int		sourceResolutionY;
	public bool		isSourceLetterboxed;
	public bool		isFilmMode;
}


// ---------------------------------------------------------------------------------------

public enum DvdAudioAppMode		// DVD_AUDIO_APPMODE
{
	None		= 0,		// DVD_AudioMode_None
	Karaoke		= 1,		// DVD_AudioMode_Karaoke
	Surround	= 2,		// DVD_AudioMode_Surround
	Other		= 3			// DVD_AudioMode_Other
}

// ---------------------------------------------------------------------------------------

public enum DvdAudioFormat		// DVD_AUDIO_FORMAT
{
	Ac3			= 0,		// DVD_AudioFormat_AC3
	Mpeg1		= 1,		// DVD_AudioFormat_MPEG1
	Mpeg1Drc	= 2,		// DVD_AudioFormat_MPEG1_DRC
	Mpeg2		= 3,		// DVD_AudioFormat_MPEG2
	Mpeg2Drc	= 4,		// DVD_AudioFormat_MPEG2_DRC
	Lpcm		= 5,		// DVD_AudioFormat_LPCM
	Dts			= 6,		// DVD_AudioFormat_DTS
	Sdds		= 7,		// DVD_AudioFormat_SDDS
	Other		= 8			// DVD_AudioFormat_Other
}


// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdAudioAttr		// DVD_AudioAttributes
{
	public DvdAudioAppMode	appMode;
	public int				appModeData;
	public DvdAudioFormat	audioFormat;
	public int				language;
	public DvdAudioLangExt	languageExtension;
	public bool				hasMultichannelInfo;
	public int				frequency;
	public byte				quantization;
	public byte				numberOfChannels;
	public short			dummy;
	public int				res1;
	public int				res2;
}


// ---------------------------------------------------------------------------------------

public enum DvdSubPicType		// DVD_SUBPICTURE_TYPE
{
	NotSpecified	= 0,		// DVD_SPType_NotSpecified
	Language		= 1,		// DVD_SPType_Language
	Other			= 2			// DVD_SPType_Other
}

public enum DvdSubPicCoding		// DVD_SUBPICTURE_CODING
{
	RunLength	= 0,		// DVD_SPCoding_RunLength
	Extended	= 1,		// DVD_SPCoding_Extended
	Other		= 2			// DVD_SPCoding_Other
}


// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdSubPicAttr		// DVD_SubpictureAttributes
{
	public DvdSubPicType	type;
	public DvdSubPicCoding	coding;
	public int				language;
	public DvdSubPicLangExt	languageExt;
}


// ---------------------------------------------------------------------------------------

public enum DvdTitleAppMode		// DVD_TITLE_APPMODE
{
	NotSpecified	= 0,		// DVD_AppMode_Not_Specified
	Karaoke			= 1,		// DVD_AppMode_Karaoke
	Other			= 3			// DVD_AppMode_Other
}

// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdTitleAttr		// DVD_TitleAttributes
{
	public DvdTitleAppMode	appMode;		// DVD_TITLE_APPMODE
	public DvdVideoAttr		videoAt;		// DVD_VideoAttributes
	public int				numberOfAudioStreams;
	// WARNING: incomplete
}


// ---------------------------------------------------------------------------------------

public enum DvdDiscSide		// DVD_DISC_SIDE
{
	A			= 1,		// DVD_SIDE_A
	B			= 2			// DVD_SIDE_B
}


// ---------------------------------------------------------------------------------------

public enum DvdCharSet		// DVD_TextCharSet
{
	Unicode			= 0,		// DVD_CharSet_Unicode
	Iso646			= 1,		// DVD_CharSet_ISO646
	Jis				= 2,		// DVD_CharSet_JIS_Roman_Kanji
	Iso8859			= 3,		// DVD_CharSet_ISO8859_1
	SiftJis			= 4			// DVD_CharSet_ShiftJIS_Kanji_Roman_Katakana
}




[Flags]
public enum DvdAudioCaps		// DVD_AUDIO_CAPS_xx
{
	Ac3			= 0x00000001,		// DVD_AUDIO_CAPS_AC3
	Mpeg2		= 0x00000002,		// DVD_AUDIO_CAPS_MPEG2
	Lpcm		= 0x00000004,		// DVD_AUDIO_CAPS_LPCM
	Dts			= 0x00000008,		// DVD_AUDIO_CAPS_DTS
	Sdds		= 0x00000010		// DVD_AUDIO_CAPS_SDDS
}

	[StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
public struct DvdDecoderCaps		// DVD_DECODER_CAPS
{
	public int			size;			// size of this struct
	public DvdAudioCaps	audioCaps;
	public double		fwdMaxRateVideo;
	public double		fwdMaxRateAudio;
	public double		fwdMaxRateSP;
	public double		bwdMaxRateVideo;
	public double		bwdMaxRateAudio;
	public double		bwdMaxRateSP;
	public int			res1;
	public int			res2;
	public int			res3;
	public int			res4;
}



// ---------------------------------------------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("34151510-EEC0-11D2-8201-00A0C9D74842"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IDvdInfo2
{
		[PreserveSig]
	int GetCurrentDomain( [Out] out DvdDomain pDomain );

		[PreserveSig]
	int GetCurrentLocation( [Out] out DvdPlayLocation pLocation );

		[PreserveSig]
	int GetTotalTitleTime( [Out] out DvdTimeCode pTotalTime, out int ulTimeCodeFlags );

		[PreserveSig]
	int GetCurrentButton( out int pulButtonsAvailable, out int pulCurrentButton );

		[PreserveSig]
	int GetCurrentAngle( out int pulAnglesAvailable, out int pulCurrentAngle );

		[PreserveSig]
	int GetCurrentAudio( out int pulStreamsAvailable, out int pulCurrentStream );

		[PreserveSig]
	int GetCurrentSubpicture( out int pulStreamsAvailable, out int pulCurrentStream,
		[Out, MarshalAs(UnmanagedType.Bool)]			out	bool	pbIsDisabled );

		[PreserveSig]
	int GetCurrentUOPS( out int pulUOPs );

		[PreserveSig]
	int GetAllSPRMs( out IntPtr pRegisterArray );

		[PreserveSig]
	int GetAllGPRMs( out IntPtr pRegisterArray );

		[PreserveSig]
	int GetAudioLanguage( int ulStream, out int pLanguage );

		[PreserveSig]
	int GetSubpictureLanguage( int ulStream, out int pLanguage );

		[PreserveSig]
	int GetTitleAttributes( int ulTitle,
		[Out] out DvdMenuAttr pMenu, IntPtr pTitle );		// incomplete

		[PreserveSig]
	int GetVMGAttributes( [Out] out DvdMenuAttr pATR );

		[PreserveSig]
	int GetCurrentVideoAttributes( [Out] out DvdVideoAttr pATR );

		[PreserveSig]
	int GetAudioAttributes( int ulStream, [Out] out DvdAudioAttr pATR );

		[PreserveSig]
	int GetKaraokeAttributes( int ulStream, IntPtr pATR );

		[PreserveSig]
	int GetSubpictureAttributes( int ulStream, [Out] out DvdSubPicAttr pATR );

		[PreserveSig]
	int GetDVDVolumeInfo( out int pulNumOfVolumes, out int pulVolume,
		out DvdDiscSide pSide, out int pulNumOfTitles );

		[PreserveSig]
	int GetDVDTextNumberOfLanguages( out int pulNumOfLangs );

		[PreserveSig]
	int GetDVDTextLanguageInfo( int ulLangIndex,
		out int pulNumOfStrings, out int pLangCode, out DvdCharSet pbCharacterSet );
    
		[PreserveSig]
	int GetDVDTextStringAsNative( int ulLangIndex, int ulStringIndex,
		IntPtr pbBuffer, int ulMaxBufferSize, out int pulActualSize, out int pType );

		[PreserveSig]
	int GetDVDTextStringAsUnicode( int ulLangIndex, int ulStringIndex,
		IntPtr pchwBuffer, int ulMaxBufferSize, out int pulActualSize, out int pType );

		[PreserveSig]
	int GetPlayerParentalLevel( out int pulParentalLevel, [Out] byte[] pbCountryCode );

		[PreserveSig]
	int GetNumberOfChapters( int ulTitle, out int pulNumOfChapters );

		[PreserveSig]
	int GetTitleParentalLevels( int ulTitle, out int pulParentalLevels );

		[PreserveSig]
	int GetDVDDirectory( IntPtr pszwPath,[In] int ulMaxSize, [Out] out int pulActualSize );

		[PreserveSig]
	int IsAudioStreamEnabled( int ulStreamNum,
		[Out, MarshalAs(UnmanagedType.Bool)]		out	bool	pbEnabled );

		[PreserveSig]
	int GetDiscID(
		[In, MarshalAs(UnmanagedType.LPWStr)]			string		pszwPath,
													out long		pullDiscID );
		[PreserveSig]
	int GetState(
		[Out]										out IDvdState	pStateData );

		[PreserveSig]
	int GetMenuLanguages( [Out] int[] pLanguages, int ulMaxLanguages, out int pulActualLanguages );

		[PreserveSig]
	int GetButtonAtPosition( DsPOINT point, out int pulButtonIndex );

		[PreserveSig]
	int GetCmdFromEvent( int lParam1,
		[Out]										out IDvdCmd		pCmdObj );

		[PreserveSig]
	int GetDefaultMenuLanguage( out int pLanguage );

		[PreserveSig]
	int GetDefaultAudioLanguage( out int pLanguage, out DvdAudioLangExt pAudioExtension );

		[PreserveSig]
	int GetDefaultSubpictureLanguage( out int pLanguage, out DvdSubPicLangExt pSubpictureExtension );

		[PreserveSig]
	int GetDecoderCaps( ref DvdDecoderCaps pCaps );

		[PreserveSig]
	int GetButtonRect( int ulButton, out DsRECT pRect );

		[PreserveSig]
	int IsSubpictureStreamEnabled( int ulStreamNum,
		[Out, MarshalAs(UnmanagedType.Bool)]		out	bool	pbEnabled );

}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public class OptIDvdCmd
{
	public IDvdCmd		dvdCmd;
}


} // namespace DShowNET.Dvd
