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

#endregion Copyright (C) 2005-2011 Team MediaPortal

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using Win32;
using Hid=SharpLib.Hid;

namespace MediaPortal.InputDevices
{
  /// <summary>
  ///   HID Handler is responsible for:
  ///   * Loading and parsing Generic HID XML configuration.
  ///   * Registering for raw input as per configuration.
  ///   * Handling HID raw input as per configuration.
  /// </summary>
  public class HidHandler
  {
    #region Constructor

    /// <summary>
    ///   Constructor: Initializes mappings from XML file
    /// </summary>
    /// <param name="deviceXmlName">Input device name</param>
    public HidHandler(string deviceXmlName)
    {
      IsLoaded = false;
      //We will need one usage/action mapping for each HID UsagePage/UsageCollection we are listening too
      _usageActions = new Dictionary<UInt32, HidUsageAction>();

      var xmlPath = GetXmlPath(deviceXmlName);

      LoadMapping(xmlPath);
    }

    #endregion Constructor

    #region Private Fields

    private const int KXmlVersion = 1;
    private const int KCurrentLayer = 1;

    private readonly Dictionary<UInt32, HidUsageAction> _usageActions;

    private Hid.Handler _handler;
    private List<MediaPortal.InputDevices.HidUsageAction.ConditionalAction> _actions;
    private bool _shouldRaiseAction;

    #endregion Private Fields

    #region Public Properties

    /// <summary>
    ///   Mapping successful loaded
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    ///   Get current Layer (Multi-Layer support)
    /// </summary>
    public int CurrentLayer
    {
      get { return KCurrentLayer; }
    }

    #endregion Public Properties

    #region Implementation

    /// <summary>
    ///   Get version of XML mapping file
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.Xml.XmlException
    public int GetXmlVersion(string xmlPath)
    {
      var doc = new XmlDocument();
      doc.Load(xmlPath);
      return Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/HidHandler").Attributes["version"].Value);
    }

    /// <summary>
    ///   Check if XML file exists and version is current
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.IO.FileNotFoundException
    /// System.Xml.XmlException
    /// ApplicationException("XML version mismatch")
    public bool CheckXmlFile(string xmlPath)
    {
      if (!File.Exists(xmlPath) || (GetXmlVersion(xmlPath) != KXmlVersion))
      {
        Log.Error("HID: File does not exists or version mismatch {0}", xmlPath);
        return false;
      }
      return true;
    }

    /// <summary>
    ///   Get path to XML mapping file for given device name
    /// </summary>
    /// <param name="deviceXmlName">Input device name</param>
    /// <returns>Path to XML file</returns>
    /// Possible exceptions: System.IO.FileNotFoundException
    /// System.Xml.XmlException
    /// ApplicationException("XML version mismatch")
    public string GetXmlPath(string deviceXmlName)
    {
      var path = string.Empty;
      var pathDefault = Path.Combine(InputHandler.DefaultsDirectory, deviceXmlName + ".xml");
      var pathCustom = Path.Combine(InputHandler.CustomizedMappingsDirectory, deviceXmlName + ".xml");

      if (File.Exists(pathCustom) && CheckXmlFile(pathCustom))
      {
        path = pathCustom;
        Log.Info("MAP: using custom mappings for {0}", deviceXmlName);
      }
      else if (File.Exists(pathDefault) && CheckXmlFile(pathDefault))
      {
        path = pathDefault;
        Log.Info("MAP: using default mappings for {0}", deviceXmlName);
      }
      return path;
    }

    /// <summary>
    ///   Try parsing the given string as a decimal or hexadecimal if 0x prefix is found.
    /// </summary>
    /// <param name="aUsageName"></param>
    /// <param name="aUsageValue"></param>
    /// <returns></returns>
    public bool TryParseDefault(string aUsageName, out ushort aUsageValue)
    {
      //Parse as Hexadecimal if prefixed with 0x, otherwise parse as unsigned
      const string KHexaPrefix = "0x";
      var usageName = aUsageName;
      var numberStyles = NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite;
      if (usageName.StartsWith(KHexaPrefix))
      {
        //Usage name starts with 0x, try parse hexadecimal after dropping the prefix
        numberStyles = NumberStyles.HexNumber;
        usageName = usageName.Substring(KHexaPrefix.Length);
      }

      if (!ushort.TryParse(usageName, numberStyles, CultureInfo.InvariantCulture, out aUsageValue))
      {
        Log.Warn("HID: XML configuration parser: TryParseDefaultUsage failed {0}", aUsageName);
        return false;
      }

      return true;
    }

