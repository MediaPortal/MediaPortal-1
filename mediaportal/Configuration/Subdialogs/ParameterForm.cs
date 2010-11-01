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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for ParameterForm.
  /// </summary>
  public class ParameterForm : MPConfigForm
  {
    private MPButton cancelButton;
    private MPButton okButton;
    private MPListView parametersListView;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public ParameterForm()
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
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(286, 144);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 2;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Enabled = false;
      this.okButton.Location = new System.Drawing.Point(206, 144);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 1;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // parametersListView
      // 
      this.parametersListView.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                 {
                                                   this.columnHeader1,
                                                   this.columnHeader2
                                                 });
      this.parametersListView.FullRowSelect = true;
      this.parametersListView.Location = new System.Drawing.Point(8, 8);
      this.parametersListView.Name = "parametersListView";
      this.parametersListView.Size = new System.Drawing.Size(352, 128);
      this.parametersListView.TabIndex = 0;
      this.parametersListView.View = System.Windows.Forms.View.Details;
      this.parametersListView.DoubleClick += new System.EventHandler(this.parametersListView_DoubleClick);
      this.parametersListView.SelectedIndexChanged +=
        new System.EventHandler(this.parametersListView_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Parameter";
      this.columnHeader1.Width = 77;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Description";
      this.columnHeader2.Width = 271;
      // 
      // ParameterForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(370, 176);
      this.Controls.Add(this.parametersListView);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "ParameterForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Parameter";
      this.ResumeLayout(false);
    }

    #endregion

    private void parametersListView_DoubleClick(object sender, EventArgs e)
    {
      if (parametersListView.SelectedItems.Count > 0)
      {
        this.DialogResult = DialogResult.OK;
        this.Hide();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void parametersListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      okButton.Enabled = parametersListView.SelectedItems.Count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void okButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cancelButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    public string SelectedParameter
    {
      get
      {
        if (parametersListView.SelectedItems.Count == 0)
        {
          return string.Empty;
        }
        else
        {
          return parametersListView.SelectedItems[0].Text;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="description"></param>
    public void AddParameter(string parameter, string description)
    {
      parametersListView.Items.Add(new ListViewItem(new string[] {parameter, description}));
    }
  }
}