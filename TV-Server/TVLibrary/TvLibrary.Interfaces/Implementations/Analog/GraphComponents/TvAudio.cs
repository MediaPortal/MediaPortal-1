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
using DirectShowLib;

namespace TvLibrary.Implementations.Analog.GraphComponents
{
  #region enum
  /// <summary>
  /// Enumeration for all possible variants of TvAudio devices
  /// </summary>
  public enum TvAudioVariant
  {
    /// <summary>
    /// Standard TvAudio device
    /// </summary>
    Normal,
    /// <summary>
    /// The tuner device is only connected to the Crossbar
    /// </summary>
    TvTunerConnection,
    /// <summary>
    /// The tuner device is also the TvAudio device
    /// </summary>
    TvTuner,
    /// <summary>
    /// No TvAudio device is available
    /// </summary>
    Unavailable
  }
  #endregion

  /// <summary>
  /// Bean class for a TvAudio in the analog graph
  /// </summary>
  public class TvAudio
  {
    #region variables
    /// <summary>
    /// The current TvAudio variant of the found filter
    /// </summary>
    private TvAudioVariant _mode;
    /// <summary>
    /// The name of the TvAudio device
    /// </summary>
    private string _name;
    /// <summary>
    /// The bitmask of the supported audio mode
    /// </summary>
    private TVAudioMode _audioModes;
    #endregion

    #region ctor
    /// <summary>
    /// Private constructor
    /// </summary>
    private TvAudio()
    {
    }
    #endregion

    #region Static CreateInstance method
    /// <summary>
    /// Creates the instance by parsing the TvAudio node in the configuration file
    /// </summary>
    /// <param name="xmlNode">The TvAudio xml node</param>
    /// <returns>TvAudio instance</returns>
    public static TvAudio CreateInstance(XmlNode xmlNode)
    {
      TvAudio tvAudio = new TvAudio();
      if (xmlNode != null)
      {
        XmlNode modeNode = xmlNode.SelectSingleNode("mode");
        TvAudioVariant mode;
        try
        {
          mode = (TvAudioVariant) Int32.Parse(modeNode.InnerText);
        }
        catch
        {
          tvAudio.Mode = TvAudioVariant.Unavailable;
          return tvAudio;
        }
        if (mode == TvAudioVariant.Normal)
        {
          XmlNode nameNode = xmlNode.SelectSingleNode("name");
          XmlNode audioModeNode = xmlNode.SelectSingleNode("audioModes");
          tvAudio.Name = nameNode.InnerText;
          try
          {
            tvAudio.AudioModes = (TVAudioMode) Int32.Parse(audioModeNode.InnerText);
          }
          catch
          {
            return tvAudio;
          }
        }
      }
      return tvAudio;
    }
    #endregion

    #region WriteGraph method
    /// <summary>
    /// Writes the TvAudio part of the graph to the configuration
    /// </summary>
    /// <param name="writer">Writer</param>
    public void WriteGraph(XmlWriter writer)
    {
      writer.WriteStartElement("tvAudio");//<tvAudio>
      writer.WriteElementString("mode", ((Int32)_mode).ToString());
      if (_mode == TvAudioVariant.Normal)
      {
        writer.WriteElementString("name", _name ?? "");
        writer.WriteElementString("audioModes", ((Int32)_audioModes).ToString());
      }
      writer.WriteEndElement();//</tvAudio>
    }
    #endregion

    #region Properties
    /// <summary>
    /// Mode of the TvAudio device
    /// </summary>
    public TvAudioVariant Mode
    {
      get { return _mode; }
      set { _mode = value; }
    }

    /// <summary>
    /// Name of the tuner device
    /// </summary>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// Flags of the supported radio modes
    /// </summary>
    public TVAudioMode AudioModes
    {
      get { return _audioModes; }
      set { _audioModes = value; }
    }
    #endregion
  }
}
