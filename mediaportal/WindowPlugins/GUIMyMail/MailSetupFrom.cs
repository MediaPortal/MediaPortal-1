/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Profile;

namespace MyMail
{
  /// <summary>
  /// Zusammenfassung für MailSetupFrom.
  /// </summary>
  public class MailSetupFrom : Form
  {
    ArrayList m_mailBox = new ArrayList();
    private NumericUpDown numericUpDown1;
    private Label label1;
    private GroupBox gbMailboxes;
    private ListBox lbMailboxes;
    private Button btnAdd;
    private Button btnDelete;
    private Button btnEdit;
    private Button btnClose;

    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private Container components = null;

    public MailSetupFrom()
    {
      //
      // Erforderlich für die Windows Form-Designerunterstützung
      //
      InitializeComponent();

      //
      // TODO: Fügen Sie den Konstruktorcode nach dem Aufruf von InitializeComponent hinzu
      //
    }

    /// <summary>
    /// Die verwendeten Ressourcen bereinigen.
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

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnAdd = new System.Windows.Forms.Button();
      this.lbMailboxes = new System.Windows.Forms.ListBox();
      this.btnDelete = new System.Windows.Forms.Button();
      this.btnClose = new System.Windows.Forms.Button();
      this.gbMailboxes = new System.Windows.Forms.GroupBox();
      this.btnEdit = new System.Windows.Forms.Button();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.gbMailboxes.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      this.SuspendLayout();
      // 
      // btnAdd
      // 
      this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnAdd.Location = new System.Drawing.Point(296, 16);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(80, 24);
      this.btnAdd.TabIndex = 1;
      this.btnAdd.Text = "Add...";
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // lbMailboxes
      // 
      this.lbMailboxes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lbMailboxes.Location = new System.Drawing.Point(8, 16);
      this.lbMailboxes.Name = "lbMailboxes";
      this.lbMailboxes.Size = new System.Drawing.Size(280, 277);
      this.lbMailboxes.TabIndex = 0;
      this.lbMailboxes.DoubleClick += new System.EventHandler(this.lbMailboxes_DoubleClick);
      this.lbMailboxes.SelectedIndexChanged += new System.EventHandler(this.lbMailboxes_SelectedIndexChanged);
      // 
      // btnDelete
      // 
      this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnDelete.Location = new System.Drawing.Point(296, 72);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(80, 24);
      this.btnDelete.TabIndex = 3;
      this.btnDelete.Text = "Delete...";
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnClose.Location = new System.Drawing.Point(320, 320);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(80, 24);
      this.btnClose.TabIndex = 2;
      this.btnClose.Text = "Close";
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // gbMailboxes
      // 
      this.gbMailboxes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.gbMailboxes.Controls.Add(this.btnEdit);
      this.gbMailboxes.Controls.Add(this.btnAdd);
      this.gbMailboxes.Controls.Add(this.btnDelete);
      this.gbMailboxes.Controls.Add(this.lbMailboxes);
      this.gbMailboxes.Location = new System.Drawing.Point(16, 8);
      this.gbMailboxes.Name = "gbMailboxes";
      this.gbMailboxes.Size = new System.Drawing.Size(384, 304);
      this.gbMailboxes.TabIndex = 0;
      this.gbMailboxes.TabStop = false;
      this.gbMailboxes.Text = "Current Mailbox List";
      // 
      // btnEdit
      // 
      this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnEdit.Location = new System.Drawing.Point(296, 40);
      this.btnEdit.Name = "btnEdit";
      this.btnEdit.Size = new System.Drawing.Size(80, 23);
      this.btnEdit.TabIndex = 2;
      this.btnEdit.Text = "Edit...";
      this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.numericUpDown1.Location = new System.Drawing.Point(264, 320);
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(40, 20);
      this.numericUpDown1.TabIndex = 1;
      this.numericUpDown1.Value = new System.Decimal(new int[] {
                                                                 1,
                                                                 0,
                                                                 0,
                                                                 0});
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label1.Location = new System.Drawing.Point(16, 324);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(248, 16);
      this.label1.TabIndex = 16;
      this.label1.Text = "Time-Interval in minutes to Auto-Check for Mail:";
      // 
      // MailSetupFrom
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(410, 352);
      this.ControlBox = false;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.numericUpDown1);
      this.Controls.Add(this.gbMailboxes);
      this.Controls.Add(this.btnClose);
      this.Name = "MailSetupFrom";
      this.Text = "Mailbox Configuration";
      this.Load += new System.EventHandler(this.MailSetupFrom_Load);
      this.gbMailboxes.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private void MailSetupFrom_Load(object sender, EventArgs e)
    {
      LoadSettings();
    }

    void RefreshListBox()
    {
      lbMailboxes.Items.Clear();
      foreach (MailBox mb in m_mailBox)
      {
        lbMailboxes.Items.Add(mb);
      }

    }


