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
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This form displays a list of all properties that MediaPortal exposes and their values.
  /// </summary>
  /// <remarks>
  /// It is show when the user checks the "Show Property Browser" checkbox in the plugin 
  /// setup form.
  /// </remarks>
  /// <author>JoeDalton</author>
  public class PropertyBrowser : MediaPortal.UserInterface.Controls.MPForm
  {
    private delegate void SetStatusDelegate(Status status);

    private delegate void SetActiveWindowDelegate(GUIWindow.Window _window);

    private DataGrid dataGrid1;
    private IContainer components = null;
    private Panel panel1;
    private MPLabel label1;
    private MPTextBox txtActiveWindow;
    private MPLabel label2;
    private MPTextBox txtStatus;
    private DataTable properties = null;

    public PropertyBrowser()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
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
      this.dataGrid1 = new System.Windows.Forms.DataGrid();
      this.panel1 = new System.Windows.Forms.Panel();
      this.txtStatus = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtActiveWindow = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // dataGrid1
      // 
      this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid1.DataMember = "";
      this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dataGrid1.Location = new System.Drawing.Point(0, 0);
      this.dataGrid1.Name = "dataGrid1";
      this.dataGrid1.Size = new System.Drawing.Size(336, 254);
      this.dataGrid1.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.txtStatus);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Controls.Add(this.txtActiveWindow);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 254);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(346, 64);
      this.panel1.TabIndex = 1;
      // 
      // txtStatus
      // 
      this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtStatus.BorderColor = System.Drawing.Color.Empty;
      this.txtStatus.Location = new System.Drawing.Point(96, 32);
      this.txtStatus.Name = "txtStatus";
      this.txtStatus.ReadOnly = true;
      this.txtStatus.Size = new System.Drawing.Size(232, 20);
      this.txtStatus.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.Location = new System.Drawing.Point(8, 32);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "Status";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtActiveWindow
      // 
      this.txtActiveWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtActiveWindow.BorderColor = System.Drawing.Color.Empty;
      this.txtActiveWindow.Location = new System.Drawing.Point(96, 8);
      this.txtActiveWindow.Name = "txtActiveWindow";
      this.txtActiveWindow.ReadOnly = true;
      this.txtActiveWindow.Size = new System.Drawing.Size(232, 20);
      this.txtActiveWindow.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label1.Location = new System.Drawing.Point(8, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Active Window";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // PropertyBrowser
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(336, 318);
      this.Controls.Add(this.dataGrid1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "PropertyBrowser";
      this.Text = "Property Browser";
      this.TopMost = true;
      this.Load += new System.EventHandler(this.PropertyBrowser_Load);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public void SetStatus(Status _status)
    {
      if (InvokeRequired)
      {
        //When MP is closing it can happen that this method is called while the PropertyBrowser
        //is already disposed.  Strangly enough testing for IsDisposed still returns false then,
        //so that didn't help.
        Invoke(new SetStatusDelegate(SetStatus), _status);
      }
      txtStatus.Text = _status.ToString();
    }

    public void SetActiveWindow(GUIWindow.Window _window)
    {
      if (InvokeRequired)
      {
        Invoke(new SetActiveWindowDelegate(SetActiveWindow), _window);
      }
      txtActiveWindow.Text = _window.ToString();
    }

    private void PropertyBrowser_Load(object sender, EventArgs e)
    {
      properties = new DataTable("Properties");
      DataColumn key = properties.Columns.Add("Key", typeof(string));
      properties.Columns.Add("Value", typeof(string));
      properties.PrimaryKey = new DataColumn[] {key};
      dataGrid1.DataSource = properties;
      GUIPropertyManager.OnPropertyChanged +=
        new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
      //Initialize some properties (because they are set before we attached our eventhandler)
      GUIPropertyManager_OnPropertyChanged("#currentmodule", GUIPropertyManager.GetProperty("#currentmodule"));
    }

    /// <summary>
    /// The eventhandler for the event that the GUIPropertyManager raises when a property
    /// changes value
    /// </summary>
    /// <param name="tag">The name of the property that was changed</param>
    /// <param name="value">The new value of the property</param>
    private void GUIPropertyManager_OnPropertyChanged(string tag, string value)
    {
      if (properties == null)
      {
        return;
      }
      DataRow r = properties.Rows.Find(tag);
      if (r == null)
      {
        properties.Rows.Add(new object[] {tag, value});
      }
      else
      {
        r["Value"] = value;
      }
    }
  }
}