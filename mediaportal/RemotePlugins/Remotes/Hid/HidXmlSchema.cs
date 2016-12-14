using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;

namespace MediaPortal.InputDevices.HidXmlSchema
{
  /// <summary>
  /// Root node of our XML schema
  /// </summary>
  [XmlRoot("HidHandler")]
  public class Root
  {
    [XmlAttribute("version")]
    public int Version { get; set; }

    [XmlElement("HidUsageAction")]
    public List<LogicalDevice> Devices = new List<LogicalDevice>();
  }

  /// <summary>
  /// Logical Device representation on our XML schema
  /// </summary>
  [XmlType("HidUsageAction")]
  public class LogicalDevice
  {
    [XmlAttribute("UsagePage")]
    public string UsagePage { get; set; }

    [XmlAttribute("UsageCollection")]
    public string UsageCollection { get; set; }

    [XmlAttribute("HandleHidEventsWhileInBackground")]
    public bool HandleHidEventsWhileInBackground { get; set; }

    [XmlElement("button")]
    public List<Usage> Usages = new List<Usage>();
  }

  /// <summary>
  /// HID usage representation in our XML schema
  /// </summary>
  [XmlType("button")]
  public class Usage
  {
    private string iName;

    [XmlAttribute("name")]
    public string Name {
      get { return (string.IsNullOrEmpty(iName) ? Code : iName); }
      set { iName = value; }
    }

    [XmlAttribute("code")]
    public string Code { get; set; }

    [XmlAttribute("repeat")]
    public bool Repeat { get; set; }

    [XmlAttribute("background")]
    public bool Background { get; set; }

    [XmlAttribute("ctrl")]
    public bool ModifierControl { get; set; }

    [XmlAttribute("shift")]
    public bool ModifierShift { get; set; }

    [XmlAttribute("alt")]
    public bool ModifierAlt { get; set; }

    [XmlAttribute("win")]
    public bool ModifierWindows { get; set; }

    [XmlElement("action")]
    public List<Action> Actions = new List<Action>();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetText()
    {
      if (Name != Code)
      {
        return Name;
      }

      //Build a neat name
      string name = Code;
      if (ModifierShift)
      {
        name += " + SHIFT";
      }

      if (ModifierControl)
      {
        name += " + CTRL";
      }

      if (ModifierAlt)
      {
        name += " + ALT";
      }

      if (ModifierWindows)
      {
        name += " + WIN";
      }

      if (Repeat || Background)
      {
        name += " ( ";
        bool needSeparator = false;
        if (Background)
        {
          name += "background";
          needSeparator = true;
        }

        if (Repeat)
        {
          if (needSeparator)
          {
            name += ", ";
          }
          name += "repeat";
        }

        name += " )";
      }

      return name;
    }


  }

  /// <summary>
  /// User action representation in our XML schema
  /// </summary>
  [XmlType("action")]
  public class Action
  {
    [XmlAttribute("layer")]
    public int Layer { get; set; }

    [XmlAttribute("condition")]
    public string Condition { get; set; }

    [XmlAttribute("conproperty")]
    public string ConditionProperty { get; set; }

    [XmlAttribute("command")]
    public string Command { get; set; }

    [XmlAttribute("cmdproperty")]
    public string CommandProperty { get; set; }

    [XmlAttribute("cmdkeychar")]
    public int CommandKeyChar { get; set; }

    [XmlAttribute("cmdkeycode")]
    public int CommandKeyCode { get; set; }

    [XmlAttribute("sound")]
    public string Sound { get; set; }

    [XmlAttribute("focus")]
    public bool Focus { get; set; }

  }


}
