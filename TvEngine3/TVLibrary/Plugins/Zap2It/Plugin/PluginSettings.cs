#region Copyright (C) 2005-2007 Team MediaPortal
/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
//using MediaPortal.GUI.Library;
//using MediaPortal.TV.Database;
using TvDatabase;
using TvLibrary.Implementations;

namespace ProcessPlugins.EpgGrabber
{
  public static class PluginSettings
  {
    // The Cached MediaPortal Settings Configuration file
    static private TvBusinessLayer tvLayer = new TvBusinessLayer();
    //static private Setting mpSettings = layer.GetSetting(Zap2itPlugin.PLUGIN_NAME);

    #region Configuration Properties
    /// <summary>
    /// Gets or sets the next poll time.
    /// </summary>
    /// <value>The next poll.</value>
    static public DateTime NextPoll
    {
      get
      {
        return DateTime.Parse(tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_NextPoll", DateTime.Now.ToString("s")).Value);
      }
      set
      {
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_NextPoll", DateTime.Now.ToString("s"));
        mpSetting.Value = value.ToString("s");
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets the name of the user to use when authenticating to Zap2it.
    /// </summary>
    /// <value>The name of the user.</value>
    static public string Username
    {
      get
      {
        return tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_UserName", string.Empty).Value;
      }
      set
      {
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_UserName", String.Empty);
        mpSetting.Value = value;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets the password to use when authenticating to Zap2it.
    /// </summary>
    /// <value>The password.</value>
    static public string Password
    {
      get
      {
        // Decrypt password using DPAPI
        string str = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_EncryptedPassword", String.Empty).Value;
        try
        {
          return Encoding.Unicode.GetString(
              ProtectedData.Unprotect(Convert.FromBase64String(str), null, DataProtectionScope.LocalMachine));

          //return Encoding.Unicode.GetString(
          //   ProtectedData.Unprotect(Convert.FromBase64String(str), null, DataProtectionScope.CurrentUser));

        }
        catch
        {
          return string.Empty;
        }
      }
      set
      {
        // Encrypt password using DPAPI
        //mpSettings.SetValue(Zap2itPlugin.PLUGIN_NAME, "EncryptedPassword",
        //    Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(value), null, DataProtectionScope.CurrentUser)));

        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_EncryptedPassword", String.Empty);
        mpSetting.Value = Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(value), null, DataProtectionScope.LocalMachine));
        //mpSetting.Value = Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(value), null, DataProtectionScope.CurrentUser));
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets the number of guide days to request from Zap2it.
    /// </summary>
    /// <value>The guide days.</value>
    static public int GuideDays
    {
      get
      {
        return Int32.Parse(tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_GuideDays", "5").Value);
      }
      set
      {
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_GuideDays", "5");
        mpSetting.Value = value.ToString();
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether new channels should be automatically added.
    /// </summary>
    /// <value><c>true</c> if [add new channels]; otherwise, <c>false</c>.</value>
    static public bool AddNewChannels
    {
      get
      {
        string bstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_AddNewChannels", "true").Value;
        if (bstr.ToLower() == "true".ToLower())
          return true;
        else
          return false;
      }
      set
      {
        string sstr = "true";
        if (value != true)
          sstr = "false";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_AddNewChannels", "true");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether existing channel names should be renamed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if [rename existing channels]; otherwise, <c>false</c>.
    /// </value>
    static public bool RenameExistingChannels
    {
      get
      {
        string gstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_RenameExistingChannels", "false").Value;
        if (gstr.ToLower() == "false".ToLower())
          return false;
        else
          return true;
      }
      set
      {
        string sstr = "false";
        if (value != false)
          sstr = "true";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_RenameExistingChannels", "false");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user wants to be notified when the import is complete.
    /// </summary>
    /// <value><c>true</c> if [notify on completion]; otherwise, <c>false</c>.</value>
    static public bool NotifyOnCompletion
    {
      get
      {
        string gstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_NotifyOnCompletion", "true").Value;
        if (gstr.ToLower() == "true".ToLower())
          return true;
        else
          return false;
      }
      set
      {
        string sstr = "true";
        if (value != true)
          sstr = "false";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_NotifyOnCompletion", "true");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether debug mode is enabled.
    /// </summary>
    /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
    static public bool DebugMode
    {
      get
      {
        string gstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_DebugMode", "false").Value;
        if (gstr.ToLower() == "false".ToLower())
          return false;
        else
          return true;
      }
      set
      {
        string sstr = "false";
        if (value != false)
          sstr = "true";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_DebugMode", "false");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets the channel name template.
    /// </summary>
    /// <value>The channel template.</value>
    static public string ChannelNameTemplate
    {
      get
      {
        return tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ChannelTemplate", "{number} {callsign}").Value;
      }
      set
      {
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ChannelTemplate", "{number} {callsign}");
        mpSetting.Value = value;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets the input for external auto-added channels.
    /// </summary>
    /// <value>The external channel input.</value>
    static public AnalogChannel.VideoInputType ExternalInput
    {
      get
      {
        string gstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ExternalInput", "SvhsInput1").Value;
        return (AnalogChannel.VideoInputType)Enum.Parse(typeof(AnalogChannel.VideoInputType), gstr, true);
      }
      set
      {
        string sstr = Enum.GetName(typeof(AnalogChannel.VideoInputType), value);
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ExternalInput", "SvhsInput1");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets the country for external auto-added channels.
    /// </summary>
    /// <value>The external channel country.</value>
    static public TvLibrary.Country ExternalInputCountry
    {
      get
      {
        string gstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ExternalInputCountry", "United States").Value;
        TvLibrary.CountryCollection cc = new TvLibrary.CountryCollection();
        return cc.GetTunerCountry(gstr);
      }
      set
      {
        string sstr = value.Name;
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ExternalInputCountry", "United States");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to allow EPG Mapping by Channel Number ONLY.
    /// </summary>
    /// <value><c>true</c> if [allow mapping by channel number]; otherwise, <c>false</c>.</value>
    static public bool AllowChannelNumberOnlyMapping
    {
      get
      {
        string bstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_AllowChannelNumberOnlyMapping", "false").Value;
        if (bstr.ToLower() == "true".ToLower())
          return true;
        else
          return false;
      }
      set
      {
        string sstr = "false";
        if (value == true)
          sstr = "true";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_AllowChannelNumberOnlyMapping", "false");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to sort channels by channel number after an update.
    /// </summary>
    /// <value><c>true</c> if [allow sorting by channel number]; otherwise, <c>false</c>.</value>
    static public bool SortChannelsByNumber
    {
      get
      {
        string bstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_SortChannelsByNumber", "false").Value;
        if (bstr.ToLower() == "true".ToLower())
          return true;
        else
          return false;
      }
      set
      {
        string sstr = "false";
        if (value == true)
          sstr = "true";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_SortChannelsByNumber", "false");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to delete channels without external EPG mapping present.
    /// </summary>
    /// <value><c>true</c> if [delete channels without external EPG mapping]; otherwise, <c>false</c>.</value>
    static public bool DeleteChannelsWithNoEPGMapping
    {
      get
      {
        string bstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_DeleteChannelsWithNoEPGMapping", "false").Value;
        if (bstr.ToLower() == "true".ToLower())
          return true;
        else
          return false;
      }
      set
      {
        string sstr = "false";
        if (value == true)
          sstr = "true";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_DeleteChannelsWithNoEPGMapping", "false");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user wants meta information appended to the description, ie. repeat, advisories, etc...
    /// </summary>
    /// <value><c>true</c> if [append meta information to description]; otherwise, <c>false</c>.</value>
    static public bool AppendMetaInformationToDescription
    {
      get
      {
        string gstr = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_AppendMetaInformationToDescription", "true").Value;
        if (gstr.ToLower() == "true".ToLower())
          return true;
        else
          return false;
      }
      set
      {
        string sstr = "true";
        if (value != true)
          sstr = "false";
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_AppendMetaInformationToDescription", "true");
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }

    #endregion

    #region Internal Properties
    /// <summary>
    /// Gets or sets the channel collection fingerprint (MD5 hash).
    /// </summary>
    /// <value>The channel fingerprint.</value>
    static internal string ChannelFingerprint
    {
      get
      {
        return tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ChannelFingerprint", string.Empty).Value;
      }
      set
      {
        Setting mpSetting = tvLayer.GetSetting(Zap2itPlugin.PLUGIN_NAME + "_ChannelFingerprint", string.Empty);
        mpSetting.Value = value;
        mpSetting.Persist();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether DVB epg grabber is enabled.
    /// </summary>
    /// <value><c>true</c> if DVB epg grabber is enabled; otherwise, <c>false</c>.</value>
    static internal bool UseDvbEpgGrabber
    {
      get
      {
        string gstr = tvLayer.GetSetting("xmltvepgdvb", "false").Value;
        if (gstr.ToLower() == "false".ToLower())
          return false;
        else
          return true;
      }
      set
      {
        Setting mpSetting = tvLayer.GetSetting("xmltvepgdvb", "false");
        string sstr = "false";
        if (value != false)
          sstr = "true";
        mpSetting.Value = sstr;
        mpSetting.Persist();
      }
    }
    #endregion
  }
}