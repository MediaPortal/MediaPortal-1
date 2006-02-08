#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.X10Plugin
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIX10 : GUIWindow, ISetupForm, IShowPlugin
  {
    enum Controls
    {
      BACKGROUND = 1,
      SELECT_LOCATION = 2,
      ON_BUTTON = 10,
      OFF_BUTTON = 11,
      BRIGHT_BUTTON = 12,
      DIM_BUTTON = 13,
      THIS_ROOM_ON_BUTTON = 15,
      THIS_ROOM_OFF_BUTTON = 16,
      THIS_ROOM_DIM50_BUTTON = 17,
      ALL_LIGHTS_ON_BUTTON = 20,
      ALL_LIGHTS_OFF_BUTTON = 21,
      ALL_UNITS_OFF_BUTTON = 22,
      LABEL_DESCRIPTION = 30,
      LISTAPPLIANCE = 50
    }

    public static int WINDOW_X10PLUGIN = 9562;
    const string PLUGIN_NAME = "X10 Automation";
    const string PLUGIN_NAME_SHORT = "X10 Plugin";

    private SendX10 x10module;
    private ApplianceConfiguration appConfig;

    private string p_selectedAppliance = "";
    private string p_selectedLocation = "";

    public GUIX10()
    {
    }

    public override int GetID
    {
      get
      {
        return GUIX10.WINDOW_X10PLUGIN;
      }
      set
      {
        base.GetID = value;
      }
    }

    /// <summary>
    /// Gets called by the runtime when a new window has been created
    /// Every window window should override this method and load itself by calling
    /// the Load() method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\x10plugin.xml");
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {

      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            appConfig = new ApplianceConfiguration();
            appConfig.LoadSettings();

            base.OnMessage(message);

            GUIControl.ClearControl(GetID, (int)Controls.SELECT_LOCATION);
            foreach (string sloc in appConfig.m_locations)
              GUIControl.AddItemLabelControl(GetID, (int)Controls.SELECT_LOCATION, sloc);

            // this does not work
            //GUIControl.SelectItemControl(GetID,(int)Controls.SELECT_LOCATION, 0);
            fillAppliances(appConfig.m_locations[0].ToString());

            x10module = new SendX10(appConfig.m_CMDevice, appConfig.m_CM1xHost, appConfig.m_CM17COMPort);

            // CM17 object does not handle all lights/units on/off
            if (appConfig.m_CMDevice == (int)SendX10.CMDevices.CM17)
            {
              if (GetControl((int)Controls.ALL_LIGHTS_OFF_BUTTON) != null)
                GetControl((int)Controls.ALL_LIGHTS_OFF_BUTTON).Disabled = true;
              if (GetControl((int)Controls.ALL_LIGHTS_ON_BUTTON) != null)
                GetControl((int)Controls.ALL_LIGHTS_ON_BUTTON).Disabled = true;
              if (GetControl((int)Controls.ALL_UNITS_OFF_BUTTON) != null)
                GetControl((int)Controls.ALL_UNITS_OFF_BUTTON).Disabled = true;
            }

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {

          }
          break;

        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          {
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (iControl == (int)Controls.SELECT_LOCATION)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
              OnMessage(msg);
              fillAppliances(msg.Label);
              return true;
            }
            string description = "";
            if (iControl == (int)Controls.LISTAPPLIANCE)
            {
              GUIListItem item = GetSelectedItem(iControl);
              if ((item != null) && (item.MusicTag != null))
              {
                p_selectedAppliance = item.MusicTag.ToString();
                description = "Selected: " + item.Label;
              }
            }

            if (iControl == (int)Controls.ON_BUTTON)
            {
              if (p_selectedAppliance.Length < 2)
                description = "No appliance selected !";
              else
              {
                description = "Switched ON " + p_selectedAppliance;
                x10module.sendX10Command(
                  p_selectedAppliance.Substring(0, 1),
                  p_selectedAppliance.Substring(1),
                  "ON");
              }
            }
            if (iControl == (int)Controls.OFF_BUTTON)
            {
              if (p_selectedAppliance.Length < 2)
                description = "No appliance selected !";
              else
              {
                description = "Switched OFF " + p_selectedAppliance;
                x10module.sendX10Command(
                  p_selectedAppliance.Substring(0, 1),
                  p_selectedAppliance.Substring(1),
                  "OFF");
              }
            }
            if (iControl == (int)Controls.BRIGHT_BUTTON)
            {
              if (p_selectedAppliance.Length < 2)
                description = "No appliance selected !";
              else
              {
                description = "Bright +10% " + p_selectedAppliance;
                x10module.sendX10Command(
                  p_selectedAppliance.Substring(0, 1),
                  p_selectedAppliance.Substring(1),
                  "BRIGHT",
                  "2");
              }
            }
            if (iControl == (int)Controls.DIM_BUTTON)
            {
              if (p_selectedAppliance.Length < 2)
                description = "No appliance selected !";
              else
              {
                description = "Dim -10% " + p_selectedAppliance;
                x10module.sendX10Command(
                  p_selectedAppliance.Substring(0, 1),
                  p_selectedAppliance.Substring(1),
                  "DIM",
                  "2");
              }
            }

            if (iControl == (int)Controls.THIS_ROOM_ON_BUTTON)
            {
              description = "";
              foreach (X10Appliance sx10 in appConfig.m_X10Appliances)
              {
                if (sx10.m_location != p_selectedLocation)
                  continue;
                if (description != "")
                  description += ", ";
                description += sx10.m_strCode;
                x10module.sendX10Command(
                  sx10.m_strCode.Substring(0, 1),
                  sx10.m_strCode.Substring(1),
                  "ON");
              }
              description = "Switched ON " + description;
            }

            if (iControl == (int)Controls.THIS_ROOM_OFF_BUTTON)
            {
              description = "";
              foreach (X10Appliance sx10 in appConfig.m_X10Appliances)
              {
                if (sx10.m_location != p_selectedLocation)
                  continue;
                if (description != "")
                  description += ", ";
                description += sx10.m_strCode;
                x10module.sendX10Command(
                  sx10.m_strCode.Substring(0, 1),
                  sx10.m_strCode.Substring(1),
                  "OFF");
              }
              description = "Switched OFF " + description;
            }

            if (iControl == (int)Controls.THIS_ROOM_DIM50_BUTTON)
            {
              description = "";
              foreach (X10Appliance sx10 in appConfig.m_X10Appliances)
              {
                if (sx10.m_location != p_selectedLocation)
                  continue;
                if (description != "")
                  description += ", ";
                description += sx10.m_strCode;
                x10module.sendX10Command(
                  sx10.m_strCode.Substring(0, 1),
                  sx10.m_strCode.Substring(1),
                  "DIM", "5");
              }
              description = @"Dimmed -50% " + description;
            }

            if (iControl == (int)Controls.ALL_LIGHTS_ON_BUTTON)
            {
              description = "All lights set to On";
              x10module.sendX10Command(
                "A",
                "ALL_LIGHTS_ON");
            }
            if (iControl == (int)Controls.ALL_LIGHTS_OFF_BUTTON)
            {
              description = "All lights set to Off";
              x10module.sendX10Command(
                "A",
                "ALL_LIGHTS_OFF");
            }
            if (iControl == (int)Controls.ALL_UNITS_OFF_BUTTON)
            {
              description = "All units set to Off";
              x10module.sendX10Command(
                "A",
                "ALL_UNITS_OFF");
            }

            if (description != "")
              GUIControl.SetControlLabel(GetID,
                (int)Controls.LABEL_DESCRIPTION,
                description);

          }
          break;
      }

      return base.OnMessage(message);
    }

    GUIListItem GetSelectedItem(int iControl)
    {
      GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return item;
    }

    private void fillAppliances(string location)
    {
      p_selectedLocation = location;

      GUIControl.ClearControl(GetID, (int)Controls.LISTAPPLIANCE);
      foreach (X10Appliance sx10 in appConfig.m_X10Appliances)
      {
        if (sx10.m_location != location)
          continue;
        GUIListItem item = new GUIListItem();
        item.Label = sx10.m_strCode + " (" + sx10.m_strDescription + ")";
        item.MusicTag = sx10.m_strCode;
        GUIControl.AddListItemControl(GetID, (int)Controls.LISTAPPLIANCE, item);
      }

      GUIControl.SetControlLabel(GetID,
        (int)Controls.LABEL_DESCRIPTION,
        "Displaying: " + location);
    }

    #region ISetupform
    public string Author()
    {
      return "Nopap";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public string Description()
    {
      return PLUGIN_NAME;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = PLUGIN_NAME;
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public int GetWindowId()
    {
      return GUIX10.WINDOW_X10PLUGIN;
    }

    public bool HasSetup()
    {
      return true;
    }

    public string PluginName()
    {
      return PLUGIN_NAME_SHORT;
    }

    public void ShowPlugin()
    {
      SetupForm sForm = new SetupForm();
      sForm.ShowDialog();
      sForm.Dispose();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion
  }
}
