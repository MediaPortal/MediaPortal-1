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
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GuiLanguage : SectionSettings
  {
    private System.Windows.Forms.CheckBox checkBoxUsePrefix;
    private MPGroupBox mpGroupBox1;
    private MPComboBox languageComboBox;
    private MPLabel label2;
    private IContainer components = null;

    public GuiLanguage()
      : this("Language Settings") {}

    public GuiLanguage(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      LoadLanguages();
    }

    private void LoadLanguages()
    {
      string[] languages = GUILocalizeStrings.SupportedLanguages();
      foreach (string language in languages)
      {
        languageComboBox.Items.Add(language);
      }

      languageComboBox.Text = GUILocalizeStrings.LocalSupported();
    }

    private void languageComboBox_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        // If the user selects another language the amount of chars in the character table might have changed.
        // Delete the font cache to trigger a recreation
        string fontCache = String.Format(@"{0}\fonts", String.Format(@"{0}\{1}", Config.GetFolder(Config.Dir.Cache), Config.SkinName));

        Log.Debug("Delete the font cache to trigger a recreation: {0}", fontCache);
        MediaPortal.Util.Utils.DirectoryDelete(fontCache, true);
      }
      catch (Exception) {}
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


    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        checkBoxUsePrefix.Checked = xmlreader.GetValueAsBool("gui", "myprefix", false);
        languageComboBox.Text = xmlreader.GetValueAsString("gui", "language", languageComboBox.Text);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        string prevLanguage = xmlwriter.GetValueAsString("gui", "language", "English");
        string skin = xmlwriter.GetValueAsString("skin", "name", "DefaultWide");
        if (prevLanguage != languageComboBox.Text)
        {
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, skin + @"\fonts"), "*");
        }

        xmlwriter.SetValue("gui", "language", languageComboBox.Text);
        xmlwriter.SetValueAsBool("gui", "myprefix", checkBoxUsePrefix.Checked);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.checkBoxUsePrefix = new System.Windows.Forms.CheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.languageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBoxUsePrefix
      // 
      this.checkBoxUsePrefix.AutoSize = true;
      this.checkBoxUsePrefix.Checked = true;
      this.checkBoxUsePrefix.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUsePrefix.Location = new System.Drawing.Point(19, 58);
      this.checkBoxUsePrefix.Name = "checkBoxUsePrefix";
      this.checkBoxUsePrefix.Size = new System.Drawing.Size(199, 17);
      this.checkBoxUsePrefix.TabIndex = 3;
      this.checkBoxUsePrefix.Text = "Use string prefixes (e.g. TV = My TV)";
      this.checkBoxUsePrefix.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.checkBoxUsePrefix);
      this.mpGroupBox1.Controls.Add(this.languageComboBox);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(460, 88);
      this.mpGroupBox1.TabIndex = 5;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Language Settings";
      // 
      // languageComboBox
      // 
      this.languageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.languageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.languageComboBox.Location = new System.Drawing.Point(118, 21);
      this.languageComboBox.Name = "languageComboBox";
      this.languageComboBox.Size = new System.Drawing.Size(325, 21);
      this.languageComboBox.TabIndex = 1;
      this.languageComboBox.DropDownClosed += new System.EventHandler(this.languageComboBox_DropDownClosed);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 16);
      this.label2.TabIndex = 0;
      this.label2.Text = "Display language:";
      // 
      // GuiLanguage
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "GuiLanguage";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion
  }
}