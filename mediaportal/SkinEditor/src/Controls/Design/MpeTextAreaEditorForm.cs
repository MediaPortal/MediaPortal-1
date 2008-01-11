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

  #region MpeTextAreaEditorForm

  public class MpeTextAreaEditorForm : UserControl
  {
    #region Variables

    private Container components = null;
    private TextBox textbox;
    private Button okButton;
    private Button cancelButton;
    private IWindowsFormsEditorService editorService = null;
    private DialogResult result;

    #endregion

    #region Constructors

    public MpeTextAreaEditorForm(string textValue, IWindowsFormsEditorService editorService)
    {
      InitializeComponent();
      EditorService = editorService;
      TextValue = textValue;
    }

    #endregion

    #region Properties

    public String TextValue
    {
      get { return textbox.Text; }
      set { textbox.Text = value != null ? value : ""; }
    }

    public IWindowsFormsEditorService EditorService
    {
      get { return editorService; }
      set { editorService = value; }
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

    private void okButton_Click(object sender, EventArgs e)
    {
      result = DialogResult.OK;
      Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      result = DialogResult.Cancel;
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
      textbox = new TextBox();
      okButton = new Button();
      cancelButton = new Button();
      SuspendLayout();
      // 
      // textbox
      // 
      textbox.AcceptsReturn = true;
      textbox.Location = new Point(8, 8);
      textbox.Multiline = true;
      textbox.Name = "textbox";
      textbox.Size = new Size(320, 88);
      textbox.TabIndex = 0;
      textbox.Text = "";
      // 
      // okButton
      // 
      okButton.Location = new Point(168, 104);
      okButton.Name = "okButton";
      okButton.TabIndex = 1;
      okButton.Text = "OK";
      okButton.Click += new EventHandler(okButton_Click);
      // 
      // cancelButton
      // 
      cancelButton.Location = new Point(248, 104);
      cancelButton.Name = "cancelButton";
      cancelButton.TabIndex = 2;
      cancelButton.Text = "Cancel";
      cancelButton.Click += new EventHandler(cancelButton_Click);
      // 
      // MpeTextAreaEditorForm
      // 
      BackColor = SystemColors.Control;
      Controls.Add(cancelButton);
      Controls.Add(okButton);
      Controls.Add(textbox);
      Name = "MpeTextAreaEditorForm";
      Size = new Size(336, 136);
      ResumeLayout(false);
    }

    #endregion
  }

  #endregion

  #region MpeTextAreaEditor

  public class MpeTextAreaEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      if (context.Instance is MpeControl)
      {
        try
        {
          MpeControl mpc = (MpeControl) context.Instance;
          IWindowsFormsEditorService editorService =
            (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
          MpeTextAreaEditorForm selector = new MpeTextAreaEditorForm((string) value, editorService);
          editorService.DropDownControl(selector);
          if (selector.Result == DialogResult.Cancel)
          {
            return value;
          }
          return selector.TextValue;
        }
        catch (Exception ee)
        {
          MpeLog.Debug(ee);
          MpeLog.Error(ee);
        }
      }
      return base.EditValue(context, provider, value);
    }
  }

  #endregion	
}