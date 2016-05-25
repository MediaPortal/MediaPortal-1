#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.DiskManagement
{
  public class WmcMetaInfo
  {
    #region constants

    /// <summary>
    /// Use for music files. Do not use for audio that is not music.
    /// </summary>
    private static readonly Guid CLASS_PRIMARY_MUSIC = new Guid(0xd1607dbc, 0xe323, 0x4be2, 0x86, 0xa1, 0x48, 0xa4, 0x2a, 0x28, 0x44, 0x1e);
    /// <summary>
    /// Use for video files.
    /// </summary>
    private static readonly Guid CLASS_PRIMARY_VIDEO = new Guid(0xdb9830bd, 0x3ab3, 0x4fab, 0x8a, 0x37, 0x1a, 0x99, 0x5f, 0x7f, 0xf7, 0x4b);
    /// <summary>
    /// Use for audio files that are not music.
    /// </summary>
    private static readonly Guid CLASS_PRIMARY_AUDIO = new Guid(0x01cd0f29, 0xda4e, 0x4157, 0x89, 0x7b, 0x62, 0x75, 0xd5, 0x0c, 0x4f, 0x11);
    /// <summary>
    /// Use for files that are neither audio or video.
    /// </summary>
    private static readonly Guid CLASS_PRIMARY_OTHER = new Guid(0xfcf24a76, 0x9a57, 0x4036, 0x99, 0x0d, 0xe3, 0x5d, 0xd8, 0xb2, 0x44, 0xe1);

    /// <summary>
    /// Use for audio book files.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_AUDIO_BOOK = new Guid(0xe0236beb, 0xc281, 0x4ede, 0xa3, 0x6d, 0x7a, 0xf7, 0x6a, 0x3d, 0x45, 0xb5);
    /// <summary>
    /// Use for audio files that contain spoken word, but are not audio books. For example, stand, up comedy routines.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_AUDIO_SPOKEN_WORD = new Guid(0x3a172a13, 0x2bd9, 0x4831, 0x83, 0x5b, 0x11, 0x4f, 0x6a, 0x95, 0x94, 0x3f);
    /// <summary>
    /// Use for audio files related to news.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_AUDIO_NEWS = new Guid(0x6677db9b, 0xe5a0, 0x4063, 0xa1, 0xad, 0xac, 0xeb, 0x52, 0x84, 0x0c, 0xf1);
    /// <summary>
    /// Use for audio files with talk show content.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_AUDIO_TALK_SHOW = new Guid(0x1b824a67, 0x3f80, 0x4e3e, 0x9c, 0xde, 0xf7, 0x36, 0x1b, 0x0f, 0x5f, 0x1b);
    /// <summary>
    /// Use for video files related to news.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_NEWS = new Guid(0x1fe2e091, 0x4e1e, 0x40ce, 0xb2, 0x2d, 0x34, 0x8c, 0x73, 0x2e, 0x0b, 0x10);
    /// <summary>
    /// Use for video files containing Web-based shows, short films, movie trailers, and so on. This is the general identifier for video entertainment that does not fit into another category.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_WEB = new Guid(0xd6de1d88, 0xc77c, 0x4593, 0xbf, 0xbc, 0x9c, 0x61, 0xe8, 0xc3, 0x73, 0xe3);
    /// <summary>
    /// Use for audio files containing sound clips from games.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_AUDIO_GAME_CLIP = new Guid(0x00033368, 0x5009, 0x4ac3, 0xa8, 0x20, 0x5d, 0x2d, 0x09, 0xa4, 0xe7, 0xc1);
    /// <summary>
    /// Use for audio files containing complete songs from game sound tracks. If only part of a song is encoded in the file, use the identifier for game sound clips instead.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_AUDIO_GAME_FULL = new Guid(0xf24ff731, 0x96fc, 0x4d0f, 0xa2, 0xf5, 0x5a, 0x34, 0x83, 0x68, 0x2b, 0x1a);
    /// <summary>
    /// Use for video files containing music videos.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_MUSIC = new Guid(0xe3e689e2, 0xba8c, 0x4330, 0x96, 0xdf, 0xa0, 0xee, 0xef, 0xfa, 0x68, 0x76);
    /// <summary>
    /// Use for video files containing general home video.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_HOME = new Guid(0xb76628f4, 0x300d, 0x443d, 0x9c, 0xb5, 0x01, 0xc2, 0x85, 0x10, 0x9d, 0xaf);
    /// <summary>
    /// Use for video files containing feature films.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_MOVIE = new Guid(0xa9b87fc9, 0xbd47, 0x4bf0, 0xac, 0x4f, 0x65, 0x5b, 0x89, 0xf7, 0xd8, 0x68);
    /// <summary>
    /// Use for video files containing television shows. For Web-based shows, use the more generic identifier.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_TV = new Guid(0xba7f258a, 0x62f7, 0x47a9, 0xb2, 0x1f, 0x46, 0x51, 0xc4, 0x2a, 0x00, 0x0e);
    /// <summary>
    /// Use for video files containing corporate video. For example, recorded meetings or training videos.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_CORPORATE = new Guid(0x44051b5b, 0xb103, 0x4b5c, 0x92, 0xab, 0x93, 0x06, 0x0a, 0x94, 0x63, 0xf0);
    /// <summary>
    /// Use for video files containing home video made from photographs.
    /// </summary>
    private static readonly Guid CLASS_SECONDARY_VIDEO_HOME_PHOTOS = new Guid(0x0b710218, 0x8c0c, 0x475e, 0xaf, 0x73, 0x4c, 0x41, 0xc0, 0xc8, 0xf8, 0xce);

    #endregion

    #region COM imports

    /// <summary>
    /// CLSID_StreamBufferRecordingAttributes
    /// </summary>
    [ComImport, Guid("ccaa63ac-1057-4778-ae92-1206ab9acee6")]
    private class StreamBufferRecordingAttributes
    {
    }

    [ComImport, SuppressUnmanagedCodeSecurity,
     Guid("56a868a6-0ad4-11ce-b03a-0020af0ba770"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileSourceFilter
    {
      [PreserveSig]
      int Load(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
        IntPtr pmt
        );

      [PreserveSig]
      int GetCurFile(
        [Out, MarshalAs(UnmanagedType.LPWStr)] out string pszFileName,
        [Out] IntPtr pmt
        );
    }

    [ComImport, SuppressUnmanagedCodeSecurity,
     Guid("16ca4e03-fe69-4705-bd41-5b7dfc0c95f3"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IStreamBufferRecordingAttribute
    {
      [PreserveSig]
      int SetAttribute(
        [In] int ulReserved,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
        [In] StreamBufferAttrDataType StreamBufferAttributeType,
        [In] IntPtr pbAttribute, // BYTE *
        [In] short cbAttributeLength
        );

      [PreserveSig]
      int GetAttributeCount(
        [In] int ulReserved,
        [Out] out short pcAttributes
        );

      [PreserveSig]
      int GetAttributeByName(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
        [In] int pulReserved,
        [Out] out StreamBufferAttrDataType pStreamBufferAttributeType,
        [In, Out] IntPtr pbAttribute, // BYTE *
        [In, Out] ref short pcbLength
        );

      [PreserveSig]
      int GetAttributeByIndex(
        [In] short wIndex,
        [In] int pulReserved,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszAttributeName,
        [In, Out] ref short pcchNameLength,
        [Out] out StreamBufferAttrDataType pStreamBufferAttributeType,
        IntPtr pbAttribute, // BYTE *
        [In, Out] ref short pcbLength
        );

      int EnumAttributes([Out] out IntPtr/*IEnumStreamBufferRecordingAttrib*/ ppIEnumStreamBufferAttrib);
    }

    /// <summary>
    /// From STREAMBUFFER_ATTR_DATATYPE
    /// </summary>
    private enum StreamBufferAttrDataType
    {
      DWord = 0,
      String = 1,
      Binary = 2,
      Bool = 3,
      QWord = 4,
      Word = 5,
      Guid = 6
    }

    #endregion

    #region variables/properties

    public MediaType MediaType = MediaType.Television;
    public ICollection<string> ChannelNames = new List<string>();
    public DateTime StartTime = SqlDateTime.MinValue.Value;
    public DateTime EndTime = SqlDateTime.MinValue.Value;
    public string Title = string.Empty;
    public string Description = string.Empty;
    public string EpisodeName = null;
    public string EpisodeDescription = null;
    public int? SeasonNumber = null;
    public int? EpisodeNumber = null;

    public bool? IsPremiere = null;
    public bool? IsFinale = null;
    public bool? IsRepeat = null;

    public ICollection<string> Categories = new List<string>();
    public string Classification = null;
    public bool? IsHighDefinition = null;
    public bool? IsLive = null;
    public int? ProductionYear = null;

    public IDictionary<string, string> Credits = new Dictionary<string, string>();    // name => credit type

    public bool IsProtected = false;
    public bool? IsWatched = null;

    #endregion

    private WmcMetaInfo()
    {
    }

    /// <summary>
    /// Read the metadata from a WTV or DVR-MS file.
    /// </summary>
    /// <param name="fileName">The full name and path to the file.</param>
    /// <returns>a WMC meta info object</returns>
    public static WmcMetaInfo Read(string fileName)
    {
      IStreamBufferRecordingAttribute sbe = null;
      try
      {
        sbe = (IStreamBufferRecordingAttribute)new StreamBufferRecordingAttributes();
        IFileSourceFilter sourceFilter = (IFileSourceFilter)sbe;
        int hr = sourceFilter.Load(fileName, IntPtr.Zero);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          Log.Error("WMC meta-info: failed to load file, hr = 0x{0:x}, file name = {1}", hr, fileName);
          return null;
        }

        short attributeCount = 0;
        hr = sbe.GetAttributeCount(0, out attributeCount);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          Log.Error("WMC meta-info: failed to get attribute count, hr = 0x{0:x}, file name = {1}", hr, fileName);
          return null;
        }

        WmcMetaInfo info = new WmcMetaInfo();
        for (short i = 0; i < attributeCount; i++)
        {
          StreamBufferAttrDataType attributeType;
          StringBuilder attributeNameBuilder = null;
          short attributeNameLength = 0;
          IntPtr attributeValuePtr = IntPtr.Zero;
          short attributeValueLength = 0;

          hr = sbe.GetAttributeByIndex(i, 0, attributeNameBuilder, ref attributeNameLength, out attributeType, attributeValuePtr, ref attributeValueLength);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            Log.Warn("WMC meta-info: failed to get attribute detail, hr = 0x{0:x}, index = {1}, file name = {2}", hr, i, fileName);
            continue;
          }

          attributeNameBuilder = new StringBuilder(attributeNameLength);
          attributeValuePtr = Marshal.AllocHGlobal(attributeValueLength);
          try
          {
            hr = sbe.GetAttributeByIndex(i, 0, attributeNameBuilder, ref attributeNameLength, out attributeType, attributeValuePtr, ref attributeValueLength);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              Log.Warn("WMC meta-info: failed to get attribute value, hr = 0x{0:x}, index = {1}, file name = {2}", hr, i, fileName);
              continue;
            }

            // WM/SeasonNumber and WM/EpisodeNumber are only listed in
            // wmsdkidl.h. Internet search suggests values are numeric as
            // expected.
            // WM/WMRVInBandRatingSystem, WM/WMRVInBandRatingAttributes and
            // WM/WMRVInBandRatingLevel seem to be present in files but are
            // completely undocumented. Values seem to be numeric; 255 or 0 if
            // not set?
            string attributeName = attributeNameBuilder.ToString();
            int valueInt;
            int duration = 0;
            Guid mediaClassPrimary = Guid.Empty;
            Guid mediaClassSecondary = Guid.Empty;
            string providerRating = null;
            int videoHeight = 0;
            int videoWidth = 0;
            switch (attributeType)
            {
              case StreamBufferAttrDataType.DWord:
                valueInt = Marshal.ReadInt32(attributeValuePtr);
                switch (attributeName)
                {
                  case "WM/SeasonNumber":
                    info.SeasonNumber = valueInt;
                    break;
                  case "WM/EpisodeNumber":
                    info.EpisodeNumber = valueInt;
                    break;
                  case "WM/VideoHeight":
                    videoHeight = valueInt;
                    break;
                  case "WM/VideoWidth":
                    videoWidth = valueInt;
                    break;
                }
                break;
              case StreamBufferAttrDataType.String:
                string valueString = Marshal.PtrToStringUni(attributeValuePtr);
                if (string.IsNullOrEmpty(valueString))
                {
                  break;
                }
                switch (attributeName)
                {
                  case "WM/MediaStationName":
                  case "WM/RadioStationName":
                  case "WM/MediaOriginalChannel":
                  case "WM/MediaStationCallSign":
                  case "WM/MediaNetworkAffiliation":
                    if (!info.ChannelNames.Contains(valueString))
                    {
                      info.ChannelNames.Add(valueString);
                    }
                    break;
                  case "WM/MediaOriginalBroadcastDateTime":   // eg. 2004-06-27T23:00:00Z
                    if (!DateTime.TryParse(valueString, out info.StartTime))
                    {
                      Log.Warn("WMC meta-info: failed to read start time, attribute value = {0}", valueString);
                    }
                    break;
                  case "Title":
                    info.Title = valueString;
                    break;
                  case "Description":
                    info.Description = valueString;
                    break;
                  case "WM/SubTitle":
                    info.EpisodeName = valueString;
                    break;
                  case "WM/SubTitleDescription":
                    info.EpisodeDescription = valueString;
                    break;
                  case "WM/SeasonNumber":
                    if (int.TryParse(valueString, out valueInt))
                    {
                      info.SeasonNumber = valueInt;
                    }
                    break;
                  case "WM/EpisodeNumber":
                    if (int.TryParse(valueString, out valueInt))
                    {
                      info.EpisodeNumber = valueInt;
                    }
                    break;
                  case "WM/Category":
                  case "WM/Genre":
                  case "WM/Mood":
                  case "WM/ProviderStyle":
                    if (!info.Categories.Contains(valueString))
                    {
                      info.Categories.Add(valueString);
                    }
                    break;
                  case "WM/ParentalRating":   // WM/ParentalRatingReason may also be available, but we don't use it
                    info.Classification = valueString;
                    break;
                  case "WM/ProviderRating":
                    providerRating = valueString;
                    break;
                  case "WM/Year":   // According to documentation this is a string.
                    if (int.TryParse(valueString, out valueInt))
                    {
                      info.ProductionYear = valueInt;
                    }
                    break;

                  case "WM/Composer":
                  case "WM/Conductor":
                  case "WM/Director":
                  case "WM/MediaCredits":
                  case "WM/Producer":
                  case "WM/Writer":
                    string creditType = attributeName.Substring(3);
                    if (attributeName.Equals("MediaCredits"))
                    {
                      creditType = "Unknown";
                    }
                    string[] people = valueString.Split(new char[] { ',', '/', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string p in people)
                    {
                      string person = p.Trim();
                      if (!string.IsNullOrEmpty(person))
                      {
                        string existingCreditType;
                        if (!info.Credits.TryGetValue(person, out existingCreditType))
                        {
                          info.Credits[person] = creditType;
                        }
                        else if (!creditType.Equals("Unknown"))
                        {
                          if (!existingCreditType.Equals("Unknown"))
                          {
                            creditType += "," + existingCreditType;
                          }
                          info.Credits[person] = creditType;
                        }
                      }
                    }
                    break;

                  case "WM/DRM":    // Format unknown.
                    if (!string.IsNullOrEmpty(valueString))
                    {
                      info.IsProtected = true;
                    }
                    break;
                }
                break;
              case StreamBufferAttrDataType.Bool:
                bool valueBool = Marshal.ReadInt32(attributeValuePtr) > 0;
                switch (attributeName)
                {
                  case "HasVideo":
                    if (!valueBool)
                    {
                      info.MediaType = MediaType.Radio;
                    }
                    break;
                  case "WM/MediaIsPremiere":
                    info.IsPremiere = valueBool;
                    break;
                  case "WM/MediaIsFinale":
                    info.IsFinale = valueBool;
                    break;
                  case "WM/MediaIsRepeat":
                    info.IsRepeat = valueBool;
                    break;
                  case "WM/MediaIsLive":
                    info.IsLive = valueBool;
                    break;
                  case "Is_Protected":
                  case "WM/WMRVContentProtected":
                    info.IsProtected |= valueBool;
                    break;
                  case "WM/WMRVWatched":
                    info.IsWatched = valueBool;
                    break;
                }
                break;
              case StreamBufferAttrDataType.QWord:
                long valueLong = Marshal.ReadInt64(attributeValuePtr);
                switch (attributeName)
                {
                  case "Duration":    // unit = 100 ns
                    duration = (int)(valueLong / 10000000);
                    break;
                  case "WM/SeasonNumber":
                    info.SeasonNumber = (int)valueLong;
                    break;
                  case "WM/EpisodeNumber":
                    info.EpisodeNumber = (int)valueLong;
                    break;
                }
                break;
              case StreamBufferAttrDataType.Word:
                short valueShort = Marshal.ReadInt16(attributeValuePtr);
                switch (attributeName)
                {
                  case "WM/SeasonNumber":
                    info.SeasonNumber = valueShort;
                    break;
                  case "WM/EpisodeNumber":
                    info.EpisodeNumber = valueShort;
                    break;
                }
                break;
              case StreamBufferAttrDataType.Guid:
                Guid valueGuid = (Guid)Marshal.PtrToStructure(attributeValuePtr, typeof(Guid));
                switch (attributeName)
                {
                  case "WM/MediaClassPrimaryID":
                    mediaClassPrimary = valueGuid;
                    break;
                  case "WM/MediaClassSecondaryID":
                    mediaClassSecondary = valueGuid;
                    break;
                }
                break;
            }

            if (duration != 0 && info.StartTime != SqlDateTime.MinValue.Value)
            {
              info.EndTime = info.StartTime.AddSeconds(duration);
            }

            if (mediaClassPrimary == CLASS_PRIMARY_AUDIO || mediaClassPrimary == CLASS_PRIMARY_MUSIC)
            {
              if (mediaClassPrimary == CLASS_PRIMARY_MUSIC)
              {
                info.Categories.Add("Music");
              }
              info.MediaType = MediaType.Radio;
            }

            if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_BOOK)
            {
              info.Categories.Add("Audio Book");
              info.MediaType = MediaType.Radio;
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_SPOKEN_WORD)
            {
              info.Categories.Add("Spoken Word");
              info.MediaType = MediaType.Radio;
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_NEWS || mediaClassSecondary == CLASS_SECONDARY_VIDEO_NEWS)
            {
              if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_NEWS)
              {
                info.MediaType = MediaType.Radio;
              }
              info.Categories.Add("News");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_TALK_SHOW)
            {
              info.Categories.Add("Talk Show");
              info.MediaType = MediaType.Radio;
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_WEB)
            {
              info.Categories.Add("Other");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_GAME_CLIP)
            {
              info.Categories.Add("Game Clip");
              info.MediaType = MediaType.Radio;
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_AUDIO_GAME_FULL)
            {
              info.Categories.Add("Game");
              info.MediaType = MediaType.Radio;
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_MUSIC)
            {
              info.Categories.Add("Music Video");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_HOME)
            {
              info.Categories.Add("Home Video");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_MOVIE)
            {
              info.Categories.Add("Movie");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_TV)
            {
              info.Categories.Add("TV Show");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_CORPORATE)
            {
              info.Categories.Add("Corporate");
            }
            else if (mediaClassSecondary == CLASS_SECONDARY_VIDEO_HOME_PHOTOS)
            {
              info.Categories.Add("Home Photos");
            }

            if (!string.IsNullOrEmpty(info.Classification) && !string.IsNullOrEmpty(providerRating))
            {
              info.Classification = providerRating;
            }

            if (videoWidth >= 1280 || videoHeight >= 720)
            {
              info.IsHighDefinition = true;
            }
            else if (videoWidth > 0 || videoHeight > 0)
            {
              info.IsHighDefinition = false;
            }
          }
          finally
          {
            Marshal.FreeHGlobal(attributeValuePtr);
          }
        }

        return info;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "WMC meta-info: failed to read file attributes, file name = {0}", fileName);
      }
      finally
      {
        if (sbe != null)
        {
          Marshal.ReleaseComObject(sbe);
        }
      }
      return null;
    }
  }
}