/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using DirectShowLib;

namespace TvLibrary.Implementations.Analog.GraphComponents
{
  #region VideoQuality class
  /// <summary>
  /// Bean class for storing one video quality setting default settings like brightness, gamma etc.
  /// </summary>
  public class VideoQuality
  {
    /// <summary>
    /// The minimum value
    /// </summary>
    private readonly int _minValue;
    /// <summary>
    /// The maximum value
    /// </summary>
    private readonly int _maxValue;
    /// <summary>
    /// The stepping delta
    /// </summary>
    private readonly int _steppingDelta;
    /// <summary>
    /// The default value
    /// </summary>
    private readonly int _defaultValue;
    /// <summary>
    /// Value can be adjusted manualyy
    /// </summary>
    private readonly bool _manual;
    /// <summary>
    /// The current value
    /// </summary>
    private int _value;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="minValue">Minimum value</param>
    /// <param name="maxValue">Maximum value</param>
    /// <param name="steppingDelta">Stepping delta</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="manual">Indicates, if the quality is adjusted manually</param>
    /// <param name="value">The current value</param>
    public VideoQuality(int minValue, int maxValue, int steppingDelta, int defaultValue, bool manual, int value)
    {
      _minValue = minValue;
      _maxValue = maxValue;
      _steppingDelta = steppingDelta;
      _defaultValue = defaultValue;
      _manual = manual;
      _value = value;
    }

    /// <summary>
    /// Gets the minimum value
    /// </summary>
    public int MinValue
    {
      get { return _minValue; }
    }

    /// <summary>
    /// Gets the maximum value
    /// </summary>
    public int MaxValue
    {
      get { return _maxValue; }
    }

    /// <summary>
    /// Gets the stepping delta
    /// </summary>
    public int SteppingDelta
    {
      get { return _steppingDelta; }
    }

    /// <summary>
    /// Gets the default value
    /// </summary>
    public int DefaultValue
    {
      get { return _defaultValue; }
    }

    /// <summary>
    /// Gets if the property is adjusted manually or automatically
    /// </summary>
    public bool IsManual
    {
      get { return _manual; }
    }

    /// <summary>
    /// Gets/Sets the current value
    /// </summary>
    public int Value
    {
      get { return _value; }
      set { _value = value; }
    }

    /// <summary>
    /// Prints the values of this video quality
    /// </summary>
    /// <returns>String</returns>
    public override string ToString()
    {
      return String.Format(
        "Max: {0}, Min {1}, Default: {2}, SteppingDelta: {3}, Manual: {4}", _maxValue, _minValue, _defaultValue,
        _steppingDelta, _manual);
    }
  }
  #endregion

  /// <summary>
  /// Bean class for a Capture in the analog graph
  /// </summary>
  public class Capture
  {
    #region variables
    /// <summary>
    /// Name of the capture file
    /// </summary>
    private string _name;
    /// <summary>
    /// Bitmask of the available video standards
    /// </summary>
    private AnalogVideoStandard _availableVideoStandard;
    /// <summary>
    /// The current video standard
    /// </summary>
    private AnalogVideoStandard _currentVideoStandard;
    /// <summary>
    /// Index of the teletext pin
    /// </summary>
    private int _teletextPin;
    /// <summary>
    /// The current frame rate
    /// </summary>
    private double _frameRate;
    /// <summary>
    /// The current image height
    /// </summary>
    private int _imageHeight;
    /// <summary>
    /// The current image width
    /// </summary>
    private int _imageWidth;
    /// <summary>
    /// Dictionary of the VideoProcAmp values
    /// </summary>
    private Dictionary<VideoProcAmpProperty, VideoQuality> _videoProcAmpValues;
    /// <summary>
    /// Index of the video input pin
    /// </summary>
    private int _videoIn;
    /// <summary>
    /// Index of the audio input pin
    /// </summary>
    private int _audioIn;
    /// <summary>
    /// Name of the optional audio capture device
    /// </summary>
    private string _audioCaptureName;
    #endregion

    #region ctor
    /// <summary>
    /// private constructor
    /// </summary>
    private Capture()
    {
      _frameRate = -1;
      _imageWidth = -1;
      _availableVideoStandard = AnalogVideoStandard.None;
      _currentVideoStandard = AnalogVideoStandard.None;
    }
    #endregion

