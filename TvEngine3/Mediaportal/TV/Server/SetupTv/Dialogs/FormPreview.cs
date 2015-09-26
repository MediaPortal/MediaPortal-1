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

#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormPreview : Form
  {
    private IVirtualCard _tuner = null;
    private Player _player = null;

    public FormPreview()
    {
      InitializeComponent();
    }

    public bool SetChannel(Channel channel)
    {
      if (channel == null)
      {
        this.LogError("preview: channel not set");
        return false;
      }

      this.LogInfo("preview: set channel, ID = {0}, name = {1}", channel.IdChannel, channel.Name);
      Text = "Preview " + channel.Name;
      
      IUser user;
      TvResult result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(string.Format("{0} - TV Server Configuration preview", System.Net.Dns.GetHostName()), channel.IdChannel, out _tuner, out user);
      if (result != TvResult.Succeeded)
      {
        MessageBox.Show("Preview result: " + result.GetDescription() + ".", SectionSettings.MESSAGE_CAPTION);
        return false;
      }

      this.LogDebug("preview: time shifting, tuner ID = {0}, file name = {1}", _tuner.Id, _tuner.TimeShiftFileName);
      _player = new Player();
      bool success = false;
      try
      {
        success = _player.Play(_tuner.TimeShiftFileName, this);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "preview: failed to start player");
        success = false;
      }
      finally
      {
        if (!success)
        {
          StopPlayer();
          MessageBox.Show("Failed to show channel. " + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      return success;
    }

    public void StopPlayer()
    {
      if (_player != null)
      {
        _player.Stop();
        _player = null;
      }
      if (_tuner != null)
      {
        _tuner.StopTimeShifting();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      StopPlayer();
      base.OnClosing(e);
    }

    private void FormPreview_Resize(object sender, EventArgs e)
    {
      if (_player != null)
      {
        _player.ResizeToParent();
      }
    }

    private void FormPreview_DoubleClick(object sender, EventArgs e)
    {
      if (this.WindowState == FormWindowState.Normal)
      {
        this.WindowState = FormWindowState.Maximized;
      }
      else if (this.WindowState == FormWindowState.Maximized)
      {
        this.WindowState = FormWindowState.Normal;
      }
    }
  }
}