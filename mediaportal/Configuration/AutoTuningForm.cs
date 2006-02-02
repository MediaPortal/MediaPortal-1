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
using MediaPortal.Configuration.Sections;
namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for AutoTuningForm.
  /// </summary>
  public class AutoTuningForm : System.Windows.Forms.Form
  {
    protected MediaPortal.UserInterface.Controls.MPButton cancelButton;
    protected MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    protected MediaPortal.UserInterface.Controls.MPButton okButton;
    protected System.Windows.Forms.ListBox itemsListBox;
    protected System.Windows.Forms.ProgressBar progressBar;
    protected MediaPortal.UserInterface.Controls.MPButton startButton;
    protected MediaPortal.UserInterface.Controls.MPButton stopButton;
    protected System.Timers.Timer tunerTimer;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox sensitivityComboBox;

    protected int Sensitivity = 5;	// Medium

    /// <summary>
    /// Required designer variable.
    /// </summary>
    protected System.ComponentModel.Container components = null;

    public AutoTuningForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // Setup defaults
      //
      stopButton.Enabled = false;
      sensitivityComboBox.SelectedItem = "Medium";
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
    protected void InitializeComponent()
    {
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.sensitivityComboBox = new System.Windows.Forms.ComboBox();
      this.stopButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.startButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.itemsListBox = new System.Windows.Forms.ListBox();
      this.label1 = new System.Windows.Forms.Label();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.tunerTimer = new System.Timers.Timer();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tunerTimer)).BeginInit();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(357, 192);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.sensitivityComboBox);
      this.groupBox1.Controls.Add(this.stopButton);
      this.groupBox1.Controls.Add(this.startButton);
      this.groupBox1.Controls.Add(this.progressBar);
      this.groupBox1.Controls.Add(this.itemsListBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(424, 176);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Auto Tuning";
      // 
      // sensitivityComboBox
      // 
      this.sensitivityComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.sensitivityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.sensitivityComboBox.Items.AddRange(new object[] {
																														 "High",
																														 "Medium",
																														 "Low"});
      this.sensitivityComboBox.Location = new System.Drawing.Point(336, 134);
      this.sensitivityComboBox.Name = "sensitivityComboBox";
      this.sensitivityComboBox.Size = new System.Drawing.Size(72, 21);
      this.sensitivityComboBox.TabIndex = 2;
      this.sensitivityComboBox.SelectedIndexChanged += new System.EventHandler(this.sensitivityComboBox_SelectedIndexChanged);
      // 
      // stopButton
      // 
      this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.stopButton.Location = new System.Drawing.Point(336, 76);
      this.stopButton.Name = "stopButton";
      this.stopButton.TabIndex = 1;
      this.stopButton.Text = "Stop";
      this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
      // 
      // startButton
      // 
      this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.startButton.Location = new System.Drawing.Point(336, 48);
      this.startButton.Name = "startButton";
      this.startButton.TabIndex = 0;
      this.startButton.Text = "Start";
      this.startButton.Click += new System.EventHandler(this.startButton_Click);
      // 
      // progressBar
      // 
      this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar.Location = new System.Drawing.Point(16, 24);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(395, 16);
      this.progressBar.TabIndex = 1;
      // 
      // itemsListBox
      // 
      this.itemsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.itemsListBox.Location = new System.Drawing.Point(16, 48);
      this.itemsListBox.Name = "itemsListBox";
      this.itemsListBox.Size = new System.Drawing.Size(312, 108);
      this.itemsListBox.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(336, 118);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 23);
      this.label1.TabIndex = 9;
      this.label1.Text = "Sensitivity";
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(277, 192);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 0;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // tunerTimer
      // 
      this.tunerTimer.Interval = 1000;
      this.tunerTimer.SynchronizingObject = this;
      this.tunerTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.tunerTimer_Elapsed);
      // 
      // AutoTuningForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(440, 222);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(448, 248);
      this.Name = "AutoTuningForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Find all radio channels";
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.tunerTimer)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    protected void okButton_Click(object sender, System.EventArgs e)
    {
      TVChannels.UpdateList();
      RadioStations.UpdateList();
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    protected void cancelButton_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    protected void startButton_Click(object sender, System.EventArgs e)
    {
      //
      // Reset progress bar
      //
      progressBar.Value = progressBar.Minimum;

      //
      // Fetch sensitivity
      //
      SetSensitivity();

      //
      // Update control status
      //
      cancelButton.Enabled = okButton.Enabled = startButton.Enabled = sensitivityComboBox.Enabled = false;
      stopButton.Enabled = true;

      OnStartTuning(progressBar.Value);

      //
      // Start timer
      //
      tunerTimer.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetSensitivity()
    {
      switch (sensitivityComboBox.Text)
      {
        case "High":
          Sensitivity = 10;
          break;

        case "Medium":
          Sensitivity = 5;
          break;

        case "Low":
          Sensitivity = 1;
          break;
      }
    }

    public virtual void OnStartTuning(int startValue)
    {
    }

    public virtual void OnStopTuning()
    {
    }

    protected void stopButton_Click(object sender, System.EventArgs e)
    {
      //
      // Reset progress bar
      //
      progressBar.Value = progressBar.Minimum;

      DoStop();
    }

    protected void DoStop()
    {
      //
      // Update control status
      //
      cancelButton.Enabled = okButton.Enabled = startButton.Enabled = sensitivityComboBox.Enabled = true;
      stopButton.Enabled = false;

      OnStopTuning();

      //
      // Stop timer
      //
      tunerTimer.Stop();
    }

    public void AddItem(object item)
    {
      itemsListBox.Items.Add(item);
    }

    public void ClearItems()
    {
      itemsListBox.Items.Clear();
    }

    public void SetInterval(int minimum, int maximum)
    {
      progressBar.Minimum = minimum;
      progressBar.Maximum = maximum;
    }

    public void SetStep(int stepLength)
    {
      progressBar.Step = stepLength;
    }

    public void Step(int stepLength)
    {
      SetStep(stepLength);
      progressBar.PerformStep();
    }

    protected void tunerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      //
      // Calculate the step size, sensitivity range from .1 MHz->1MHz per step
      //
      float width = (1000000f - 100000f) / 10;
      int stepSize = (int)((11f - (float)Sensitivity) * width);

      Step(stepSize);

      OnPerformTuning(stepSize);

      //
      // Check if we have reached the end of the tuning
      //
      if (progressBar.Value == progressBar.Maximum)
      {
        DoStop();
      }
    }

    public virtual void OnPerformTuning(int stepSize)
    {
    }

    private void sensitivityComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetSensitivity();
    }

    public ArrayList TunedItems
    {
      get
      {
        ArrayList tunedItems = new ArrayList();

        foreach (object item in itemsListBox.Items)
        {
          tunedItems.Add(item);
        }

        return tunedItems;
      }
    }
  }
}