    private void lbMailboxes_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lbMailboxes.SelectedIndex != -1)
        if (lbMailboxes.SelectedIndex < m_mailBox.Count)
        {
          // todo:
        }
    }

    void DeleteItem()
    {
      if (lbMailboxes.SelectedIndex != -1)
        if (m_mailBox[lbMailboxes.SelectedIndex] != null)
        {
          DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this mail configuration?", "Information", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
          if (dialogResult == DialogResult.Yes)
          {
            m_mailBox.RemoveAt(lbMailboxes.SelectedIndex);
            SaveConfigFile();
            RefreshListBox();
            lbMailboxes.SelectedIndex = -1;
          }
        }

    }

    bool SaveConfigFile()
    {
      int count = 0;
      foreach (MailBox mb in m_mailBox)
      {
        string tmpLabel = mb.BoxLabel;
        for (int i = 0; i < m_mailBox.Count - 1; i++)
          if (tmpLabel.ToLower() == ((MailBox) m_mailBox[i]).BoxLabel.ToLower() && i != count)
          {
            MessageBox.Show("There are indentical Mail-Box Labels. Please change!");
            return false;
          }
        count++;
      }

      string applicationPath = Application.ExecutablePath;
      applicationPath = Path.GetFullPath(applicationPath);
      applicationPath = Path.GetDirectoryName(applicationPath);

      using (Xml xmlwriter = new Xml("MediaPortal.xml"))
      {
        MailBox tmpBox;
        int boxCount = m_mailBox.Count;
        xmlwriter.SetValue("mymail", "mailBoxCount", boxCount);
        for (int i = 0; i < boxCount; i++)
        {
          tmpBox = (MailBox) m_mailBox[i];
          // check the must set properties
          if (tmpBox.MailboxFolder == "") // this must set
            tmpBox.MailboxFolder = tmpBox.BoxLabel + "__Folder";
          if (tmpBox.AttachmentFolder == "") // this must set
            tmpBox.AttachmentFolder = tmpBox.MailboxFolder + @"\Attachments";

          // check full pathnames
          if (!Path.IsPathRooted(tmpBox.AttachmentFolder))
            tmpBox.AttachmentFolder = applicationPath + @"\email\" + tmpBox.AttachmentFolder;
          if (!Path.IsPathRooted(tmpBox.MailboxFolder))
            tmpBox.MailboxFolder = applicationPath + @"\email\" + tmpBox.MailboxFolder;

          if (tmpBox.BoxLabel == "")
          {
            MessageBox.Show("The BoxLabel property cant be empty!");
            return false;
          }
          //
          string mailBoxString = tmpBox.BoxLabel + ";" + tmpBox.Username + ";" + tmpBox.Password + ";" + tmpBox.ServerAddress + ";" + Convert.ToString(tmpBox.Port) + ";" + tmpBox.MailboxFolder + ";" + tmpBox.AttachmentFolder;
          if (tmpBox.Enabled)
          { mailBoxString += ";T";}
          else
          { mailBoxString += ";F";}
          xmlwriter.SetValue("mymail", "mailBox" + Convert.ToString(i), mailBoxString);
        }
        xmlwriter.SetValue("mymail", "timer", numericUpDown1.Value*60000);
        return true;
      }
    }

    void AddItem()
    {
      MailBox mailbox = new MailBox("New MailBox", "", "", "", 110, @"MyMailFiles" + Convert.ToString(m_mailBox.Count + 1), @"MailBoxAttachments");
      MailDetailSetup frmMailDetails = new MailDetailSetup();
      frmMailDetails.CurMailBox = mailbox;
      DialogResult dialogResult = frmMailDetails.ShowDialog(this);
      if (dialogResult == DialogResult.OK)
      {
        m_mailBox.Add(mailbox);
        SaveConfigFile();
      }
      RefreshListBox();
    }

    void LoadSettings()
    {
      using (Xml xmlreader = new Xml("MediaPortal.xml"))
      {
        int boxCount = 0;
        MailBox tmpBox;
        m_mailBox.Clear();
        numericUpDown1.Value = xmlreader.GetValueAsInt("mymail", "timer", 300000)/60000;
        boxCount = xmlreader.GetValueAsInt("mymail", "mailBoxCount", 0);

        if (boxCount > 0)
        {
          for (int i = 0; i < boxCount; i++)
          {
            string[] boxData = null;
            string mailBoxString = xmlreader.GetValueAsString("mymail", "mailBox" + Convert.ToString(i), "");
            if (mailBoxString.Length > 0)
            {
              boxData = mailBoxString.Split(new char[] {';'});
              if (boxData.Length >= 7)
              {
                tmpBox = new MailBox(boxData[0], boxData[1], boxData[2], boxData[3], Convert.ToInt16(boxData[4]), boxData[5], boxData[6]);
                if (tmpBox != null)
                {
                  if (boxData.Length > 7)
                  {
                    tmpBox.Enabled = (boxData[7] == "T");
                  }
                  m_mailBox.Add(tmpBox);
                }
              }
            }
          }
          if (m_mailBox.Count > 0)
            RefreshListBox();
        }
      }
    }

    private void lbMailboxes_DoubleClick(object sender, EventArgs e)
    {
      EditItem();
    }

    void EditItem()
    {
      if (lbMailboxes.SelectedIndex != -1)
        if (lbMailboxes.SelectedIndex < m_mailBox.Count)
        {
          MailBox mb = (MailBox) m_mailBox[lbMailboxes.SelectedIndex];
          MailDetailSetup frmMailDetails = new MailDetailSetup();
          frmMailDetails.CurMailBox = mb;
          DialogResult dialogResult = frmMailDetails.ShowDialog(this);
          if (dialogResult == DialogResult.OK)
          {
            SaveConfigFile();
            RefreshListBox();
          }
          else
          {
            LoadSettings();
          }
        }
    }

    private void btnAdd_Click(object sender, System.EventArgs e)
    {
      AddItem();
    }

    private void btnEdit_Click(object sender, System.EventArgs e)
    {
      EditItem();
    }

    private void btnDelete_Click(object sender, System.EventArgs e)
    {
      DeleteItem();
    }

    private void btnClose_Click(object sender, System.EventArgs e)
    {
      if (SaveConfigFile() == true)
        this.Close();
    }

  }
}