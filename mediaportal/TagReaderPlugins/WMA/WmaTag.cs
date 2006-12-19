#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using MediaPortal.TagReader;

using MediaPortal.GUI.Library;

namespace Tag.WMA
{
  public class WmaTag : TagBase
  {
    #region Enums
    private enum WMFAttributeField
    {
      ASFLeakyBucketPairs,					// ASFLeakyBucketPairs
      AspectRatioX,					        // AspectRatioX
      AspectRatioY,					        // AspectRatioY
      Author,					                // Author
      AverageLevel,					        // AverageLevel
      BannerImageData,					    // BannerImageData
      BannerImageType,					    // BannerImageType
      BannerImageURL,					        // BannerImageURL
      Bitrate,					            // Bitrate
      Broadcast,					            // Broadcast
      BufferAverage,					        // BufferAverage
      Can_Skip_Backward,					    // Can_Skip_Backward
      Can_Skip_Forward,					    // Can_Skip_Forward
      Copyright,					            // Copyright
      CopyrightURL,					        // CopyrightURL
      CurrentBitrate,					        // CurrentBitrate
      Description,					        // Description
      DRM_ContentID,					        // DRM_ContentID
      DRM_DRMHeader_ContentDistributor,		// DRM_DRMHeader_ContentDistributor			
      DRM_DRMHeader_ContentID,				// DRM_DRMHeader_ContentID	
      DRM_DRMHeader_IndividualizedVersion,	// DRM_DRMHeader_IndividualizedVersion				
      DRM_DRMHeader_KeyID,					// DRM_DRMHeader_KeyID
      DRM_DRMHeader_LicenseAcqURL,			// DRM_DRMHeader_LicenseAcqURL		
      DRM_DRMHeader_SubscriptionContentID,	// DRM_DRMHeader_SubscriptionContentID				
      DRM_DRMHeader,					        // DRM_DRMHeader
      DRM_IndividualizedVersion,				// DRM_IndividualizedVersion	
      DRM_KeyID,					            // DRM_KeyID
      DRM_LASignatureCert,					// DRM_LASignatureCert
      DRM_LASignatureLicSrvCert,				// DRM_LASignatureLicSrvCert	
      DRM_LASignaturePrivKey,					// DRM_LASignaturePrivKey
      DRM_LASignatureRootCert,				// DRM_LASignatureRootCert	
      DRM_LicenseAcqURL,					    // DRM_LicenseAcqURL
      DRM_V1LicenseAcqURL,					// DRM_V1LicenseAcqURL
      Duration,					            // Duration
      FileSize,					            // FileSize
      HasArbitraryDataStream,					// HasArbitraryDataStream
      HasAttachedImages,					    // HasAttachedImages
      HasAudio,					            // HasAudio
      HasFileTransferStream,					// HasFileTransferStream
      HasImage,					            // HasImage
      HasScript,					            // HasScript
      HasVideo,					            // HasVideo
      Is_Protected,					        // Is_Protected
      Is_Trusted,					            // Is_Trusted
      IsVBR,					                // IsVBR
      NSC_Address,					        // NSC_Address
      NSC_Description,					    // NSC_Description
      NSC_Email,					            // NSC_Email
      NSC_Name,					            // NSC_Name
      NSC_Phone,					            // NSC_Phone
      NumberOfFrames,					        // NumberOfFrames
      OptimalBitrate,					        // OptimalBitrate
      PeakValue,					            // PeakValue
      Rating,					                // Rating
      Seekable,					            // Seekable
      Signature_Name,					        // Signature_Name
      Stridable,					            // Stridable
      Title,					                // Title
      VBRPeak,					            // VBRPeak
      WM_AlbumArtist,					        // WM/AlbumArtist
      WM_AlbumCoverURL,					    // WM/AlbumCoverURL
      WM_AlbumTitle,					        // WM/AlbumTitle
      WM_ASFPacketCount,					    // WM/ASFPacketCount
      WM_ASFSecurityObjectsSize,				// WM/ASFSecurityObjectsSize	
      WM_AudioFileURL,					    // WM/AudioFileURL
      WM_AudioSourceURL,					    // WM/AudioSourceURL
      WM_AuthorURL,					        // WM/AuthorURL
      WM_BeatsPerMinute,					    // WM/BeatsPerMinute
      WM_Category,					        // WM/Category
      WM_Codec,					            // WM/Codec
      WM_Composer,					        // WM/Composer
      WM_Conductor,					        // WM/Conductor
      WM_ContainerFormat,					    // WM/ContainerFormat
      WM_ContentDistributor,					// WM/ContentDistributor
      WM_ContentGroupDescription,				// WM/ContentGroupDescription	
      WM_Director,					        // WM/Director
      WM_DRM,					                // WM/DRM
      WM_DVDID,					            // WM/DVDID
      WM_EncodedBy,					        // WM/EncodedBy
      WM_EncodingSettings,					// WM/EncodingSettings
      WM_EncodingTime,					    // WM/EncodingTime
      WM_Genre,					            // WM/Genre
      WM_GenreID,					            // WM/GenreID
      WM_InitialKey,					        // WM/InitialKey
      WM_ISRC,					            // WM/ISRC
      WM_Language,					        // WM/Language
      WM_Lyrics,					            // WM/Lyrics
      WM_Lyrics_Synchronised,					// WM/Lyrics_Synchronised
      WM_MCDI,					            // WM/MCDI
      WM_MediaClassPrimaryID,					// WM/MediaClassPrimaryID
      WM_MediaClassSecondaryID,				// WM/MediaClassSecondaryID	
      WM_MediaCredits,					    // WM/MediaCredits
      WM_MediaIsDelay,					    // WM/MediaIsDelay
      WM_MediaIsFinale,					    // WM/MediaIsFinale
      WM_MediaIsLive,					        // WM/MediaIsLive
      WM_MediaIsPremiere,					    // WM/MediaIsPremiere
      WM_MediaIsRepeat,					    // WM/MediaIsRepeat
      WM_MediaIsSAP,					        // WM/MediaIsSAP
      WM_MediaIsStereo,					    // WM/MediaIsStereo
      WM_MediaIsSubtitled,					// WM/MediaIsSubtitled
      WM_MediaIsTape,					        // WM/MediaIsTape
      WM_MediaNetworkAffiliation,				// WM/MediaNetworkAffiliation	
      WM_MediaOriginalBroadcastDateTime,		// WM/MediaOriginalBroadcastDateTime			
      WM_MediaOriginalChannel,				// WM/MediaOriginalChannel	
      WM_MediaStationCallSign,				// WM/MediaStationCallSign	
      WM_MediaStationName,					// WM/MediaStationName
      WM_ModifiedBy,					        // WM/ModifiedBy
      WM_Mood,					            // WM/Mood
      WM_OriginalAlbumTitle,					// WM/OriginalAlbumTitle
      WM_OriginalArtist,					    // WM/OriginalArtist
      WM_OriginalFilename,					// WM/OriginalFilename
      WM_OriginalLyricist,					// WM/OriginalLyricist
      WM_OriginalReleaseTime,					// WM/OriginalReleaseTime
      WM_OriginalReleaseYear,					// WM/OriginalReleaseYear
      WM_ParentalRating,					    // WM/ParentalRating
      WM_ParentalRatingReason,				// WM/ParentalRatingReason	
      WM_PartOfSet,					        // WM/PartOfSet
      WM_PeakBitrate,					        // WM/PeakBitrate
      WM_Period,					            // WM/Period
      WM_Picture,					            // WM/Picture
      WM_PlaylistDelay,					    // WM/PlaylistDelay
      WM_Producer,					        // WM/Producer
      WM_PromotionURL,					    // WM/PromotionURL
      WM_ProtectionType,					    // WM/ProtectionType
      WM_Provider,					        // WM/Provider
      WM_ProviderCopyright,					// WM/ProviderCopyright
      WM_ProviderRating,					    // WM/ProviderRating
      WM_ProviderStyle,					    // WM/ProviderStyle
      WM_Publisher,					        // WM/Publisher
      WM_RadioStationName,					// WM/RadioStationName
      WM_RadioStationOwner,					// WM/RadioStationOwner
      WM_SharedUserRating,					// WM/SharedUserRating
      WM_StreamTypeInfo,					    // WM/StreamTypeInfo
      WM_SubscriptionContentID,				// WM/SubscriptionContentID	
      WM_SubTitle,					        // WM/SubTitle
      WM_SubTitleDescription,					// WM/SubTitleDescription
      WM_Text,					            // WM/Text
      WM_ToolName,					        // WM/ToolName
      WM_ToolVersion,					        // WM/ToolVersion
      WM_Track,					            // WM/Track
      WM_TrackNumber,					        // WM/TrackNumber
      WM_UniqueFileIdentifier,				// WM/UniqueFileIdentifier	
      WM_UserWebURL,					        // WM/UserWebURL
      WM_VideoClosedCaptioning,				// WM/VideoClosedCaptioning	
      WM_VideoFrameRate,					    // WM/VideoFrameRate
      WM_VideoHeight,					        // WM/VideoHeight
      WM_VideoWidth,					        // WM/VideoWidth
      WM_WMADRCAverageReference,				// WM/WMADRCAverageReference	
      WM_WMADRCAverageTarget,					// WM/WMADRCAverageTarget
      WM_WMADRCPeakReference,					// WM/WMADRCPeakReference
      WM_WMADRCPeakTarget,					// WM/WMADRCPeakTarget
      WM_WMCollectionGroupID,					// WM/WMCollectionGroupID
      WM_WMCollectionID,					    // WM/WMCollectionID
      WM_WMContentID,					        // WM/WMContentID
      WM_Writer,					            // WM/Writer
      WM_Year,					            // WM/Year
      WMFSDKVersion,
      WMFSDKNeeded,
    };
    #endregion

