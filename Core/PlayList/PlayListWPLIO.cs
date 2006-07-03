using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Playlists
{
  public class PlayListWPLIO : IPlayListIO
  {
    public bool Load(PlayList playlist, string fileName)
    {
      playlist.Clear();

      try
      {
        string basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        if (doc.DocumentElement == null)
          return false;
        XmlNode nodeRoot = doc.DocumentElement.SelectSingleNode("/smil/body/seq");
        if (nodeRoot == null)
          return false;
        XmlNodeList nodeEntries = nodeRoot.SelectNodes("media");
        foreach (XmlNode node in nodeEntries)
        {
          XmlNode srcNode = node.Attributes.GetNamedItem("src");
          if (srcNode != null)
          {
            if (srcNode.InnerText != null)
            {
              if (srcNode.InnerText.Length > 0)
              {
                fileName = srcNode.InnerText;
                Utils.GetQualifiedFilename(basePath, ref fileName);
                PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
                newItem.Type = PlayListItem.PlayListItemType.Audio;
                string description;
                description = Path.GetFileName(fileName);
                newItem.Description = description;
                playlist.Add(newItem);
              }
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("exception loading playlist {0} err:{1} stack:{2}", fileName, ex.Message, ex.StackTrace);
      }
      return false;
    }

    public void Save(PlayList playlist, string fileName)
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}
