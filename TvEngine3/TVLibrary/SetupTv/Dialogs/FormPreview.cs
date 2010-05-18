#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using SetupControls;
using TvDatabase;
using TvLibrary.Log;
using TvControl;

namespace SetupTv.Sections
{
  public partial class FormPreview : MPForm
  {
    private Channel _channel;
    private VirtualCard _card;
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
      Text = "Preview " + _channel.Name;

      TvServer server = new TvServer();
      User user = new User();
      TvResult result = server.StartTimeShifting(ref user, _channel.IdChannel, out _card);
      if (result != TvResult.Succeeded)
      {
        MessageBox.Show("Preview failed:" + result);
        Close();
        return DialogResult.None;
      }

      Log.Info("preview {0} user:{1} {2} {3} {4}", _channel.Name, user.CardId, user.SubChannel, user.Name,
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
      _player.ResizeToParent();
    }
  }
}