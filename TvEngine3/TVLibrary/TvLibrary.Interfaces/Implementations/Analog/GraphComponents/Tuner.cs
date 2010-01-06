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
using System.Xml;

namespace TvLibrary.Implementations.Analog.GraphComponents
{

  #region enum

  /// <summary>
  /// Enumeration of all possible radio modes of a tuner device
  /// </summary>
  [Flags]
  public enum RadioMode
  {
    /// <summary>
    /// No radio support
    /// </summary>
    None,
    /// <summary>
    /// Tuner supports FM radio
    /// </summary>
    FM,
    /// <summary>
    /// Tuner supports AM radio
    /// </summary>
    AM
  }

  #endregion

  /// <summary>
  /// Bean class for a tuner in the analog graph
  /// </summary>
  public class Tuner
  {
    #region variables

    /// <summary>
    /// Name of the tuner device
    /// </summary>
    private string _name;

    /// <summary>
    /// Index of the video output pin
    /// </summary>
    private int _videoPin;

    /// <summary>
    /// Index of the audio output pin
    /// </summary>
    private int _audioPin;

    /// <summary>
    /// The supported radio mode
    /// </summary>
    private RadioMode _radioMode;

    #endregion

    #region cotr

    private Tuner() {}

    #endregion

    #region Static CreateInstance method

    /// <summary>
    /// Creates the instance by parsing the Tuner node in the configuration file
    /// </summary>
    /// <param name="xmlNode">The Tuner xml node</param>
    /// <returns>Tuner instance</returns>
    public static Tuner CreateInstance(XmlNode xmlNode)
    {
      Tuner tuner = new Tuner();
      if (xmlNode != null)
      {
        XmlNode nameNode = xmlNode.SelectSingleNode("name");
        XmlNode videoPinNode = xmlNode.SelectSingleNode("videoPin");
        XmlNode audioPinNode = xmlNode.SelectSingleNode("audioPin");
        XmlNode radioModeNode = xmlNode.SelectSingleNode("radioMode");
        try
        {
          tuner.VideoPin = Int32.Parse(videoPinNode.InnerText);
          tuner.AudioPin = Int32.Parse(audioPinNode.InnerText);
          tuner.RadioMode = (RadioMode)Int32.Parse(radioModeNode.InnerText);
        }
        catch
        {
          tuner.RadioMode = RadioMode.None;
          return tuner;
        }
        tuner.Name = nameNode.InnerText;
      }
      return tuner;
    }

    #endregion

    #region WriteGraph method

    /// <summary>
    /// Writes the tuner part of the graph to the configuration
    /// </summary>
    /// <param name="writer">Writer</param>
    public void WriteGraph(XmlWriter writer)
    {
      writer.WriteStartElement("tuner"); //<tuner>
      writer.WriteElementString("name", _name ?? "");
      writer.WriteElementString("videoPin", _videoPin.ToString());
      writer.WriteElementString("audioPin", _audioPin.ToString());
      writer.WriteElementString("radioMode", ((Int32)_radioMode).ToString());
      writer.WriteEndElement(); //</tuner>
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
    /// Index of the video pin
    /// </summary>
    public int VideoPin
    {
      get { return _videoPin; }
      set { _videoPin = value; }
    }

    /// <summary>
    /// Index of the audio pin
    /// </summary>
    public int AudioPin
    {
      get { return _audioPin; }
      set { _audioPin = value; }
    }

    /// <summary>
    /// Flags of the supported radio modes
    /// </summary>
    public RadioMode RadioMode
    {
      get { return _radioMode; }
      set { _radioMode = value; }
    }

    #endregion
  }
}