    /// <summary>
    ///   Generic function for parsing enumeration.
    ///   It supports name parsing, decimal parsing and hexadecimal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aUsageName"></param>
    /// <param name="aUsageValue"></param>
    /// <returns></returns>
    public bool TryParseEnum<T>(string aUsageName, out ushort aUsageValue) where T : struct, IConvertible
    {
      T usage;
      if (Enum.TryParse(aUsageName, out usage))
      {
        aUsageValue = usage.ToUInt16(CultureInfo.InvariantCulture);
        return true;
      }

      return TryParseDefault(aUsageName, out aUsageValue);
    }

    /// <summary>
    ///   Used to load our XML file.
    /// </summary>
    /// <param name="aUsageName"></param>
    /// <returns></returns>
    public bool TryParseWindowsMediaCenterRemoteControl(string aUsageName, out ushort aUsageValue)
    {
      {
        //Try parsing it as a standard MCE usage
        Hid.Usage.WindowsMediaCenterRemoteControl usage;
        if (Enum.TryParse(aUsageName, out usage))
        {
          aUsageValue = (ushort) usage;
          return true;
        }
      }

      {
        //Try parsing is a an HP MCE usage
        Hid.Usage.HpWindowsMediaCenterRemoteControl usage;
        if (Enum.TryParse(aUsageName, out usage))
        {
          aUsageValue = (ushort) usage;
          return true;
        }
      }

      return TryParseDefault(aUsageName, out aUsageValue);
    }

    /// <summary>
    ///   Load mapping from XML file
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    public void LoadMapping(string xmlPath)
    {
      if (xmlPath != string.Empty)
      {
        //_remote = new ArrayList();

        var doc = new XmlDocument();
        doc.Load(xmlPath);
        var usageActionNodes = doc.DocumentElement.SelectNodes("/HidHandler/HidUsageAction");
        foreach (XmlNode usageActionNode in usageActionNodes)
        {
          var usagePageName = usageActionNode.Attributes["UsagePage"].Value;
          var usageCollectionName = usageActionNode.Attributes["UsageCollection"].Value;

          ushort rawUsagePage = 0;
          ushort rawUsageCollection = 0;

          //Parse usage page
          if (!TryParseEnum<Hid.UsagePage>(usagePageName, out rawUsagePage))
          {
            Log.Error("HID: Unknown usage page {0}", usagePageName);
            //Move on to the next usage action then
            continue;
          }

          //Create our usage/action mapping
          var usageAction = new HidUsageAction();

          usageAction.HandleHidEventsWhileInBackground =
            (usageActionNode.Attributes["HandleHidEventsWhileInBackground"] != null &&
             (usageActionNode.Attributes["HandleHidEventsWhileInBackground"].Value == "true" ||
              usageActionNode.Attributes["HandleHidEventsWhileInBackground"].Value == "1")
              ? true
              : false);

          switch (rawUsagePage)
          {
            case (ushort) Hid.UsagePage.WindowsMediaCenterRemoteControl:
            {
              if (!TryParseEnum<Hid.UsageCollection.WindowsMediaCenter>(usageCollectionName, out rawUsageCollection))
              {
                Log.Error("HID: XML configuration could not parse UsageCollectionWindowsMediaCenter {0}", usageCollectionName);
                continue;
              }

              //Now parse usage action mapping
              usageAction.Load(usageActionNode, TryParseWindowsMediaCenterRemoteControl);
            }
              break;

            case (ushort) Hid.UsagePage.GenericDesktopControls:
            {
              if (!TryParseEnum<Hid.UsageCollection.GenericDesktop>(usageCollectionName, out rawUsageCollection))
              {
                Log.Error("HID: XML configuration could not parse UsageCollectionGenericDesktop {0}", usageCollectionName);
                continue;
              }

              //Now parse usage action mapping
              usageAction.Load(usageActionNode, TryParseEnum<Hid.Usage.GenericDesktop>);
            }
              break;

            case (ushort) Hid.UsagePage.Consumer:
            {
              if (!TryParseEnum<Hid.UsageCollection.Consumer>(usageCollectionName, out rawUsageCollection))
              {
                Log.Error("HID: XML configuration could not parse UsageCollectionConsumer {0}", usageCollectionName);
                continue;
              }

              //Now parse usage action mapping
              usageAction.Load(usageActionNode, TryParseEnum<Hid.Usage.ConsumerControl>);
            }
              break;

            case (ushort) Hid.UsagePage.SimulationControls:
            {
              if (!TryParseDefault(usageCollectionName, out rawUsageCollection))
              {
                Log.Error("HID: XML configuration could not parse SimulationControls collection {0}", usageCollectionName);
                continue;
              }

              //Now parse usage action mapping
              usageAction.Load(usageActionNode, TryParseEnum<Hid.Usage.SimulationControl>);
            }
              break;

            case (ushort) Hid.UsagePage.GameControls:
            {
              if (!TryParseDefault(usageCollectionName, out rawUsageCollection))
              {
                Log.Error("HID: XML configuration could not parse GameControls collection {0}", usageCollectionName);
                continue;
              }

              //Now parse usage action mapping
              usageAction.Load(usageActionNode, TryParseEnum<Hid.Usage.GameControl>);
            }
              break;

            case (ushort) Hid.UsagePage.Telephony:
            {
              if (!TryParseDefault(usageCollectionName, out rawUsageCollection))
              {
                Log.Error("HID: XML configuration could not parse Telephony collection {0}", usageCollectionName);
                continue;
              }

              //Now parse usage action mapping
              usageAction.Load(usageActionNode, TryParseEnum<Hid.Usage.TelephonyDevice>);
            }
              break;
          }

          //Check if we could determine our usage collection
          if (rawUsageCollection == 0)
          {
            //Name parsing failed or usage page not handled just try get the ushort
            if (!TryParseDefault(usageCollectionName, out rawUsageCollection))
            {
              Log.Error("HID: XML configuration could not parse UsageCollection {0}", usageCollectionName);
              continue;
            }

            usageAction.Load(usageActionNode, TryParseDefault);
          }

          //Workout our usage ID and set usage page and collection for that usage action mapping
          var usageId = (uint) rawUsagePage << 16 | rawUsageCollection;
          usageAction.UsagePage = rawUsagePage;
          usageAction.UsageCollection = rawUsageCollection;

          //Add that usage action mapping to our dictionary
          //TODO: check for duplicate?
          _usageActions.Add(usageId, usageAction);
        }
        IsLoaded = true;
      }
    }

