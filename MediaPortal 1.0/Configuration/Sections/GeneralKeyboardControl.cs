#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class GeneralKeyboardControl : MediaPortal.Configuration.SectionSettings
  {

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView sharesListView;
    private System.ComponentModel.IContainer components = null;
    public GeneralKeyboardControl()
      : base("Keyboard shortcuts")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public GeneralKeyboardControl(string name)
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
      System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "Cursor keys",
            "Select item"}, -1);
      System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "Home",
            "Select first item"}, -1);
      System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "End",
            "Select last item"}, -1);
      System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "PageUp",
            "Previous item page"}, -1);
      System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "PageDown",
            "Next item page"}, -1);
      System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "Enter",
            "Perform action"}, -1);
      System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "Escape",
            "Previous window"}, -1);
      System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "U",
            "parent folder"}, -1);
      System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "H",
            "Switch between classic and basic home-screen"}, -1);
      System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "Space",
            "Pause/Resume"}, -1);
      System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "X",
            "Switch between GUI and fullscreen tv/video"}, -1);
      System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "R",
            "Record current TV program"}, -1);
      System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "P",
            "Start playing"}, -1);
      System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "B",
            "Stop playing"}, -1);
      System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "F3",
            "Show information about selected item"}, -1);
      System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "F5",
            "<< Rewind"}, -1);
      System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "F6",
            "Fast Forward >>"}, -1);
      System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "F7",
            "|<< Play previous"}, -1);
      System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "F8",
            "Play next >>|"}, -1);
      System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "F9",
            "Show Context menu"}, -1);
      System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "-",
            "Volume -"}, -1);
      System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "=",
            "Volume +"}, -1);
      System.Windows.Forms.ListViewItem listViewItem23 = new System.Windows.Forms.ListViewItem(new string[] {
            "General",
            "M",
            "Mute"}, -1);
      System.Windows.Forms.ListViewItem listViewItem24 = new System.Windows.Forms.ListViewItem(new string[] {
            "My Music",
            "F1",
            "Switch between playlist and module"}, -1);
      System.Windows.Forms.ListViewItem listViewItem25 = new System.Windows.Forms.ListViewItem(new string[] {
            "My Music",
            "Y",
            "Add song/folder to playlist"}, -1);
      System.Windows.Forms.ListViewItem listViewItem26 = new System.Windows.Forms.ListViewItem(new string[] {
            "My Pictures",
            "0",
            "Delete selected picture"}, -1);
      System.Windows.Forms.ListViewItem listViewItem27 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "F3",
            "Show picture details"}, -1);
      System.Windows.Forms.ListViewItem listViewItem28 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "left",
            "Previous picture or move left in zoom mode"}, -1);
      System.Windows.Forms.ListViewItem listViewItem29 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "right",
            "Next picture or move right in zoom mode"}, -1);
      System.Windows.Forms.ListViewItem listViewItem30 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "Enter",
            "Pause/Resume slideshow"}, -1);
      System.Windows.Forms.ListViewItem listViewItem31 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "PageUp",
            "Zoom out"}, -1);
      System.Windows.Forms.ListViewItem listViewItem32 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "PageDown",
            "Zoom in"}, -1);
      System.Windows.Forms.ListViewItem listViewItem33 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "1-9",
            "Zoom level 1-9"}, -1);
      System.Windows.Forms.ListViewItem listViewItem34 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "up",
            "Move picture up in zoom mode"}, -1);
      System.Windows.Forms.ListViewItem listViewItem35 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "down",
            "Move picture down in zoom mode"}, -1);
      System.Windows.Forms.ListViewItem listViewItem36 = new System.Windows.Forms.ListViewItem(new string[] {
            "Slideshow",
            "R",
            "Rotate picture"}, -1);
      System.Windows.Forms.ListViewItem listViewItem37 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "left",
            "Step backward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem38 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "right",
            "Step forward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem39 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "down",
            "Big step backward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem40 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "up",
            "Big step forward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem41 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "0-9",
            "Enter time"}, -1);
      System.Windows.Forms.ListViewItem listViewItem42 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "Y",
            "Show/hide OSD"}, -1);
      System.Windows.Forms.ListViewItem listViewItem43 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "C",
            "Show/hide MSN chat window"}, -1);
      System.Windows.Forms.ListViewItem listViewItem44 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "S",
            "Switch zoom mode / aspect ratio"}, -1);
      System.Windows.Forms.ListViewItem listViewItem45 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "L",
            "Enable subtitles"}, -1);
      System.Windows.Forms.ListViewItem listViewItem46 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen Video",
            "A",
            "Switch audio language / stream"}, -1);
      System.Windows.Forms.ListViewItem listViewItem47 = new System.Windows.Forms.ListViewItem(new string[] {
            "My Videos",
            "0",
            "Delete selected movie"}, -1);
      System.Windows.Forms.ListViewItem listViewItem48 = new System.Windows.Forms.ListViewItem(new string[] {
            "My Videos",
            "F1",
            "Switch to/from video playlist"}, -1);
      System.Windows.Forms.ListViewItem listViewItem49 = new System.Windows.Forms.ListViewItem(new string[] {
            "My Videos",
            "Y",
            "Add selected item to video playlist"}, -1);
      System.Windows.Forms.ListViewItem listViewItem50 = new System.Windows.Forms.ListViewItem(new string[] {
            "Recorded TV",
            "0",
            "Delete selected recording"}, -1);
      System.Windows.Forms.ListViewItem listViewItem51 = new System.Windows.Forms.ListViewItem(new string[] {
            "TV Guide",
            "F1",
            "Default time interval"}, -1);
      System.Windows.Forms.ListViewItem listViewItem52 = new System.Windows.Forms.ListViewItem(new string[] {
            "TV Guide",
            "F7",
            "Decrease time interval"}, -1);
      System.Windows.Forms.ListViewItem listViewItem53 = new System.Windows.Forms.ListViewItem(new string[] {
            "TV Guide",
            "F8",
            "Increase time interval"}, -1);
      System.Windows.Forms.ListViewItem listViewItem54 = new System.Windows.Forms.ListViewItem(new string[] {
            "TV Guide",
            "Home",
            "Go to current time"}, -1);
      System.Windows.Forms.ListViewItem listViewItem55 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "left",
            "Step backward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem56 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "right",
            "Step forward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem57 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "up",
            "Big step backward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem58 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "down",
            "Big step forward"}, -1);
      System.Windows.Forms.ListViewItem listViewItem59 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "F7",
            "Previous TV channel"}, -1);
      System.Windows.Forms.ListViewItem listViewItem60 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "F8",
            "Next TV channel"}, -1);
      System.Windows.Forms.ListViewItem listViewItem61 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "Y",
            "Show/hide OSD"}, -1);
      System.Windows.Forms.ListViewItem listViewItem62 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "C",
            "Show/hide MSN chat window"}, -1);
      System.Windows.Forms.ListViewItem listViewItem63 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "S",
            "Switch zoom mode / aspect ratio"}, -1);
      System.Windows.Forms.ListViewItem listViewItem64 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "L",
            "Enable subtitles"}, -1);
      System.Windows.Forms.ListViewItem listViewItem65 = new System.Windows.Forms.ListViewItem(new string[] {
            "Fullscreen TV",
            "A",
            "Switch audio language / stream"}, -1);
      System.Windows.Forms.ListViewItem listViewItem66 = new System.Windows.Forms.ListViewItem(new string[] {
            "DVD",
            "D",
            "Show DVD menu"}, -1);
      System.Windows.Forms.ListViewItem listViewItem67 = new System.Windows.Forms.ListViewItem(new string[] {
            "DVD",
            "F7",
            "Previous chapter"}, -1);
      System.Windows.Forms.ListViewItem listViewItem68 = new System.Windows.Forms.ListViewItem(new string[] {
            "DVD",
            "F8",
            "Next chapter"}, -1);
      System.Windows.Forms.ListViewItem listViewItem69 = new System.Windows.Forms.ListViewItem(new string[] {
            "Movie calibration",
            "enter",
            "Swap arrows"}, -1);
      System.Windows.Forms.ListViewItem listViewItem70 = new System.Windows.Forms.ListViewItem(new string[] {
            "Movie calibration",
            "F1",
            "Reset calibration"}, -1);
      System.Windows.Forms.ListViewItem listViewItem71 = new System.Windows.Forms.ListViewItem(new string[] {
            "GUI calibration",
            "F1",
            "Reset calibration"}, -1);
      System.Windows.Forms.ListViewItem listViewItem72 = new System.Windows.Forms.ListViewItem(new string[] {
            "OSD",
            "PageUp",
            "value plus"}, -1);
      System.Windows.Forms.ListViewItem listViewItem73 = new System.Windows.Forms.ListViewItem(new string[] {
            "OSD",
            "PageDown",
            "value min"}, -1);
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
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Overview";
      // 
      // sharesListView
      // 
      this.sharesListView.AllowRowReorder = false;
      this.sharesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.sharesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2});
      this.sharesListView.FullRowSelect = true;
      this.sharesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.sharesListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9,
            listViewItem10,
            listViewItem11,
            listViewItem12,
            listViewItem13,
            listViewItem14,
            listViewItem15,
            listViewItem16,
            listViewItem17,
            listViewItem18,
            listViewItem19,
            listViewItem20,
            listViewItem21,
            listViewItem22,
            listViewItem23,
            listViewItem24,
            listViewItem25,
            listViewItem26,
            listViewItem27,
            listViewItem28,
            listViewItem29,
            listViewItem30,
            listViewItem31,
            listViewItem32,
            listViewItem33,
            listViewItem34,
            listViewItem35,
            listViewItem36,
            listViewItem37,
            listViewItem38,
            listViewItem39,
            listViewItem40,
            listViewItem41,
            listViewItem42,
            listViewItem43,
            listViewItem44,
            listViewItem45,
            listViewItem46,
            listViewItem47,
            listViewItem48,
            listViewItem49,
            listViewItem50,
            listViewItem51,
            listViewItem52,
            listViewItem53,
            listViewItem54,
            listViewItem55,
            listViewItem56,
            listViewItem57,
            listViewItem58,
            listViewItem59,
            listViewItem60,
            listViewItem61,
            listViewItem62,
            listViewItem63,
            listViewItem64,
            listViewItem65,
            listViewItem66,
            listViewItem67,
            listViewItem68,
            listViewItem69,
            listViewItem70,
            listViewItem71,
            listViewItem72,
            listViewItem73});
      this.sharesListView.Location = new System.Drawing.Point(16, 24);
      this.sharesListView.Name = "sharesListView";
      this.sharesListView.Size = new System.Drawing.Size(440, 368);
      this.sharesListView.TabIndex = 0;
      this.sharesListView.UseCompatibleStateImageBehavior = false;
      this.sharesListView.View = System.Windows.Forms.View.Details;
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
  }
}

