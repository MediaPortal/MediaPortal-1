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
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.InputDevices
{
  public delegate bool TryParseUsage(string aUsageName, out ushort aUsage);

  /// <summary>
  ///   Define the MP actions for various HID usages from a usage page and collection.
  ///   Expects an XML file with mappings on construction.
  ///   Maps button code numbers to conditions and actions.
  /// </summary>
  public class HidUsageAction
  {
    #region Constructor

    public HidUsageAction()
    {
      IsLoaded = false;
      using (Settings xmlreader = new MPSettings())
      {
        _basicHome = xmlreader.GetValueAsBool("gui", "startbasichome", true);
      }
    }

    #endregion Constructor

    #region Private Fields

    private List<Button> _buttons;
    private int _currentLayer = 1;
    private readonly bool _basicHome;

    #endregion Private Fields

    #region Public Properties

    public ushort UsagePage { get; set; }

    public ushort UsageCollection { get; set; }

    public bool HandleHidEventsWhileInBackground { get; set; }

    /// <summary>
    ///   Mapping successful loaded
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    ///   Get current Layer (Multi-Layer support)
    /// </summary>
    public int CurrentLayer
    {
      get { return _currentLayer; }
    }

    #endregion Public Properties

    #region Nested Classes

    /// <summary>
    ///   Condition/action class
    /// </summary>
    public class ConditionalAction
    {
      public ConditionalAction(int newLayer, string newCondition, string newConProperty, string newCommand,
        string newCmdProperty, int newCmdKeyChar, int newCmdKeyCode, string newSound, bool newFocus)
      {
        Layer = newLayer;
        Condition = newCondition;
        ConProperty = newConProperty;
        Command = newCommand;
        CmdProperty = newCmdProperty;
        CmdKeyChar = newCmdKeyChar;
        CmdKeyCode = newCmdKeyCode;
        Sound = newSound;
        Focus = newFocus;
      }

      public int Layer { get; private set; }
      public string Condition { get; private set; }
      public string ConProperty { get; private set; }
      public string Command { get; private set; }
      public string CmdProperty { get; private set; }
      public int CmdKeyChar { get; private set; }
      public int CmdKeyCode { get; private set; }
      public string Sound { get; private set; }
      public bool Focus { get; private set; }
    }

    /// <summary>
    ///   Button/mapping class
    ///   We will create one of this for each 'button' element.
    /// </summary>
    private class Button
    {
      private readonly List<ConditionalAction> _actions;

      public Button(string aCode, string aName, bool aBackground, bool aRepeat, bool aShift, bool aCtrl, bool aAlt, bool aWin, List<ConditionalAction> aActions)
      {
        Code = aCode;
        Name = aName;
        Background = aBackground;
        Repeat = aRepeat;
        NeedsModifierShift = aShift;
        NeedsModifierControl = aCtrl;
        NeedsModifierAlt = aAlt;
        NeedsModifierWindows = aWin;
        _actions = aActions;
      }

      public string Code { get; private set; }
      public string Name { get; private set; }

      /// <summary>
      ///   Tells whether this button works while MP is in background.
      /// </summary>
      public bool Background { get; private set; }

      /// <summary>
      ///   Tells whether this button should repeat when being held down.
      /// </summary>
      public bool Repeat { get; private set; }

      /// <summary>
      ///   Tells whether this button needs shift modifier.
      /// </summary>
      public bool NeedsModifierShift { get; private set; }

      /// <summary>
      ///   Tells whether this button needs control modifier.
      /// </summary>
      public bool NeedsModifierControl { get; private set; }

      /// <summary>
      ///   Tells whether this button needs alt modifier.
      /// </summary>
      public bool NeedsModifierAlt { get; private set; }

      /// <summary>
      ///   Tells whether this button needs windows modifier.
      /// </summary>
      public bool NeedsModifierWindows { get; private set; }

      /// <summary>
      /// Actions this buttons can trigger.
      /// </summary>
      public List<ConditionalAction> Actions
      {
        get { return _actions; }
      }
    }

    #endregion Nested Classes

    #region Implementation


    /// <summary>
    /// Convert an XML attribute value to boolean
    /// </summary>
    /// <param name="aAttribute"></param>
    /// <param name="aDefault"></param>
    /// <returns></returns>
    public static bool AttributeValueToBoolean(XmlAttribute aAttribute, bool aDefault=false)
    {
      if (aAttribute == null)
      {
        return aDefault;
      }

      return AttributeValueToBoolean(aAttribute.Value, aDefault);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aValue"></param>
    /// <param name="aDefault"></param>
    /// <returns></returns>
    public static bool AttributeValueToBoolean(string aValue, bool aDefault = false)
    {
      string val = aValue.ToLower();

      if (val.Equals("true") || val.Equals("on") || val.Equals("enabled") || val.Equals("1"))
      {
        return true;
      }

      if (val.Equals("false") || val.Equals("off") || val.Equals("disabled") || val.Equals("0"))
      {
        return false;
      }

      Log.Warn("HID XML configuration: can't parse attribute value '{0}' using default '{1}' instead.", aValue, aDefault);
      return aDefault;      
    }

    /// <summary>
    /// Load mapping from XML file
    /// </summary>
    /// <param name="aXmlNode"></param>
    /// <param name="aTryParseUsage"></param>
    public void Load(XmlNode aXmlNode, TryParseUsage aTryParseUsage)
    {
      _buttons = new List<Button>();
      var listButtons = aXmlNode.ChildNodes;
      foreach (XmlNode nodeButton in listButtons)
      {
        //Only process button elements
        if (!(nodeButton.NodeType == XmlNodeType.Element && nodeButton.Name == "button"))
        {
          continue;
        }

        //Get element name and code
        var code = nodeButton.Attributes["code"].Value;

        //We do not require a name attribute anymore as the code itself can in most cases be used as a name too
        var name = "";
        var nameAttribute = nodeButton.Attributes["name"];
        if (nameAttribute != null)
        {
          //If we have a name attribute do use it.
          name = nameAttribute.Value;
        }
        else
        {
          //Otherwise use the code itself as a name
          name = code;
        }

        //Check if this command is supported while MP is in background
        bool background = AttributeValueToBoolean(nodeButton.Attributes["background"]);

        //Check if this command supports repeats
        bool repeat = AttributeValueToBoolean(nodeButton.Attributes["repeat"]);

        // Get the required keyboard modifiers 
        bool shift = AttributeValueToBoolean(nodeButton.Attributes["shift"]);
        bool ctrl = AttributeValueToBoolean(nodeButton.Attributes["ctrl"]);
        bool alt = AttributeValueToBoolean(nodeButton.Attributes["alt"]);
        bool win = AttributeValueToBoolean(nodeButton.Attributes["win"]);

        //Now try and parse our usage code using the provided method
        ushort usage = 0;
        if (!aTryParseUsage(code, out usage))
        {
          Log.Warn("HID: XML loader can't parse usage {0} for button {1}", code, name);
          continue;
        }

        //Feed back our usage integer as a string into our legacy action parser
        code = usage.ToString();

        //Perform legacy parsing of our actions
        var actions = new List<ConditionalAction>();
        var listActions = nodeButton.SelectNodes("action");
        foreach (XmlNode nodeAction in listActions)
        {
          var cmdKeyChar = 0;
          var cmdKeyCode = 0;
          var condition = nodeAction.Attributes["condition"].Value.ToUpperInvariant();
          var conProperty = nodeAction.Attributes["conproperty"].Value.ToUpperInvariant();
          var command = nodeAction.Attributes["command"].Value.ToUpperInvariant();
          var cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpperInvariant();
          if ((command == "ACTION") && (cmdProperty == "93"))
          {
            cmdKeyChar = Convert.ToInt32(nodeAction.Attributes["cmdkeychar"].Value);
            cmdKeyCode = Convert.ToInt32(nodeAction.Attributes["cmdkeycode"].Value);
          }
          var sound = string.Empty;
          var soundAttribute = nodeAction.Attributes["sound"];
          if (soundAttribute != null)
          {
            sound = soundAttribute.Value;
          }
          var focus = false;
          var focusAttribute = nodeAction.Attributes["focus"];
          if (focusAttribute != null)
          {
            focus = Convert.ToBoolean(focusAttribute.Value);
          }
          var layer = Convert.ToInt32(nodeAction.Attributes["layer"].Value);
          var conditionMap = new ConditionalAction(layer, condition, conProperty, command,
            cmdProperty, cmdKeyChar, cmdKeyCode, sound, focus);
          actions.Add(conditionMap);
        }
        var button = new Button(code, name, background, repeat, shift, ctrl, alt, win, actions);
        _buttons.Add(button);
      }
      IsLoaded = true;
    }


    /// <summary>
    /// Execute the given conditional action if needed.
    /// </summary>
    /// <param name="aAction">The action we want to conditionally execute.</param>
    /// <param name="aProcessId">Process-ID for close/kill commands.</param>
    /// <returns></returns>
    public bool ExecuteActionIfNeeded(ConditionalAction aAction, int aProcessId = -1)
    {
      if (aAction == null)
      {
        return false;
      }

#if DEBUG
      Log.Info("{0} / {1} / {2} / {3}", aAction.Condition, aAction.ConProperty, aAction.Command, aAction.CmdProperty);
#endif
      Action action;
      if (aAction.Sound != string.Empty)
      {
        Util.Utils.PlaySound(aAction.Sound, false, true);
      }
      if (aAction.Focus && !GUIGraphicsContext.HasFocus)
      {
        GUIGraphicsContext.ResetLastActivity();
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GETFOCUS, 0, 0, 0, 0, 0, null);
        //GUIWindowManager.SendThreadMessage(msg);
        GUIGraphicsContext.SendMessage(msg);
        return true;
      }
      switch (aAction.Command)
      {
        case "ACTION": // execute Action x
          var key = new Key(aAction.CmdKeyChar, aAction.CmdKeyCode);
#if DEBUG
          Log.Info("Executing: key {0} / {1} / Action: {2} / {3}", aAction.CmdKeyChar, aAction.CmdKeyCode,
            aAction.CmdProperty,
            ((Action.ActionType)Convert.ToInt32(aAction.CmdProperty)).ToString());
#endif
          action = new Action(key, (Action.ActionType)Convert.ToInt32(aAction.CmdProperty), 0, 0);
          GUIGraphicsContext.OnAction(action);
          break;

        case "KEY": // send Key x
          SendKeys.SendWait(aAction.CmdProperty);
          break;

        case "WINDOW": // activate Window x
          GUIGraphicsContext.ResetLastActivity();
          GUIMessage msg;
          if ((Convert.ToInt32(aAction.CmdProperty) == (int)GUIWindow.Window.WINDOW_HOME) ||
              (Convert.ToInt32(aAction.CmdProperty) == (int)GUIWindow.Window.WINDOW_SECOND_HOME))
          {
            if (_basicHome)
            {
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                (int)GUIWindow.Window.WINDOW_SECOND_HOME, 0, null);
            }
            else
            {
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                (int)GUIWindow.Window.WINDOW_HOME, 0, null);
            }
          }
          else
          {
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
              Convert.ToInt32(aAction.CmdProperty),
              0, null);
          }

          GUIWindowManager.SendThreadMessage(msg);
          break;

        case "TOGGLE": // toggle Layer 1/2
          if (_currentLayer == 1)
          {
            _currentLayer = 2;
          }
          else
          {
            _currentLayer = 1;
          }
          break;

        case "POWER": // power down commands

          if ((aAction.CmdProperty == "STANDBY") || (aAction.CmdProperty == "HIBERNATE"))
          {
            GUIGraphicsContext.ResetLastActivity();
          }

          switch (aAction.CmdProperty)
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
              action = new Action(Action.ActionType.ACTION_SUSPEND, 1, 0); //1 = ignore prompt
              GUIGraphicsContext.OnAction(action);
              break;

            case "HIBERNATE":
              action = new Action(Action.ActionType.ACTION_HIBERNATE, 1, 0); //1 = ignore prompt
              GUIGraphicsContext.OnAction(action);
              break;
          }
          break;

        case "PROCESS":
          {
            GUIGraphicsContext.ResetLastActivity();
            if (aProcessId > 0)
            {
              var proc = Process.GetProcessById(aProcessId);
              if (null != proc)
              {
                switch (aAction.CmdProperty)
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
          }
          break;

        default:
          return false;
      }
      return true;
    }



    /// <summary>
    /// /// Get mappings for a given button code based on the current conditions.
    /// </summary>
    /// <param name="aButtonCode"></param>
    /// <param name="aIsBackground"></param>
    /// <param name="aIsRepeat"></param>
    /// <param name="aShift"></param>
    /// <param name="aCtrl"></param>
    /// <param name="aAlt"></param>
    /// <param name="aWin"></param>
    /// <returns></returns>
    public ConditionalAction GetAction(string aButtonCode, bool aIsBackground, bool aIsRepeat, bool aShift, bool aCtrl, bool aAlt, bool aWin)
    {
      Button button = null;
      ConditionalAction found = null;

      // Try find a button that's matching the provided criteria
      foreach (Button btn in _buttons)
      {
        if (aButtonCode == btn.Code)
        {
          if (aIsBackground && !btn.Background)
          {
            //We don't proceed button while in background unless they are marked as supporting background
            continue;
          }

          if (aIsRepeat && !btn.Repeat)
          {
            //We don't proceed button repeat unless otherwise specified
            continue;
          }

          //We need our modifiers to match too
          if (aShift != btn.NeedsModifierShift)
          {
            continue;
          }

          if (aCtrl != btn.NeedsModifierControl)
          {
            continue;
          }

          if (aAlt != btn.NeedsModifierAlt)
          {
            continue;
          }

          if (aWin != btn.NeedsModifierWindows)
          {
            continue;
          }

          //We are happy with this button
          button = btn;
          break;
        }
      }
      if (button != null)
      {
        foreach (var map in button.Actions)
        {
          if ((map.Layer == 0) || (map.Layer == _currentLayer))
          {
            switch (map.Condition)
            {
              case "*": // wildcard, no further condition
                found = map;
                break;

              case "WINDOW": // Window-ID = x
                if ((!GUIWindowManager.IsOsdVisible &&
                     (GUIWindowManager.ActiveWindowEx == Convert.ToInt32(map.ConProperty))) ||
                    ((int) GUIWindowManager.VisibleOsd == Convert.ToInt32(map.ConProperty)))
                {
                  found = map;
                }
                break;

              case "FULLSCREEN": // Fullscreen = true/false
                if ((GUIGraphicsContext.IsFullScreenVideo == Convert.ToBoolean(map.ConProperty)) &&
                    !GUIWindowManager.IsRouted && !GUIWindowManager.IsOsdVisible)
                {
                  found = map;
                }
                break;

              case "PLAYER": // Playing TV/DVD/general
                if (!GUIWindowManager.IsRouted)
                {
                  switch (map.ConProperty)
                  {
                    case "TV":
                      if (g_Player.IsTimeShifting || g_Player.IsTV || g_Player.IsTVRecording)
                      {
                        found = map;
                      }
                      break;

                    case "DVD":
                      if (g_Player.IsDVD)
                      {
                        found = map;
                      }
                      break;

                    case "MUSIC":
                      if (g_Player.Playing && g_Player.IsMusic)
                      {
                        found = map;
                      }
                      break;

                    case "MEDIA":
                      if (g_Player.Playing)
                      {
                        found = map;
                      }
                      break;
                  }
                }
                break;
            }
            if (found != null)
            {
              return found;
            }
          }
        }
      }
      return null;
    }

    #endregion Implementation
  }
}