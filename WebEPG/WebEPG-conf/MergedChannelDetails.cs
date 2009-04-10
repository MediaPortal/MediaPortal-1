using System;
using System.Windows.Forms;
using MediaPortal.WebEPG.config;

namespace WebEPG_conf
{
  public partial class MergedChannelDetails : Form
  {
    private fSelection _selection;
    private TreeNode _tGrabbers;

    public MergedChannelDetails(TreeNode grabbers, MergedChannel channel, EventHandler ok_click)
    {
      InitializeComponent();


      if (channel != null)
      {
        tbChannelName.Text = channel.id;
        tbGrabSite.Text = channel.grabber;
        tbStart.Text = channel.start;
        tbEnd.Text = channel.end;
      }
      _tGrabbers = grabbers;
      bOk.Click += ok_click;
    }

    public MergedChannel ChannelDetails
    {
      get
      {
        MergedChannel channel = new MergedChannel();
        channel.id = tbChannelName.Text;
        channel.grabber = tbGrabSite.Text;
        channel.start = tbStart.Text;
        channel.end = tbEnd.Text;
        return channel;
      }
    }

    private void bCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void bChannelID_Click(object sender, EventArgs e)
    {
      _selection = new fSelection(_tGrabbers, true, this.DoSelect);
      _selection.MinimizeBox = false;
      _selection.Text = "Merge Selection";
      _selection.Closed += new EventHandler(this.CloseSelect);
      _selection.Show();
    }

    private void bGrabber_Click(object sender, EventArgs e)
    {
      //_selection = new fSelection(_tChannels, _tGrabbers, false, this.DoSelect);
      //_selection.MinimizeBox = false;
      //_selection.Text = "Merge Selection";
      //_selection.Closed += new System.EventHandler(this.CloseSelect);
      //_selection.Show();
    }

    private void DoSelect(Object source, EventArgs e)
    {
      this.Activate();
      string[] id = _selection.Selected;

      if (id != null)
      {
        tbChannelName.Text = id[0];
        tbGrabSite.Text = id[1];
      }

      _selection.Close();
    }

    private void CloseSelect(Object source, EventArgs e)
    {
      if (source == _selection)
      {
        _selection = null;
      }
    }
  }
}