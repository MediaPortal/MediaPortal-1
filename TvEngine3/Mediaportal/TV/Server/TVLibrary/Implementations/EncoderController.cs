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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog.QualityControl
{
  /// <summary>
  /// Base classes for the ICodecAPI, IEncoderAPI and IVideoEncoder interfaces
  /// </summary>
  public abstract class BaseControl : IQuality
  {


    #region variables

    /// <summary>
    /// Current Quality type
    /// </summary>
    protected QualityType _qualityType;

    /// <summary>
    /// Indicates if the encoder supports to set a bit rate mode
    /// </summary>
    protected bool _supported_BitRateMode;

    /// <summary>
    /// Indicates if the encoder supports to set a bit rate
    /// </summary>
    protected bool _supported_BitRate;

    /// <summary>
    /// Indicates if the encoder supports to set the peak bit rate mode
    /// </summary>
    protected bool _supported_PeakBitRate;

    /// <summary>
    /// The current bit rate mode
    /// </summary>
    protected VIDEOENCODER_BITRATE_MODE _bitRateMode;

    #endregion

    #region IQuality Members

    /// <summary>
    /// use this method to set the quality of a recording
    /// </summary>
    public QualityType QualityType
    {
      get { return _qualityType; }
      set
      {
        if (_supported_BitRate)
        {
          _qualityType = value;
          ApplyQualityBitRate();
        }
      }
    }

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRateModes()
    {
      return _supported_BitRateMode;
    }

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode()
    {
      return _supported_PeakBitRate;
    }

    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRate()
    {
      return _supported_BitRate;
    }

    /// <summary>
    /// Gets/Sets the bit rate mode. Works only if this is supported
    /// </summary>
    public VIDEOENCODER_BITRATE_MODE BitRateMode
    {
      get { return _bitRateMode; }
      set
      {
        if (_supported_BitRateMode)
        {
          _bitRateMode = value;
          ApplyQualityBitRateMode();
        }
      }
    }

    #endregion

    #region private methods

    /// <summary>
    /// Calculate the bitrate for the specified quality percentage
    /// </summary>
    private static int CalcQualityBitrate(double quality, int valMin, int valMax, int valStepDelta)
    {
      if (quality > 100)
        quality = 100;
      if (quality < 0)
        quality = 0;

      if (quality == 100)
        return valMax;
      if (quality == 0)
        return valMin;
      int delta = valMax - valMin;
      int targetquality = valMin + (int)(delta * quality / 100);

      int newQuality = valMin;
      while (newQuality < targetquality)
        newQuality += valStepDelta;
      return newQuality;
    }

    /// <summary>
    /// Set the quality
    /// </summary>
    public void ApplyQuality()
    {
      ApplyQualityBitRateMode();
      ApplyQualityBitRate();
    }

    /// <summary>
    /// Set the quality (bit rate mode)
    /// </summary>
    private void ApplyQualityBitRateMode()
    {
      try
      {
        // Set new bit rate mode
        if (_supported_BitRateMode)
        {
          this.LogInfo("analog: Encoder mode setting to {0}", _bitRateMode);
          int newMode = (int)_bitRateMode;
          object newBitRateModeO = newMode;
          Marshal.WriteInt32(newBitRateModeO, 0, newMode);
          if (SetValue(PropSetID.ENCAPIPARAM_BitRateMode, ref newBitRateModeO))
          {
            this.LogInfo("analog: Encoder mode set to {0}", _bitRateMode);
          }
          else
          {
            this.LogDebug("analog: Encoder mode setTo FAILresult");
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "analog: BitRate Mode ERROR");
      }
    }

    /// <summary>
    /// Set the quality (bit rte)
    /// </summary>
    private void ApplyQualityBitRate()
    {
      try
      {
        if (_supported_BitRate)
        {
          this.LogInfo("analog: Encoder BitRate setting to {0}", _qualityType);
          object valueMin, valueMax, steppingDelta;
          if (GetValueRange(PropSetID.ENCAPIPARAM_BitRate, out valueMin, out valueMax, out steppingDelta))
          {
            int valMin = Marshal.ReadInt32(valueMin, 0);
            int valMax = Marshal.ReadInt32(valueMax, 0);
            int valStepDelta = Marshal.ReadInt32(steppingDelta, 0);

            this.LogInfo("analog: Encoder BitRate Min {0:D} Max {1:D} Delta {2:D}", valMin, valMax, valStepDelta);

            int newBitrate;

            int qualityToSet = SettingsManagement.GetValue("tuner" + _cardId + "QualityCustomValue", 50);
            this.LogInfo("analog: Encoder custom quality:{0}", qualityToSet);
            newBitrate = CalcQualityBitrate(qualityToSet, valMin, valMax, valStepDelta);

            object newQualityO = valueMin;
            Marshal.WriteInt32(newQualityO, 0, newBitrate);
            if (SetValue(PropSetID.ENCAPIPARAM_BitRate, ref newQualityO))
            {
              this.LogInfo("analog: Encoder BitRate set to {0:D}", newQualityO);
            }
            else
            {
              this.LogDebug("analog: Range SetEncoder(BitRate) FAILresult");
            }
          }
          else
          {
            this.LogDebug("analog: Range GetParameterRange(BitRate) FAILresult");
          }

          if (_bitRateMode == VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak && _supported_PeakBitRate)
          {
            if (GetValueRange(PropSetID.ENCAPIPARAM_PeakBitRate, out valueMin, out valueMax, out steppingDelta))
            {
              int valMin = Marshal.ReadInt32(valueMin, 0);
              int valMax = Marshal.ReadInt32(valueMax, 0);
              int valStepDelta = Marshal.ReadInt32(steppingDelta, 0);

              this.LogInfo("analog: Encoder BitRatePeak Min {0:D} Max {1:D} Delta {2:D}", valMin, valMax, valStepDelta);

              int newBitrate;

              int qualityToSet = SettingsManagement.GetValue("tuner" + _cardId + "QualityCustomPeakValue", 75);
              this.LogInfo("analog: Encoder custom quality:{0}", qualityToSet);
              newBitrate = CalcQualityBitrate(qualityToSet, valMin, valMax, valStepDelta);

              object newQualityO = valueMin;
              Marshal.WriteInt32(newQualityO, 0, newBitrate);
              if (SetValue(PropSetID.ENCAPIPARAM_PeakBitRate, ref newQualityO))
              {
                this.LogInfo("analog: Encoder BitRatePeak setTo {0:D}", newQualityO);
              }
              else
              {
                this.LogDebug("analog: Range SetEncoder(BitRatePeak) FAILresult");
              }
            }
            else
            {
              this.LogDebug("analog: Range GetParameterRange(BitRatePeak) FAILresult");
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "analog: BitRate ERROR");
      }
    }

    #endregion

    #region protected methods

    /// <summary>
    /// Checks the capabilities of the given encoder
    /// </summary>
    protected void CheckCapabilities()
    {
      try
      {
        // Can we set the encoding mode?
        //ENCAPIPARAM_BITRATE_MODE 	Specifies the bit-rate mode, as a VIDEOENCODER_BITRATE_MODE enumeration value (32-bit signed long).
        _supported_BitRateMode = IsSupported(PropSetID.ENCAPIPARAM_BitRateMode);
        if (_supported_BitRateMode)
          this.LogDebug("analog: Encoder supports ENCAPIPARAM_BitRateMode");

        // Can we specify the bitrate?
        //ENCAPIPARAM_BITRATE 	Specifies the bit rate, in bits per second. In constant bit rate (CBR) mode, the value gives the constant bitrate. In either variable bit rate mode, it gives the average bit rate. The value is a 32-bit unsigned long.
        _supported_BitRate = IsSupported(PropSetID.ENCAPIPARAM_BitRate);
        if (_supported_BitRate)
          this.LogDebug("analog: Encoder supports ENCAPIPARAM_BitRate");

        // Can we specify the peak bitrate for variable bit rate peak
        //ENCAPIPARAM_PEAK_BITRATE 	Secifies the peak bit rate. This parameter is relevant only when ENCAPIPARAM_BITRATE_MODE has been set to VariableBitRatePeak.
        _supported_PeakBitRate = IsSupported(PropSetID.ENCAPIPARAM_PeakBitRate);
        if (_supported_PeakBitRate)
          this.LogDebug("analog: Encoder supports ENCAPIPARAM_PeakBitRate");
      }
      catch (Exception e)
      {
        this.LogError(e, "analog: Encoder CheckCapabilities");
      }
    }

    #endregion
  }
}