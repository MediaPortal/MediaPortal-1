#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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

#endregion

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;


namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Remotecontrol-mapping class
  /// Expects an XML file with mappings on construction
  /// Maps button code numbers to conditions and actions
  /// </summary>
  public class InputHandler
  {
    int xmlVersion = 3;
    ArrayList remote;
    int currentLayer = 1;


    /// <summary>
    /// Get current Layer (Multi-Layer support)
    /// </summary>
    public int CurrentLayer { get { return currentLayer; } }


    /// <summary>
    /// Condition/action class
    /// </summary>
    public class Mapping
    {
      string condition;
      string conProperty;
      int layer;
      string command;
      string cmdProperty;
      int cmdKeyChar;
      int cmdKeyCode;
      string sound;

      public int Layer { get { return layer; } }
      public string Condition { get { return condition; } }
      public string ConProperty { get { return conProperty; } }
      public string Command { get { return command; } }
      public string CmdProperty { get { return cmdProperty; } }
      public int CmdKeyChar { get { return cmdKeyChar; } }
      public int CmdKeyCode { get { return cmdKeyCode; } }
      public string Sound { get { return sound; } }

      public Mapping(int newLayer, string newCondition, string newConProperty, string newCommand,
        string newCmdProperty, int newCmdKeyChar, int newCmdKeyCode, string newSound)
      {
        layer = newLayer;
        condition = newCondition;
        conProperty = newConProperty;
        command = newCommand;
        cmdProperty = newCmdProperty;
        cmdKeyChar = newCmdKeyChar;
        cmdKeyCode = newCmdKeyCode;
        sound = newSound;
      }
    }


    /// <summary>
    /// Button/mapping class
    /// </summary>
    class RemoteMap
    {
      int code;
      string name;
      ArrayList mapping = new ArrayList();

      public int Code { get { return code; } }
      public string Name { get { return name; } }
      public ArrayList Mapping { get { return mapping; } }

      public RemoteMap(int newCode, string newName, ArrayList newMapping)
      {
        code = newCode;
        name = newName;
        mapping = newMapping;
      }
    }


    /// <summary>
    /// Constructor: Initializes mappings from XML file
    /// </summary>
    /// <param name="deviceXmlName">Input device name</param>
    /// Possible exceptions: System.IO.FileNotFoundException
    ///                      System.Xml.XmlException
    ///                      ApplicationException("XML version mismatch")
    public InputHandler(string deviceXmlName)
    {
      string xmlPath = GetXmlPath(deviceXmlName);
      LoadMapping(xmlPath);
    }


    /// <summary>
    /// Get version of XML mapping file 
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.Xml.XmlException
    public int GetXmlVersion(string xmlPath)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(xmlPath);
      return Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/mappings").Attributes["version"].Value);
    }


    /// <summary>
    /// Check if XML file exists and version is current
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.IO.FileNotFoundException
    ///                      System.Xml.XmlException
    ///                      ApplicationException("XML version mismatch")
    public void CheckXmlFile(string xmlPath)
    {
      if (!File.Exists(xmlPath))
        throw new System.IO.FileNotFoundException();
      if (GetXmlVersion(xmlPath) != xmlVersion)
        throw new ApplicationException("XML version mismatch");
    }


    /// <summary>
    /// Get path to XML mmapping file for given device name
    /// </summary>
    /// <param name="deviceXmlName">Input device name</param>
    /// <returns>Path to XML file</returns>
    /// Possible exceptions: System.IO.FileNotFoundException
    ///                      System.Xml.XmlException
    ///                      ApplicationException("XML version mismatch")
    public string GetXmlPath(string deviceXmlName)
    {
      string path = string.Empty;
      string pathCustom = "InputDeviceMappings\\custom\\" + deviceXmlName + ".xml";
      string pathDefault = "InputDeviceMappings\\defaults\\" + deviceXmlName + ".xml";

      try
      {
        CheckXmlFile(pathCustom);
        path = pathCustom;
        Log.Write("MAP: using custom mappings for {0}", deviceXmlName);
      }
      catch
      {
        CheckXmlFile(pathDefault);
        path = pathDefault;
        Log.Write("MAP: using default mappings for {0}", deviceXmlName);
      }
      return path;
    }


    /// <summary>
    /// Load mapping from XML file
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.IO.FileNotFoundException
    ///                      System.Xml.XmlException
    public void LoadMapping(string xmlPath)
    {
      remote = new ArrayList();
      XmlDocument doc = new XmlDocument();
      doc.Load(xmlPath);
      XmlNodeList listButtons = doc.DocumentElement.SelectNodes("/mappings/remote/button");
      foreach (XmlNode nodeButton in listButtons)
      {
        string name = nodeButton.Attributes["name"].Value;
        int value = Convert.ToInt32(nodeButton.Attributes["code"].Value);

        ArrayList mapping = new ArrayList();
        XmlNodeList listActions = nodeButton.SelectNodes("action");
        foreach (XmlNode nodeAction in listActions)
        {
          int cmdKeyChar = 0;
          int cmdKeyCode = 0;
          string condition = nodeAction.Attributes["condition"].Value.ToUpper();
          string conProperty = nodeAction.Attributes["conproperty"].Value.ToUpper();
          string command = nodeAction.Attributes["command"].Value.ToUpper();
          string cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpper();
          if ((command == "ACTION") && (cmdProperty == "93"))
          {
            cmdKeyChar = Convert.ToInt32(nodeAction.Attributes["cmdkeychar"].Value);
            cmdKeyCode = Convert.ToInt32(nodeAction.Attributes["cmdkeycode"].Value);
          }
          string sound = nodeAction.Attributes["sound"].Value;
          int layer = Convert.ToInt32(nodeAction.Attributes["layer"].Value);
          Mapping conditionMap = new Mapping(layer, condition, conProperty, command, cmdProperty, cmdKeyChar, cmdKeyCode, sound);
          mapping.Add(conditionMap);
        }
        RemoteMap remoteMap = new RemoteMap(value, name, mapping);
        remote.Add(remoteMap);
      }
    }


    /// <summary>
    /// Evaluates the button number, gets its mapping and executes the action
    /// </summary>
    /// <param name="btnCode">Button code (ref: XML file)</param>
    /// Possible exceptions: ApplicationException("No button mapping found")
    ///                      ApplicationException("No button mapping loaded")
    public void MapAction(int btnCode)
    {
      DoMapAction(btnCode, -1);
    }


    /// <summary>
    /// Evaluates the button number, gets its mapping and executes the action with an optional paramter
    /// </summary>
    /// <param name="btnCode">Button code (ref: XML file)</param>
    /// <param name="processID">Process-ID for close/kill commands</param>
    /// Possible exceptions: ApplicationException("No button mapping found")
    ///                      ApplicationException("No button mapping loaded")
    public void MapAction(int btnCode, int processID)
    {
      DoMapAction(btnCode, processID);
    }


    /// <summary>
    /// Evaluates the button number, gets its mapping and executes the action
    /// </summary>
    /// <param name="btnCode">Button code (ref: XML file)</param>
    /// <param name="processID">Process-ID for close/kill commands</param>
    /// Possible exceptions: ApplicationException("No button mapping found")
    ///                      ApplicationException("No button mapping loaded")
    void DoMapAction(int btnCode, int processID)
    {
      if (remote == null)   // No mapping loaded
        throw new ApplicationException("No button mapping loaded");
      Mapping map = null;
      map = GetMapping(btnCode);
#if DEBUG
      Log.Write("{0} / {1} / {2} / {3}", map.Condition, map.ConProperty, map.Command, map.CmdProperty);
#endif
      Action action;
      if (map.Sound != string.Empty)
        Utils.PlaySound(map.Sound, false, true);
      switch (map.Command)
      {
        case "ACTION":  // execute Action x
          Key key = new Key(map.CmdKeyChar, map.CmdKeyCode);
#if DEBUG
          Log.Write("Executing: key {0} / {1} / Action: {2} / {3}", map.CmdKeyChar, map.CmdKeyCode, map.CmdProperty, ((Action.ActionType)Convert.ToInt32(map.CmdProperty)).ToString());
#endif
          action = new Action(key, (Action.ActionType)Convert.ToInt32(map.CmdProperty), 0, 0);
          GUIGraphicsContext.OnAction(action);
          break;
        case "KEY": // send Key x
          SendKeys.SendWait(map.CmdProperty);
          break;
        case "WINDOW":  // activate Window x
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, Convert.ToInt32(map.CmdProperty), 0, null);
          GUIWindowManager.SendThreadMessage(msg);
          break;
        case "TOGGLE":  // toggle Layer 1/2
          if (currentLayer == 1)
            currentLayer = 2;
          else
            currentLayer = 1;
          break;
        case "POWER": // power down commands
          {
            if ((map.CmdProperty == "STANDBY") || (map.CmdProperty == "HIBERNATE"))
            {
              // Stop all media before suspending or hibernating
              g_Player.Stop();
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
            }
            switch (map.CmdProperty)
            {
              case "EXIT":
                action = new Action(Action.ActionType.ACTION_EXIT, 0, 0);
                GUIGraphicsContext.OnAction(action);
                break;
              case "REBOOT":
                action = new Action(Action.ActionType.ACTION_REBOOT, 0, 0);
                GUIGraphicsContext.OnAction(action);
                break;
              case "SHUTDOWN":
                action = new Action(Action.ActionType.ACTION_SHUTDOWN, 0, 0);
                GUIGraphicsContext.OnAction(action);
                break;
              case "STANDBY":
                WindowsController.ExitWindows(RestartOptions.Suspend, true);
                break;
              case "HIBERNATE":
                WindowsController.ExitWindows(RestartOptions.Hibernate, true);
                break;
            }
          }
          break;
        case "PROCESS":
          {
            if (processID > 0)
            {
              Process proc = Process.GetProcessById(processID);
              if (null != proc)
                switch (map.CmdProperty)
                {
                  case "CLOSE":
                    proc.CloseMainWindow();
                    break;
                  case "KILL":
                    proc.Kill();
                    break;
                }
            }
          }
          break;
      }
    }


    /// <summary>
    /// Get mappings for a given button code based on the current conditions
    /// </summary>
    /// <param name="btnCode">Button code (ref: XML file)</param>
    /// <returns>Mapping</returns>
    /// Possible exceptions: ApplicationException("No button mapping found")
    public Mapping GetMapping(int btnCode)
    {
      RemoteMap button = null;
      Mapping found = null;

      foreach (RemoteMap btn in remote)
        if (btnCode == btn.Code)
        {
          button = btn;
          break;
        }
      if (button != null)
        foreach (Mapping map in button.Mapping)
          if ((map.Layer == 0) || (map.Layer == currentLayer))
          {
            switch (map.Condition)
            {
              case "*": // wildcard, no further condition
                found = map;
                break;
              case "WINDOW":  // Window-ID = x
                if (GUIWindowManager.ActiveWindowEx == Convert.ToInt32(map.ConProperty))
                  found = map;
                break;
              case "FULLSCREEN":  // Fullscreen = true/false
                if (GUIGraphicsContext.IsFullScreenVideo == Convert.ToBoolean(map.ConProperty))
                  found = map;
                break;
              case "PLAYER":  // Playing TV/DVD/general
                switch (map.ConProperty)
                {
                  case "TV":
                    if (g_Player.IsTV)
                      found = map;
                    break;
                  case "DVD":
                    if (g_Player.IsDVD)
                      found = map;
                    break;
                  case "MEDIA":
                    if (g_Player.Playing)
                      found = map;
                    break;
                }
                break;
            }
            if (found != null)
            {
              return found;
            }
          }
      throw new ApplicationException("No button mapping found");
    }
  }
}