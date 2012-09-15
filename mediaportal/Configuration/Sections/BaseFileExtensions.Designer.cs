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

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  partial class BaseFileExtensions
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.resetButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.removeButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.extensionsListView = new System.Windows.Forms.ListView();
      this.extensionTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.resetButton);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.removeButton);
      this.groupBox1.Controls.Add(this.extensionsListView);
      this.groupBox1.Controls.Add(this.extensionTextBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(6, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // resetButton
      // 
      this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.resetButton.Location = new System.Drawing.Point(374, 144);
      this.resetButton.Name = "resetButton";
      this.resetButton.Size = new System.Drawing.Size(72, 22);
      this.resetButton.TabIndex = 6;
      this.resetButton.Text = "Default";
      this.resetButton.UseVisualStyleBackColor = true;
      this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(430, 32);
      this.label1.TabIndex = 0;
      this.label1.Text = "Files matching an extension listed below will be considered a known media type.";
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(374, 64);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(72, 22);
      this.addButton.TabIndex = 2;
      this.addButton.Text = "Add";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // removeButton
      // 
      this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.removeButton.Enabled = false;
      this.removeButton.Location = new System.Drawing.Point(374, 88);
      this.removeButton.Name = "removeButton";
      this.removeButton.Size = new System.Drawing.Size(72, 22);
      this.removeButton.TabIndex = 4;
      this.removeButton.Text = "Remove";
      this.removeButton.UseVisualStyleBackColor = true;
      this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
      // 
      // extensionsListBox
      // 
      this.extensionsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionsListView.Location = new System.Drawing.Point(16, 88);
      this.extensionsListView.Name = "extensionsListView";
      this.extensionsListView.Size = new System.Drawing.Size(350, 303);
      this.extensionsListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.extensionsListView.TabIndex = 3;
      this.extensionsListView.UseCompatibleStateImageBehavior = false;
      this.extensionsListView.View = System.Windows.Forms.View.List;
      this.extensionsListView.SelectedIndexChanged += new System.EventHandler(this.extensionsListBox_SelectedIndexChanged);
      this.extensionsListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.extensionsListBox_KeyDown);
      // 
      // extensionTextBox
      // 
      this.extensionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionTextBox.BorderColor = System.Drawing.Color.Empty;
      this.extensionTextBox.Location = new System.Drawing.Point(16, 64);
      this.extensionTextBox.Name = "extensionTextBox";
      this.extensionTextBox.Size = new System.Drawing.Size(350, 20);
      this.extensionTextBox.TabIndex = 1;
      this.extensionTextBox.Enter += new System.EventHandler(this.extensionTextBox_Enter);
      this.extensionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.extensionTextBox_KeyDown);
      this.extensionTextBox.Leave += new System.EventHandler(this.extensionTextBox_Leave);
      // 
      // BaseFileExtensions
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "BaseFileExtensions";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton removeButton;
    private MediaPortal.UserInterface.Controls.MPButton addButton;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox extensionTextBox;
    private System.Windows.Forms.ListView extensionsListView;
    private MediaPortal.UserInterface.Controls.MPButton resetButton;
  }
}
