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
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls.Design
{

  #region MpeImageEditorForm

  public class MpeImageEditorForm : UserControl
  {
    #region Variables

    private Container components = null;
    private ListBox imageList;
    private MpeParser skinParser;
    private string selectedImageName;
    private Panel thumbPanel;
    private PictureBox thumbPictureBox;
    private IWindowsFormsEditorService editorService;

    #endregion

    #region Constructors

    public MpeImageEditorForm(FileInfo currentValue, MpeParser skinParser, IWindowsFormsEditorService editorService)
    {
      InitializeComponent();
      this.skinParser = skinParser;
      this.editorService = editorService;
      imageList.SelectionMode = SelectionMode.One;
      imageList.Items.Add("(none)");
      for (int i = 0; i < skinParser.ImageFiles.Length; i++)
      {
        imageList.Items.Add(skinParser.ImageFiles[i]);
        if (skinParser.ImageFiles[i].Equals(currentValue))
        {
          imageList.SelectedIndex = (i + 1);
        }
      }
      MpeScreen window = (MpeScreen) skinParser.GetControl(MpeControlType.Screen);
      if (window.TextureBack != null)
      {
        thumbPanel.BackgroundImage = new Bitmap(window.TextureBack.FullName);
      }
      imageList.MouseWheel += new MouseEventHandler(imageList_MouseWheel);
    }

    #endregion

    #region Properties

    public string SelectedImageName
    {
      get { return selectedImageName; }
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

    private void imageList_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (imageList.SelectedIndex >= 0)
      {
        selectedImageName = imageList.SelectedItem.ToString();
        if (selectedImageName.Equals("(none)"))
        {
          selectedImageName = null;
        }
        else
        {
          thumbPictureBox.Image = skinParser.GetImageThumbnail(selectedImageName);
        }
      }
    }

    private void imageList_MouseWheel(object sender, MouseEventArgs e)
    {
      int i = 0;
      if (e.Delta > 0)
      {
        i = imageList.SelectedIndex - 1;
      }
      else
      {
        i = imageList.SelectedIndex + 1;
      }
      if (i < 0)
      {
        i = 0;
      }
      else if (i >= imageList.Items.Count)
      {
        i = imageList.Items.Count - 1;
      }
      imageList.SelectedIndex = i;
    }

    private void imageList_DoubleClick(object sender, EventArgs e)
    {
      Close();
    }

    private void thumbPictureBox_Click(object sender, EventArgs e)
    {
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
      imageList = new ListBox();
      thumbPanel = new Panel();
      thumbPictureBox = new PictureBox();
      thumbPanel.SuspendLayout();
      SuspendLayout();
      // 
      // imageList
      // 
      imageList.BorderStyle = BorderStyle.None;
      imageList.Location = new Point(140, 4);
      imageList.Name = "imageList";
      imageList.Size = new Size(256, 130);
      imageList.Sorted = true;
      imageList.TabIndex = 5;
      imageList.DoubleClick += new EventHandler(imageList_DoubleClick);
      imageList.SelectedIndexChanged += new EventHandler(imageList_SelectedIndexChanged);
      // 
      // thumbPanel
      // 
      thumbPanel.BackColor = Color.Transparent;
      thumbPanel.Controls.Add(thumbPictureBox);
      thumbPanel.Location = new Point(4, 4);
      thumbPanel.Name = "thumbPanel";
      thumbPanel.Size = new Size(132, 132);
      thumbPanel.TabIndex = 6;
      // 
      // thumbPictureBox
      // 
      thumbPictureBox.BackColor = Color.Transparent;
      thumbPictureBox.Location = new Point(2, 2);
      thumbPictureBox.Name = "thumbPictureBox";
      thumbPictureBox.Size = new Size(128, 128);
      thumbPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
      thumbPictureBox.TabIndex = 4;
      thumbPictureBox.TabStop = false;
      thumbPictureBox.Click += new EventHandler(thumbPictureBox_Click);
      // 
      // ImageSelector
      // 
      Controls.Add(thumbPanel);
      Controls.Add(imageList);
      Name = "ImageSelector";
      Size = new Size(400, 140);
      thumbPanel.ResumeLayout(false);
      ResumeLayout(false);
    }

    #endregion
  }

  #endregion

  #region MpeImageEditor

  public class MpeImageEditor : UITypeEditor
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
          MpeImageEditorForm selector = new MpeImageEditorForm((FileInfo) value, mpc.Parser, editorService);
          editorService.DropDownControl(selector);
          if (selector.SelectedImageName == null)
          {
            MpeLog.Info("Clearing Image...");
            return null;
          }
          MpeLog.Info("Changing texture to [" + selector.SelectedImageName + "]");
          return mpc.Parser.GetImageFile(selector.SelectedImageName);
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