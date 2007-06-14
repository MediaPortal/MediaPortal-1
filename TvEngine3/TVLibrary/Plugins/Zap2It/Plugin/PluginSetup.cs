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
using SetupTv;
//using MediaPortal.GUI.Library;
//using MediaPortal.TV.Database;

namespace ProcessPlugins.EpgGrabber
{
  public class PluginSetup
  {
    internal const string PLUGIN_NAME = "Zap2it EPG Client";
    internal const string PLUGIN_VERSION = "1.8a";

    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string Name
    {
      get { return PLUGIN_NAME; }
    }

    // Returns the name of the plugin which is shown in the plugin menu
    public string Version
    {
      get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(2); }
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author
    {
      get { return "Jonathan <bradsjm>"; }
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public bool MasterOnly
    {
      get { return true; }
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Electronic Program Guide client for Zap2it Labs DataDirect US and Canada free TV listing service. Signup at labs.zap2it.com";
    }

    // indicates if a plugin has its own setup screen
    public bool HasSetup()
    {
      return true;
    }

    // show the setup dialog
    public SetupTv.SectionSettings Setup
    {
      get
      {
        Zap2itPluginConfig configForm = new Zap2itPluginConfig();
        //using (configForm)
        //{
        //   configForm.Text += " v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
        //   configForm.textBoxUsername.Text                = PluginSettings.Username;
        //   configForm.textBoxPassword.Text                = PluginSettings.Password;
        //   configForm.numericUpDownDays.Value             = PluginSettings.GuideDays;
        //   configForm.checkBoxAddChannels.Checked         = PluginSettings.AddNewChannels;
        //   configForm.checkBoxRenameChannels.Checked      = PluginSettings.RenameExistingChannels;
        //   configForm.comboBoxNameFormat.Text             = PluginSettings.ChannelNameTemplate;
        //   configForm.checkBoxNotification.Checked        = PluginSettings.NotifyOnCompletion;
        //   configForm.comboBoxExternalInput.Items.AddRange(Enum.GetNames(typeof(TvLibrary.Implementations.AnalogChannel.VideoInputType)));
        //   configForm.comboBoxExternalInput.SelectedIndex = configForm.comboBoxExternalInput.FindStringExact(PluginSettings.ExternalInput.ToString());
        //   configForm.comboBoxExternalInput.Enabled       = configForm.checkBoxAddChannels.Checked;

        //   //if (configForm.ShowDialog() == DialogResult.OK)
        //   //{
        //   //   PluginSettings.UseDvbEpgGrabber = false;
        //   //   PluginSettings.Username = configForm.textBoxUsername.Text;
        //   //   PluginSettings.Password = configForm.textBoxPassword.Text;
        //   //   PluginSettings.GuideDays = (int)configForm.numericUpDownDays.Value;
        //   //   PluginSettings.AddNewChannels = configForm.checkBoxAddChannels.Checked;
        //   //   PluginSettings.RenameExistingChannels = configForm.checkBoxRenameChannels.Checked;
        //   //   PluginSettings.ChannelNameTemplate = configForm.comboBoxNameFormat.Text;
        //   //   PluginSettings.NotifyOnCompletion = configForm.checkBoxNotification.Checked;
        //   //   PluginSettings.ExternalInput = (TvLibrary.Implementations.AnalogChannel.VideoInputType)Enum.Parse(typeof(TvLibrary.Implementations.AnalogChannel.VideoInputType), configForm.comboBoxExternalInput.SelectedItem.ToString());
        //   //   if (configForm.checkboxForceUpdate.Checked)
        //   //   {
        //   //      PluginSettings.NextPoll = DateTime.Now;
        //   //   }
        //   //}
        //}

        return configForm;
      }
    }
    #endregion
  }

}