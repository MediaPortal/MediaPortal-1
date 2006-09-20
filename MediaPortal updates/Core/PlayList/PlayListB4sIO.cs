using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using System;

namespace MediaPortal.Playlists
{
  public class PlayListB4sIO : IPlayListIO
  {
    private static bool LoadXml(string fileName, out XmlNodeList nodeEntries)
    {
      nodeEntries = null;

      XmlDocument doc = new XmlDocument();
      doc.Load(fileName);
      if (doc.DocumentElement == null) return false;
      string root = doc.DocumentElement.Name;
      if (root != "WinampXML") return false;

      XmlNode nodeRoot = doc.DocumentElement.SelectSingleNode("/WinampXML/playlist");
      if (nodeRoot == null) return false;
      nodeEntries = nodeRoot.SelectNodes("entry");
      return true;
    }

    public bool Load(PlayList playlist, string fileName)
    {
      playlist.Clear();
      XmlNodeList nodeEntries;

      if (!LoadXml(fileName, out nodeEntries))
        return false;

      try
      {
        string basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
        foreach (XmlNode node in nodeEntries)
        {
          string file = ReadFileName(node);

          if (file == null)
            return false;

          string infoLine = ReadInfoLine(node, file);
          int duration = ReadLength(node);

          Utils.GetQualifiedFilename(basePath, ref file);
          PlayListItem newItem = new PlayListItem(infoLine, file, duration);
          playlist.Add(newItem);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("exception loading playlist {0} err:{1} stack:{2}", fileName, ex.Message, ex.StackTrace);
        return false;
      }
    }

    private static int ReadLength(XmlNode node)
    {
      XmlNode nodeLength = node.SelectSingleNode("Length");
      if (nodeLength == null)
        return 0;
      return Int32.Parse(nodeLength.InnerText);
    }

    private static string ReadInfoLine(XmlNode node, string file)
    {
      string infoLine;
      XmlNode nodeName = node.SelectSingleNode("Name");
      if (nodeName == null)
        infoLine = Path.GetFileName(file);
      else
        infoLine = nodeName.InnerText;
      return infoLine;
    }

    private static string ReadFileName(XmlNode node)
    {
      string file = node.Attributes["Playstring"].Value;

      if (file == null)
        return null;

      file = RemovePrefix(file, "file:");
      return file;
    }

    private static string RemovePrefix(string file, string prefix)
    {
      if (file.StartsWith(prefix))
        file = file.Substring(prefix.Length);
      return file;
    }

    public void Save(PlayList playlist, string fileName)
    {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.OmitXmlDeclaration = true;
      using (XmlWriter writer = XmlWriter.Create(fileName, settings))
      {
        writer.WriteStartElement("WinampXML");
        writer.WriteStartElement("playlist");
        writer.WriteAttributeString("num_entries", playlist.Count.ToString());
        writer.WriteAttributeString("label", playlist.Name);

        foreach(PlayListItem item in playlist)
        {
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("Playstring", "file:" + item.FileName);
          writer.WriteElementString("Name", item.Description);
          writer.WriteElementString("Length", item.Duration.ToString());
          writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
      }

    }
  }
}
