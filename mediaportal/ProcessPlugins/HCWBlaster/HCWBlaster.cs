#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.HCWBlaster
{
  /// <summary>
  /// Summary description for HCWBlaster.
  /// </summary>
  public class HCWBlaster : IPlugin, ISetupForm
  {
    private static int _MPWindowID = 9090;
    private static string _Description = "Controls your external settop box via infrared remote commands";
    private static string _Author = "unknown";
    private static string _PluginName = "Hauppauge IR Blaster";
    private static bool _CanEnable = true;
    private static bool _DefEnabled = false;
    private static bool _HasSetup = true;


    private const string _version = "0.1";
    private bool _ExLogging = false;

    private HCWIRBlaster irblaster;

    #region MPInteraction

    public HCWBlaster()
    {
      //
      //irblaster = new HCWIRBlaster();
    }

    public void Start()
    {
      Log.Info("HCWBlaster: HCWBlaster {0} plugin starting.", _version);

      LoadSettings();

      if (_ExLogging == true)
      {
        Log.Info("HCWBlaster: Extended Logging is Enabled.");
      }

      if (_ExLogging == false)
      {
        Log.Info("HCWBlaster: Extended Logging is NOT Enabled.");
      }

      if (_ExLogging == true)
      {
        Log.Info("HCWBlaster: Creating IRBlaster Object.");
      }

      irblaster = new HCWIRBlaster();

      Log.Info("HCWBlaster: Adding message handler for HCWBlaster {0}.", _version);

      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return;
    }

    public void Stop()
    {
      Log.Info("HCWBlaster: HCWBlaster {0} plugin stopping.", _version);
      return;
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _ExLogging = xmlreader.GetValueAsBool("HCWBlaster", "ExtendedLogging", false);
      }
    }

    private void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
          bool bIsInteger;
          double retNum;
          bIsInteger = Double.TryParse(message.Label, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
          this.ChangeTunerChannel(message.Label);
          break;
      }
    }

    public void ChangeTunerChannel(string channel_data)
    {
      if (_ExLogging == true)
      {
        Log.Info("HCWBlaster: Calling IR Blaster Code for: {0}", channel_data);
      }
      irblaster.blast(channel_data, _ExLogging);
    }

    #endregion

    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return _PluginName.ToString();
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return _Description.ToString();
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return _Author.ToString();
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return _CanEnable;
    }

    // get ID of windowplugin belonging to this setup
    public int GetWindowId()
    {
      return _MPWindowID;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return _DefEnabled;
    }

    // indicates if a plugin has its own setup screen
    public bool HasSetup()
    {
      return _HasSetup;
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      //MessageBox.Show("Nothing to configure, this is just an example");
      Form setup = new HCWBlasterSetupForm();
      setup.ShowDialog();
    }

    /// <summary>
    /// If the plugin should have its own button on the main menu of MediaPortal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true  : plugin needs its own button on home
    ///          false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = string.Empty;
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return false;
    }

    #endregion
  }
}