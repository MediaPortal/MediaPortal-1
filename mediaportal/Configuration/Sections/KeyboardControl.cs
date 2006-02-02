#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
  public class KeyboardControl : MediaPortal.Configuration.SectionSettings
  {

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView sharesListView;
    private System.ComponentModel.IContainer components = null;
    public KeyboardControl()
      : base("Keyboard shortcuts")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public KeyboardControl(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      ArrayList items = new ArrayList();

      items.Add(new ListViewItem(new string[] { "General", "Cursor keys", "Select item" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "Home", "Select first item" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "End", "Select last item" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "PageUp", "Previous item page" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "PageDown", "Next item page" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "Enter", "Perform action" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "F3", "Show information about selected item" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "F5", "<<Rewind" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "F6", "Fast Forward>>" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "F7", "|<< Play previous" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "F8", "Play next >>|" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "F9", "Show Context menu" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "Escape", "Previous window" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "Space", "Pause/Resume" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "X", "Switch between GUI and fullscreen tv/video" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "U", "parent folder" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "-", "Volume -" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "=", "Volume +" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "M", "Mute" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "R", "Record current TV program" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "B", "Stop playing" }, -1));
      items.Add(new ListViewItem(new string[] { "General", "up", "Big step forward" }, -1));
      items.Add(new ListViewItem(new string[] { "My Music", "F1", "Switch between playlist and module" }, -1));
      items.Add(new ListViewItem(new string[] { "My Music", "Y", "Add song/folder to playlist" }, -1));
      items.Add(new ListViewItem(new string[] { "My Pictures", "0", "Delete selected picture" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "F3", "Show picture details" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "left", "Previous picture or move left in zoom mode" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "right", "Next picture or move right in zoom mode" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "Enter", "Pause/Resume slideshow" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "PageUp", "Zoom out" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "PageUp", "Zoom in" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "1-9", "Zoom level 1-9" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "up", "Move picture up in zoommode" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "down", "Move picture down in zoommode" }, -1));
      items.Add(new ListViewItem(new string[] { "Slideshow", "R", "Rotate picture" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen Video", "S", "Zoom A/R mode" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen Video", "left", "Step backward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen Video", "right", "Step forward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen Video", "up", "Big step backward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen video", "Y", "Show/hide OSD" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen video", "C", "Toggle MSN chat window" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen video", "0-9", "Enter time" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen video", "L", "Enable Subtitiles" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen video", "A", "Switch Audio Language / Stream" }, -1));
      items.Add(new ListViewItem(new string[] { "Movie Calibration", "enter", "Swap arrows" }, -1));
      items.Add(new ListViewItem(new string[] { "Movie Calibration", "F1", "reset calibration" }, -1));
      items.Add(new ListViewItem(new string[] { "GUI Calibration", "F1", "reset calibration" }, -1));
      items.Add(new ListViewItem(new string[] { "OSD", "PageUp", "value plus" }, -1));
      items.Add(new ListViewItem(new string[] { "OSD", "PageDown", "value min" }, -1));
      items.Add(new ListViewItem(new string[] { "My Videos", "F1", "Switch to/from video playlist" }, -1));
      items.Add(new ListViewItem(new string[] { "My Videos", "Y", "Add selected item to video playlist" }, -1));
      items.Add(new ListViewItem(new string[] { "My Videos/Title", "0", "Delete selected movie" }, -1));
      items.Add(new ListViewItem(new string[] { "Recorded TV", "0", "Delete selected recording" }, -1));
      items.Add(new ListViewItem(new string[] { "TV Guide", "F7", "Decrease time interval" }, -1));
      items.Add(new ListViewItem(new string[] { "TV Guide", "F8", "Increase time interval" }, -1));
      items.Add(new ListViewItem(new string[] { "TV Guide", "Home", "goto current time" }, -1));
      items.Add(new ListViewItem(new string[] { "TV Guide", "F1", "Default time interval" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "S", "Switch Zoom mode/AR" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "left", "Step backward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "right", "Step forward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "up", "Big step Backward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "down", "Big step forward" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "Y", "Show/hide OSD" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "F7", "Previous TV channel" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "F8", "Next TV channel" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "C", "Show/hide MSN chat window" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "L", "Enable Subtitiles" }, -1));
      items.Add(new ListViewItem(new string[] { "Fullscreen TV", "A", "Switch Audio Language / Stream" }, -1));
      items.Add(new ListViewItem(new string[] { "DVD", "D", "Show DVD menu" }, -1));
      items.Add(new ListViewItem(new string[] { "DVD", "F7", "Previous chapter" }, -1));
      items.Add(new ListViewItem(new string[] { "DVD", "F8", "Next chapter" }, -1));

      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.sharesListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.sharesListView);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Overview";
      // 
      // sharesListView
      // 
      this.sharesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.sharesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							 this.columnHeader1,
																							 this.columnHeader3,
																							 this.columnHeader2});
      this.sharesListView.FullRowSelect = true;
      this.sharesListView.Items.AddRange((ListViewItem[])items.ToArray(typeof(ListViewItem)));

      this.sharesListView.Location = new System.Drawing.Point(16, 24);
      this.sharesListView.Name = "sharesListView";
      this.sharesListView.Size = new System.Drawing.Size(440, 368);
      this.sharesListView.TabIndex = 0;
      this.sharesListView.View = System.Windows.Forms.View.Details;
      this.sharesListView.SelectedIndexChanged += new System.EventHandler(this.sharesListView_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Module";
      this.columnHeader1.Width = 126;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Key";
      this.columnHeader3.Width = 69;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Function";
      this.columnHeader2.Width = 224;
      // 
      // KeyboardControl
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "KeyboardControl";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void sharesListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {

    }



  }
}

