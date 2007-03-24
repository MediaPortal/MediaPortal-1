using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace MyTv
{
  class UserSettings
  {
    static XmlDocument _settings;
    static UserSettings()
    {
      _settings = new XmlDocument();
      if (File.Exists("settings.xml"))
      {
        _settings.Load("settings.xml");
      }
      else
      {
        XmlNode node = _settings.CreateElement("root");
        _settings.AppendChild(node);
      }
    }

    static public string GetString(string topic, string tag)
    {
      XmlNode node = AddTagNode(topic, tag);
      return (string)(node.InnerText);
    }
    static public void SetString(string topic, string tag, string tagValue)
    {
      XmlNode nodeTag = AddTagNode(topic, tag);
      nodeTag.InnerText = tagValue;
      SaveSettings();
    }

    static XmlNode AddTopicNode(string topic)
    {
      XmlNode node = _settings.DocumentElement.SelectSingleNode(String.Format("/root/{0}", topic));
      if (node != null) return node;
      XmlNode nodeRoot = _settings.DocumentElement.SelectSingleNode("/root");
      node = _settings.CreateElement(topic);
      nodeRoot.AppendChild(node);
      return node;
    }

    static XmlNode AddTagNode(string topic, string tag)
    {
      XmlNode node = _settings.DocumentElement.SelectSingleNode(String.Format("/root/{0}/{1}", topic, tag));
      if (node != null) return node;
      XmlNode nodeTopic = AddTopicNode(topic);
      node = _settings.CreateElement(tag);
      node.InnerText = "";
      nodeTopic.AppendChild(node);
      return node;
    }
    static void SaveSettings()
    {
      if (File.Exists("settings.xml"))
        File.Delete("settings.xml");

      _settings.Save("settings.xml");
    }
  }
}
