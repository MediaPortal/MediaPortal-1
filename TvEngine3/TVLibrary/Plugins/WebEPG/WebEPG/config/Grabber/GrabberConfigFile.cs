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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace MediaPortal.WebEPG.Config.Grabber
{
  /// <summary>
  /// The Grabber Config information.
  /// </summary>
  [XmlRoot("Grabber")]
  public class GrabberConfigFile
  {
    #region Variables

    [XmlElement("Info")] public GrabberInfo Info;
    [XmlArray("Channels")] [XmlArrayItem("Channel")] public List<ChannelInfo> Channels;
    [XmlElement("Listing")] public ListingInfo Listing;
    [XmlArray("Actions")] [XmlArrayItem("Modify")] public List<ModifyInfo> Actions;

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the site id for a channel.
    /// </summary>
    /// <param name="id">The site id.</param>
    /// <returns></returns>
    public string GetChannel(string id)
    {
      for (int i = 0; i < Channels.Count; i++)
      {
        if (Channels[i].id == id)
        {
          return Channels[i].siteId;
        }
      }

      return null;
    }

    #endregion
  }
}