    #region Static CreateInstance method
    /// <summary>
    /// Creates the instance by parsing the Capture node in the configuration file
    /// </summary>
    /// <param name="xmlNode">The TvAudio xml node</param>
    /// <returns>TvAudio instance</returns>
    public static Capture CreateInstance(XmlNode xmlNode)
    {
      Capture capture = new Capture();
      Dictionary<VideoProcAmpProperty, VideoQuality> videoProcAmpValues = new Dictionary<VideoProcAmpProperty, VideoQuality>();
      capture.VideoProcAmpValues = videoProcAmpValues;
      if (xmlNode != null)
      {
        XmlNode viceoCaptureNode = xmlNode.SelectSingleNode("videoCapture");
        XmlNode nameNode = viceoCaptureNode.SelectSingleNode("name");
        XmlNode videoInNode = viceoCaptureNode.SelectSingleNode("videoIn");
        XmlNode audioInNode = viceoCaptureNode.SelectSingleNode("audioIn");
        XmlNode teletextPinNode = viceoCaptureNode.SelectSingleNode("teletextPin");
        XmlNode frameRateNode = viceoCaptureNode.SelectSingleNode("frameRate");
        XmlNode imageResolutionNode = viceoCaptureNode.SelectSingleNode("imageResolution");
        XmlNode availableVideoStandardNode = viceoCaptureNode.SelectSingleNode("videoStandard/available");
        XmlNode currentVideoStandardNode = viceoCaptureNode.SelectSingleNode("videoStandard/selected");
        string resolution = imageResolutionNode.InnerText;
        try
        {
          capture.TeletextPin = Int32.Parse(teletextPinNode.InnerText);
          capture.FrameRate = Double.Parse(frameRateNode.InnerText, CultureInfo.GetCultureInfo("en-GB").NumberFormat);
          capture.VideoIn = Int32.Parse(videoInNode.InnerText);
          if(audioInNode!=null)
          {
            capture.AudioCaptureName = nameNode.InnerText;
          }else
          {
            XmlNode audioCaptureNode = xmlNode.SelectSingleNode("audioCapture");
            XmlNode audioCaptureNameNode = audioCaptureNode.SelectSingleNode("audioIn");
            audioInNode = audioCaptureNameNode.SelectSingleNode("audioIn");
            capture.AudioCaptureName = audioCaptureNameNode.InnerText;
          }
          capture.AudioIn = Int32.Parse(audioInNode.InnerText);
          if (resolution != null)
          {
            string[] imageResolutions = resolution.Split('x');
            capture.ImageWidth = Int32.Parse(imageResolutions[0]);
            capture.ImageHeight = Int32.Parse(imageResolutions[1]);
          }
          capture.CurrentVideoStandard = (AnalogVideoStandard)Int32.Parse(currentVideoStandardNode.InnerText);
          capture.AvailableVideoStandard = (AnalogVideoStandard)Int32.Parse(availableVideoStandardNode.InnerText);
          XmlNodeList videoQualityList = viceoCaptureNode.SelectSingleNode("videoProcAmp").SelectNodes("videoQuality");
          if (videoQualityList != null)
          {
            foreach (XmlNode pin in videoQualityList)
            {
              int minValue = Int32.Parse(pin.Attributes["minValue"].Value);
              int maxValue = Int32.Parse(pin.Attributes["maxValue"].Value);
              int defaultValue = Int32.Parse(pin.Attributes["defaultValue"].Value);
              int delta = Int32.Parse(pin.Attributes["delta"].Value);
              VideoProcAmpFlags flags = (VideoProcAmpFlags)Int32.Parse(pin.Attributes["flags"].Value);
              int value = Int32.Parse(pin.Attributes["value"].Value);
              VideoProcAmpProperty property = (VideoProcAmpProperty)Int32.Parse(pin.InnerText);
              VideoQuality quality = new VideoQuality(minValue, maxValue, delta, defaultValue,
                                                      flags == VideoProcAmpFlags.Manual, value);
              videoProcAmpValues.Add(property, quality);
            }
          }
        } catch
        {
          return capture;
        }
        capture.Name = nameNode.InnerText;
      }
      return capture;
    }
    #endregion