    /// <summary>
    ///   Register raw input device as defined and imported from our XML.
    /// </summary>
    /// <param name="aHWND"></param>
    public void Register(IntPtr aHWND)
    {
      SharpLib.Win32.RAWINPUTDEVICE[] rid = new SharpLib.Win32.RAWINPUTDEVICE[_usageActions.Count];

      var i = 0;
      foreach (var entry in _usageActions)
      {
        rid[i].usUsagePage = entry.Value.UsagePage;
        rid[i].usUsage = entry.Value.UsageCollection;
        rid[i].dwFlags = (entry.Value.HandleHidEventsWhileInBackground ? Const.RIDEV_EXINPUTSINK : 0);
        rid[i].hwndTarget = aHWND;
        i++;
      }

      _handler = new SharpLib.Hid.Handler(rid, true);
      if (!_handler.IsRegistered)
      {
        Debug.WriteLine("Failed to register raw input devices: " + Marshal.GetLastWin32Error().ToString());
      }
      _handler.OnHidEvent += OnHidEvent;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aMessage"></param>
    /// <param name="shouldRaiseAction"></param>
    /// <returns></returns>
    public List<MediaPortal.InputDevices.HidUsageAction.ConditionalAction> ProcessInput(Message aMessage, bool shouldRaiseAction = true)
    {
      _shouldRaiseAction = shouldRaiseAction;
      //Processing HID events
      _handler.ProcessInput(ref aMessage);

      return _actions;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="aHidEvent"></param>
    public void OnHidEvent(object aSender, Hid.Event aHidEvent)
    {
      if (aHidEvent.IsRepeat)
      {
        HidListener.LogInfo("HID: Repeat");  
      }

      _actions = null;
      _actions = new List<MediaPortal.InputDevices.HidUsageAction.ConditionalAction>();
      HidUsageAction usageAction;
      if (_usageActions.TryGetValue(aHidEvent.UsageId, out usageAction))
      {
        //Alright we do handle this usage ID
        //Try mapping actions to each of our usage
        foreach (ushort usage in aHidEvent.Usages)
        {
          HidListener.LogInfo("HID: try mapping usage {0}", usage.ToString("X4"));
          _actions.Add(usageAction.GetAction(usage.ToString(), aHidEvent.IsBackground, aHidEvent.IsRepeat));
          if (_shouldRaiseAction)
          {
            usageAction.MapAction(usage, aHidEvent.IsBackground, aHidEvent.IsRepeat);
          }
        }

        //Do some extra checks if our device is a gamepad
        if (aHidEvent.Device.IsGamePad)
        {
          //Check if dpad needs to be handled too
          HidListener.LogInfo("HID: try mapping dpad {0}", aHidEvent.GetDirectionPadState());
          const int KDPadButtonOffset = 1000; //This is our magic dpad button offset. Should be good enough as it leaves us with 998 genuine buttons. 
          ushort dpadFakeUsage = (ushort)(KDPadButtonOffset + (int)aHidEvent.GetDirectionPadState());
          _actions.Add(usageAction.GetAction(dpadFakeUsage.ToString(), aHidEvent.IsBackground, aHidEvent.IsRepeat));
          if (_shouldRaiseAction)
          {
            usageAction.MapAction(dpadFakeUsage, aHidEvent.IsBackground, aHidEvent.IsRepeat);
          }
        }
      }
    }

    #endregion Implementation

  }
}