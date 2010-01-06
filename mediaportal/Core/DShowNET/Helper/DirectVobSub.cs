#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DShowNET.Helper
{
  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public struct NORMALIZEDRECT
  {
    public float left;
    public float top;
    public float right;
    public float bottom;
  } ;


  /// <summary>
  /// Interop declaration for log font.
  /// </summary>
  /// 
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), ComVisible(false)]
  public class LOGFONT //  LOGFONTW
  {
    public int lfHeight;
    public int lfWidth;
    public int lfEscapement;
    public int lfOrientation;
    public int lfWeight;
    public byte lfItalic;
    public byte lfUnderline;
    public byte lfStrikeOut;
    public byte lfCharSet;
    public byte lfOutPrecision;
    public byte lfClipPrecision;
    public byte lfQuality;
    public byte lfPitchAndFamily;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)] public string lfFaceName;

    private const int LF_FACESIZE = 32;
    public const int SIZE_OF_LOGFONTW = 92;
  }


  /// <summary>
  /// Interop interface for IDirectVobSub
  /// </summary>
  /// 
  [ComVisible(true), ComImport,
   Guid("EBE1FB08-3957-47ca-AF13-5827E5442E56"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDirectVobSub
  {
    /// <summary>
    /// Get the file name of media file.
    /// </summary>
    /// <param name="fn">The file name buffer.</param>
    /// <returns></returns>
    /// <remarks>fn should point to a buffer allocated to at least the length of MAX_PATH (=260)</remarks>
    [PreserveSig]
    int get_FileName(
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder fn
      );

    /// <summary>
    /// Set the file name of media file.
    /// </summary>
    /// <param name="fn">File name</param>
    /// <returns></returns>
    [PreserveSig]
    int put_FileName(
      [In, MarshalAs(UnmanagedType.LPWStr)] string fn
      );

    /// <summary>
    /// Get number of languages
    /// </summary>
    /// <param name="nLangs"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_LanguageCount(
      out int nLangs
      );

    /// <summary>
    /// Get name of language.
    /// </summary>
    /// <param name="iLanguage"></param>
    /// <param name="ppName"></param>
    /// <returns></returns>
    /// <remarks>The returned *ppName is allocated with CoTaskMemAlloc.</remarks>
    [PreserveSig]
    int get_LanguageName(
      int iLanguage,
      out IntPtr ppName
      );

    /// <summary>
    /// Get currently selected language index.
    /// </summary>
    /// <param name="iSelected"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_SelectedLanguage(
      out int iSelected
      );

    /// <summary>
    /// Set currently selected language index.
    /// </summary>
    /// <param name="iSelected"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_SelectedLanguage(
      int iSelected
      );

    /// <summary>
    /// If subtitle is hided.
    /// </summary>
    /// <param name="fHideSubtitles"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_HideSubtitles(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fHideSubtitles
      );

    /// <summary>
    /// Set if subtitle is hided.
    /// </summary>
    /// <param name="fHideSubtitles"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_HideSubtitles(
      [In, MarshalAs(UnmanagedType.Bool)] bool fHideSubtitles
      );

    /// <summary>
    /// If subtitle is buffered in memory.
    /// </summary>
    /// <param name="fDoPreBuffering"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_PreBuffering(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fDoPreBuffering
      );

    /// <summary>
    /// If subtitle is buffered in memory.
    /// </summary>
    /// <param name="fDoPreBuffering"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_PreBuffering(
      [In, MarshalAs(UnmanagedType.Bool)] bool fDoPreBuffering
      );

    /// <summary>
    /// Get placement of subtitle.
    /// </summary>
    /// <param name="fOverridePlacement"></param>
    /// <param name="xperc"></param>
    /// <param name="yperc"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_Placement(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fOverridePlacement,
      out int xperc,
      out int yperc
      );

    /// <summary>
    /// Set placement of subtitle.
    /// </summary>
    /// <param name="fOverridePlacement"></param>
    /// <param name="xperc"></param>
    /// <param name="yperc"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_Placement(
      [In, MarshalAs(UnmanagedType.Bool)] bool fOverridePlacement,
      int xperc,
      int yperc
      );

    /// <summary>
    /// Get setting.
    /// </summary>
    /// <param name="fBuffer"></param>
    /// <param name="fOnlyShowForcedSubs"></param>
    /// <param name="fPolygonize"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_VobSubSettings(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fBuffer,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fOnlyShowForcedSubs,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fPolygonize
      );

    /// <summary>
    /// Set setting.
    /// </summary>
    /// <param name="fBuffer"></param>
    /// <param name="fOnlyShowForcedSubs"></param>
    /// <param name="fPolygonize"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_VobSubSettings(
      [In, MarshalAs(UnmanagedType.Bool)] bool fBuffer,
      [In, MarshalAs(UnmanagedType.Bool)] bool fOnlyShowForcedSubs,
      [In, MarshalAs(UnmanagedType.Bool)] bool fPolygonize
      );

    /// <summary>
    /// Get text setting. Very important for localized font.
    /// </summary>
    /// <param name="lf"></param>
    /// <param name="lflen"></param>
    /// <param name="color"></param>
    /// <param name="fShadow"></param>
    /// <param name="fOutline"></param>
    /// <param name="fAdvancedRenderer"></param>
    /// <returns></returns>
    /// <remarks>lflen parameter must be the length of either LOGFONTW or LOGFONTA</remarks>
    [PreserveSig]
    int get_TextSettings(
      [Out, MarshalAs(UnmanagedType.LPStruct)] LOGFONT lf,
      int lflen,
      out int color,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fShadow,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fOutline,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fAdvancedRenderer
      );

    /// <summary>
    /// Put text setting.
    /// </summary>
    /// <param name="lf"></param>
    /// <param name="lflen"></param>
    /// <param name="color"></param>
    /// <param name="fShadow"></param>
    /// <param name="fOutline"></param>
    /// <param name="fAdvancedRenderer"></param>
    /// <returns></returns>
    /// <remarks>lflen parameter must be the length of either LOGFONTW or LOGFONTA</remarks>
    [PreserveSig]
    int put_TextSettings(
      [In, MarshalAs(UnmanagedType.LPStruct)] LOGFONT lf,
      int lflen, // depending on lflen, lf must point LOGFONTW, not LOGFONTA
      int color,
      [In, MarshalAs(UnmanagedType.Bool)] bool fShadow,
      [In, MarshalAs(UnmanagedType.Bool)] bool fOutline,
      [In, MarshalAs(UnmanagedType.Bool)] bool fAdvancedRenderer
      );

    /// <summary>
    /// Get flip setting.
    /// </summary>
    /// <param name="fPicture"></param>
    /// <param name="fSubtitles"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_Flip(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fPicture,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fSubtitles
      );

    /// <summary>
    /// Set flip.
    /// </summary>
    /// <param name="fPicture"></param>
    /// <param name="fSubtitles"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_Flip(
      [In, MarshalAs(UnmanagedType.Bool)] bool fPicture,
      [In, MarshalAs(UnmanagedType.Bool)] bool fSubtitles
      );

    /// <summary>
    /// OSD setting.
    /// </summary>
    /// <param name="fOSD"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_OSD(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fOSD
      );

    /// <summary>
    /// OSD setting.
    /// </summary>
    /// <param name="fOSD"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_OSD(
      [In, MarshalAs(UnmanagedType.Bool)] bool fOSD
      );

    /// <summary>
    /// Full path.
    /// </summary>
    /// <param name="fSaveFullPath"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_SaveFullPath(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fSaveFullPath
      );

    /// <summary>
    /// Full path.
    /// </summary>
    /// <param name="fSaveFullPath"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_SaveFullPath(
      [In, MarshalAs(UnmanagedType.Bool)] bool fSaveFullPath
      );

    /// <summary>
    /// Timing information.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="speedmul"></param>
    /// <param name="speeddiv"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_SubtitleTiming(
      out int delay,
      out int speedmul,
      out int speeddiv
      );

    /// <summary>
    /// Timing information.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="speedmul"></param>
    /// <param name="speeddiv"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_SubtitleTiming(
      int delay,
      int speedmul,
      int speeddiv
      );

    /// <summary>
    /// Media FPS information.
    /// </summary>
    /// <param name="fEnabled"></param>
    /// <param name="fps"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_MediaFPS(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fEnabled,
      out double fps
      );

    /// <summary>
    /// Media FPS information.
    /// </summary>
    /// <param name="fEnabled"></param>
    /// <param name="fps"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_MediaFPS(
      [In, MarshalAs(UnmanagedType.Bool)] bool fEnabled,
      double fps
      );

    /// <summary>
    /// Color format
    /// </summary>
    /// <param name="iPosition"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_ColorFormat(
      out int iPosition
      );

    /// <summary>
    /// Color format
    /// </summary>
    /// <param name="iPosition"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_ColorFormat(
      int iPosition
      );

    /// <summary>
    /// Zoom rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_ZoomRect(
      [Out, MarshalAs(UnmanagedType.LPStruct)] NORMALIZEDRECT rect
      );

    /// <summary>
    /// Zoom rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_ZoomRect(
      [In, MarshalAs(UnmanagedType.LPStruct)] NORMALIZEDRECT rect
      );

    /// <summary>
    /// Should never be called in this application.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int UpdateRegistry();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iSelected"></param>
    /// <returns></returns>
    [PreserveSig]
    int HasConfigDialog(
      int iSelected
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iSelected"></param>
    /// <param name="hWndParent"></param>
    /// <returns></returns>
    [PreserveSig]
    int ShowConfigDialog(
      int iSelected,
      IntPtr hWndParent
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fLocked"></param>
    /// <returns></returns>
    [PreserveSig]
    int IsSubtitleReloaderLocked(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fLocked
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fLocked"></param>
    /// <returns></returns>
    [PreserveSig]
    int LockSubtitleReloader(
      [In, MarshalAs(UnmanagedType.Bool)] bool fLocked
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fDisabled"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_SubtitleReloader(
      [Out, MarshalAs(UnmanagedType.U1)] out bool fDisabled
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fDisabled"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_SubtitleReloader(
      [In, MarshalAs(UnmanagedType.Bool)] bool fDisabled
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="horizontal"></param>
    /// <param name="vertical"></param>
    /// <param name="resx2"></param>
    /// <param name="resx2minw"></param>
    /// <param name="resx2minh"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_ExtendPicture(
      out int horizontal, // 0 - disabled, 1 - mod32 extension (width = (width+31)&~31)
      out int vertical,
      // 0 - disabled, 1 - 16:9, 2 - 4:3, 0x80 - crop (use crop together with 16:9 or 4:3, eg 0x81 will crop to 16:9 if the picture was taller)
      out int resx2, // 0 - disabled, 1 - enabled, 2 - depends on the original resolution
      out int resx2minw,
      // resolution doubler will be used if width*height <= resx2minw*resx2minh (resx2minw*resx2minh equals to 384*288 by default)
      out int resx2minh
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="horizontal"></param>
    /// <param name="vertical"></param>
    /// <param name="resx2"></param>
    /// <param name="resx2minw"></param>
    /// <param name="resx2minh"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_ExtendPicture(
      int horizontal,
      int vertical,
      int resx2,
      int resx2minw,
      int resx2minh
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    /// <param name="fExternalLoad"></param>
    /// <param name="fWebLoad"></param>
    /// <param name="fEmbeddedLoad"></param>
    /// <returns></returns>
    [PreserveSig]
    int get_LoadSettings(
      out int level, // 0 - when needed, 1 - always, 2 - disabled
      [Out, MarshalAs(UnmanagedType.U1)] out bool fExternalLoad,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fWebLoad,
      [Out, MarshalAs(UnmanagedType.U1)] out bool fEmbeddedLoad
      );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    /// <param name="fExternalLoad"></param>
    /// <param name="fWebLoad"></param>
    /// <param name="fEmbeddedLoad"></param>
    /// <returns></returns>
    [PreserveSig]
    int put_LoadSettings(
      int level,
      [In, MarshalAs(UnmanagedType.Bool)] bool fExternalLoad,
      [In, MarshalAs(UnmanagedType.Bool)] bool fWebLoad,
      [In, MarshalAs(UnmanagedType.Bool)] bool fEmbeddedLoad
      );
  }
}