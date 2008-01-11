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
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace Mpe.Forms
{
  public class MpeSkinBrowserDialog : Form
  {
    #region Variables

    private Button okButton;
    private GroupBox groupBox1;
    private Button cancelButton;
    private Label label3;
    private Container components = null;
    private MpePreferences preferences;
    private Panel panel1;
    private ListView skinList;
    private ColumnHeader colName;
    private Splitter splitter1;
    private PictureBox previewBox;
    private TextBox nameTextBox;
    private Hashtable previews;
    private Label nameLabel;
    private Label listLabel;
    private MpeSkinBrowserMode mode;
    private DirectoryInfo skinDir;

    #endregion

    #region Constructors

    public MpeSkinBrowserDialog() : this(MpeSkinBrowserMode.New)
    {
      //
    }

    public MpeSkinBrowserDialog(MpeSkinBrowserMode browserMode)
    {
      InitializeComponent();
      mode = browserMode;
      preferences = MediaPortalEditor.Global.Preferences;
      previews = new Hashtable();
      if (preferences == null)
      {
        throw new Exception("Could not load preferences");
      }
    }

    #endregion

    #region Properties

    public Image SelectedSkinPreview
    {
      get
      {
        if (skinList.SelectedItems.Count > 0)
        {
          return (Image) previews[skinList.SelectedItems[0].Text];
        }
        return null;
      }
    }

    public DirectoryInfo SelectedSkinDir
    {
      get
      {
        if (skinList.SelectedItems.Count > 0)
        {
          return (DirectoryInfo) skinList.SelectedItems[0].Tag;
        }
        return null;
      }
    }

    public DirectoryInfo NewSkinDir
    {
      get { return skinDir; }
    }

    public string SkinName
    {
      get { return nameTextBox.Text.Trim(); }
    }

    #endregion

    #region Methods

    #endregion

    #region Event Handlers

    private void OnLoad(object sender, EventArgs e)
    {
      CenterToParent();
      if (mode == MpeSkinBrowserMode.Open)
      {
        listLabel.Text = "Skins";
        Height = 336;
        Text = "Open Skin";
      }
      else
      {
        listLabel.Text = "Templates";
        Height = 368;
        Text = "New Skin";
      }
      DirectoryInfo[] skins = preferences.MediaPortalSkins;
      for (int i = 0; i < skins.Length; i++)
      {
        ListViewItem item = skinList.Items.Add(skins[i].Name);
        item.Tag = skins[i];
        string f = skins[i].FullName + @"\media\preview.png";
        if (File.Exists(f))
        {
          Bitmap b = new Bitmap(f);
          previews.Add(skins[i].Name, b);
        }
      }
    }

    private void OnCancelClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void OnOkClick(object sender, EventArgs e)
    {
      if (mode == MpeSkinBrowserMode.New)
      {
        if (SelectedSkinDir == null)
        {
          MessageBox.Show(this, "Please select the skin that will be used a template for your new skin.",
                          "Skin Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
          return;
        }
        if (SkinName.Length == 0)
        {
          MessageBox.Show(this, "Please specify a name for the new skin.", "Name Required", MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
          nameTextBox.Focus();
          return;
        }
        if (SkinName.IndexOfAny(new char[] {'\\', '/', ':', '*', '?', '\"', '<', '>', '|'}) >= 0)
        {
          MessageBox.Show(this,
                          "The skin name cannot contain any of the following characters:" + Environment.NewLine +
                          "/ \\ : * ? \" < > | ", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
          nameTextBox.Focus();
          return;
        }
        try
        {
          skinDir = new DirectoryInfo(preferences.MediaPortalSkinDir.FullName + "\\" + SkinName);
          if (skinDir.Exists)
          {
            MessageBox.Show(this, "A skin with the specified name already exists.", "Invalid Skin Name",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            nameTextBox.Focus();
            return;
          }
        }
        catch (Exception ee)
        {
          MessageBox.Show(this, ee.Message, "Invalid Skin Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
          nameTextBox.Focus();
          return;
        }
      }
      else
      {
        if (SelectedSkinDir == null)
        {
          MessageBox.Show(this, "Please select the skin that you want to open.", "Skin Required", MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
          return;
        }
      }
      DialogResult = DialogResult.OK;
      Close();
    }

    private void OnIndexChanged(object sender, EventArgs e)
    {
      previewBox.Image = SelectedSkinPreview;
    }

    private void OnDoubleClick(object sender, EventArgs e)
    {
      if (mode == MpeSkinBrowserMode.Open)
      {
        OnOkClick(sender, e);
      }
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
        if (previews != null)
        {
          ICollection c = previews.Values;
          IEnumerator e = c.GetEnumerator();
          while (e.MoveNext())
          {
            Bitmap b = (Bitmap) e.Current;
            b.Dispose();
          }
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
      ResourceManager resources = new ResourceManager(typeof(MpeSkinBrowserDialog));
      okButton = new Button();
      nameLabel = new Label();
      nameTextBox = new TextBox();
      cancelButton = new Button();
      groupBox1 = new GroupBox();
      listLabel = new Label();
      label3 = new Label();
      panel1 = new Panel();
      previewBox = new PictureBox();
      splitter1 = new Splitter();
      skinList = new ListView();
      colName = new ColumnHeader();
      panel1.SuspendLayout();
      SuspendLayout();
      // 
      // okButton
      // 
      okButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      okButton.Location = new Point(304, 304);
      okButton.Name = "okButton";
      okButton.TabIndex = 1;
      okButton.Text = "OK";
      okButton.Click += new EventHandler(OnOkClick);
      // 
      // nameLabel
      // 
      nameLabel.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      nameLabel.AutoSize = true;
      nameLabel.Location = new Point(8, 267);
      nameLabel.Name = "nameLabel";
      nameLabel.Size = new Size(88, 16);
      nameLabel.TabIndex = 3;
      nameLabel.Text = "New Skin Name:";
      // 
      // nameTextBox
      // 
      nameTextBox.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      nameTextBox.Location = new Point(96, 264);
      nameTextBox.Name = "nameTextBox";
      nameTextBox.Size = new Size(368, 20);
      nameTextBox.TabIndex = 4;
      nameTextBox.Text = "";
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      cancelButton.DialogResult = DialogResult.Cancel;
      cancelButton.Location = new Point(384, 304);
      cancelButton.Name = "cancelButton";
      cancelButton.TabIndex = 5;
      cancelButton.Text = "Cancel";
      cancelButton.Click += new EventHandler(OnCancelClick);
      // 
      // groupBox1
      // 
      groupBox1.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      groupBox1.Location = new Point(8, 296);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(456, 3);
      groupBox1.TabIndex = 6;
      groupBox1.TabStop = false;
      // 
      // listLabel
      // 
      listLabel.AutoSize = true;
      listLabel.Location = new Point(8, 8);
      listLabel.Name = "listLabel";
      listLabel.Size = new Size(57, 16);
      listLabel.TabIndex = 7;
      listLabel.Text = "Templates";
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new Point(416, 8);
      label3.Name = "label3";
      label3.Size = new Size(45, 16);
      label3.TabIndex = 8;
      label3.Text = "Preview";
      // 
      // panel1
      // 
      panel1.Controls.Add(previewBox);
      panel1.Controls.Add(splitter1);
      panel1.Controls.Add(skinList);
      panel1.Location = new Point(8, 24);
      panel1.Name = "panel1";
      panel1.Size = new Size(456, 232);
      panel1.TabIndex = 10;
      // 
      // previewBox
      // 
      previewBox.BackColor = Color.White;
      previewBox.BorderStyle = BorderStyle.Fixed3D;
      previewBox.Dock = DockStyle.Fill;
      previewBox.Location = new Point(163, 0);
      previewBox.Name = "previewBox";
      previewBox.Size = new Size(293, 232);
      previewBox.SizeMode = PictureBoxSizeMode.StretchImage;
      previewBox.TabIndex = 12;
      previewBox.TabStop = false;
      // 
      // splitter1
      // 
      splitter1.Location = new Point(160, 0);
      splitter1.Name = "splitter1";
      splitter1.Size = new Size(3, 232);
      splitter1.TabIndex = 11;
      splitter1.TabStop = false;
      // 
      // skinList
      // 
      skinList.Columns.AddRange(new ColumnHeader[]
                                  {
                                    colName
                                  });
      skinList.Dock = DockStyle.Left;
      skinList.FullRowSelect = true;
      skinList.HeaderStyle = ColumnHeaderStyle.None;
      skinList.HideSelection = false;
      skinList.Location = new Point(0, 0);
      skinList.MultiSelect = false;
      skinList.Name = "skinList";
      skinList.Size = new Size(160, 232);
      skinList.Sorting = SortOrder.Ascending;
      skinList.TabIndex = 10;
      skinList.View = View.Details;
      skinList.DoubleClick += new EventHandler(OnDoubleClick);
      skinList.SelectedIndexChanged += new EventHandler(OnIndexChanged);
      // 
      // colName
      // 
      colName.Text = "Name";
      colName.Width = 120;
      // 
      // MpeSkinBrowserDialog
      // 
      AcceptButton = okButton;
      AutoScaleBaseSize = new Size(5, 13);
      CancelButton = cancelButton;
      ClientSize = new Size(474, 336);
      Controls.Add(panel1);
      Controls.Add(label3);
      Controls.Add(listLabel);
      Controls.Add(nameTextBox);
      Controls.Add(nameLabel);
      Controls.Add(groupBox1);
      Controls.Add(cancelButton);
      Controls.Add(okButton);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      Icon = ((Icon) (resources.GetObject("$this.Icon")));
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "MpeSkinBrowserDialog";
      ShowInTaskbar = false;
      Text = "New Skin";
      Load += new EventHandler(OnLoad);
      panel1.ResumeLayout(false);
      ResumeLayout(false);
    }

    #endregion
  }


  public enum MpeSkinBrowserMode
  {
    New,
    Open
  } ;
}