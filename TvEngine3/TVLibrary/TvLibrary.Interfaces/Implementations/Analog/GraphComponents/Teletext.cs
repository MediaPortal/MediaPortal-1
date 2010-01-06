#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Xml;

namespace TvLibrary.Implementations.Analog.GraphComponents
{
  /// <summary>
  /// Bean class for a Teletext in the analog graph
  /// </summary>
  public class Teletext
  {
    #region variables

    /// <summary>
    /// Name of the teletext device
    /// </summary>
    private string _name;

    /// <summary>
    /// GUID of the category of the teletext device
    /// </summary>
    private Guid _category;

    #endregion

    #region ctor

    private Teletext() {}

    #endregion

    #region Static CreateInstance method

    /// <summary>
    /// Creates the instance by parsing the Teletext node in the configuration file
    /// </summary>
    /// <param name="xmlNode">The Teletext xml node</param>
    /// <returns>Teletext instance</returns>
    public static Teletext CreateInstance(XmlNode xmlNode)
    {
      Teletext teletext = new Teletext();
      if (xmlNode != null)
      {
        XmlNode nameNode = xmlNode.SelectSingleNode("name");
        XmlNode categoryNode = xmlNode.SelectSingleNode("category");
        try
        {
          teletext.Category = new Guid(
            categoryNode.InnerText);
        }
        catch
        {
          return teletext;
        }
        teletext.Name = nameNode.InnerText;
      }
      return teletext;
    }

    #endregion

    #region WriteGraph method

    /// <summary>
    /// Writes the Teletext part of the graph to the configuration
    /// </summary>
    /// <param name="writer">Writer</param>
    public void WriteGraph(XmlWriter writer)
    {
      writer.WriteStartElement("teletext"); //<teletext>
      writer.WriteElementString("name", _name ?? "");
      writer.WriteElementString("category", _category.ToString());
      writer.WriteEndElement(); //</teletext>
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
    /// Category of the filter
    /// </summary>
    public Guid Category
    {
      get { return _category; }
      set { _category = value; }
    }

    #endregion
  }
}