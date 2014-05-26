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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  internal class EncoderController : IQuality
  {
    #region variables

    private IList<IEncoder> _encoders = new List<IEncoder>();
    private QualityType _bitRateProfile = QualityType.Custom;
    private VIDEOENCODER_BITRATE_MODE _bitRateMode = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;

    #endregion

    public EncoderController(IList<ICustomDevice> tunerExtensions)
    {
      foreach (ICustomDevice extension in tunerExtensions)
      {
        IEncoder encoder = extension as IEncoder;
        if (encoder != null)
        {
          _encoders.Add(encoder);
        }
      }
    }

    #region IQuality Members

    public bool SupportsBitRateModes()
    {
      // TODO the IQuality interface is annoying. It needs a complete redo... but that would be a
      // lot of work due to the configuration and plugin user interfaces. We'll come back to this
      // later.
      return true;
    }

    public bool SupportsPeakBitRateMode()
    {
      // TODO
      return true;
    }

    public bool SupportsBitRate()
    {
      // TODO
      return true;
    }

    public QualityType QualityType
    {
      get
      {
        return _bitRateProfile;
      }
      set
      {
        bool success = false;
        switch (value)
        {
          case QualityType.Custom:
            // TODO read from database
            //customValue.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _tunerId + "CustomBitRate", 50);
            success = SetParameterByRange(PropSetID.ENCAPIPARAM_BitRate, 50);
            break;
          case QualityType.Portable:
            success = SetParameterByRange(PropSetID.ENCAPIPARAM_BitRate, 20);
            break;
          case QualityType.Low:
            success = SetParameterByRange(PropSetID.ENCAPIPARAM_BitRate, 33);
            break;
          case QualityType.Medium:
            success = SetParameterByRange(PropSetID.ENCAPIPARAM_BitRate, 66);
            break;
          case QualityType.High:
            success = SetParameterByRange(PropSetID.ENCAPIPARAM_BitRate, 100);
            break;
          default:
            success = SetParameterByDefault(PropSetID.ENCAPIPARAM_BitRate);
            break;
        }
        if (_bitRateMode == VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak)
        {
          switch (value)
          {
            case QualityType.Custom:
              // TODO read from database
              //customValuePeak.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tuner" + _tunerId + "CustomPeakBitRate", 75);
              success |= SetParameterByRange(PropSetID.ENCAPIPARAM_PeakBitRate, 75);
              break;
            case QualityType.Portable:
              success |= SetParameterByRange(PropSetID.ENCAPIPARAM_PeakBitRate, 45);
              break;
            case QualityType.Low:
              success |= SetParameterByRange(PropSetID.ENCAPIPARAM_PeakBitRate, 55);
              break;
            case QualityType.Medium:
              success |= SetParameterByRange(PropSetID.ENCAPIPARAM_PeakBitRate, 88);
              break;
            case QualityType.High:
              success |= SetParameterByRange(PropSetID.ENCAPIPARAM_PeakBitRate, 100);
              break;
            default:
              success |= SetParameterByDefault(PropSetID.ENCAPIPARAM_PeakBitRate);
              break;
          }
        }
        if (success)
        {
          _bitRateProfile = value;
        }
      }
    }

    public VIDEOENCODER_BITRATE_MODE BitRateMode
    {
      get
      {
        return _bitRateMode;
      }
      set
      {
        int newMode = (int)value;
        object newModeObj = newMode;
        Marshal.WriteInt32(newModeObj, 0, newMode);
        if (SetParameterByValues(PropSetID.ENCAPIPARAM_BitRateMode, newModeObj))
        {
          _bitRateMode = value;
        }
      }
    }

    #endregion

    private bool SetParameterByValues(Guid parameter, object value)
    {
      this.LogDebug("encoder: set parameter {0} to {1}", parameter, value);
      bool success = false;
      foreach (IEncoder encoder in _encoders)
      {
        this.LogDebug("  try encoder {0}", encoder.Name);
        if (!encoder.IsParameterSupported(parameter))
        {
          this.LogDebug("    parameter not supported");
          continue;
        }

        bool isValueSupported = false;
        object[] supportedValues;
        if (encoder.GetParameterValues(parameter, out supportedValues))
        {
          foreach (object val in supportedValues)
          {
            if (val == value)
            {
              this.LogDebug("    value supported");
              isValueSupported = true;
              break;
            }
          }
        }
        else
        {
          this.LogDebug("    assume value supported");
          isValueSupported = true;  // assume => force try
        }

        if (isValueSupported)
        {
          if (encoder.SetParameterValue(parameter, value))
          {
            this.LogDebug("    success!");
            success = true;
          }
          else
          {
            this.LogWarn("encoder: failed to set parameter {0} to {1} for encoder {2}", parameter, value, encoder.Name);
          }
        }
        else
        {
          this.LogDebug("    value not supported");
        }
      }
      return success;
    }

    private bool SetParameterByRange(Guid parameter, int valuePercent)
    {
      this.LogDebug("encoder: set parameter {0} to {1}%", parameter, valuePercent);
      bool success = false;
      foreach (IEncoder encoder in _encoders)
      {
        this.LogDebug("  try encoder {0}", encoder.Name);
        if (!encoder.IsParameterSupported(parameter))
        {
          this.LogDebug("    parameter not supported");
          continue;
        }

        object minimum;
        object maximum;
        object resolution;
        if (!encoder.GetParameterRange(parameter, out minimum, out maximum, out resolution))
        {
          this.LogWarn("encoder: failed to get parameter {0} range for encoder {1}", parameter, encoder.Name);
          continue;
        }

        // TODO non-int type parameters not supported, is this a problem?
        int minimumValue = Marshal.ReadInt32(minimum, 0);
        int maximumValue = Marshal.ReadInt32(maximum, 0);
        int resolutionValue = Marshal.ReadInt32(resolution, 0);
        this.LogDebug("    range, minimum = {0}, maximum = {1}, resolution = {2}", minimumValue, maximumValue, resolutionValue);

        int value = minimumValue;
        int rawValue = minimumValue;
        if (valuePercent <= 0)
        {
          value = minimumValue;
          rawValue = minimumValue;
        }
        else if (valuePercent >= 100)
        {
          value = maximumValue;
          rawValue = maximumValue;
        }
        else
        {
          rawValue = minimumValue + (value * (maximumValue - minimumValue) / 100);
          int currentQuanta = minimumValue;
          while (rawValue > currentQuanta)
          {
            currentQuanta += resolutionValue;
          }
          int lowerQuanta = currentQuanta - resolutionValue;
          if ((rawValue - lowerQuanta) < (currentQuanta - lowerQuanta))
          {
            value = lowerQuanta;
          }
          else
          {
            if (currentQuanta > maximumValue)
            {
              value = maximumValue;
            }
            else
            {
              value = currentQuanta;
            }
          }
        }
        this.LogDebug("    raw value = {0}, quantised value = {1}", rawValue, value);
        object valueObj = value;
        Marshal.WriteInt32(valueObj, 0, value);
        if (encoder.SetParameterValue(parameter, valueObj))
        {
          this.LogDebug("    success!");
          success = true;
        }
        else
        {
          this.LogWarn("encoder: failed to set parameter {0} to {1} for encoder {2}", parameter, value, encoder.Name);
        }
      }
      return success;
    }

    private bool SetParameterByDefault(Guid parameter)
    {
      this.LogDebug("encoder: set parameter {0} to default", parameter);
      bool success = false;
      foreach (IEncoder encoder in _encoders)
      {
        this.LogDebug("  try encoder {0}", encoder.Name);
        if (!encoder.IsParameterSupported(parameter))
        {
          this.LogDebug("    parameter not supported");
          continue;
        }

        object defaultValue;
        if (!encoder.GetParameterDefaultValue(parameter, out defaultValue))
        {
          this.LogWarn("encoder: failed to get parameter {0} default value for encoder {1}", parameter, encoder.Name);
          continue;
        }

        this.LogDebug("    default value = {0}", defaultValue);
        if (encoder.SetParameterValue(parameter, defaultValue))
        {
          this.LogDebug("    success!");
          success = true;
        }
        else
        {
          this.LogWarn("encoder: failed to set parameter {0} to default ({1}) for encoder {2}", parameter, defaultValue, encoder.Name);
        }
      }
      return success;
    }
  }
}