    #region WriteGraph method
    /// <summary>
    /// Writes the Capture part of the graph to the configuration
    /// </summary>
    /// <param name="writer">Writer</param>
    public void WriteGraph(XmlWriter writer)
    {
      writer.WriteStartElement("capture");//<capture>
      writer.WriteStartElement("videoCapture");//<videoCapture>
      writer.WriteElementString("name", _name ?? "");
      writer.WriteElementString("videoIn",_videoIn.ToString());
      if(_name!=null && _name.Equals(_audioCaptureName))
      {
        writer.WriteElementString("audioIn", _audioIn.ToString());
      }
      writer.WriteStartElement("videoStandard");//<videoStandard>
      writer.WriteElementString("available", ((Int32)_availableVideoStandard).ToString());
      writer.WriteElementString("selected", ((Int32)_currentVideoStandard).ToString());
      writer.WriteEndElement();//</videoStandard>
      writer.WriteStartElement("videoProcAmp");//<videoProcAmp>
      foreach (VideoProcAmpProperty property in _videoProcAmpValues.Keys)
      {
        writer.WriteStartElement("videoQuality");//<videoQuality>
        writer.WriteAttributeString("minValue", _videoProcAmpValues[property].MinValue.ToString());
        writer.WriteAttributeString("maxValue", _videoProcAmpValues[property].MaxValue.ToString());
        writer.WriteAttributeString("defaultValue", _videoProcAmpValues[property].DefaultValue.ToString());
        writer.WriteAttributeString("delta", _videoProcAmpValues[property].SteppingDelta.ToString());
        VideoProcAmpFlags prop = _videoProcAmpValues[property].IsManual
                                   ? VideoProcAmpFlags.Manual
                                   : VideoProcAmpFlags.Auto;
        writer.WriteAttributeString("flags", ((Int32)prop).ToString());
        writer.WriteAttributeString("value", _videoProcAmpValues[property].Value.ToString());
        writer.WriteValue((Int32)property);
        writer.WriteEndElement();//<</videoQuality>
      }
      writer.WriteEndElement();//</videoProcAmp>
      writer.WriteElementString("imageResolution", _imageWidth + "x" + _imageHeight);
      writer.WriteElementString("frameRate", _frameRate.ToString(CultureInfo.GetCultureInfo("en-GB")));
      writer.WriteElementString("teletextPin", _teletextPin.ToString());
      writer.WriteEndElement();//</videoCapture>
      if (_name != null && !_name.Equals(_audioCaptureName))
      {
        writer.WriteStartElement("audioCapture");//<audioCapture>
        writer.WriteElementString("name", _audioCaptureName);
        writer.WriteElementString("audioIn", _audioIn.ToString());
        writer.WriteEndElement();//</audioCapture>
      }
      writer.WriteEndElement();//</capture>
    }
    #endregion

    #region Poperties
    /// <summary>
    /// Name of the tuner device
    /// </summary>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// Map of the available video standard
    /// </summary>
    public AnalogVideoStandard AvailableVideoStandard
    {
      get { return _availableVideoStandard; }
      set { _availableVideoStandard = value; }
    }

    /// <summary>
    /// The current video standard
    /// </summary>
    public AnalogVideoStandard CurrentVideoStandard
    {
      get { return _currentVideoStandard; }
      set { _currentVideoStandard = value; }
    }

    /// <summary>
    /// Map with the VideoProcAmp settings
    /// </summary>
    public Dictionary<VideoProcAmpProperty, VideoQuality> VideoProcAmpValues
    {
      get { return _videoProcAmpValues; }
      set { _videoProcAmpValues = value; }
    }

    /// <summary>
    /// Index of the teletext pin
    /// </summary>
    public int TeletextPin
    {
      get { return _teletextPin; }
      set { _teletextPin = value; }
    }

    /// <summary>
    /// The frame rate
    /// </summary>
    public double FrameRate
    {
      get { return _frameRate; }
      set { _frameRate = value; }
    }

    /// <summary>
    /// The Image height
    /// </summary>
    public int ImageHeight
    {
      get { return _imageHeight; }
      set { _imageHeight = value; }
    }

    /// <summary>
    /// The Image width
    /// </summary>
    public int ImageWidth
    {
      get { return _imageWidth; }
      set { _imageWidth = value; }
    }

    /// <summary>
    /// Index of the video in pin
    /// </summary>
    public int VideoIn
    {
      get { return _videoIn; }
      set { _videoIn = value; }
    }

    /// <summary>
    /// Index of the audio in pin
    /// </summary>
    public int AudioIn
    {
      get { return _audioIn; }
      set { _audioIn = value; }
    }

    /// <summary>
    /// Name of the audio capture device
    /// </summary>
    public string AudioCaptureName
    {
      get { return _audioCaptureName; }
      set { _audioCaptureName = value; }
    }
    #endregion
  }
}
