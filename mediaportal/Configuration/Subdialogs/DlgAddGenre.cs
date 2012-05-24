#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.IO;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgAddGenre.
  /// </summary>
  public class DlgAddGenre : MPConfigForm
  {
    private MPButton _buttonAdd;
    private MPButton _buttonCancel;
    private MPLabel _labelName;
    private MPTextBox _textBoxGenreName;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container _components = null;

    public DlgAddGenre()
    {
      InitializeComponent();
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this._buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this._labelName = new MediaPortal.UserInterface.Controls.MPLabel();
      this._textBoxGenreName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this._buttonAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // _buttonCancel
      // 
      this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._buttonCancel.Location = new System.Drawing.Point(292, 46);
      this._buttonCancel.Name = "_buttonCancel";
      this._buttonCancel.Size = new System.Drawing.Size(56, 23);
      this._buttonCancel.TabIndex = 2;
      this._buttonCancel.Text = "Cancel";
      this._buttonCancel.UseVisualStyleBackColor = true;
      this._buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // _labelName
      // 
      this._labelName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._labelName.Location = new System.Drawing.Point(28, 19);
      this._labelName.Name = "_labelName";
      this._labelName.Size = new System.Drawing.Size(38, 20);
      this._labelName.TabIndex = 5;
      this._labelName.Text = "Name:";
      this._labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // _textBoxGenreName
      // 
      this._textBoxGenreName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._textBoxGenreName.BorderColor = System.Drawing.Color.Empty;
      this._textBoxGenreName.Location = new System.Drawing.Point(72, 19);
      this._textBoxGenreName.Name = "_textBoxGenreName";
      this._textBoxGenreName.Size = new System.Drawing.Size(276, 20);
      this._textBoxGenreName.TabIndex = 0;
      // 
      // _buttonAdd
      // 
      this._buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._buttonAdd.Location = new System.Drawing.Point(230, 46);
      this._buttonAdd.Name = "_buttonAdd";
      this._buttonAdd.Size = new System.Drawing.Size(56, 23);
      this._buttonAdd.TabIndex = 7;
      this._buttonAdd.Text = "Add";
      this._buttonAdd.UseVisualStyleBackColor = true;
      this._buttonAdd.Click += new System.EventHandler(this._buttonAdd_Click);
      // 
      // DlgAddGenre
      // 
      this.AcceptButton = this._buttonAdd;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this._buttonCancel;
      this.ClientSize = new System.Drawing.Size(382, 81);
      this.Controls.Add(this._buttonAdd);
      this.Controls.Add(this._textBoxGenreName);
      this.Controls.Add(this._labelName);
      this.Controls.Add(this._buttonCancel);
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(390, 115);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(390, 115);
      this.Name = "DlgAddGenre";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add New Genre";
      this.Load += new System.EventHandler(this.DlgAddGenre_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public string Value
    {
      get { return _textBoxGenreName.Text; }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void _buttonAdd_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void DlgAddGenre_Load(object sender, EventArgs e)
    {
      this._textBoxGenreName.Focus();
      this._textBoxGenreName.TabIndex = 0;
    }
  }
}