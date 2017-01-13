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
using System.Collections.ObjectModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts
{
  /// <summary>
  /// A class that models the transport stream program map table section
  /// defined in ISO/IEC 13818-1.
  /// </summary>
  public class TableProgramMap
  {
    #region constants

    /// <summary>
    /// The maximum size of an ISO/IEC 13818-1 PMT section, in bytes.
    /// </summary>
    public const int MAX_SIZE = 1024;

    #endregion

    #region variables

    private byte _tableId;
    private bool _sectionSyntaxIndicator;
    private ushort _sectionLength;
    private ushort _programNumber;
    private byte _version;
    private bool _currentNextIndicator;
    private byte _sectionNumber;
    private byte _lastSectionNumber;
    private ushort _pcrPid;
    private ushort _programInfoLength;
    private List<IDescriptor> _programDescriptors;
    private List<IDescriptor> _programCaDescriptors;
    private List<PmtElementaryStream> _elementaryStreams;
    private byte[] _crc;

    private byte[] _rawPmt;
    private CamType _camType = CamType.Default;

    #endregion

    // This class has a specific purpose - decoding and translating between
    // various PMT formats. Although it may be tempting, we want to prevent it
    // being used for holding various other info. Therefore the only way you can
    // get an instance is by calling Decode() with a valid PMT section.
    private TableProgramMap()
    {
    }

    #region properties

    /// <summary>
    /// The program map table identifier.
    /// </summary>
    /// <remarks>
    /// Expected to be <c>0x02</c>.
    /// </remarks>
    public byte TableId
    {
      get
      {
        return _tableId;
      }
    }

    /// <summary>
    /// The program map section syntax indicator.
    /// </summary>
    /// <remarks>
    /// Expected to be <c>true</c>.
    /// </remarks>
    public bool SectionSyntaxIndicator
    {
      get
      {
        return _sectionSyntaxIndicator;
      }
    }

    /// <summary>
    /// The length of the program map section.
    /// </summary>
    /// <remarks>
    /// Includes the CRC but not the table ID, section syntax indicator or
    /// section length bytes.
    /// </remarks>
    public ushort SectionLength
    {
      get
      {
        return _sectionLength;
      }
    }

    /// <summary>
    /// The program number of the program that the program map describes.
    /// </summary>
    public ushort ProgramNumber
    {
      get
      {
        return _programNumber;
      }
    }

    /// <summary>
    /// The version number of the program map.
    /// </summary>
    public byte Version
    {
      get
      {
        return _version;
      }
    }

    /// <summary>
    /// When <c>true</c>, indicates that the program map describes the
    /// program's current state. Otherwise, indicates that the program map
    /// describes the next program state.
    /// </summary>
    public bool CurrentNextIndicator
    {
      get
      {
        return _currentNextIndicator;
      }
    }

    /// <summary>
    /// The index corresponding with this section of the program map.
    /// </summary>
    /// <remarks>Expected to be <c>0</c>.</remarks>
    public byte SectionNumber
    {
      get
      {
        return _sectionNumber;
      }
    }

    /// <summary>
    /// The total number of sections (minus one) that comprise the complete
    /// program map.
    /// </summary>
    /// <remarks>Expected to be <c>0</c>.</remarks>
    public byte LastSectionNumber
    {
      get
      {
        return _lastSectionNumber;
      }
    }

    /// <summary>
    /// The PID containing the program clock reference data for the program
    /// described by the program map.
    /// </summary>
    public ushort PcrPid
    {
      get
      {
        return _pcrPid;
      }
    }

    /// <summary>
    /// The total number of bytes in the program map program descriptors.
    /// </summary>
    public ushort ProgramInfoLength
    {
      get
      {
        return _programInfoLength;
      }
    }

    /// <summary>
    /// The descriptors for the program described by the program map.
    /// </summary>
    /// <remarks>
    /// Conditional access descriptors are not included.
    /// </remarks>
    public ReadOnlyCollection<IDescriptor> ProgramDescriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_programDescriptors);
      }
    }

    /// <summary>
    /// The conditional access descriptors for the program described by the
    /// program map.
    /// </summary>
    public ReadOnlyCollection<IDescriptor> ProgramCaDescriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_programCaDescriptors);
      }
    }

    /// <summary>
    /// The elementary streams described in the program map.
    /// </summary>
    public ReadOnlyCollection<PmtElementaryStream> ElementaryStreams
    {
      get
      {
        return new ReadOnlyCollection<PmtElementaryStream>(_elementaryStreams);
      }
    }

    /// <summary>
    /// Cyclic redundancy check bytes for confirming the integrity of the
    /// program map section data.
    /// </summary>
    public ReadOnlyCollection<byte> Crc
    {
      get
      {
        return new ReadOnlyCollection<byte>(_crc);
      }
    }

    #endregion

    /// <summary>
    /// Decode and check the validity of raw program map section data.
    /// </summary>
    /// <param name="data">The raw program map section data.</param>
    /// <returns>a fully populated Pmt instance if the section is valid, otherwise <c>null</c></returns>
    public static TableProgramMap Decode(byte[] data)
    {
      Log.Debug("PMT: decode");
      if (data == null || data.Length < 16)
      {
        Log.Error("PMT: data not supplied or too short");
        return null;
      }

      try
      {
        if (data[0] != 0x02)
        {
          Log.Error("PMT: invalid table ID {0}", data[0]);
          throw new System.Exception();
        }
        if ((data[1] & 0x80) != 0x80)
        {
          Log.Error("PMT: section syntax indicator is 0, should be 1");
          throw new System.Exception();
        }
        if ((data[1] & 0x40) != 0)
        {
          Log.Debug("PMT: corruption detected at header zero bit");
          throw new System.Exception();
        }
        if (data[6] != 0)
        {
          Log.Error("PMT: section number is {0}, should be 0", data[6]);
          throw new System.Exception();
        }
        if (data[7] != 0)
        {
          Log.Error("PMT: last section number is {0}, should be 0", data[7]);
          throw new System.Exception();
        }

        TableProgramMap pmt = new TableProgramMap();
        pmt._tableId = data[0];
        pmt._sectionSyntaxIndicator = (data[1] & 0x80) != 0;
        pmt._sectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        if (3 + pmt._sectionLength != data.Length)
        {
          Log.Error("PMT: section length {0} is invalid, data length = {1}", pmt._sectionLength, data.Length);
          throw new System.Exception();
        }

        pmt._programNumber = (ushort)((data[3] << 8) | data[4]);
        pmt._version = (byte)((data[5] & 0x3e) >> 1);
        pmt._currentNextIndicator = (data[5] & 0x01) != 0;
        pmt._sectionNumber = data[6];
        pmt._lastSectionNumber = data[7];
        pmt._pcrPid = (ushort)(((data[8] & 0x1f) << 8) | data[9]);
        pmt._programInfoLength = (ushort)(((data[10] & 0x0f) << 8) | data[11]);
        if (12 + pmt._programInfoLength + 4 > data.Length)
        {
          Log.Error("PMT: program info length {0} is invalid, data length = {1}", pmt._programInfoLength, data.Length);
          throw new System.Exception();
        }

        // Program descriptors.
        bool isScteTs = false;
        int offset = 12;
        int endProgramDescriptors = offset + pmt._programInfoLength;
        pmt._programDescriptors = new List<IDescriptor>(10);
        pmt._programCaDescriptors = new List<IDescriptor>(5);
        while (offset + 1 < endProgramDescriptors)
        {
          IDescriptor d = Descriptor.Decode(data, offset);
          if (d == null)
          {
            Log.Error("PMT: program descriptor {0} is invalid", pmt._programDescriptors.Count + pmt._programCaDescriptors.Count + 1);
            throw new System.Exception();
          }
          offset += d.Length + 2;
          if (d.Tag == DescriptorTag.ConditionalAccess)
          {
            pmt._programCaDescriptors.Add(d);
          }
          else
          {
            pmt._programDescriptors.Add(d);
            if (d.Tag == DescriptorTag.Registration && d.Length >= 4)
            {
              if (d.Data[0] == 'S' && d.Data[1] == 'C' && d.Data[2] == 'T' && d.Data[3] == 'E')
              {
                isScteTs = true;
              }
            }
          }
        }
        if (offset != endProgramDescriptors)
        {
          Log.Error("PMT: corruption detected at end of program descriptors, offset = {0}, program descriptors end = {1}", offset, endProgramDescriptors);
          throw new System.Exception();
        }

        // Elementary streams.
        bool isNotDc2Ts = false;
        bool foundDc2VideoStream = false;
        pmt._elementaryStreams = new List<PmtElementaryStream>(10);
        int endEsData = data.Length - 4;
        while (offset + 4 < endEsData)
        {
          PmtElementaryStream es = new PmtElementaryStream();
          es.StreamType = (StreamType)data[offset++];
          if (System.Enum.IsDefined(typeof(LogicalStreamType), (int)es.StreamType))
          {
            es.LogicalStreamType = (LogicalStreamType)(int)es.StreamType;
            // Could this still be a DC II transport stream?
            if (!isNotDc2Ts && !StreamTypeHelper.IsValidDigiCipher2Stream(es.StreamType))
            {
              isNotDc2Ts = true;
              if (foundDc2VideoStream)
              {
                // Fix false positive DC II video detections.
                for (int i = 0; i < pmt._elementaryStreams.Count; i++)
                {
                  if (pmt._elementaryStreams[i].StreamType == StreamType.DigiCipher2Video)
                  {
                    pmt._elementaryStreams[i].LogicalStreamType = LogicalStreamType.Unknown;
                  }
                }
                foundDc2VideoStream = false;
              }
            }
          }
          // We'll allow DC II video stream detection until we see clear
          // indication that this is not a DC II transport stream.
          else if (!isNotDc2Ts && es.StreamType == StreamType.DigiCipher2Video)
          {
            foundDc2VideoStream = true;
            es.LogicalStreamType = LogicalStreamType.VideoMpeg2;
          }
          else
          {
            es.LogicalStreamType = LogicalStreamType.Unknown;
          }
          es.Pid = (ushort)(((data[offset] & 0x1f) << 8) + data[offset + 1]);
          offset += 2;
          es.EsInfoLength = (ushort)(((data[offset] & 0x0f) << 8) + data[offset + 1]);
          offset += 2;

          // Elementary stream descriptors.
          int endEsDescriptors = offset + es.EsInfoLength;
          if (endEsDescriptors > endEsData)
          {
            Log.Error("PMT: elementary stream info length for PID {0} is invalid, ES data end = {1}, ES descriptors end = {2}", es.Pid, endEsData, endEsDescriptors);
            throw new System.Exception();
          }
          es.Descriptors = new List<IDescriptor>(15);
          es.CaDescriptors = new List<IDescriptor>(5);
          while (offset + 1 < endEsDescriptors)
          {
            IDescriptor d = Descriptor.Decode(data, offset);
            if (d == null)
            {
              Log.Error("PMT: elementary stream descriptor {0} for PID {1} is invalid", es.Descriptors.Count + es.CaDescriptors.Count + 1, es.Pid);
              throw new System.Exception();
            }

            if (d.Tag == DescriptorTag.ConditionalAccess)
            {
              es.CaDescriptors.Add(d);
            }
            else
            {
              es.Descriptors.Add(d);
              if (es.LogicalStreamType == LogicalStreamType.Unknown)
              {
                switch (d.Tag)
                {
                  case DescriptorTag.Mpeg4Video:
                    es.LogicalStreamType = LogicalStreamType.VideoMpeg4Part2;
                    break;
                  case DescriptorTag.AvcVideo:
                    es.LogicalStreamType = LogicalStreamType.VideoMpeg4Part10;
                    break;
                  case DescriptorTag.Registration:
                    // Old style DVB signalling. These days VC-1 can be
                    // signalled with stream type 0xea.
                    if (d.Data[0] == 'V' && d.Data[1] == 'C' && d.Data[2] == '-' && d.Data[3] == '1')
                    {
                      es.LogicalStreamType = LogicalStreamType.VideoVc1;
                    }
                    break;
                  case DescriptorTag.Mpeg2AacAudio:
                    es.LogicalStreamType = LogicalStreamType.AudioMpeg2Part7;
                    break;
                  case DescriptorTag.Mpeg4Audio:    // MPEG
                  case DescriptorTag.Aac:           // DVB
                  case DescriptorTag.MpegAac:       // SCTE
                    es.LogicalStreamType = LogicalStreamType.AudioMpeg4Part3Latm;
                    break;
                  case DescriptorTag.Ac3:           // DVB
                  case DescriptorTag.Ac3Audio:      // ATSC
                    es.LogicalStreamType = LogicalStreamType.AudioAc3;
                    break;
                  case DescriptorTag.EnhancedAc3:   // DVB
                  case DescriptorTag.Eac3:          // ATSC
                    es.LogicalStreamType = LogicalStreamType.AudioEnhancedAc3;
                    break;
                  case DescriptorTag.Dts:
                    if (isScteTs)
                    {
                      // Old style SCTE signalling. These days DTS-HD can be
                      // signalled with stream type 0x88.
                      es.LogicalStreamType = LogicalStreamType.AudioDtsHd;
                    }
                    else
                    {
                      es.LogicalStreamType = LogicalStreamType.AudioDts;
                    }
                    break;
                  case DescriptorTag.DvbExtension:
                    ExtensionDescriptorTag tag = (ExtensionDescriptorTag)d.Data[0];
                    if (tag == ExtensionDescriptorTag.DtsHdAudioStream)
                    {
                      es.LogicalStreamType = LogicalStreamType.AudioDtsHd;
                    }
                    else if (tag == ExtensionDescriptorTag.Ac4)
                    {
                      es.LogicalStreamType = LogicalStreamType.AudioAc4;
                    }
                    break;
                  case DescriptorTag.Subtitling:
                    es.LogicalStreamType = LogicalStreamType.Subtitles;
                    break;
                  case DescriptorTag.Teletext:
                  case DescriptorTag.VbiTeletext:
                    es.LogicalStreamType = LogicalStreamType.Teletext;
                    break;
                }
              }
            }
            offset += d.Length + 2;
          }
          if (offset != endEsDescriptors)
          {
            Log.Error("PMT: corruption detected at end of elementary strea descriptors for PID {0}, offset = {1}, ES descriptors end = {2}", es.Pid, offset, endEsDescriptors);
            throw new System.Exception();
          }

          pmt._elementaryStreams.Add(es);
        }
        if (offset != endEsData)
        {
          Log.Error("PMT: corruption detected at end of elementary stream data, offset = {0}, ES data end = {1}", offset, endEsData);
          throw new System.Exception();
        }

        pmt._crc = new byte[4];
        Buffer.BlockCopy(data, offset, pmt._crc, 0, 4);

        // Make a copy of the PMT so that changes made by the caller on the
        // original array have no effect on our reference/copy.
        pmt._rawPmt = new byte[data.Length];
        Buffer.BlockCopy(data, 0, pmt._rawPmt, 0, data.Length);

        //pmt.Dump();

        return pmt;
      }
      catch
      {
        Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(data, data.Length);
        return null;
      }
    }

    /// <summary>
    /// Patch the program map table to make it compatible with a specific type
    /// of conditional access module.
    /// </summary>
    /// <param name="camType">The target conditional access module type.</param>
    /// <returns>the patched program map table</returns>
    public TableProgramMap PatchForCam(CamType camType)
    {
      if (_camType == camType)
      {
        return this;
      }

      if (_camType != CamType.Default)
      {
        throw new TvException("Not possible to patch PMT that is already patched.");
      }

      TableProgramMap pmt = Decode(_rawPmt);
      if (camType == CamType.Astoncrypt2)
      {
        // For Astoncrypt 2 CAMs, we patch the stream type on AC3 streams.
        int offset = 12 + _programInfoLength;   // move to the first ES stream type
        foreach (PmtElementaryStream es in _elementaryStreams)
        {
          if (es.StreamType == StreamType.Mpeg2Part1PrivateData && es.LogicalStreamType == LogicalStreamType.AudioAc3)
          {
            es.StreamType = StreamType.Ac3Audio;
            pmt._rawPmt[offset] = (byte)StreamType.Ac3Audio;
          }
          offset += 5 + es.EsInfoLength;
        }
      }
      return pmt;
    }

    /// <summary>
    /// Retrieve a read-only copy of the original program map section data that
    /// was decoded to create this Pmt instance.
    /// </summary>
    /// <returns>a copy of the raw program map section data</returns>
    public ReadOnlyCollection<byte> GetRawPmt()
    {
      return new ReadOnlyCollection<byte>(_rawPmt);
    }

    /// <summary>
    /// Generate a conditional access program map command suitable for passing
    /// to an EN 50221 compliant conditional access module.
    /// </summary>
    /// <param name="listAction">The context of the command (in terms of other programs that the conditional access module will need to deal with).</param>
    /// <param name="command">The type of conditional access command.</param>
    /// <returns></returns>
    public byte[] GetCaPmt(CaPmtListManagementAction listAction, CaPmtCommand command)
    {
      this.LogDebug("PMT: get CA PMT, list action = {0}, command = {1}", listAction, command);
      byte[] tempCaPmt = new byte[MAX_SIZE];  // size of CA PMT <= size of PMT
      tempCaPmt[0] = (byte)listAction;
      tempCaPmt[1] = _rawPmt[3];
      tempCaPmt[2] = _rawPmt[4];
      tempCaPmt[3] = _rawPmt[5];

      // Program descriptors. As per EN 50221, we only add conditional access descriptors to the CA PMT.
      int offset = 6;
      int programInfoLength = 0;
      foreach (IDescriptor d in _programCaDescriptors)
      {
        if (programInfoLength == 0)
        {
          tempCaPmt[offset++] = (byte)command;
          programInfoLength++;
        }
        ReadOnlyCollection<byte> descriptorData = d.GetRawData();
        descriptorData.CopyTo(tempCaPmt, offset);
        offset += descriptorData.Count;
        programInfoLength += descriptorData.Count;
      }

      // Set the program_info_length now that we know what the length is.
      tempCaPmt[4] = (byte)((programInfoLength >> 8) & 0x0f);
      tempCaPmt[5] = (byte)(programInfoLength & 0xff);

      // Elementary streams.
      foreach (PmtElementaryStream es in _elementaryStreams)
      {
        // We add each video, audio, subtitle and teletext stream with their
        // corresponding conditional access descriptors to the CA PMT.
        if (StreamTypeHelper.IsVideoStream(es.LogicalStreamType) ||
          StreamTypeHelper.IsAudioStream(es.LogicalStreamType) ||
          es.LogicalStreamType == LogicalStreamType.Subtitles ||
          es.LogicalStreamType == LogicalStreamType.Teletext)
        {
          tempCaPmt[offset++] = (byte)es.StreamType;
          tempCaPmt[offset++] = (byte)((es.Pid >> 8) & 0x1f);
          tempCaPmt[offset++] = (byte)(es.Pid & 0xff);

          // Skip the ES_info_length field until we know what the length is.
          int esInfoLengthOffset = offset;
          offset += 2;

          // As per EN 50221, we only add conditional access descriptors to the CA PMT.
          int esInfoLength = 0;
          foreach (IDescriptor d in es.CaDescriptors)
          {
            if (esInfoLength == 0)
            {
              tempCaPmt[offset++] = (byte)command;
              esInfoLength++;
            }
            ReadOnlyCollection<byte> descriptorData = d.GetRawData();
            descriptorData.CopyTo(tempCaPmt, offset);
            offset += descriptorData.Count;
            esInfoLength += descriptorData.Count;
          }

          // Set the ES_info_length now that we know what the length is.
          tempCaPmt[esInfoLengthOffset++] = (byte)((esInfoLength >> 8) & 0x0f);
          tempCaPmt[esInfoLengthOffset] = (byte)(esInfoLength & 0xff);
        }
      }

      // There is no length output parameter, so we need to resize tempCaPmt to match the number of
      // meaningful CA PMT bytes.
      byte[] caPmt = new byte[offset];
      Buffer.BlockCopy(tempCaPmt, 0, caPmt, 0, offset);

      //Dump.DumpBinary(caPmt, caPmt.Length);

      return caPmt;
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      this.LogDebug("PMT: dump...");
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_rawPmt, _rawPmt.Length);
      this.LogDebug("  table ID                 = {0}", _tableId);
      this.LogDebug("  section syntax indicator = {0}", _sectionSyntaxIndicator);
      this.LogDebug("  section length           = {0}", _sectionLength);
      this.LogDebug("  program number           = {0}", _programNumber);
      this.LogDebug("  version                  = {0}", _version);
      this.LogDebug("  current next indicator   = {0}", _currentNextIndicator);
      this.LogDebug("  section number           = {0}", _sectionNumber);
      this.LogDebug("  last section number      = {0}", _lastSectionNumber);
      this.LogDebug("  PCR PID                  = {0}", _pcrPid);
      this.LogDebug("  program info length      = {0}", _programInfoLength);
      this.LogDebug("  CRC                      = 0x{0:x2}{1:x2}{2:x2}{3:x2}", _crc[0], _crc[1], _crc[2], _crc[3]);
      this.LogDebug("  {0} descriptor(s)...", _programDescriptors.Count + _programCaDescriptors.Count);
      foreach (IDescriptor d in _programDescriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _programCaDescriptors)
      {
        cad.Dump();
      }
      this.LogDebug("  {0} elementary stream(s)...", _elementaryStreams.Count);
      foreach (PmtElementaryStream es in _elementaryStreams)
      {
        es.Dump();
      }
    }
  }
}