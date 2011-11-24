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

using System.ComponentModel;
using System.Collections.Generic;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TVTeletext : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPRadioButton radioButton1;
    private IContainer components = null;
    private bool _init = false;
    private MPCheckBox cbHiddenMode;
    private MPCheckBox cbTransparentMode;
    private MPCheckBox cbRememberValue;
    private MPLabel FontSizeLbl;
    private MPLabel FontSizeValueLbl;
    private MPNumericUpDown nudFontSize;
    public class ValueTextPair
    {
      public string _value;
      public string _displayText;

      public ValueTextPair(string newValue, string newDisplayText)
      {
        _value = newValue;
        _displayText = newDisplayText;
      }

      public string Value
      {
        get
        {
          return _value;
        }
      }
      public string DislpayText
      {
        get
        {
          return _displayText;
        }
      }

      public override string ToString()
      {
        return _displayText;
      }
    }

    private MediaPortal.UserInterface.Controls.MPComboBox LanguageComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel LanguageLabel;
    private MediaPortal.UserInterface.Controls.MPComboBox FontQualityComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel FontQualityLabel;
    public int pluginVersion;

    public TVTeletext()
      : this("TV Teletext") {}

    public TVTeletext(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      LanguageComboBox.DisplayMember = "DisplayText";
      LanguageComboBox.ValueMember = "Value";
      LanguageComboBox.DataSource = GetLanguageOptions();
      
      FontQualityComboBox.DisplayMember = "DisplayText";
      FontQualityComboBox.ValueMember = "Value";
      FontQualityComboBox.DataSource = GetFontQualityOptions();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;
        LoadSettings();
      }
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.nudFontSize = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.FontSizeValueLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.FontSizeLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbHiddenMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbRememberValue = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTransparentMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.FontQualityLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.FontQualityComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.LanguageLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.LanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.nudFontSize);
      this.groupBox1.Controls.Add(this.FontSizeValueLbl);
      this.groupBox1.Controls.Add(this.FontSizeLbl);
      this.groupBox1.Controls.Add(this.cbHiddenMode);
      this.groupBox1.Controls.Add(this.cbRememberValue);
      this.groupBox1.Controls.Add(this.cbTransparentMode);
      this.groupBox1.Controls.Add(this.FontQualityComboBox);
      this.groupBox1.Controls.Add(this.FontQualityLabel);
      this.groupBox1.Controls.Add(this.LanguageLabel);
      this.groupBox1.Controls.Add(this.LanguageComboBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(6, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 188);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // nudFontSize
      // 
      this.nudFontSize.Location = new System.Drawing.Point(114, 97);
      this.nudFontSize.Minimum = new decimal(new int[]
                                               {
                                                 50,
                                                 0,
                                                 0,
                                                 0
                                               });
      this.nudFontSize.Name = "nudFontSize";
      this.nudFontSize.Size = new System.Drawing.Size(56, 20);
      this.nudFontSize.TabIndex = 1;
      this.nudFontSize.Value = new decimal(new int[]
                                             {
                                               80,
                                               0,
                                               0,
                                               0
                                             });
      // 
      // FontSizeValueLbl
      // 
      this.FontSizeValueLbl.AutoSize = true;
      this.FontSizeValueLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
                                                           System.Drawing.FontStyle.Regular,
                                                           System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FontSizeValueLbl.Location = new System.Drawing.Point(176, 99);
      this.FontSizeValueLbl.Name = "FontSizeValueLbl";
      this.FontSizeValueLbl.Size = new System.Drawing.Size(105, 13);
      this.FontSizeValueLbl.TabIndex = 3;
      this.FontSizeValueLbl.Text = "% of maximum height";
      this.FontSizeValueLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // FontSizeLbl
      // 
      this.FontSizeLbl.AutoSize = true;
      this.FontSizeLbl.Location = new System.Drawing.Point(14, 99);
      this.FontSizeLbl.Name = "FontSizeLbl";
      this.FontSizeLbl.Size = new System.Drawing.Size(52, 13);
      this.FontSizeLbl.TabIndex = 1;
      this.FontSizeLbl.Text = "Font size:";
      // 
      // cbHiddenMode
      // 
      this.cbHiddenMode.AutoSize = true;
      this.cbHiddenMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbHiddenMode.Location = new System.Drawing.Point(17, 28);
      this.cbHiddenMode.Name = "cbHiddenMode";
      this.cbHiddenMode.Size = new System.Drawing.Size(87, 17);
      this.cbHiddenMode.TabIndex = 0;
      this.cbHiddenMode.Text = "Hidden mode";
      this.cbHiddenMode.UseVisualStyleBackColor = true;
      // 
      // cbRememberValue
      // 
      this.cbRememberValue.AutoSize = true;
      this.cbRememberValue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbRememberValue.Location = new System.Drawing.Point(17, 74);
      this.cbRememberValue.Name = "cbRememberValue";
      this.cbRememberValue.Size = new System.Drawing.Size(123, 17);
      this.cbRememberValue.TabIndex = 2;
      this.cbRememberValue.Text = "Remember last value";
      this.cbRememberValue.UseVisualStyleBackColor = true;
      // 
      // cbTransparentMode
      // 
      this.cbTransparentMode.AutoSize = true;
      this.cbTransparentMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTransparentMode.Location = new System.Drawing.Point(17, 51);
      this.cbTransparentMode.Name = "cbTransparentMode";
      this.cbTransparentMode.Size = new System.Drawing.Size(169, 17);
      this.cbTransparentMode.TabIndex = 1;
      this.cbTransparentMode.Text = "Transparent mode in fullscreen";
      this.cbTransparentMode.UseVisualStyleBackColor = true;
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 24);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // FontQualityComboBox
      // 
      this.FontQualityComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.FontQualityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.FontQualityComboBox.FormattingEnabled = true;
      this.FontQualityComboBox.Location = new System.Drawing.Point(114, 148);
      this.FontQualityComboBox.Name = "FontQualityComboBox";
      this.FontQualityComboBox.Size = new System.Drawing.Size(325, 21);
      this.FontQualityComboBox.TabIndex = 6;
      // 
      // FontQualityLabel
      // 
      this.FontQualityLabel.AutoSize = true;
      this.FontQualityLabel.Location = new System.Drawing.Point(14, 151);
      this.FontQualityLabel.Name = "FontQualityLabel";
      this.FontQualityLabel.Size = new System.Drawing.Size(63, 13);
      this.FontQualityLabel.TabIndex = 5;
      this.FontQualityLabel.Text = "Font Quality";
      // 
      // LanguageLabel
      // 
      this.LanguageLabel.AutoSize = true;
      this.LanguageLabel.Location = new System.Drawing.Point(14, 126);
      this.LanguageLabel.Name = "LanguageLabel";
      this.LanguageLabel.Size = new System.Drawing.Size(88, 13);
      this.LanguageLabel.TabIndex = 4;
      this.LanguageLabel.Text = "Default language";
      // 
      // LanguageComboBox
      // 
      this.LanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.LanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.LanguageComboBox.FormattingEnabled = true;
      this.LanguageComboBox.Location = new System.Drawing.Point(114, 123);
      this.LanguageComboBox.Name = "LanguageComboBox";
      this.LanguageComboBox.Size = new System.Drawing.Size(325, 21);
      this.LanguageComboBox.TabIndex = 3;
      // 
      // TVTeletext
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVTeletext";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }


      using (Settings xmlreader = new MPSettings())
      {
        string strValue;

        cbHiddenMode.Checked = xmlreader.GetValueAsBool("mytv", "teletextHidden", false);
        cbTransparentMode.Checked = xmlreader.GetValueAsBool("mytv", "teletextTransparent", false);
        cbRememberValue.Checked = xmlreader.GetValueAsBool("mytv", "teletextRemember", true);
        nudFontSize.Value = xmlreader.GetValueAsInt("mytv", "teletextMaxFontSize", 100);

        // Read the language setting
        strValue = xmlreader.GetValueAsString("myteletext", "defaultLanguage", "latin1");
        try
        { 
          LanguageComboBox.SelectedValue = strValue.ToLower(); 
        }
        catch
        { 
        }
        strValue = xmlreader.GetValueAsString("myteletext", "fontQuality", "normal-gridfit");
        try
        {
          FontQualityComboBox.SelectedValue = strValue.ToLower(); 
        }
        catch 
        { 
        }

      }
    }

    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("mytv", "teletextHidden", cbHiddenMode.Checked);
        xmlwriter.SetValueAsBool("mytv", "teletextTransparent", cbTransparentMode.Checked);
        xmlwriter.SetValueAsBool("mytv", "teletextRemember", cbRememberValue.Checked);
        xmlwriter.SetValue("mytv", "teletextMaxFontSize", nudFontSize.Value);
        xmlwriter.SetValue("myteletext", "defaultLanguage", LanguageComboBox.SelectedValue);
        xmlwriter.SetValue("myteletext", "fontQuality", FontQualityComboBox.SelectedValue);    
      }
    }
    protected List<ValueTextPair> GetLanguageOptions()
    {
      List<ValueTextPair> al = new List<ValueTextPair>();

      
      al.Add(new ValueTextPair("latin1", "Latin"));
      al.Add(new ValueTextPair("latin2", "Latin / Polish"));
      al.Add(new ValueTextPair("latin3", "Latin / Turkish"));
      al.Add(new ValueTextPair("latin4", "Latin: Serbian / Croatian / Slovenian / Romanian"));
      al.Add(new ValueTextPair("cyrilic", "Cyrilic"));
      al.Add(new ValueTextPair("greek", "Greek / Turkish"));
      al.Add(new ValueTextPair("arabic", "Arabic"));
      al.Add(new ValueTextPair("hebrew", "Hebrew / Arabic"));
      return al;
    }

    protected List<ValueTextPair> GetFontQualityOptions()
    {
      List<ValueTextPair> al = new List<ValueTextPair>();
      al.Add(new ValueTextPair("normal", "Normal (Fastest)"));
      al.Add(new ValueTextPair("normal-gridfit", "Normal with hinting (Faster)"));
      al.Add(new ValueTextPair("smooth", "Smooth (Slower)"));
      al.Add(new ValueTextPair("smooth-gridfit", "Smooth with hinting (Slowest)"));
      return al;
    }
  }
}