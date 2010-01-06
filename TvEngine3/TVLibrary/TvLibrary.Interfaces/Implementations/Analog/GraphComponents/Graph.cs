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

using System.Xml;

namespace TvLibrary.Implementations.Analog.GraphComponents
{
  /// <summary>
  /// An analog graph
  /// </summary>
  public class Graph
  {
    #region variables

    /// <summary>
    /// The Tuner component
    /// </summary>
    private Tuner _tuner;

    /// <summary>
    /// The TvAudio component
    /// </summary>
    private TvAudio _tvAudio;

    /// <summary>
    /// The Crossbar component
    /// </summary>
    private Crossbar _crossbar;

    /// <summary>
    /// The Capture component
    /// </summary>
    private Capture _capture;

    /// <summary>
    /// The Teletext component
    /// </summary>
    private Teletext _teletext;

    #endregion

    #region ctor

    private Graph() {}

    #endregion

    #region Static CreateInstance method

    /// <summary>
    /// Creates the Graph instance which represents an analog graph
    /// </summary>
    /// <param name="xmlNode">The graph xml node</param>
    /// <returns>Graph instance</returns>
    public static Graph CreateInstance(XmlNode xmlNode)
    {
      Graph graph = new Graph();
      XmlNode tunerNode = null;
      XmlNode tvAudioNode = null;
      XmlNode crossbarNode = null;
      XmlNode captureNode = null;
      XmlNode teletextNode = null;
      if (xmlNode != null)
      {
        tunerNode = xmlNode.SelectSingleNode("tuner");
        tvAudioNode = xmlNode.SelectSingleNode("tvAudio");
        crossbarNode = xmlNode.SelectSingleNode("crossbar");
        captureNode = xmlNode.SelectSingleNode("capture");
        teletextNode = xmlNode.SelectSingleNode("teletext");
      }
      graph.Tuner = Tuner.CreateInstance(tunerNode);
      graph.TvAudio = TvAudio.CreateInstance(tvAudioNode);
      graph.Crossbar = Crossbar.CreateInstance(crossbarNode);
      graph.Capture = Capture.CreateInstance(captureNode);
      graph.Teletext = Teletext.CreateInstance(teletextNode);
      return graph;
    }

    #endregion

    #region WRiteGraph method

    /// <summary>
    /// Writes the graph to the configuration
    /// </summary>
    /// <param name="writer">Writer</param>
    public void WriteGraph(XmlWriter writer)
    {
      writer.WriteStartElement("graph"); //<graph>
      _tuner.WriteGraph(writer);
      _tvAudio.WriteGraph(writer);
      _crossbar.WriteGraph(writer);
      _capture.WriteGraph(writer);
      _teletext.WriteGraph(writer);
      writer.WriteEndElement(); //</graph>
    }

    #endregion

    #region Properties

    /// <summary>
    /// The tuner part of the graph
    /// </summary>
    public Tuner Tuner
    {
      get { return _tuner; }
      set { _tuner = value; }
    }

    /// <summary>
    /// The TvAudio part of the graph
    /// </summary>
    public TvAudio TvAudio
    {
      get { return _tvAudio; }
      set { _tvAudio = value; }
    }

    /// <summary>
    /// The Crossbar part of the graph
    /// </summary>
    public Crossbar Crossbar
    {
      get { return _crossbar; }
      set { _crossbar = value; }
    }

    /// <summary>
    /// The Capture part of the graph
    /// </summary>
    public Capture Capture
    {
      get { return _capture; }
      set { _capture = value; }
    }

    /// <summary>
    /// The Teletext part of the graph
    /// </summary>
    public Teletext Teletext
    {
      get { return _teletext; }
      set { _teletext = value; }
    }

    #endregion
  }
}