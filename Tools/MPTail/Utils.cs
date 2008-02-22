using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;

namespace MPTail
{
  public struct LastSettings
  {
    public int left;
    public int top;
    public int width;
    public int height;
    public int categoryIndex;
    public int tabIndex;
  }
  public class MyDateTime : IComparable
  {
    DateTime dt;
    int counter;

    public MyDateTime(int instanceId, DateTime dateTime)
    {
      counter = instanceId;
      dt = dateTime;
    }
    public MyDateTime(int instanceId, string dateTimeStr)
    {
      counter = instanceId;
      dt = DateTime.Parse(dateTimeStr);
    }

    public int CompareTo(object obj)
    {
      MyDateTime mdt = (MyDateTime)obj;
      int ret = DateTime.Compare(dt, mdt.dt);
      if (ret == 0)
        ret = 1;
      return ret;
    }
  }
  public class SearchParameters
  {
    public string searchStr = "";
    public bool caseSensitive = false;
    public System.Drawing.Color highlightColor = System.Drawing.Color.Yellow;
  }

  public enum LoggerCategory
  {
    MediaPortal,
    TvEngine,
    Custom
  }
  class XmlUtils
  {
    public static void NewAttribute(XmlNode node,string name, string value)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
      attr.InnerText = value;
      node.Attributes.Append(attr);
    }
    public static void NewAttribute(XmlNode node, string name, bool value)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
      if (value)
        attr.InnerText = "1";
      else
        attr.InnerText = "0";
      node.Attributes.Append(attr);
    }
    public static void NewAttribute(XmlNode node, string name, int value)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
      attr.InnerText = value.ToString();
      node.Attributes.Append(attr);
    }
    public static void NewAttribute(XmlNode node, string name, float value)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
      attr.InnerText = value.ToString();
      node.Attributes.Append(attr);
    }
    public static void NewAttribute(XmlNode node, string name, Color value)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
      attr.InnerText = value.ToArgb().ToString();
      node.Attributes.Append(attr);
    }
  }
}
