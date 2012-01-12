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
using System.Xml;
using System.IO;
using TvLibrary.Log;

namespace MediaPortal.Playlists
{
  public class PlayListWPLIO : IPlayListIO
  {
    public bool Load(PlayList playlist, string playlistFileName)
    {
      playlist.Clear();

      try
      {
        var doc = new XmlDocument();
        doc.Load(playlistFileName);
        if (doc.DocumentElement == null)
          return false;
        XmlNode nodeRoot = doc.DocumentElement.SelectSingleNode("/smil/body/seq");
        if (nodeRoot == null)
          return false;
        XmlNodeList nodeEntries = nodeRoot.SelectNodes("media");
        if (nodeEntries != null)
          foreach (XmlNode node in nodeEntries)
          {
            XmlNode srcNode = node.Attributes.GetNamedItem("src");
            if (srcNode != null)
            {
              if (srcNode.InnerText != null)
              {
                if (srcNode.InnerText.Length > 0)
                {
                  var playlistUrl = srcNode.InnerText;
                  var newItem = new PlayListItem(playlistUrl, playlistUrl, 0)
                                  {
                                    Type = PlayListItem.PlayListItemType.Audio
                                  };
                  string description = Path.GetFileName(playlistUrl);
                  newItem.Description = description;
                  playlist.Add(newItem);
                }
              }
            }
          }
        return true;
      }
      catch (Exception e)
      {
        Log.Error(e.StackTrace);
      }
      return false;
    }

    public void Save(PlayList playListParam, string fileName)
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}