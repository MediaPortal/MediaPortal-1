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
using Mediaportal.TV.Server.SetupTV.Sections;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormPreview : MPForm
  {

    private Channel _channel;
    private IVirtualCard _card;
    private Player _player;

    public FormPreview()
    {
      InitializeComponent();
    }

    public Channel Channel
    {
      get { return _channel; }
      set { _channel = value; }
    }

    public new DialogResult ShowDialog(IWin32Window owner)
    {
      Text = "Preview " + _channel.DisplayName;
      
      IUser user = UserFactory.CreateBasicUser("setuptv");
      TvResult result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(user.Name, _channel.IdChannel, out _card, out user);
      if (result != TvResult.Succeeded)
      {
        MessageBox.Show("Preview failed:" + result);
        Close();
        return DialogResult.None;
      }

      this.LogInfo("preview {0} user:{1} {2} {3} {4}", _channel.DisplayName, user.CardId, "n/a", user.Name,
               _card.TimeShiftFileName);
      _player = new Player();
      _player.Play(_card.TimeShiftFileName, this);

      return base.ShowDialog(owner);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (_player != null)
      {
        _player.Stop();
        _player = null;
      }
      if (_card != null)
      {
        _card.StopTimeShifting();
      }
      base.OnClosing(e);
    }

    private void FormPreview_Resize(object sender, EventArgs e)
    {
      if (_player != null)
        _player.ResizeToParent();
    }
  }
}