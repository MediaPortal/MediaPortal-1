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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  internal class EncoderController : IQualityControlInternal
  {
    private class Settings
    {
      public EncodeMode EncodeMode = EncodeMode.Default;
      public int BitRateAverage = DEFAULT_BIT_RATE;
      public int BitRatePeak = DEFAULT_BIT_RATE;
    }

    private const int DEFAULT_BIT_RATE = -1;

    #region variables

    private IList<IEncoder> _encoders = new List<IEncoder>();

    private object _lock = new object();
    private int _countTimeShifters = 0;
    private int _countRecorders = 0;

    private EncodeMode _supportedEncodeModes = EncodeMode.Default;
    private bool _canSetBitRate = false;

    private bool _isCustomSettings = false;
    private Settings _settingsCurrent = new Settings();
    private Settings _settingsTimeShift = new Settings();
    private Settings _settingsRecord = new Settings();

    #endregion

    public EncoderController(IList<ITunerExtension> tunerExtensions)
    {
      foreach (ITunerExtension extension in tunerExtensions)
      {
        IEncoder encoder = extension as IEncoder;
        if (encoder != null)
        {
          _encoders.Add(encoder);
          if (encoder.IsParameterSupported(PropSetID.ENCAPIPARAM_BitRateMode))
          {
            object[] supportedValues;
            if (encoder.GetParameterValues(PropSetID.ENCAPIPARAM_BitRateMode, out supportedValues))
            {
              foreach (object value in supportedValues)
              {
                EncodeMode mode = (EncodeMode)value;
                if (mode != EncodeMode.VariablePeakBitRate || encoder.IsParameterSupported(PropSetID.ENCAPIPARAM_PeakBitRate))
                {
                  _supportedEncodeModes |= mode;
                }
              }
            }
          }
          if (!_canSetBitRate && encoder.IsParameterSupported(PropSetID.ENCAPIPARAM_BitRate))
          {
            _canSetBitRate = true;
          }
        }
      }

      if (_encoders.Count > 0)
      {
        this.LogInfo("encoder: supported features...");
        this.LogInfo("  supported encode modes = [{0}]", _supportedEncodeModes);
        this.LogInfo("  can set bit-rate?      = {0}", _canSetBitRate);
      }
    }

    #region IQualityControl members

    /// <summary>
    /// Determine which (if any) quality control features are supported by the tuner.
    /// </summary>
    /// <param name="supportedEncodeModes">The encoding modes supported by the tuner.</param>
    /// <param name="canSetBitRate"><c>True</c> if the tuner's encoding bit-rate can be set.</param>
    public void GetSupportedFeatures(out EncodeMode supportedEncodeModes, out bool canSetBitRate)
    {
      supportedEncodeModes = _supportedEncodeModes;
      canSetBitRate = _canSetBitRate;
    }

    /// <summary>
    /// Get and/or set the tuner's video and/or audio encoding mode.
    /// </summary>
    public EncodeMode EncodeMode
    {
      get
      {
        return _settingsCurrent.EncodeMode;
      }
      set
      {
        if (!_supportedEncodeModes.HasFlag(value) || _settingsCurrent.EncodeMode == value)
        {
          return;
        }

        if (value == EncodeMode.Default)
        {
          if (SetParameterByDefault(PropSetID.ENCAPIPARAM_BitRateMode))
          {
            _settingsCurrent.EncodeMode = value;
            _isCustomSettings = true;
          }
          return;
        }

        VideoEncoderBitrateMode mode = VideoEncoderBitrateMode.ConstantBitRate;
        if (value == EncodeMode.VariableBitRate)
        {
          mode = VideoEncoderBitrateMode.VariableBitRateAverage;
        }
        else if (value == EncodeMode.VariablePeakBitRate)
        {
          mode = VideoEncoderBitrateMode.VariableBitRatePeak;
        }
        int modeInt = (int)mode;
        if (SetParameterByValues(PropSetID.ENCAPIPARAM_BitRateMode, modeInt))
        {
          _settingsCurrent.EncodeMode = value;
          _isCustomSettings = true;
        }
      }
    }

    /// <summary>
    /// Get and/or set the tuner's average video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    public int AverageBitRate
    {
      get
      {
        return _settingsCurrent.BitRateAverage;
      }
      set
      {
        if (!_canSetBitRate)
        {
          return;
        }
        if (_settingsCurrent.BitRateAverage == value)
        {
          return;
        }
        bool success = false;
        if (value == DEFAULT_BIT_RATE)
        {
          success = SetParameterByDefault(PropSetID.ENCAPIPARAM_BitRate);
        }
        else
        {
          success = SetParameterByRange(PropSetID.ENCAPIPARAM_BitRate, value, typeof(uint));
        }
        if (success)
        {
          _settingsCurrent.BitRateAverage = value;
          _isCustomSettings = true;
        }
      }
    }

    /// <summary>
    /// Get and/or set the tuner's peak video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    public int PeakBitRate
    {
      get
      {
        return _settingsCurrent.BitRatePeak;
      }
      set
      {
        if (!_supportedEncodeModes.HasFlag(EncodeMode.VariablePeakBitRate))
        {
          return;
        }
        if (_settingsCurrent.BitRatePeak == value)
        {
          return;
        }
        bool success = false;
        if (value == DEFAULT_BIT_RATE)
        {
          success = SetParameterByDefault(PropSetID.ENCAPIPARAM_PeakBitRate);
        }
        else
        {
          success = SetParameterByRange(PropSetID.ENCAPIPARAM_PeakBitRate, value, typeof(uint));
        }
        if (success)
        {
          _settingsCurrent.BitRatePeak = value;
          _isCustomSettings = true;
        }
      }
    }

    #endregion

    #region IQualityControlInternal members

    /// <summary>
    /// Reload the control's configuration.
    /// </summary>
    /// <param name="configuration">The configuration of the associated tuner.</param>
    public void ReloadConfiguration(Tuner configuration)
    {
      if (_encoders.Count == 0)
      {
        return;
      }

      this.LogDebug("encoder: reload configuration");
      if (configuration.AnalogTunerSettings == null)
      {
        _settingsTimeShift.EncodeMode = EncodeMode.ConstantBitRate;
        _settingsTimeShift.BitRateAverage = 100;
        _settingsTimeShift.BitRatePeak = 100;
        _settingsRecord.EncodeMode = EncodeMode.ConstantBitRate;
        _settingsRecord.BitRateAverage = 100;
        _settingsRecord.BitRatePeak = 100;
      }
      else
      {
        _settingsTimeShift.EncodeMode = (EncodeMode)configuration.AnalogTunerSettings.EncoderBitRateModeTimeShifting;
        _settingsTimeShift.BitRateAverage = configuration.AnalogTunerSettings.EncoderBitRateTimeShifting;
        _settingsTimeShift.BitRatePeak = configuration.AnalogTunerSettings.EncoderBitRatePeakTimeShifting;
        _settingsRecord.EncodeMode = (EncodeMode)configuration.AnalogTunerSettings.EncoderBitRateModeRecording;
        _settingsRecord.BitRateAverage = configuration.AnalogTunerSettings.EncoderBitRateRecording;
        _settingsRecord.BitRatePeak = configuration.AnalogTunerSettings.EncoderBitRatePeakRecording;
      }

      this.LogDebug("  time-shifting...");
      this.LogDebug("    encode mode      = {0}", _settingsTimeShift.EncodeMode);
      this.LogDebug("    bit-rate average = {0} %", _settingsTimeShift.BitRateAverage);
      this.LogDebug("    bit-rate peak    = {0} %", _settingsTimeShift.BitRatePeak);
      this.LogDebug("  recording...");
      this.LogDebug("    encode mode      = {0}", _settingsRecord.EncodeMode);
      this.LogDebug("    bit-rate average = {0} %", _settingsRecord.BitRateAverage);
      this.LogDebug("    bit-rate peak    = {0} %", _settingsRecord.BitRatePeak);

      if (_isCustomSettings)
      {
        return;
      }

      if (_countRecorders > 0)
      {
        this.LogInfo("encoder: update recording settings");
        EncodeMode = _settingsRecord.EncodeMode;
        AverageBitRate = _settingsRecord.BitRateAverage;
        PeakBitRate = _settingsRecord.BitRatePeak;
        _isCustomSettings = false;
      }
      else if (_countTimeShifters > 0)
      {
        this.LogInfo("encoder: update time-shifting settings");
        EncodeMode = _settingsTimeShift.EncodeMode;
        AverageBitRate = _settingsTimeShift.BitRateAverage;
        PeakBitRate = _settingsTimeShift.BitRatePeak;
        _isCustomSettings = false;
      }
    }

    /// <summary>
    /// Notify the control that time-shifting has started.
    /// </summary>
    public void OnStartTimeShifting()
    {
      if (_encoders.Count == 0)
      {
        return;
      }
      lock (_lock)
      {
        if (_countTimeShifters++ == 0 && _countRecorders == 0)
        {
          this.LogInfo("encoder: apply time-shifting settings");
          EncodeMode = _settingsTimeShift.EncodeMode;
          AverageBitRate = _settingsTimeShift.BitRateAverage;
          PeakBitRate = _settingsTimeShift.BitRatePeak;
          _isCustomSettings = false;
        }
      }
    }

    /// <summary>
    /// Notify the control that time-shifting has stopped.
    /// </summary>
    public void OnStopTimeShifting()
    {
      if (_encoders.Count == 0)
      {
        return;
      }
      lock (_lock)
      {
        if (_countTimeShifters-- == 1 && _countRecorders == 0)
        {
          _isCustomSettings = false;
        }
      }
    }

    /// <summary>
    /// Notify the control that recording has started.
    /// </summary>
    public void OnStartRecording()
    {
      if (_encoders.Count == 0)
      {
        return;
      }
      lock (_lock)
      {
        if (_countRecorders++ == 0)
        {
          this.LogInfo("encoder: apply recording settings");
          // We intentionally override time-shifting settings, including custom
          // time-shift settings.
          EncodeMode = _settingsRecord.EncodeMode;
          AverageBitRate = _settingsRecord.BitRateAverage;
          PeakBitRate = _settingsRecord.BitRatePeak;
          _isCustomSettings = false;
        }
      }
    }

    /// <summary>
    /// Notify the control that recording has stopped.
    /// </summary>
    public void OnStopRecording()
    {
      if (_encoders.Count == 0)
      {
        return;
      }
      lock (_lock)
      {
        if (_countRecorders-- == 1)
        {
          if (_countTimeShifters == 0)
          {
            _isCustomSettings = false;
          }
          else if (!_isCustomSettings)
          {
            this.LogInfo("encoder: apply time-shifting settings");
            EncodeMode = _settingsTimeShift.EncodeMode;
            AverageBitRate = _settingsTimeShift.BitRateAverage;
            PeakBitRate = _settingsTimeShift.BitRatePeak;
            _isCustomSettings = false;
          }
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

    private bool SetParameterByRange(Guid parameter, int valuePercentage, Type valueType)
    {
      this.LogDebug("encoder: set parameter {0} to {1}%", parameter, valuePercentage);
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

        this.LogDebug("    range, minimum = {0}, maximum = {1}, resolution = {2}", minimum, maximum, resolution);

        // It's difficult to do calculations at run-time with dynamically typed
        // objects. Convert to the widest numeric type for the calculation
        // step, convert the result to the target type, and hope it all works!
        object value = minimum;
        decimal unquantisedValue;
        if (valuePercentage <= 0)
        {
          value = minimum;
          unquantisedValue = Convert.ToDecimal(minimum);
        }
        else if (valuePercentage >= 100)
        {
          value = maximum;
          unquantisedValue = Convert.ToDecimal(maximum);
        }
        else
        {
          // Calculate the value.
          decimal minimumDecimal = Convert.ToDecimal(minimum);
          decimal maximumDecimal = Convert.ToDecimal(maximum);
          decimal resolutionDecimal = Convert.ToDecimal(resolution);
          unquantisedValue = minimumDecimal + (valuePercentage * (maximumDecimal - minimumDecimal) / 100);

          // Quantise the calculated value.
          decimal quantisedValueUpper = minimumDecimal;
          while (unquantisedValue > quantisedValueUpper)
          {
            quantisedValueUpper += resolutionDecimal;
          }
          decimal quantisedValueLower = quantisedValueUpper - resolutionDecimal;
          if ((unquantisedValue - quantisedValueLower) < (quantisedValueUpper - quantisedValueLower))
          {
            value = Convert.ChangeType(quantisedValueLower, valueType);
          }
          else if (quantisedValueUpper > maximumDecimal)
          {
            value = maximum;
          }
          else
          {
            value = Convert.ChangeType(quantisedValueUpper, valueType);
          }
        }
        this.LogDebug("    unquantised value = {0}, quantised value = {1}", unquantisedValue, value);

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