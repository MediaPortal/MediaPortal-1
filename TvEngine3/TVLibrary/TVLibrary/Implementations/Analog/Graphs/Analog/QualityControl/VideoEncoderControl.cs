using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog.QualityControl
{
  public class VideoEncoderControl : IQuality
  {
    private IVideoEncoder _videoEncoder;
    private QualityType _qualityType;
    private bool _supported_BitRateMode;
    private bool _supported_BitRate;
    private bool _supported_PeakBitRate;
    private VIDEOENCODER_BITRATE_MODE _bitRateMode;
    private Configuration _configuration;

    public VideoEncoderControl(Configuration configuration,IVideoEncoder videoEncoder)
    {
      _configuration = configuration;
      _videoEncoder = videoEncoder;
      CheckCapabilities();
    }

    #region IQuality Members

    /// <summary>
    /// use this method to set the quality of a recording
    /// </summary>
    public QualityType QualityType
    {
      get
      {
        return _qualityType;
      }
      set
      {
        if (_supported_BitRate)
        {
          _qualityType = value;
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
      get
      {
        return _bitRateMode;
      }
      set
      {
        if (_supported_BitRateMode)
        {
          _bitRateMode = value;
          ApplyQuality();
        }
      }
    }

    /// <summary>
    /// Called when playback starts
    /// </summary>
    public void StartPlayback()
    {
      _bitRateMode = _configuration.PlaybackQualityMode;
      _qualityType = _configuration.PlaybackQualityType;
      ApplyQuality();
    }

    /// <summary>
    /// Called when record starts
    /// </summary>
    public void StartRecord()
    {
      _bitRateMode = _configuration.RecordQualityMode;
      _qualityType = _configuration.RecordQualityType;
      ApplyQuality();
    }

    /// <summary>
    /// Sets the new configuration object
    /// </summary>
    public void SetConfiguration(Configuration configuration)
    {
      _configuration = configuration;
    }

    #endregion

    private void CheckCapabilities()
    {
      try
      {
        int hr;
        Log.Log.WriteFile("analog: IVideoEncoder supported by: " + FilterGraphTools.GetFilterName(_videoEncoder as IBaseFilter) + "; Checking capabilities ");

        // Can we set the encoding mode?
        //ENCAPIPARAM_BITRATE_MODE 	Specifies the bit-rate mode, as a VIDEOENCODER_BITRATE_MODE enumeration value (32-bit signed long).
        hr = _videoEncoder.IsSupported(PropSetID.ENCAPIPARAM_BitRateMode);
        _supported_BitRateMode = hr == 0;
        Log.Log.WriteFile("analog: IVideoEncoder supports ENCAPIPARAM_BitRateMode");

        // Can we specify the bitrate?
        //ENCAPIPARAM_BITRATE 	Specifies the bit rate, in bits per second. In constant bit rate (CBR) mode, the value gives the constant bitrate. In either variable bit rate mode, it gives the average bit rate. The value is a 32-bit unsigned long.
        hr = _videoEncoder.IsSupported(PropSetID.ENCAPIPARAM_BitRate);
        _supported_BitRate = hr == 0;
        Log.Log.WriteFile("analog: IVideoEncoder supports ENCAPIPARAM_BitRate");

        // Can we specify the peak bitrate for variable bit rate peak
        //ENCAPIPARAM_PEAK_BITRATE 	Secifies the peak bit rate. This parameter is relevant only when ENCAPIPARAM_BITRATE_MODE has been set to VariableBitRatePeak.
        hr = _videoEncoder.IsSupported(PropSetID.ENCAPIPARAM_PeakBitRate);
        _supported_PeakBitRate = hr == 0;
        Log.Log.WriteFile("analog: IVideoEncoder supports ENCAPIPARAM_PeakBitRate");
      } catch (Exception e)
      {
        Log.Log.WriteFile("analog: IVideEncoder CheckCapabilities", e);
        Log.Log.Write(e);
      }
    }
    /// <summary>
    /// Calculate the bitrate for the specified quality percentage
    /// </summary>
    private int CalcQualityBitrate(double quality, int valMin, int valMax, int valStepDelta)
    {
      if (quality > 100) quality = 100;
      if (quality < 0) quality = 0;

      if (quality == 100)
        return valMax;
      else if (quality == 0)
        return valMin;
      else
      {
        int delta = valMax - valMin;
        int targetquality = valMin + (int)(delta * quality / 100);

        int newQuality = valMin;
        while (newQuality < targetquality)
          newQuality += valStepDelta;
        return newQuality;
      };
    }

    /// <summary>
    /// Set the quality
    /// </summary>
    private void ApplyQuality()
    {
      try
      {
        int hr;
        // Set new bit rate mode
        if (_supported_BitRateMode)
        {
          int newMode = (int)_bitRateMode;
          object newBitRateModeO = newMode;
          Marshal.WriteInt32(newBitRateModeO, 0, newMode);
          hr = _videoEncoder.SetValue(PropSetID.ENCAPIPARAM_BitRateMode, ref newBitRateModeO);
          if (hr == 0)
          {
            Log.Log.WriteFile("analog: Encoder mode setTo {0}", _bitRateMode);
          }
        }

        if (_supported_BitRate)
        {

          object valueMin, valueMax, steppingDelta;
          hr = _videoEncoder.GetParameterRange(PropSetID.ENCAPIPARAM_BitRate, out valueMin, out valueMax, out steppingDelta);
          if (hr == 0)
          {
            int valMin = Marshal.ReadInt32(valueMin, 0);
            int valMax = Marshal.ReadInt32(valueMax, 0);
            int valStepDelta = Marshal.ReadInt32(steppingDelta, 0);

            Log.Log.WriteFile("analog: Encoder BitRate Min {0:D} Max {1:D} Delta {2:D}", valMin, valMax, valStepDelta);

            Int32 newBitrate = 50;

            switch (_qualityType)
            {
              case QualityType.Custom:
                int qualityToSet;
                TvBusinessLayer layer = new TvBusinessLayer();
                String defaultRecordingQualityStr = layer.GetSetting("custom", "50").Value;
                Int32.TryParse(defaultRecordingQualityStr, out qualityToSet);
                newBitrate = CalcQualityBitrate(qualityToSet, valMin, valMax, valStepDelta);
                break;
              case QualityType.Portable:
                newBitrate = CalcQualityBitrate(20, valMin, valMax, valStepDelta);
                break;
              case QualityType.Low:
                newBitrate = CalcQualityBitrate(33, valMin, valMax, valStepDelta);
                break;
              case QualityType.Medium:
                newBitrate = CalcQualityBitrate(66, valMin, valMax, valStepDelta);
                break;
              case QualityType.High:
                newBitrate = CalcQualityBitrate(100, valMin, valMax, valStepDelta);
                break;
              case QualityType.Default:
                object qualityObject = null;
                _videoEncoder.GetDefaultValue(PropSetID.ENCAPIPARAM_BitRate, out qualityObject);
                newBitrate = Marshal.ReadInt32(qualityObject, 0);
                break;
              default:
                qualityToSet = 50;
                break;
            }

            object newQualityO = valueMin;
            Marshal.WriteInt32(newQualityO, 0, newBitrate);
            hr = _videoEncoder.SetValue(PropSetID.ENCAPIPARAM_BitRate, ref newQualityO);
            if (hr == 0)
            {
              Log.Log.WriteFile("analog: Encoder BitRate setTo {0:D}", newQualityO);
            } else
            {
              Log.Log.WriteFile("analog: Range SetEncoder(BitRate) result: 0x{0:x}", hr);
            }
          } else
          {
            Log.Log.WriteFile("analog: Range GetParameterRange(BitRate) result: 0x{0:x}", hr);
          }

          if (_bitRateMode == VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak && _supported_PeakBitRate)
          {
            hr = _videoEncoder.GetParameterRange(PropSetID.ENCAPIPARAM_PeakBitRate, out valueMin, out valueMax, out steppingDelta);
            if (hr == 0)
            {
              int valMin = Marshal.ReadInt32(valueMin, 0);
              int valMax = Marshal.ReadInt32(valueMax, 0);
              int valStepDelta = Marshal.ReadInt32(steppingDelta, 0);

              Log.Log.WriteFile("analog: Encoder BitRatePeak Min {0:D} Max {1:D} Delta {2:D}", valMin, valMax, valStepDelta);

              Int32 newBitrate = 75;

              switch (_qualityType)
              {
                case QualityType.Custom:
                  int qualityToSet;
                  TvBusinessLayer layer = new TvBusinessLayer();
                  String defaultRecordingQualityStr = layer.GetSetting("customPeak", "75").Value;
                  Int32.TryParse(defaultRecordingQualityStr, out qualityToSet);
                  newBitrate = CalcQualityBitrate(qualityToSet, valMin, valMax, valStepDelta);
                  break;
                case QualityType.Portable:
                  newBitrate = CalcQualityBitrate(45, valMin, valMax, valStepDelta);
                  break;
                case QualityType.Low:
                  newBitrate = CalcQualityBitrate(55, valMin, valMax, valStepDelta);
                  break;
                case QualityType.Medium:
                  newBitrate = CalcQualityBitrate(88, valMin, valMax, valStepDelta);
                  break;
                case QualityType.High:
                  newBitrate = CalcQualityBitrate(100, valMin, valMax, valStepDelta);
                  break;
                case QualityType.Default:
                  object qualityObject = null;
                  _videoEncoder.GetDefaultValue(PropSetID.ENCAPIPARAM_PeakBitRate, out qualityObject);
                  newBitrate = Marshal.ReadInt32(qualityObject, 0);
                  break;
                default:
                  qualityToSet = 50;
                  break;
              }

              object newQualityO = valueMin;
              Marshal.WriteInt32(newQualityO, 0, newBitrate);
              hr = _videoEncoder.SetValue(PropSetID.ENCAPIPARAM_PeakBitRate, ref newQualityO);
              if (hr == 0)
              {
                Log.Log.WriteFile("analog: Encoder BitRatePeak setTo {0:D}", newQualityO);
              } else
              {
                Log.Log.WriteFile("analog: Range SetEncoder(BitRatePeak) result: 0x{0:x}", hr);
              }
            } else
            {
              Log.Log.WriteFile("analog: Range GetParameterRange(BitRatePeak) result: 0x{0:x}", hr);
            }

          }
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile("analog: SetupCaptureFormat ERROR: " + ex.Message);
      }
    }


  }
}
