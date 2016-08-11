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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A base class for WDM analog DirectShow graph components.
  /// </summary>
  internal class BaseComponent
  {
    #region constants

    public const int PIN_INDEX_NOT_SET = -1;

    private static readonly IDictionary<PinType, IList<AMMediaType>> MEDIA_TYPES = new Dictionary<PinType, IList<AMMediaType>>
    {
      {
        PinType.Capture,
        new List<AMMediaType>
        {
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Transport },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Program },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1System }
        }
      },
      {
        PinType.Video,
        new List<AMMediaType>
        {
          new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.MPEG1Payload },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1Video },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Video },
          new AMMediaType() { majorType = MediaType.Video, subType = MediaSubType.Null }
        }
      },
      {
        PinType.Audio,
        new List<AMMediaType>
        {
          new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.MPEG1Payload },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1Audio },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.MPEG1AudioPayload },
          new AMMediaType() { majorType = MediaType.Null, subType = MediaSubType.Mpeg2Audio },
          new AMMediaType() { majorType = MediaType.Audio, subType = MediaSubType.Null }
        }
      },
      {
        PinType.VerticalBlankingInterval,
        new List<AMMediaType> { new AMMediaType() { majorType = MediaType.VBI, subType = TveGuid.KS_DATA_FORMAT_SUB_TYPE_RAW_8 } }
      },
      {
        PinType.ClosedCaptions,
        new List<AMMediaType> { new AMMediaType() { majorType = MediaType.AuxLine21Data, subType = MediaSubType.Line21_BytePair } }
      },
      {
        PinType.Teletext,
        new List<AMMediaType> { new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.TELETEXT } }
      },
      {
        PinType.VideoProgrammingSystem,
        new List<AMMediaType> { new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.VPS } }
      },
      {
        PinType.WideScreenSignalling,
        new List<AMMediaType> { new AMMediaType() { majorType = MediaType.VBI, subType = MediaSubType.WSS } }
      }
    };

    #endregion

    #region enums

    [Flags]
    protected enum PinType
    {
      Unknown = 0,
      Capture = 0x0001,
      Video = 0x0002,
      Audio = 0x0004,
      VerticalBlankingInterval = 0x0008,
      RadioDataSystem = 0x0010,
      ClosedCaptions = 0x0020,
      Teletext = 0x0040,
      VideoProgrammingSystem = 0x0080,
      WideScreenSignalling = 0x0100,
      Max = 0x0200,

      AnyVbiSubType = ClosedCaptions | Teletext | VideoProgrammingSystem | WideScreenSignalling
    }

    #endregion

    /// <summary>
    /// Find one or more particular output pins on a filter.
    /// </summary>
    /// <param name="filterDescription">A description of the filter. Only used for logging.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="pinTypes">The types of pins that the caller wants to find.</param>
    /// <param name="pins">The pin(s) that were found.</param>
    /// <param name="isVideoOutputCapture"><c>True</c> if the video output is a capture output.</param>
    /// <param name="isAudioOutputCapture"><c>True</c> if the audio output is a capture output.</param>
    protected void FindOutputPins(string filterDescription, IBaseFilter filter, PinType pinTypes, IDictionary<PinType, IPin> pins, ref bool isVideoOutputCapture, ref bool isAudioOutputCapture)
    {
      this.LogDebug("WDM analog component: find {0} output pins, pin types = [{1}]", filterDescription, pinTypes);

      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator for {0} filter.", filterDescription);
      try
      {
        // For each pin...
        int pinIndex = 0;
        int pinCount = 0;
        IPin[] tempPins = new IPin[2];
        while (pinEnum.Next(1, tempPins, out pinCount) == (int)NativeMethods.HResult.S_OK && pinCount == 1)
        {
          bool matched = false;
          bool releasePin = true;
          IPin pin = tempPins[0];
          try
          {
            PinInfo pinInfo;
            hr = pin.QueryPinInfo(out pinInfo);
            TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin information for {0} filter pin {1}.", filterDescription, pinIndex);

            Release.PinInfo(ref pinInfo);
            if (pinInfo.dir != PinDirection.Output)
            {
              pinIndex--;     // compensate for the undesirable increment in the finally clause
              continue;
            }

            IEnumMediaTypes mediaTypeEnum;
            hr = pin.EnumMediaTypes(out mediaTypeEnum);
            TvExceptionDirectShowError.Throw(hr, "Failed to obtain media type enumerator for {0} filter pin {1}.", filterDescription, pinIndex);
            try
            {
              // For each pin media type...
              int mediaTypeCount;
              AMMediaType[] mediaTypes = new AMMediaType[2];
              while (mediaTypeEnum.Next(1, mediaTypes, out mediaTypeCount) == (int)NativeMethods.HResult.S_OK && mediaTypeCount == 1)
              {
                AMMediaType mediaType = mediaTypes[0];
                try
                {
                  PinType pinType = (PinType)1;
                  bool isCaptureOutput = false;
                  while (pinType != PinType.Max)
                  {
                    foreach (AMMediaType matchMediaType in MEDIA_TYPES[pinType])
                    {
                      if (
                        (matchMediaType.majorType == Guid.Empty || matchMediaType.majorType == mediaType.majorType) &&
                        (matchMediaType.subType == Guid.Empty || matchMediaType.subType == mediaType.subType)
                      )
                      {
                        matched = true;
                        isCaptureOutput = (pinType == PinType.Video || pinType == PinType.Audio) && matchMediaType.subType != MediaSubType.Null;
                        break;
                      }
                    }

                    if (matched)
                    {
                      this.LogDebug("WDM analog component:   {0} pin, index = {1}, name = {2}, major type = {3}, sub-type = {4}, is configurable = {5}", pinType, pinIndex, pinInfo.name, mediaType.majorType, mediaType.subType, pin is IAMStreamConfig);
                      if (pinTypes.HasFlag(pinType))
                      {
                        IPin tempPin;
                        if (!pins.TryGetValue(pinType, out tempPin))
                        {
                          releasePin = false;
                        }
                        else if (pinType == PinType.Video && isCaptureOutput && !isVideoOutputCapture)
                        {
                          releasePin = false;
                          Release.ComObject(string.Format("WDM analog component {0} filter video output pin", filterDescription), ref tempPin);
                         }
                        else if (pinType == PinType.Audio && isCaptureOutput && !isAudioOutputCapture)
                        {
                          releasePin = false;
                          Release.ComObject(string.Format("WDM analog component {0} filter audio output pin", filterDescription), ref tempPin);
                        }
                        if (!releasePin)
                        {
                          pins[pinType] = pin;
                          if (pinType == PinType.Video)
                          {
                            isVideoOutputCapture = isCaptureOutput;
                          }
                          else if (pinType == PinType.Audio)
                          {
                            isAudioOutputCapture = isCaptureOutput;
                          }
                        }
                      }
                      break;
                    }

                    pinType = (PinType)((int)pinType << 1);
                  }

                  if (matched)
                  {
                    break;
                  }
                }
                finally
                {
                  Release.AmMediaType(ref mediaType);
                }
              }
            }
            finally
            {
              Release.ComObject(string.Format("WDM analog component {0} filter pin media type enumerator", filterDescription), ref mediaTypeEnum);
            }

            if (!matched)
            {
              this.LogDebug("WDM analog component:   unknown pin, index = {0}, name = {1}", pinIndex, pinInfo.name);
            }
          }
          finally
          {
            if (releasePin)
            {
              Release.ComObject(string.Format("WDM analog component {0} filter pin", filterDescription), ref pin);
            }
            pinIndex++;
          }
        }
      }
      finally
      {
        Release.ComObject(string.Format("WDM analog component {0} filter pin enumerator", filterDescription), ref pinEnum);
      }
    }
  }
}