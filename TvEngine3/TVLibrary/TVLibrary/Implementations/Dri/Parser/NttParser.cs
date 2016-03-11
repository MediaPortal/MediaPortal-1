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
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Dri.Parser
{
  public delegate void NttSourceNameDelegate(AtscTransmissionMedium transmissionMedium, bool applicationType, int sourceId, string name);

  /// <summary>
  /// ATSC/SCTE network text table parser. Refer to ATSC A-56 and SCTE 65.
  /// </summary>
  public class NttParser : BaseDriParser
  {
    private enum TableSubtype
    {
      TransponderName = 1,
      SatelliteText,
      RatingsText,
      RatingSystem,
      CurrencySystem,
      SourceName,
      MapName
    }

    public event TableCompleteDelegate OnTableComplete = null;
    public event NttSourceNameDelegate OnSourceName = null;

    public NttParser()
      : base(1, 7)
    {
    }

    public void Decode(byte[] section)
    {
      if (OnTableComplete == null)
      {
        return;
      }
      if (section.Length < 14)
      {
        Log.Log.Error("NTT: invalid section size {0}, expected at least 14 bytes", section.Length);
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc3)
      {
        return;
      }
      int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)  // 2 for section length bytes, 3 for table ID and PID
      {
        Log.Log.Error("NTT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      int protocolVersion = (section[5] & 0x1f);
      string isoLangCode = System.Text.Encoding.ASCII.GetString(section, 6, 3);
      AtscTransmissionMedium transmissionMedium = (AtscTransmissionMedium)(section[9] >> 4);
      TableSubtype tableSubtype = (TableSubtype)(section[9] & 0x0f);
      Log.Log.Debug("NTT: section length = {0}, protocol version = {1}, ISO language code = {2}, transmission medium = {3}, table subtype = {4}",
        sectionLength, protocolVersion, isoLangCode, transmissionMedium, tableSubtype);

      int pointer = 10;
      int endOfSection = section.Length - 4;

      try
      {
        switch (tableSubtype)
        {
          case TableSubtype.TransponderName:
            DecodeTransponderName(section, endOfSection, ref pointer);
            break;
          case TableSubtype.SatelliteText:
            DecodeSatelliteText(section, endOfSection, ref pointer);
            break;
          case TableSubtype.RatingsText:
            DecodeRatingsText(section, endOfSection, ref pointer);
            break;
          case TableSubtype.RatingSystem:
            DecodeRatingSystem(section, endOfSection, ref pointer);
            break;
          case TableSubtype.CurrencySystem:
            DecodeCurrencySystem(section, endOfSection, ref pointer);
            break;
          case TableSubtype.SourceName:
            DecodeSourceName(section, endOfSection, ref pointer, transmissionMedium);
            break;
          case TableSubtype.MapName:
            DecodeMapName(section, endOfSection, ref pointer);
            break;
          default:
            Log.Log.Error("NTT: unsupported table subtype {0}", tableSubtype);
            return;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error(ex.Message);
        return;
      }

      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("NTT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("NTT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
          return;
        }

        if (tag == 0x93)  // revision detection descriptor
        {
          DecodeRevisionDetectionDescriptor(section, pointer, length, (int)tableSubtype);
        }

        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("NTT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }

      if (tableSubtype == TableSubtype.SourceName &&
        _currentVersions[(int)TableSubtype.SourceName] != -1 &&
        _unseenSections[(int)TableSubtype.SourceName].Count == 0 &&
        OnTableComplete != null)
      {
        OnTableComplete(MgtTableType.NttSns);
        OnTableComplete = null;
        OnSourceName = null;
      }
    }

    private void DecodeTransponderName(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at transponder name, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte satelliteId = section[pointer++];
      byte firstIndex = section[pointer++];
      byte numberOfTntRecords = section[pointer++];
      Log.Log.Debug("NTT: transponder name, satellite ID = {0}, first index = {1}, number of TNT records = {2}", satelliteId, firstIndex, numberOfTntRecords);

      for (byte i = 0; i < numberOfTntRecords; i++)
      {
        if (pointer + 3 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected transponder name table number of TNT records {0} is invalid, pointer = {1}, end of section = {2}", numberOfTntRecords, pointer, endOfSection, i));
        }
        int transponderNumber = (section[pointer++] & 0x3f);
        int transponderNameLength = (section[pointer++] & 0x1f);
        if (pointer + transponderNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid transponder name table transponder name length {0}, pointer = {1}, end of section = {2}, loop = {3}", transponderNameLength, pointer, endOfSection, i));
        }
        string transponderName = DecodeMultilingualText(section, pointer + transponderNameLength, ref pointer);
        Log.Log.Debug("NTT: transponder name, number = {0}, name = {1}", transponderNumber, transponderName);

        // table descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at transponder name table descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected transponder name table descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: transponder name table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid transponder name table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d));
          }
          pointer += length;
        }
      }
    }

    private void DecodeSatelliteText(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 2 > endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at satellite text, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte firstIndex = section[pointer++];
      byte numberOfSttRecords = section[pointer++];
      Log.Log.Debug("NTT: satellite text, first index = {0}, number of STT records = {1}", firstIndex, numberOfSttRecords);

      for (byte i = 0; i < numberOfSttRecords; i++)
      {
        if (pointer + 4 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected satellite text table number of STT records {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numberOfSttRecords, pointer, endOfSection, i));
        }
        byte satelliteId = section[pointer++];
        int satelliteReferenceNameLength = (section[pointer++] & 0x0f);
        if (pointer + satelliteReferenceNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid satellite text table satellite reference name length {0}, pointer = {1}, end of section = {2}, loop = {3}", satelliteReferenceNameLength, pointer, endOfSection, i));
        }
        string satelliteReferenceName = DecodeMultilingualText(section, pointer + satelliteReferenceNameLength, ref pointer);

        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: corruption detected at satellite text table full satellite name, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
        }
        int fullSatelliteNameLength = (section[pointer++] & 0x1f);
        if (pointer + fullSatelliteNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid satellite text table full satellite name length {0}, pointer = {1}, end of section = {2}, loop = {3}", fullSatelliteNameLength, pointer, endOfSection, i));
        }
        string fullSatelliteName = DecodeMultilingualText(section, pointer + fullSatelliteNameLength, ref pointer);

        Log.Log.Debug("NTT: satellite text, satellite ID = {0}, reference name = {1}, full name = {2}", satelliteId, satelliteReferenceName, fullSatelliteName);

        // table descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at satellite text table descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected satellite text table descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: satellite text table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid satellite text table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d));
          }
          pointer += length;
        }
      }
    }

    private void DecodeRatingsText(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at ratings text, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte ratingRegion = section[pointer++];
      for (byte i = 0; i < 6; i++)
      {
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: corruption detected at ratings text table levels defined, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
        }
        byte levelsDefined = section[pointer++];
        if (levelsDefined > 0)
        {
          if (pointer >= endOfSection)
          {
            throw new Exception(string.Format("NTT: corruption detected at ratings text table dimension name length, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
          }
          byte dimensionNameLength = section[pointer++];
          if (pointer + dimensionNameLength > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid ratings text table dimension name length {0}, pointer = {1}, end of section = {2}, loop = {3}", dimensionNameLength, pointer, endOfSection, i));
          }
          string dimensionName = DecodeMultilingualText(section, pointer + dimensionNameLength, ref pointer);
          Log.Log.Debug("NTT: ratings text, dimension name = {0}, levels defined = {1}", dimensionName, levelsDefined);
          for (byte l = 0; l < levelsDefined; l++)
          {
            byte ratingNameLength = section[pointer++];
            if (pointer + ratingNameLength > endOfSection)
            {
              throw new Exception(string.Format("NTT: invalid ratings text table rating name length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", ratingNameLength, pointer, endOfSection, i, l));
            }
            string ratingName = DecodeMultilingualText(section, pointer + ratingNameLength, ref pointer);
            Log.Log.Debug("NTT: rating name = {0}", ratingName);
          }
        }
      }
    }

    private void DecodeRatingSystem(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at rating system, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte regionsDefined = section[pointer++];
      for (byte i = 0; i < regionsDefined; i++)
      {
        if (pointer + 3 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected rating system table regions defined {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", regionsDefined, pointer, endOfSection, i));
        }
        byte dataLength = section[pointer++];
        int endOfData = pointer + dataLength;
        if (endOfData > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid rating system table data length {0}, pointer = {1}, end of section = {2}, loop = {3}", dataLength, pointer, endOfSection, i));
        }
        byte ratingRegion = section[pointer++];
        byte stringLength = section[pointer++];
        if (pointer + stringLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid rating system table string length {0}, pointer = {1}, end of section = {2}, loop = {3}", stringLength, pointer, endOfSection, i));
        }
        string ratingSystem = DecodeMultilingualText(section, pointer + stringLength, ref pointer);
        Log.Log.Debug("NTT: rating system, region = {0}, system = {1}", ratingRegion, ratingSystem);

        // table descriptors
        while (pointer + 1 < endOfData)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: rating system table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid rating system table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}", length, pointer, endOfSection, i));
          }
          pointer += length;
        }
        if (pointer != endOfData)
        {
          throw new Exception(string.Format("NTT: corruption detected at end of rating system data, pointer = {0}, end of section = {1}, end of data = {2}, loop = {3}", pointer, endOfSection, endOfData, i));
        }
      }
    }

    private void DecodeCurrencySystem(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at currency system, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte regionsDefined = section[pointer++];
      for (byte i = 0; i < regionsDefined; i++)
      {
        if (pointer + 3 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected currency system table regions defined {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", regionsDefined, pointer, endOfSection, i));
        }
        byte dataLength = section[pointer++];
        int endOfData = pointer + dataLength;
        if (endOfData > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid currency system table data length {0}, pointer = {1}, end of section = {2}, loop = {3}", dataLength, pointer, endOfSection, i));
        }
        byte currencyRegion = section[pointer++];
        byte stringLength = section[pointer++];
        if (pointer + stringLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid currency system table string length {0}, pointer = {1}, end of section = {2}, loop = {3}", stringLength, pointer, endOfSection, i));
        }
        string currencySystem = DecodeMultilingualText(section, pointer + stringLength, ref pointer);
        Log.Log.Debug("NTT: currency system, region = {0}, system = {1}", currencyRegion, currencySystem);

        // table descriptors
        while (pointer + 1 < endOfData)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: currency system table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid currency system table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}", length, pointer, endOfSection, i));
          }
          pointer += length;
        }
        if (pointer != endOfData)
        {
          throw new Exception(string.Format("NTT: corruption detected at end of currency system data, pointer = {0}, end of section = {1}, end of data = {2}, loop = {3}", pointer, endOfSection, endOfData, i));
        }
      }
    }

    private void DecodeSourceName(byte[] section, int endOfSection, ref int pointer, AtscTransmissionMedium transmissionMedium)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at source name, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte numberOfSntRecords = section[pointer++];
      for (byte i = 0; i < numberOfSntRecords; i++)
      {
        if (pointer + 5 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected source name table number of SNT records {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numberOfSntRecords, pointer, endOfSection, i));
        }
        bool applicationType = ((section[pointer++] & 0x80) != 0);
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        byte nameLength = section[pointer++];
        if (pointer + nameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid source name table string length {0}, pointer = {1}, end of section = {2}, loop = {3}", nameLength, pointer, endOfSection, i));
        }
        string sourceName = DecodeMultilingualText(section, pointer + nameLength, ref pointer);
        Log.Log.Debug("NTT: source name, source ID = 0x{0:x}, name = {1}, application type = {2}", sourceId, sourceName, applicationType);
        if (OnSourceName != null)
        {
          OnSourceName(transmissionMedium, applicationType, sourceId, sourceName);
        }

        // table descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at source name table descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected source name table descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: source name table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid source name table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d));
          }
          pointer += length;
        }
      }
    }

    private void DecodeMapName(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at map name, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte numberOfMntRecords = section[pointer++];
      for (byte i = 0; i < numberOfMntRecords; i++)
      {
        if (pointer + 4 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected map name table number of MNT records {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numberOfMntRecords, pointer, endOfSection, i));
        }
        int vctId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        byte mapNameLength = section[pointer++];
        if (pointer + mapNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid map name table map name length {0}, pointer = {1}, end of section = {2}, loop = {3}", mapNameLength, pointer, endOfSection, i));
        }
        string mapName = DecodeMultilingualText(section, pointer + mapNameLength, ref pointer);
        Log.Log.Debug("NTT: map name, VCT ID = 0x{0:x}, name = {1}", vctId, mapName);

        // table descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at map name table descriptor count, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected map name table descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: map name table descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid map name table descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d));
          }
          pointer += length;
        }
      }
    }

    private string DecodeMultilingualText(byte[] section, int endOfString, ref int pointer)
    {
      string text = string.Empty;
      while (pointer + 1 < endOfString)
      {
        byte mode = section[pointer++];
        byte segmentLength = section[pointer++];
        if (pointer + segmentLength > endOfString)
        {
          throw new Exception(string.Format("NTT: invalid multilingual text segment length {0}, pointer = {1}, end of string = {2}", segmentLength, pointer, endOfString));
        }

        if (mode == 0)
        {
          // We only support ASCII encoding at this time.
          text += System.Text.Encoding.ASCII.GetString(section, pointer, segmentLength);
        }
        else
        {
          Log.Log.Debug("NTT: unsupported multilingual text segment mode 0x{0:x}", mode);
          DVB_MMI.DumpBinary(section, pointer, segmentLength);
        }
        pointer += segmentLength;
      }
      if (pointer != endOfString)
      {
        throw new Exception(string.Format("NTT: corruption detected at end of multilingual string, pointer = {0}, end of string = {2}", pointer, endOfString));
      }
      return text;
    }
  }
}
