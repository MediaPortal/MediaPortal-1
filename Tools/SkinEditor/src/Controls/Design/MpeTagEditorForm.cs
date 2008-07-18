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
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mpe.Controls.Design
{

  #region MpeTagEditorForm

  /// <summary>
  ///
  /// </summary>
  public class MpeTagEditorForm : UserControl
  {
    #region Variables

    private Container components = null;
    private Label label1;
    private Label label2;
    private TextBox tbName;
    private TextBox tbValue;
    private Button okButton;
    private Button cancelButton;
    private Button deleteButton;
    private IWindowsFormsEditorService editorService;
    private DialogResult result;

    #endregion

    #region Constructors

    public MpeTagEditorForm(MpeTag tag, IWindowsFormsEditorService service)
    {
      InitializeComponent();
      editorService = service;
      tbName.Text = tag.Name;
      tbValue.Text = tag.Value;
      result = DialogResult.Cancel;
    }

    #endregion

    #region Properties

    public string TagName
    {
      get { return tbName.Text; }
    }

    public string TagValue
    {
      get { return tbValue.Text; }
    }

    public DialogResult Result
    {
      get { return result; }
    }

    #endregion

    #region Methods

    public void Close()
    {
      if (editorService != null)
      {
        editorService.CloseDropDown();
      }
    }

    #endregion

    #region Event Handlers

    private void OnOkClick(object sender, EventArgs e)
    {
      result = DialogResult.OK;
      Close();
    }

    private void OnCancelClick(object sender, EventArgs e)
    {
      result = DialogResult.Cancel;
      Close();
    }

    private void OnDeleteClick(object sender, EventArgs e)
    {
      result = DialogResult.Abort;
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      okButton = new Button();
      label1 = new Label();
      tbName = new TextBox();
      tbValue = new TextBox();
      label2 = new Label();
      cancelButton = new Button();
      deleteButton = new Button();
      SuspendLayout();
      // 
      // okButton
      // 
      okButton.Location = new Point(48, 56);
      okButton.Name = "okButton";
      okButton.TabIndex = 4;
      okButton.Text = "Ok";
      okButton.Click += new EventHandler(OnOkClick);
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new Point(8, 8);
      label1.Name = "label1";
      label1.Size = new Size(34, 16);
      label1.TabIndex = 0;
      label1.Text = "Name";
      // 
      // tbName
      // 
      tbName.Location = new Point(48, 5);
      tbName.Name = "tbName";
      tbName.Size = new Size(240, 20);
      tbName.TabIndex = 1;
      tbName.Text = "Name";
      // 
      // tbValue
      // 
      tbValue.Location = new Point(48, 29);
      tbValue.Name = "tbValue";
      tbValue.Size = new Size(240, 20);
      tbValue.TabIndex = 3;
      tbValue.Text = "Value";
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new Point(8, 32);
      label2.Name = "label2";
      label2.Size = new Size(33, 16);
      label2.TabIndex = 2;
      label2.Text = "Value";
      // 
      // cancelButton
      // 
      cancelButton.Location = new Point(128, 56);
      cancelButton.Name = "cancelButton";
      cancelButton.TabIndex = 5;
      cancelButton.Text = "Cancel";
      cancelButton.Click += new EventHandler(OnCancelClick);
      // 
      // deleteButton
      // 
      deleteButton.Location = new Point(208, 56);
      deleteButton.Name = "deleteButton";
      deleteButton.TabIndex = 6;
      deleteButton.Text = "Delete";
      deleteButton.Click += new EventHandler(OnDeleteClick);
      // 
      // MpeTagEditorForm
      // 
      Controls.Add(deleteButton);
      Controls.Add(cancelButton);
      Controls.Add(label2);
      Controls.Add(tbValue);
      Controls.Add(tbName);
      Controls.Add(label1);
      Controls.Add(okButton);
      Name = "MpeTagEditorForm";
      Size = new Size(296, 88);
      ResumeLayout(false);
      BackColor = SystemColors.Control;
    }

    #endregion
  }

  #endregion

  #region MpeTagEditor

  public class MpeTagEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      if (context.Instance is MpeTagCollection)
      {
        MpeTagCollection tags = (MpeTagCollection) context.Instance;
        MpeTag tag = tags[context.PropertyDescriptor.DisplayName];
        if (tag != null)
        {
          IWindowsFormsEditorService editorService =
            (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
          MpeTagEditorForm form = new MpeTagEditorForm(tag, editorService);
          editorService.DropDownControl(form);
          switch (form.Result)
          {
            case DialogResult.OK:
              if (tag.Name == form.TagName && tag.Name != "-")
              {
                tag.Value = form.TagValue;
                MpeLog.Info("Tag Updated! Name = " + tag.Name + " Value = " + tag.Value);
              }
              else if (tag.Name == form.TagName)
              {
                MpeLog.Warn("Invalid tag name specified");
              }
              else
              {
                tags.Remove(tag.Name);
                tag.Name = form.TagName;
                tag.Value = form.TagValue;
                tags.Add(tag);
                MpeLog.Info("Tag Updated! Name = [" + tag.Name + "] Value = [" + tag.Value + "]");
              }
              break;
            case DialogResult.Abort:
              tags.Remove(tag.Name);
              MpeLog.Info("Tag Removed! Name = [" + tag.Name + "[ Value = [" + tag.Value + "]");
              break;
          }
          return tag;
        }
      }
      return base.EditValue(context, provider, value);
    }
  }

  #endregion
}