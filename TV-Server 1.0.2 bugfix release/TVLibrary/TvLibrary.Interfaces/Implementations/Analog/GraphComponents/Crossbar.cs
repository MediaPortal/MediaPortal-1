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
using System.Xml;

namespace TvLibrary.Implementations.Analog.GraphComponents
{
  /// <summary>
  /// Bean class for a Crossbar in the analog graph
  /// </summary>
  public class Crossbar
  {
    #region variables
    /// <summary>
    /// Name of the crossbar device
    /// </summary>
    private string _name;
    /// <summary>
    /// Index of the video output pin
    /// </summary>
    private int _videoOut;
    /// <summary>
    /// Index of the audio output pin
    /// </summary>
    private int _audioOut;
    /// <summary>
    /// Mapping of the available video sources and their pin index
    /// </summary>
    private Dictionary<AnalogChannel.VideoInputType, int> _videoPinMap;
    /// <summary>
    /// Mapping of the available video source to the related audio pin index
    /// </summary>
    private Dictionary<AnalogChannel.VideoInputType, int> _videoPinRelatedAudioMap;
    /// <summary>
    /// Mapping of the available audio source and their pin index
    /// </summary>
    private Dictionary<AnalogChannel.AudioInputType, int> _audioPinMap;
    #endregion

    #region ctor
    /// <summary>
    /// Private constructor
    /// </summary>
    private Crossbar()
    {
    }
    #endregion

    #region Static CreateInstance method
    /// <summary>
    /// Creates the instance by parsing the Crossbar node in the configuration file
    /// </summary>
    /// <param name="xmlNode">The TvAudio xml node</param>
    /// <returns>TvAudio instance</returns>
    public static Crossbar CreateInstance(XmlNode xmlNode)
    {
      Crossbar crossbar = new Crossbar();
      Dictionary<AnalogChannel.VideoInputType, int> videoPinMap = new Dictionary<AnalogChannel.VideoInputType, int>();
      Dictionary<AnalogChannel.VideoInputType, int> videoPinRelatedAudioMap = new Dictionary<AnalogChannel.VideoInputType, int>();
      Dictionary<AnalogChannel.AudioInputType, int> audioPinMap = new Dictionary<AnalogChannel.AudioInputType, int>();
      crossbar.AudioPinMap = audioPinMap;
      crossbar.VideoPinMap = videoPinMap;
      crossbar.VideoPinRelatedAudioMap = videoPinRelatedAudioMap;
      if (xmlNode != null)
      {
        XmlNode nameNode = xmlNode.SelectSingleNode("name");
        XmlNode videoOutNode = xmlNode.SelectSingleNode("videoOut");
        XmlNode audiouOutNode = xmlNode.SelectSingleNode("audioOut");
        try
        {
          crossbar.VideoOut = Int32.Parse(videoOutNode.InnerText);
          crossbar.AudioOut = Int32.Parse(audiouOutNode.InnerText);
          XmlNodeList videoPinList = xmlNode.SelectNodes("videoPin");
          XmlNodeList audioPinList = xmlNode.SelectNodes("audioPin");

          if (videoPinList != null)
          {
            foreach (XmlNode pin in videoPinList)
            {
              AnalogChannel.VideoInputType type =
                (AnalogChannel.VideoInputType)Int32.Parse(pin.Attributes["type"].Value);
              int index = Int32.Parse(pin.Attributes["index"].Value);
              int related = Int32.Parse(pin.Attributes["related"].Value);
              videoPinMap.Add(type, index);
              videoPinRelatedAudioMap.Add(type, related);
            }
          }
          if (audioPinList != null)
          {
            foreach (XmlNode pin in audioPinList)
            {
              AnalogChannel.AudioInputType type =
                (AnalogChannel.AudioInputType)Int32.Parse(pin.Attributes["type"].Value);
              int index = Int32.Parse(pin.Attributes["index"].Value);
              audioPinMap.Add(type, index);
            }
          }
        } catch
        {
          return crossbar;
        }
        crossbar.Name = nameNode.InnerText;
      }
      return crossbar;
    }
    #endregion

    #region WriteGraph method
    /// <summary>
    /// Writes the Crossbar part of the graph to the configuration
    /// </summary>
    /// <param name="writer">Writer</param>
    public void WriteGraph(XmlWriter writer)
    {
      writer.WriteStartElement("crossbar");//<crossbar>
      writer.WriteElementString("name", _name ?? "");
      writer.WriteElementString("videoOut", _videoOut.ToString());
      writer.WriteElementString("audioOut", _audioOut.ToString());
      foreach (AnalogChannel.VideoInputType type in _videoPinMap.Keys)
      {
        writer.WriteStartElement("videoPin");//<videoPin>
        writer.WriteAttributeString("type", ((Int32)type).ToString());
        writer.WriteAttributeString("index", _videoPinMap[type].ToString());
        writer.WriteAttributeString("related", _videoPinRelatedAudioMap[type].ToString());
        writer.WriteEndElement();//<</videoPin>
      }
      foreach (AnalogChannel.AudioInputType type in _audioPinMap.Keys)
      {
        writer.WriteStartElement("audioPin");//<audioPin>
        writer.WriteAttributeString("type", ((Int32)type).ToString());
        writer.WriteAttributeString("index", _audioPinMap[type].ToString());
        writer.WriteEndElement();//</audioPin>
      }
      writer.WriteEndElement();//</crossbar>
    }
    #endregion

    #region Properties
    /// <summary>
    /// Name of the tuner device
    /// </summary>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// Index of the input pin
    /// </summary>
    public int VideoOut
    {
      get { return _videoOut; }
      set { _videoOut = value; }
    }

    /// <summary>
    /// Index of the audio pin
    /// </summary>
    public int AudioOut
    {
      get { return _audioOut; }
      set { _audioOut = value; }
    }

    /// <summary>
    /// Map of the available video input pins
    /// </summary>
    public Dictionary<AnalogChannel.VideoInputType, int> VideoPinMap
    {
      get { return _videoPinMap; }
      set { _videoPinMap = value; }
    }

    /// <summary>
    /// Map of the related audio pins for the available video input pins
    /// </summary>
    public Dictionary<AnalogChannel.VideoInputType, int> VideoPinRelatedAudioMap
    {
      get { return _videoPinRelatedAudioMap; }
      set { _videoPinRelatedAudioMap = value; }
    }

    /// <summary>
    /// Map of the available audio input pins
    /// </summary>
    public Dictionary<AnalogChannel.AudioInputType, int> AudioPinMap
    {
      get { return _audioPinMap; }
      set { _audioPinMap = value; }
    }
    #endregion
  }
}
