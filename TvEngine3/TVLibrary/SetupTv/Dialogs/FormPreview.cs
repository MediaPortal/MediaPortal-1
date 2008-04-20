using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvLibrary;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using TvControl;

namespace SetupTv.Sections
{
  public partial class FormPreview : Form
  {
    Channel _channel;
    VirtualCard _card = null;
    Player _player = null;
    public FormPreview()
    {
      InitializeComponent();
    }

    public Channel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = value;
      }
    }

      public new DialogResult ShowDialog(IWin32Window owner)
      {
          this.Text = "Preview " + _channel.Name;

          TvServer server = new TvServer();
          TvResult result;
          User user = new User();
          result = server.StartTimeShifting(ref user, _channel.IdChannel, out _card);
          if (result != TvResult.Succeeded)
          {
              MessageBox.Show("Preview failed:" + result.ToString());
              this.Close();
              return DialogResult.None;
          }

          Log.Info("preview {0} user:{1} {2} {3} {4}", _channel.Name, user.CardId, user.SubChannel, user.Name, _card.TimeShiftFileName);
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