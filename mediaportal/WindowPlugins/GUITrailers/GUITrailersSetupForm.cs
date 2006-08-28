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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Util;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for GUITrailersSetupForm.
  /// </summary>
  public class GUITrailersSetupForm : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox300;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private MediaPortal.UserInterface.Controls.MPCheckBox GermanTrailerCheckBox;
    private CheckedListBox YahooServerListBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox TsrVodCheckBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox TsrVodBitrateStreamGrpBox;
    private MediaPortal.UserInterface.Controls.MPRadioButton bitrate1500;
    private MediaPortal.UserInterface.Controls.MPRadioButton bitrate450;
    private MediaPortal.UserInterface.Controls.MPRadioButton bitrate160;
    private MediaPortal.UserInterface.Controls.MPRadioButton bitrate80;
    private NumericUpDown upDowNmbOfResults;
    private MediaPortal.UserInterface.Controls.MPGroupBox TsrVodnbrOfResultGrpBox;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton2;
    private MediaPortal.UserInterface.Controls.MPRadioButton rbnmbOfResultsDef;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public GUITrailersSetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.checkBox300 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.GermanTrailerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.YahooServerListBox = new System.Windows.Forms.CheckedListBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.TsrVodCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.TsrVodBitrateStreamGrpBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.bitrate1500 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.bitrate450 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.bitrate160 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.bitrate80 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.upDowNmbOfResults = new System.Windows.Forms.NumericUpDown();
      this.TsrVodnbrOfResultGrpBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButton2 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.rbnmbOfResultsDef = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.TsrVodBitrateStreamGrpBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.upDowNmbOfResults)).BeginInit();
      this.TsrVodnbrOfResultGrpBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBox300
      // 
      this.checkBox300.AutoSize = true;
      this.checkBox300.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBox300.Location = new System.Drawing.Point(48, 12);
      this.checkBox300.Name = "checkBox300";
      this.checkBox300.Size = new System.Drawing.Size(300, 17);
      this.checkBox300.TabIndex = 1;
      this.checkBox300.Text = "Always start streaming movies with 300kb/s, skip 700kb/s.";
      this.checkBox300.UseVisualStyleBackColor = true;
      this.checkBox300.CheckedChanged += new System.EventHandler(this.checkBox300_CheckedChanged);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(361, 248);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 32);
      this.button1.TabIndex = 2;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // GermanTrailerCheckBox
      // 
      this.GermanTrailerCheckBox.AutoSize = true;
      this.GermanTrailerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.GermanTrailerCheckBox.Location = new System.Drawing.Point(48, 33);
      this.GermanTrailerCheckBox.Name = "GermanTrailerCheckBox";
      this.GermanTrailerCheckBox.Size = new System.Drawing.Size(122, 17);
      this.GermanTrailerCheckBox.TabIndex = 3;
      this.GermanTrailerCheckBox.Text = "Show german trailers";
      this.GermanTrailerCheckBox.UseVisualStyleBackColor = true;
      this.GermanTrailerCheckBox.Visible = false;
      // 
      // YahooServerListBox
      // 
      this.YahooServerListBox.CheckOnClick = true;
      this.YahooServerListBox.Items.AddRange(new object[] {
            "wmcontent74.bcst.yahoo.com",
            "wmcontent78.bcst.yahoo.com"});
      this.YahooServerListBox.Location = new System.Drawing.Point(48, 78);
      this.YahooServerListBox.Name = "YahooServerListBox";
      this.YahooServerListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
      this.YahooServerListBox.Size = new System.Drawing.Size(174, 49);
      this.YahooServerListBox.TabIndex = 4;
      this.YahooServerListBox.Visible = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(45, 62);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Yahoo Servers:";
      this.label1.Visible = false;
      // 
      // TsrVodCheckBox
      // 
      this.TsrVodCheckBox.AutoSize = true;
      this.TsrVodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.TsrVodCheckBox.Location = new System.Drawing.Point(48, 133);
      this.TsrVodCheckBox.Name = "TsrVodCheckBox";
      this.TsrVodCheckBox.Size = new System.Drawing.Size(196, 17);
      this.TsrVodCheckBox.TabIndex = 6;
      this.TsrVodCheckBox.Text = "Show TSR Video on Demand (VOD)";
      this.TsrVodCheckBox.UseVisualStyleBackColor = true;
      this.TsrVodCheckBox.CheckedChanged += new System.EventHandler(this.TsrVodCheckBox_CheckedChanged);
      // 
      // TsrVodBitrateStreamGrpBox
      // 
      this.TsrVodBitrateStreamGrpBox.Controls.Add(this.bitrate1500);
      this.TsrVodBitrateStreamGrpBox.Controls.Add(this.bitrate450);
      this.TsrVodBitrateStreamGrpBox.Controls.Add(this.bitrate160);
      this.TsrVodBitrateStreamGrpBox.Controls.Add(this.bitrate80);
      this.TsrVodBitrateStreamGrpBox.Enabled = false;
      this.TsrVodBitrateStreamGrpBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.TsrVodBitrateStreamGrpBox.Location = new System.Drawing.Point(48, 156);
      this.TsrVodBitrateStreamGrpBox.Name = "TsrVodBitrateStreamGrpBox";
      this.TsrVodBitrateStreamGrpBox.Size = new System.Drawing.Size(196, 73);
      this.TsrVodBitrateStreamGrpBox.TabIndex = 7;
      this.TsrVodBitrateStreamGrpBox.TabStop = false;
      this.TsrVodBitrateStreamGrpBox.Text = "Bitrate Stream";
      // 
      // bitrate1500
      // 
      this.bitrate1500.AutoSize = true;
      this.bitrate1500.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.bitrate1500.Location = new System.Drawing.Point(105, 42);
      this.bitrate1500.Name = "bitrate1500";
      this.bitrate1500.Size = new System.Drawing.Size(70, 17);
      this.bitrate1500.TabIndex = 3;
      this.bitrate1500.TabStop = true;
      this.bitrate1500.Text = "1500kb/s";
      this.bitrate1500.UseVisualStyleBackColor = true;
      this.bitrate1500.Visible = false;
      // 
      // bitrate450
      // 
      this.bitrate450.AutoSize = true;
      this.bitrate450.Checked = true;
      this.bitrate450.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.bitrate450.Location = new System.Drawing.Point(105, 19);
      this.bitrate450.Name = "bitrate450";
      this.bitrate450.Size = new System.Drawing.Size(64, 17);
      this.bitrate450.TabIndex = 2;
      this.bitrate450.TabStop = true;
      this.bitrate450.Text = "450kb/s";
      this.bitrate450.UseVisualStyleBackColor = true;
      // 
      // bitrate160
      // 
      this.bitrate160.AutoSize = true;
      this.bitrate160.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.bitrate160.Location = new System.Drawing.Point(6, 19);
      this.bitrate160.Name = "bitrate160";
      this.bitrate160.Size = new System.Drawing.Size(64, 17);
      this.bitrate160.TabIndex = 1;
      this.bitrate160.TabStop = true;
      this.bitrate160.Text = "160kb/s";
      this.bitrate160.UseVisualStyleBackColor = true;
      // 
      // bitrate80
      // 
      this.bitrate80.AutoSize = true;
      this.bitrate80.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.bitrate80.Location = new System.Drawing.Point(6, 42);
      this.bitrate80.Name = "bitrate80";
      this.bitrate80.Size = new System.Drawing.Size(58, 17);
      this.bitrate80.TabIndex = 0;
      this.bitrate80.TabStop = true;
      this.bitrate80.Text = "80kb/s";
      this.bitrate80.UseVisualStyleBackColor = true;
      this.bitrate80.Visible = false;
      // 
      // upDowNmbOfResults
      // 
      this.upDowNmbOfResults.Enabled = false;
      this.upDowNmbOfResults.Location = new System.Drawing.Point(41, 42);
      this.upDowNmbOfResults.Name = "upDowNmbOfResults";
      this.upDowNmbOfResults.Size = new System.Drawing.Size(68, 20);
      this.upDowNmbOfResults.TabIndex = 8;
      // 
      // TsrVodnbrOfResultGrpBox
      // 
      this.TsrVodnbrOfResultGrpBox.Controls.Add(this.radioButton2);
      this.TsrVodnbrOfResultGrpBox.Controls.Add(this.rbnmbOfResultsDef);
      this.TsrVodnbrOfResultGrpBox.Controls.Add(this.upDowNmbOfResults);
      this.TsrVodnbrOfResultGrpBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.TsrVodnbrOfResultGrpBox.Location = new System.Drawing.Point(250, 156);
      this.TsrVodnbrOfResultGrpBox.Name = "TsrVodnbrOfResultGrpBox";
      this.TsrVodnbrOfResultGrpBox.Size = new System.Drawing.Size(126, 73);
      this.TsrVodnbrOfResultGrpBox.TabIndex = 9;
      this.TsrVodnbrOfResultGrpBox.TabStop = false;
      this.TsrVodnbrOfResultGrpBox.Text = "Number of Result";
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton2.Location = new System.Drawing.Point(21, 46);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(13, 12);
      this.radioButton2.TabIndex = 10;
      this.radioButton2.UseVisualStyleBackColor = true;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // rbnmbOfResultsDef
      // 
      this.rbnmbOfResultsDef.AutoSize = true;
      this.rbnmbOfResultsDef.Checked = true;
      this.rbnmbOfResultsDef.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.rbnmbOfResultsDef.Location = new System.Drawing.Point(21, 23);
      this.rbnmbOfResultsDef.Name = "rbnmbOfResultsDef";
      this.rbnmbOfResultsDef.Size = new System.Drawing.Size(56, 17);
      this.rbnmbOfResultsDef.TabIndex = 9;
      this.rbnmbOfResultsDef.TabStop = true;
      this.rbnmbOfResultsDef.Text = "default";
      this.rbnmbOfResultsDef.UseVisualStyleBackColor = true;
      // 
      // GUITrailersSetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(448, 292);
      this.Controls.Add(this.TsrVodnbrOfResultGrpBox);
      this.Controls.Add(this.TsrVodBitrateStreamGrpBox);
      this.Controls.Add(this.TsrVodCheckBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.YahooServerListBox);
      this.Controls.Add(this.GermanTrailerCheckBox);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.checkBox300);
      this.Name = "GUITrailersSetupForm";
      this.Text = "My Trailers Setup";
      this.Load += new System.EventHandler(this.GUITrailersSetupForm_Load);
      this.TsrVodBitrateStreamGrpBox.ResumeLayout(false);
      this.TsrVodBitrateStreamGrpBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.upDowNmbOfResults)).EndInit();
      this.TsrVodnbrOfResultGrpBox.ResumeLayout(false);
      this.TsrVodnbrOfResultGrpBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private void checkBox300_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void GUITrailersSetupForm_Load(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        if (xmlreader.GetValue("mytrailers", "speed") == "300")
          checkBox300.Checked = true;
        else checkBox300.Checked = false;
        if (xmlreader.GetValueAsBool("mytrailers", "Show german trailers", false) == true)
          GermanTrailerCheckBox.Checked = true;
        else GermanTrailerCheckBox.Checked = false;
        if (xmlreader.GetValueAsBool("mytrailers", "Show tsr vod", false) == true)
          TsrVodCheckBox.Checked = true;
        else TsrVodCheckBox.Checked = false;
        if (xmlreader.GetValue("mytrailers", "TSR speed") == "1500")
          bitrate1500.Checked = true;
        else if (xmlreader.GetValue("mytrailers", "TSR speed") == "450")
          bitrate450.Checked = true;
        else if (xmlreader.GetValue("mytrailers", "TSR speed") == "160")
          bitrate160.Checked = true;
        else
          bitrate80.Checked = true;

        if (xmlreader.GetValue("mytrailers", "TSR nmbOfResults") == "-1")
          rbnmbOfResultsDef.Checked = true;
        else
        {
          radioButton2.Checked = true;
          upDowNmbOfResults.Value = xmlreader.GetValueAsInt("mytrailers", "TSR nmbOfResults", 6);
        }

        //if (xmlreader.GetValue("mytrailers", "YahooServer") == YahooServerListBox.Items[0].ToString)
        //    YahooServerListBox.SetItemChecked(0, true);
        //else if (xmlreader.GetValue("mytrailers", "YahooServer") == YahooServerListBox.Items[1].ToString)
        //    YahooServerListBox.SetItemChecked(1, true);
      }
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        if (checkBox300.Checked == true)
          xmlwriter.SetValue("mytrailers", "speed", "300".ToString());
        else
          xmlwriter.SetValue("mytrailers", "speed", "700".ToString());
        if (bitrate1500.Checked == true)
          xmlwriter.SetValue("mytrailers", "TSR speed", "1500".ToString());
        else if (bitrate450.Checked == true)
          xmlwriter.SetValue("mytrailers", "TSR speed", "450".ToString());
        else if (bitrate160.Checked == true)
          xmlwriter.SetValue("mytrailers", "TSR speed", "160".ToString());
        else
          xmlwriter.SetValue("mytrailers", "TSR speed", "80".ToString());
        if (rbnmbOfResultsDef.Checked == true)
          xmlwriter.SetValue("mytrailers", "TSR nmbOfResults", "-1".ToString());
        else
          xmlwriter.SetValue("mytrailers", "TSR nmbOfResults", upDowNmbOfResults.Value);

        //if(GermanTrailerCheckBox.Checked==true)
        xmlwriter.SetValueAsBool("mytrailers", "Show german trailers", GermanTrailerCheckBox.Checked);
        xmlwriter.SetValueAsBool("mytrailers", "Show tsr vod", TsrVodCheckBox.Checked);
        //if(GermanTrailerCheckBox.Checked==false)
        //	xmlwriter.SetValueAsBool("mytrailers","Show german trailers", false);
        if (YahooServerListBox.GetItemChecked(0) == true)
          xmlwriter.SetValue("mytrailers", "YahooServer", YahooServerListBox.GetItemText(YahooServerListBox.Items[1]));
        else if (YahooServerListBox.GetItemChecked(1) == true)
          xmlwriter.SetValue("mytrailers", "YahooServer", YahooServerListBox.GetItemText(YahooServerListBox.Items[1]));
        else
          xmlwriter.SetValue("mytrailers", "YahooServer", string.Empty);

      }
      this.Close();

    }

    private void TsrVodCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      TsrVodBitrateStreamGrpBox.Enabled = TsrVodCheckBox.Checked;
      TsrVodnbrOfResultGrpBox.Enabled = TsrVodCheckBox.Checked;
    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      upDowNmbOfResults.Enabled = radioButton2.Checked;
    }


  }
}
