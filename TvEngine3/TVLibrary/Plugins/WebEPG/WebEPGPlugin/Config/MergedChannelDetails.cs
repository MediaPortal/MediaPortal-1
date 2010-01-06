using System;
using System.Windows.Forms;
using MediaPortal.WebEPG.Config;

namespace SetupTv.Sections.WebEPGConfig
{
  public partial class MergedChannelDetails : Form
  {
    //private fSelection _selection;
    private GetGrabberSelectorCallback _getGrabberSelector;
    //private TreeNode _tGrabbers;


    public MergedChannelDetails(MergedChannel channel, GetGrabberSelectorCallback getGrabberSelectorCallback,
                                EventHandler ok_click)
    {
      InitializeComponent();

      _getGrabberSelector = getGrabberSelectorCallback;

      if (channel != null)
      {
        tbChannelName.Text = channel.id;
        tbGrabSite.Text = channel.grabber;
        tbStart.Text = channel.start;
        tbEnd.Text = channel.end;
      }
      bOk.Click += ok_click;
    }

    //public static bool EditMergedChannel(MergedChannel channel, GetGrabberSelectorCallback getGrabberSelectorCallback)
    //{
    //  using (MergedChannelDetails dlg = new MergedChannelDetails(channel, getGrabberSelectorCallback))
    //  {
    //    if (dlg.ShowDialog() == DialogResult.OK)
    //    {
    //      channel.id = dlg.tbChannelName.Text;
    //      channel.grabber = dlg.tbGrabSite.Text;
    //      channel.start = dlg.tbStart.Text;
    //      channel.end = dlg.tbEnd.Text;

    //      return true;
    //    }
    //    else
    //    {
    //      return false;
    //    }
    //  }
    //}

    //public static MergedChannel AddMergedChannel(GetGrabberSelectorCallback getGrabberSelectorCallback)
    //{
    //  using (MergedChannelDetails dlg = new MergedChannelDetails(null, getGrabberSelectorCallback))
    //  {
    //    if (dlg.ShowDialog() == DialogResult.OK)
    //    {
    //      return dlg.ChannelDetails;
    //    }
    //    else
    //    {
    //      return null;
    //    }
    //  }
    //}

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

    private void ShowGrabberSelection()
    {
      _getGrabberSelector();
      //if (_selection == null)
      //{
      //  _selection = _getGrabberSelector();
      //  _selection.GrabberSelected += this.DoSelect;
      //  _selection.Closed += new EventHandler(this.CloseSelect);
      //}
      //else
      //{
      //  _selection.BringToFront();
      //}
    }

    private void bCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void bChannelID_Click(object sender, EventArgs e)
    {
      ShowGrabberSelection();
    }

    private void bGrabber_Click(object sender, EventArgs e)
    {
      //_selection = new fSelection(_tChannels, _tGrabbers, false, this.DoSelect);
      //_selection.MinimizeBox = false;
      //_selection.Text = "Merge Selection";
      //_selection.Closed += new System.EventHandler(this.CloseSelect);
      //_selection.Show();
    }

    public void DoSelect(Object source, GrabberSelectedEventArgs e)
    {
      this.Activate();
      GrabberSelectionInfo id = e.Selection;

      if (id != null)
      {
        tbChannelName.Text = id.ChannelId;
        tbGrabSite.Text = id.GrabberId;
      }

      this.BringToFront();
      //_selection.Close();
    }

    //private void CloseSelect(Object source, EventArgs e)
    //{
    //  if (source == _selection)
    //  {
    //    _selection.GrabberSelected -= this.DoSelect;
    //    _selection = null;
    //  }
    //}

    //private void MergedChannelDetails_FormClosing(object sender, FormClosingEventArgs e)
    //{
    //  if (_selection != null)
    //  {
    //    _selection.GrabberSelected -= this.DoSelect;
    //    _selection = null;
    //  }
    //}
  }
}