    #region Variables

    private List<WMFAttribute> AttributeList = new List<WMFAttribute>();
    internal IWMSyncReader WMReader = null;
    private WaveFormatEx WaveFmtEx = new WaveFormatEx();
    private IWMMetadataEditor2 WMMetaDataEditor = null;
    private IWMHeaderInfo3 WMHeaderInfo = null;
    private ushort AtrributeCount;

    #endregion

    #region Constructors/Destructors
    public WmaTag()
      : base()
    {
    }

    ~WmaTag()
    {
      Dispose();
    }
    #endregion

    #region Properties

    public override string Album
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_AlbumTitle); }
    }

    public override string Artist
    {
      get { return GetStringAttributeValue(WMFAttributeField.Author); }
    }

    public override string AlbumArtist
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_AlbumArtist); }
    }

    public override string ArtistURL
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_AuthorURL); }
    }

    public override int AverageBitrate
    {
      get { return (int)GetUInt32AttributeValue(WMFAttributeField.Bitrate) / 1000; }
    }

    public override int BitsPerSample
    {
      get { return (int)WaveFmtEx.wBitsPerSample; }
    }

    public override int BlocksPerFrame
    {
      get { return base.BlocksPerFrame; }
    }

    public override string BuyURL
    {
      get { return base.BuyURL; }
    }

    public override int BytesPerSample
    {
      get { return base.BytesPerSample; }
    }

    public override int Channels
    {
      get { return (int)WaveFmtEx.nChannels; }
    }

    public override string Comment
    {
      get { return base.Comment; }
    }

    public override string Composer
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_Composer); }
    }

    public override int CompressionLevel
    {
      get { return base.CompressionLevel; }
    }

    public override string Copyright
    {
      get { return GetStringAttributeValue(WMFAttributeField.Copyright); }
    }

    public override string CopyrightURL
    {
      get { return GetStringAttributeValue(WMFAttributeField.CopyrightURL); }
    }

    public override byte[] CoverArtImageBytes
    {
      get
      {
        byte[] tempBytes = GetBinaryAttributeValue(WMFAttributeField.WM_Picture);

        if (tempBytes == null)
          return null;

        MemoryStream s = new MemoryStream(tempBytes);
        BinaryReader r = new BinaryReader(s);
        string pwszMIMEType = Marshal.PtrToStringUni(new IntPtr(r.ReadUInt32()));
        byte bPictureType = r.ReadByte();
        string pwszDescription = Marshal.PtrToStringUni(new IntPtr(r.ReadUInt32()));
        uint dwDataLen = r.ReadUInt32();

        byte[] imgBytes = new byte[dwDataLen];
        Buffer.BlockCopy(tempBytes, tempBytes.Length - (int)dwDataLen, imgBytes, 0, (int)dwDataLen);

        return imgBytes;
      }
    }

    public override string FileURL
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_AudioFileURL); }
    }

    public override int FormatFlags
    {
      get { return base.FormatFlags; }
    }

    public override string Genre
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_Genre); }
    }

    public override bool IsVBR
    {
      get { return GetBooleanAttributeValue(WMFAttributeField.IsVBR); }
    }

    public override string Keywords
    {
      get { return base.Keywords; }
    }

    public override string Length
    {
      get { return Utils.GetDurationString(LengthMS); }
    }

    public override int LengthMS
    {
      get { return (int)(GetUInt64AttributeValue(WMFAttributeField.Duration) / 10000); }
    }

    public override string Lyrics
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_Lyrics); }
    }

    public override string Notes
    {
      get { return base.Notes; }
    }

    public override string PeakLevel
    {
      //get { return ""; }
      get
      {
        try
        {
          uint peakLvl = GetUInt32AttributeValue(WMFAttributeField.PeakValue);

          if (peakLvl > 0)
            return peakLvl.ToString();

          return
              string.Empty;
        }

        catch (Exception ex)
        {
          Log.Error("WmaTag.get_PeakLevel caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return string.Empty;
        }
      }
    }

    public override string PublisherURL
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_Publisher); }
    }

    public override string ReplayGainAlbum
    {
      get { return base.ReplayGainAlbum; }
    }

    public override string ReplayGainRadio
    {
      get { return base.ReplayGainRadio; }
    }

    public override int SampleRate
    {
      get { return WaveFmtEx.nSamplesPerSec; }
    }

    public override string Title
    {
      get { return GetStringAttributeValue(WMFAttributeField.Title); }
    }

    public override string ToolName
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_ToolName); }
    }

    public override string ToolVersion
    {
      get { return GetStringAttributeValue(WMFAttributeField.WM_ToolVersion); }
    }

    public override int TotalBlocks
    {
      get { return base.TotalBlocks; }
    }

    public override int TotalFrames
    {
      get
      {
        try
        {
          int totFrames = (int)GetUInt64AttributeValue(WMFAttributeField.NumberOfFrames);
          return totFrames;
        }

        catch (Exception ex)
        {
          Log.Error("WmaTag.get_PeakLevel caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    public override int Track
    {
      get
      {
        try
        {
          string sTrackNum = GetStringAttributeValue(WMFAttributeField.WM_TrackNumber);

          if (sTrackNum.Length > 0)
            return int.Parse(sTrackNum);

          // Check the old WM/Track attribute
          sTrackNum = GetStringAttributeValue(WMFAttributeField.WM_Track);

          if (sTrackNum.Length > 0)
          {
            // The old WM/Track attribute is zero based
            int trackNumber = int.Parse(sTrackNum) + 1;
            return trackNumber;
          }

          // If we still don't have a track number it's possible that the data is stored 
          // as a DWORD type instead of a string type...

          bool success = false;
          int trackNum = (int)GetUInt32AttributeValue(WMFAttributeField.WM_TrackNumber, out success);

          if (success)
            return trackNum;

          trackNum = (int)GetUInt32AttributeValue(WMFAttributeField.WM_Track, out success);

          if (success)
            return ++trackNum;

        }

        catch (Exception ex)
        {
          Log.Error("WmaTag.get_Track caused an exception in file {0} : {1}", base.FileName, ex.Message);
        }

        return 0;
      }
    }

    public override string Version
    {

      get { return GetStringAttributeValue(WMFAttributeField.WMFSDKVersion); }
    }

    public override int Year
    {
      get
      {
        try
        {
          string sYear = GetStringAttributeValue(WMFAttributeField.WM_Year);

          if (sYear.Length == 0)
            sYear = GetStringAttributeValue(WMFAttributeField.WM_OriginalReleaseYear);

          return Utils.GetYear(sYear);
        }

        catch (Exception ex)
        {
          Log.Error("WmaTag.get_Year caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    #region Public Methods
    public override bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower() == ".wma") return true;
      return false;
    }

    public override bool Read(string fileName)
    {
      if (fileName.Length == 0)
        throw new Exception("No file name specified");

      if (!File.Exists(fileName))
        throw new Exception("Unable to open file.  File does not exist.");

      if (Path.GetExtension(fileName).ToLower() != ".wma")
        throw new AudioFileTypeException("Expected WMA file type.");

      base.Read(fileName);
      bool result = true;

      try
      {
        if (!GetWmaAttributes())
          return false;
      }

      catch (Exception ex)
      {
        Log.Error("WmaTag.Read caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }
    #endregion

    #region Private Methods
    private bool GetWmaAttributes()
    {
      bool result = true;

      try
      {
        WindowsMediaWrapper.CreateEditor(out WMMetaDataEditor);
        WMMetaDataEditor.Open(AudioFilePath);
        WMHeaderInfo = (IWMHeaderInfo3)WMMetaDataEditor;
        WMMetaDataEditor.Close();
        WMHeaderInfo.GetAttributeCount(0xFFFF, out AtrributeCount);

        GetAttributes();
      }

      catch (COMException cex)
      {
        Log.Error("WmaTag.GetWmaAttribute caused an exception in file {0} : {1}", base.FileName, cex.Message);

        result = false;

        if ((ComError)cex.ErrorCode == ComError.NS_E_FILE_OPEN_FAILED)
          throw new FileNotFoundException("Unable to open file: {0}.", AudioFilePath);

        else
          throw cex;
      }

      finally
      {
        // I'm not sure why but the WMMetaDataEditor and WMHeaderInfo get destroyed 
        // after this method terminates so we need to make sure we flush and close.
        // Otherwise, the AudioFilePath file will be locked the next time we try to 
        // Get the attributes.

        if (WMMetaDataEditor != null)
        {
          WMMetaDataEditor.Flush();
          WMMetaDataEditor.Close();
          WMMetaDataEditor = null;
        }

        if (WMHeaderInfo != null)
        {
          ((IWMMetadataEditor2)WMHeaderInfo).Flush();
          ((IWMMetadataEditor2)WMHeaderInfo).Close();
          WMHeaderInfo = null;
        }
      }

      return result;
    }

    private bool GetWmaPoperties()
    {
      bool result = true;

      try
      {
        IWMOutputMediaProps wmMediaPoperties = null;
        WindowsMediaWrapper.WMCreateSyncReader(IntPtr.Zero, WMT_RIGHTS.WMT_RIGHT_PLAYBACK | WMT_RIGHTS.WMT_RIGHT_NODRM, out WMReader);
        WMReader.Open(AudioFilePath);
        uint outputCount = 0;
        WMReader.GetOutputCount(out outputCount);
        WMReader.GetOutputProps(outputCount - 1, out wmMediaPoperties);

        if (wmMediaPoperties != null)
        {
          WM_MEDIA_TYPE wmMediaType;
          uint size = 0;

          wmMediaPoperties.GetMediaType(IntPtr.Zero, ref size);
          IntPtr buffer = Marshal.AllocCoTaskMem((int)size);
          wmMediaPoperties.GetMediaType(buffer, ref size);
          wmMediaType = (WM_MEDIA_TYPE)Marshal.PtrToStructure(buffer, typeof(WM_MEDIA_TYPE));

          if (wmMediaType.formattype == WindowsMediaWrapper.WMFORMAT_WaveFormatEx)
          {
            WaveFmtEx = new WaveFormatEx();
            Marshal.PtrToStructure(wmMediaType.pbFormat, WaveFmtEx);
          }
        }
      }


      catch (Exception ex)
      {
        Log.Error("WmaTag.GetWmaProperty caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      finally
      {
        if (WMReader != null)
        {
          WMReader.Close();
          WMReader = null;
        }
      }

      return result;
    }

    private bool GetAttributes()
    {
      bool result = true;

      try
      {
        for (int i = 0; i < AtrributeCount; i++)
        {
          WMFAttribute wmfAttrib = GetAttributeByIndex(i);

          if (wmfAttrib != null)
          {
            //Console.WriteLine("{0}   {1}", wmfAttrib.Name, wmfAttrib.AttributeType.ToString());
            AttributeList.Add(wmfAttrib);
          }
        }
      }

      catch (Exception ex)
      {
        Log.Error("WmaTag.GetAttributes caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }

    private WMFAttribute GetAttributeByIndex(int index)
    {
      if (index < 0 || index >= this.AtrributeCount)
        return null;

      ushort globalStream = 0xFFFF;
      StringBuilder attribName = null;
      ushort attribNameLen = 0;
      WMT_ATTR_DATATYPE attribType;
      byte[] attribVal = null;
      ushort attribValLen = 0;

      WMHeaderInfo.GetAttributeByIndex(
          (ushort)index, ref globalStream,
          attribName, ref attribNameLen,
          out attribType, attribVal,
          ref attribValLen);

      attribName = new StringBuilder(attribNameLen);
      attribVal = new byte[attribValLen];

      WMHeaderInfo.GetAttributeByIndex(
          (ushort)index, ref globalStream,
          attribName, ref attribNameLen,
          out attribType, attribVal,
          ref attribValLen);

      WMFAttribute wmfAttrib = new WMFAttribute(attribName.ToString().Replace("/", "_"), attribVal, attribType);
      return wmfAttrib;
    }

    private WMFAttribute GetAttribute(WMFAttributeField attribField, out WMT_ATTR_DATATYPE attribType)
    {
      string attribName = attribField.ToString();

      byte[] attribVal = null;
      ushort attribValLen = 0;
      ushort globalStream = 0xFFFF;

      attribType = new WMT_ATTR_DATATYPE();

      try
      {
        // Call it once to get the length of attribVal...
        WMHeaderInfo.GetAttributeByName(ref globalStream, attribName,
            out attribType, attribVal, ref attribValLen);

        attribVal = new byte[attribValLen];

        // Call it again to get the actual attribVal
        WMHeaderInfo.GetAttributeByName(ref globalStream, attribName,
            out attribType, attribVal, ref attribValLen);
      }

      catch (COMException comEx)
      {
        // Did we find the attribute name?
        if ((ComError)comEx.ErrorCode == ComError.ASF_E_NOTFOUND)
          return null;

      // Nope, re-throw the exception
        else
        {
          Log.Error("WmaTag.GetAttribute caused an exception in file {0} : {1}", base.FileName, comEx.Message);
          throw comEx;
        }
      }

      WMFAttribute wmfAttrib = new WMFAttribute(attribName.ToString().Replace("/", "_"), attribVal, attribType);
      return wmfAttrib;
    }

    private string GetStringAttributeValue(WMFAttributeField attribField)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_STRING)
          {
            if (attrib.Value.Length <= 2)
              return string.Empty;

            string sVal = Encoding.Unicode.GetString(attrib.Value, 0, attrib.Value.Length - 2).Trim();
            return sVal;
          }
        }
      }

      return string.Empty;
    }

    private UInt32 GetUInt32AttributeValue(WMFAttributeField attribField)
    {
      bool success;
      return GetUInt32AttributeValue(attribField, out success);
    }

    private UInt32 GetUInt32AttributeValue(WMFAttributeField attribField, out bool success)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_DWORD)
          {
            success = true;
            return BitConverter.ToUInt32(attrib.Value, 0);
          }
        }
      }

      success = false;
      return 0;
    }

    private UInt16 GetUInt16AttributeValue(WMFAttributeField attribField)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_WORD)
            return BitConverter.ToUInt16(attrib.Value, 0);
        }
      }

      return 0;
    }

    private bool GetBooleanAttributeValue(WMFAttributeField attribField)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_BOOL)
            return BitConverter.ToBoolean(attrib.Value, 0);
        }
      }

      return false;
    }

    private byte[] GetBinaryAttributeValue(WMFAttributeField attribField)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_BINARY)
            return attrib.Value;
        }
      }

      return null;
    }

    private UInt64 GetUInt64AttributeValue(WMFAttributeField attribField)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_QWORD)
            return BitConverter.ToUInt64(attrib.Value, 0);
        }
      }

      return 0;
    }

    private string GetGuidAttributeValue(WMFAttributeField attribField)
    {
      string attributeName = attribField.ToString().ToLower();

      foreach (WMFAttribute attrib in AttributeList)
      {
        if (attrib.Name.ToLower().CompareTo(attributeName) == 0)
        {
          if (attrib.AttributeType == WMT_ATTR_DATATYPE.WMT_TYPE_GUID)
            return BitConverter.ToString(attrib.Value, 0, attrib.Value.Length);
        }
      }

      return string.Empty;
    }
    #endregion
  }
}