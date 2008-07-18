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
using Mpe.Designers;

namespace Mpe.Forms
{
  /// <summary>
  /// Summary description for SaveChangesForm.
  /// </summary>
  public class MpeSaveForm : Form
  {
    #region Variables

    private Button cancelButton;
    private Button noButton;
    private Button yesButton;
    private Label title;
    private ColumnHeader nameCol;
    private ColumnHeader typeCol;
    private ListView changeList;

    #endregion

    private Container components = null;

    #region Constructor

    public MpeSaveForm(MpeDesigner[] designers, ImageList imageList)
    {
      InitializeComponent();
      for (int i = 0; designers != null && i < designers.Length; i++)
      {
        ListViewItem item = changeList.Items.Add(designers[i].ResourceName);
        item.Tag = designers[i];
        item.Selected = true;
        item.SubItems.Add(designers[i].GetType().Name.Replace("Designer", "").Replace("Mpe", ""));
      }
      DialogResult = DialogResult.Cancel;
    }

    #endregion

    #region Properties

    public MpeDesigner[] SelectedDesigners
    {
      get
      {
        ArrayList array = new ArrayList();
        for (int i = 0; changeList.SelectedItems != null && i < changeList.SelectedItems.Count; i++)
        {
          ListViewItem item = changeList.SelectedItems[i];
          array.Add(item.Tag);
        }
        return (MpeDesigner[]) array.ToArray(typeof(MpeDesigner));
      }
    }

    #endregion

    #region Event Handlers

    private void SaveChangesForm_Load(object sender, EventArgs e)
    {
      CenterToParent();
    }

    private void yesButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Yes;
      Close();
    }

    private void noButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.No;
      Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    #endregion

    #region Windows Form Designer Generated Code

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

    /// <summary>
    /// Required method for Designer support - Do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      cancelButton = new Button();
      title = new Label();
      noButton = new Button();
      yesButton = new Button();
      changeList = new ListView();
      nameCol = new ColumnHeader();
      typeCol = new ColumnHeader();
      SuspendLayout();
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      cancelButton.DialogResult = DialogResult.Cancel;
      cancelButton.Location = new Point(304, 192);
      cancelButton.Name = "cancelButton";
      cancelButton.Size = new Size(75, 24);
      cancelButton.TabIndex = 3;
      cancelButton.Text = "Cancel";
      cancelButton.Click += new EventHandler(cancelButton_Click);
      // 
      // title
      // 
      title.AutoSize = true;
      title.Location = new Point(7, 6);
      title.Name = "title";
      title.Size = new Size(213, 16);
      title.TabIndex = 4;
      title.Text = "Save changes to the following resources?";
      // 
      // noButton
      // 
      noButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      noButton.Location = new Point(224, 192);
      noButton.Name = "noButton";
      noButton.Size = new Size(75, 24);
      noButton.TabIndex = 2;
      noButton.Text = "No";
      noButton.Click += new EventHandler(noButton_Click);
      // 
      // yesButton
      // 
      yesButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      yesButton.Location = new Point(144, 192);
      yesButton.Name = "yesButton";
      yesButton.Size = new Size(75, 24);
      yesButton.TabIndex = 1;
      yesButton.Text = "Yes";
      yesButton.Click += new EventHandler(yesButton_Click);
      // 
      // changeList
      // 
      changeList.Columns.AddRange(new ColumnHeader[]
                                    {
                                      nameCol,
                                      typeCol
                                    });
      changeList.FullRowSelect = true;
      changeList.HeaderStyle = ColumnHeaderStyle.None;
      changeList.HideSelection = false;
      changeList.Location = new Point(8, 24);
      changeList.Name = "changeList";
      changeList.Size = new Size(376, 160);
      changeList.TabIndex = 0;
      changeList.View = View.Details;
      // 
      // nameCol
      // 
      nameCol.Text = "Name";
      nameCol.Width = 280;
      // 
      // typeCol
      // 
      typeCol.Text = "Type";
      typeCol.Width = 72;
      // 
      // MpeSaveForm
      // 
      AcceptButton = yesButton;
      AutoScaleBaseSize = new Size(5, 13);
      CancelButton = cancelButton;
      ClientSize = new Size(394, 224);
      Controls.Add(changeList);
      Controls.Add(yesButton);
      Controls.Add(noButton);
      Controls.Add(title);
      Controls.Add(cancelButton);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "MpeSaveForm";
      ShowInTaskbar = false;
      Text = "Save Confirmation";
      Load += new EventHandler(SaveChangesForm_Load);
      ResumeLayout(false);
    }

    #endregion
  }
}