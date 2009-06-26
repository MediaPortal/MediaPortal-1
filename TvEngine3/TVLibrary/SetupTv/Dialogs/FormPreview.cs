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
    Channel _channel;
    VirtualCard _card;
    Player _player;
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