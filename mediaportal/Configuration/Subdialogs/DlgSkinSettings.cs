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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using MediaPortal.WinCustomControls;


namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DlgSkinSettings.
  /// </summary>
  public class DlgSkinSettings : MPConfigForm
  {
    private MPTabControl tabControlSkinSettings;
    private TabPage tabPageTVGuideSettings;
    private MPTabControl tabControlTvGuideSettings;
    private MPTabPage tabPageTvGuideColors;
    private MPGroupBox groupGenreColors;
    private MPListView listViewGuideGenres;
    private ColumnHeader columnHeader9;
    private MPButton mpButtonOnLaterColor;
    private MPButton mpButtonOnNowColor;
    private GroupBox groupGroupColor;
    private MPLabel mpLabel11;
    private WinCustomControls.ColorComboBox colorComboBoxGroupSel;
    private MPLabel mpLabel12;
    private WinCustomControls.ColorComboBox colorComboBoxGroup;
    private GroupBox groupChannelColors;
    private MPLabel mpLabel9;
    private WinCustomControls.ColorComboBox colorComboBoxChannelSel;
    private MPLabel mpLabel10;
    private WinCustomControls.ColorComboBox colorComboBoxChannel;
    private GroupBox groupDefaultColors;
    private MPLabel mpLabel14;
    private WinCustomControls.ColorComboBox colorComboBoxPgmOnLater;
    private MPLabel mpLabel15;
    private WinCustomControls.ColorComboBox colorComboBoxPgmOnNow;
    private MPLabel mpLabel13;
    private WinCustomControls.ColorComboBox colorComboBoxPgmBorder;
    private MPLabel mpLabel8;
    private WinCustomControls.ColorComboBox colorComboBoxPgmSel;
    private MPLabel mpLabel4;
    private WinCustomControls.ColorComboBox colorComboBoxPgmEnded;

    protected const int LOCALIZED_GENRE_STRING_BASE = 1250;
    protected const int LOCALIZED_GENRE_STRING_COUNT = 7;
    protected List<string> _defaultGenreColors = new List<string>()
	    {
       "FF18D22E,FF18D22E",
       "FFFF69B4,FFFF69B4",
       "FFFA1919,FFFA1919",
       "FF800000,FF800000",
       "FF2169EE,FF2169EE",
       "FFFFD700,FFFFD700",
       "FF006400,FF006400"
      };

    protected bool _guideColorsLoaded = false;
    protected long _guideColorProgramOnNow = 0;
    protected long _guideColorProgramOnLater = 0;
    protected long _guideColorChannelButton = 0;
    protected long _guideColorChannelButtonSelected = 0;
    protected long _guideColorGroupButton = 0;
    protected long _guideColorGroupButtonSelected = 0;
    protected long _guideColorProgramEnded = 0;
    protected long _guideColorProgramSelected = 0;
    protected long _guideColorBorderHighlight = 0;
    protected List<string> _genreList = new List<string>();
    protected IDictionary<string, long> _genreColorsOnNow = new Dictionary<string, long>();
    protected IDictionary<string, long> _genreColorsOnLater = new Dictionary<string, long>();
    private Button mpButtonOk;
    private Button mpButtonCancel;
    private MPBeveledLine beveledLine1;
    private TabPage tabPageGeneral;

    private string _selectedSkin = "";
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private MPGradientLabel headerLabel;
    private MPGroupBox groupBoxTheme;
    private Panel panelFitImage;
    private PictureBox previewPictureBox;
    private ListView listViewAvailableThemes;
    private ColumnHeader colName;
    private ColumnHeader colVersion;
    private GroupBox gbGenreSettings;
    private CheckBox cbGenreColorKey;
    private CheckBox cbGenreColoring;
    private TabPage tabPageTvGuideGeneral;
    private CheckBox cbBorderHighlight;
    private CheckBox cbColoredGuide;
    private Label labelTVPluginNotInstalled;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public DlgSkinSettings(ListView listViewAvailableSkins)
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      // If TV is not used then remove the tabs for TV guide settings.
      if (!UseTvServer)
      {
        this.tabControlTvGuideSettings.Enabled = false;
        this.gbGenreSettings.Visible = false;

        this.labelTVPluginNotInstalled.Text = GUILocalizeStrings.Get(59);
        this.labelTVPluginNotInstalled.Visible = true;
        this.labelTVPluginNotInstalled.Enabled = true;
      }
      else
      {
        this.tabControlTvGuideSettings.Enabled = true;
        this.gbGenreSettings.Visible = true;

        this.labelTVPluginNotInstalled.Visible = false;
        this.labelTVPluginNotInstalled.Enabled = false;
      }

      // Identify the selected skin.
      _selectedSkin = listViewAvailableSkins.Items[listViewAvailableSkins.SelectedIndices[0]].Text;
      Config.SkinName = _selectedSkin;  // Need to set the Config property so SKSettings reads the correct skin settings file.
      headerLabel.Caption = _selectedSkin;

      // Load settings for selected skin.
      LoadSettings(_selectedSkin);
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
      this.tabControlSkinSettings = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageGeneral = new System.Windows.Forms.TabPage();
      this.groupBoxTheme = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.panelFitImage = new System.Windows.Forms.Panel();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.listViewAvailableThemes = new System.Windows.Forms.ListView();
      this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tabPageTVGuideSettings = new System.Windows.Forms.TabPage();
      this.tabControlTvGuideSettings = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageTvGuideGeneral = new System.Windows.Forms.TabPage();
      this.labelTVPluginNotInstalled = new System.Windows.Forms.Label();
      this.gbGenreSettings = new System.Windows.Forms.GroupBox();
      this.cbColoredGuide = new System.Windows.Forms.CheckBox();
      this.cbBorderHighlight = new System.Windows.Forms.CheckBox();
      this.cbGenreColorKey = new System.Windows.Forms.CheckBox();
      this.cbGenreColoring = new System.Windows.Forms.CheckBox();
      this.tabPageTvGuideColors = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupGenreColors = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.listViewGuideGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpButtonOnLaterColor = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOnNowColor = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupGroupColor = new System.Windows.Forms.GroupBox();
      this.mpLabel11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxGroupSel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxGroup = new MediaPortal.WinCustomControls.ColorComboBox();
      this.groupChannelColors = new System.Windows.Forms.GroupBox();
      this.mpLabel9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxChannelSel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxChannel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.groupDefaultColors = new System.Windows.Forms.GroupBox();
      this.mpLabel14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmOnLater = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmOnNow = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmBorder = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmSel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmEnded = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpButtonOk = new System.Windows.Forms.Button();
      this.mpButtonCancel = new System.Windows.Forms.Button();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.tabControlSkinSettings.SuspendLayout();
      this.tabPageGeneral.SuspendLayout();
      this.groupBoxTheme.SuspendLayout();
      this.panelFitImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
      this.tabPageTVGuideSettings.SuspendLayout();
      this.tabControlTvGuideSettings.SuspendLayout();
      this.tabPageTvGuideGeneral.SuspendLayout();
      this.gbGenreSettings.SuspendLayout();
      this.tabPageTvGuideColors.SuspendLayout();
      this.groupGenreColors.SuspendLayout();
      this.groupGroupColor.SuspendLayout();
      this.groupChannelColors.SuspendLayout();
      this.groupDefaultColors.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlSkinSettings
      // 
      this.tabControlSkinSettings.Controls.Add(this.tabPageGeneral);
      this.tabControlSkinSettings.Controls.Add(this.tabPageTVGuideSettings);
      this.tabControlSkinSettings.Location = new System.Drawing.Point(17, 42);
      this.tabControlSkinSettings.Name = "tabControlSkinSettings";
      this.tabControlSkinSettings.SelectedIndex = 0;
      this.tabControlSkinSettings.Size = new System.Drawing.Size(472, 479);
      this.tabControlSkinSettings.TabIndex = 12;
      // 
      // tabPageGeneral
      // 
      this.tabPageGeneral.Controls.Add(this.groupBoxTheme);
      this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabPageGeneral.Name = "tabPageGeneral";
      this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageGeneral.Size = new System.Drawing.Size(464, 453);
      this.tabPageGeneral.TabIndex = 6;
      this.tabPageGeneral.Text = "General";
      this.tabPageGeneral.UseVisualStyleBackColor = true;
      // 
      // groupBoxTheme
      // 
      this.groupBoxTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTheme.Controls.Add(this.panelFitImage);
      this.groupBoxTheme.Controls.Add(this.listViewAvailableThemes);
      this.groupBoxTheme.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTheme.Location = new System.Drawing.Point(6, 5);
      this.groupBoxTheme.Name = "groupBoxTheme";
      this.groupBoxTheme.Size = new System.Drawing.Size(452, 442);
      this.groupBoxTheme.TabIndex = 5;
      this.groupBoxTheme.TabStop = false;
      this.groupBoxTheme.Text = "Theme selection";
      // 
      // panelFitImage
      // 
      this.panelFitImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.panelFitImage.Controls.Add(this.previewPictureBox);
      this.panelFitImage.Location = new System.Drawing.Point(12, 22);
      this.panelFitImage.Name = "panelFitImage";
      this.panelFitImage.Size = new System.Drawing.Size(430, 222);
      this.panelFitImage.TabIndex = 5;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.previewPictureBox.Image = global::MediaPortal.Configuration.Properties.Resources.mplogo;
      this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(430, 222);
      this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.previewPictureBox.TabIndex = 5;
      this.previewPictureBox.TabStop = false;
      // 
      // listViewAvailableThemes
      // 
      this.listViewAvailableThemes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewAvailableThemes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colVersion});
      this.listViewAvailableThemes.FullRowSelect = true;
      this.listViewAvailableThemes.HideSelection = false;
      this.listViewAvailableThemes.Location = new System.Drawing.Point(11, 262);
      this.listViewAvailableThemes.MultiSelect = false;
      this.listViewAvailableThemes.Name = "listViewAvailableThemes";
      this.listViewAvailableThemes.Size = new System.Drawing.Size(430, 168);
      this.listViewAvailableThemes.TabIndex = 3;
      this.listViewAvailableThemes.UseCompatibleStateImageBehavior = false;
      this.listViewAvailableThemes.View = System.Windows.Forms.View.Details;
      this.listViewAvailableThemes.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableThemes_SelectedIndexChanged);
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 329;
      // 
      // colVersion
      // 
      this.colVersion.Text = "Version";
      this.colVersion.Width = 86;
      // 
      // tabPageTVGuideSettings
      // 
      this.tabPageTVGuideSettings.Controls.Add(this.tabControlTvGuideSettings);
      this.tabPageTVGuideSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageTVGuideSettings.Name = "tabPageTVGuideSettings";
      this.tabPageTVGuideSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTVGuideSettings.Size = new System.Drawing.Size(464, 453);
      this.tabPageTVGuideSettings.TabIndex = 5;
      this.tabPageTVGuideSettings.Text = "TV guide";
      this.tabPageTVGuideSettings.UseVisualStyleBackColor = true;
      // 
      // tabControlTvGuideSettings
      // 
      this.tabControlTvGuideSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlTvGuideSettings.Controls.Add(this.tabPageTvGuideGeneral);
      this.tabControlTvGuideSettings.Controls.Add(this.tabPageTvGuideColors);
      this.tabControlTvGuideSettings.HotTrack = true;
      this.tabControlTvGuideSettings.Location = new System.Drawing.Point(6, 6);
      this.tabControlTvGuideSettings.Name = "tabControlTvGuideSettings";
      this.tabControlTvGuideSettings.SelectedIndex = 0;
      this.tabControlTvGuideSettings.Size = new System.Drawing.Size(452, 441);
      this.tabControlTvGuideSettings.TabIndex = 1;
      // 
      // tabPageTvGuideGeneral
      // 
      this.tabPageTvGuideGeneral.Controls.Add(this.labelTVPluginNotInstalled);
      this.tabPageTvGuideGeneral.Controls.Add(this.gbGenreSettings);
      this.tabPageTvGuideGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabPageTvGuideGeneral.Name = "tabPageTvGuideGeneral";
      this.tabPageTvGuideGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTvGuideGeneral.Size = new System.Drawing.Size(444, 415);
      this.tabPageTvGuideGeneral.TabIndex = 2;
      this.tabPageTvGuideGeneral.Text = "Settings";
      this.tabPageTvGuideGeneral.UseVisualStyleBackColor = true;
      // 
      // labelTVPluginNotInstalled
      // 
      this.labelTVPluginNotInstalled.AutoSize = true;
      this.labelTVPluginNotInstalled.ForeColor = System.Drawing.SystemColors.ControlText;
      this.labelTVPluginNotInstalled.Location = new System.Drawing.Point(6, 28);
      this.labelTVPluginNotInstalled.Name = "labelTVPluginNotInstalled";
      this.labelTVPluginNotInstalled.Size = new System.Drawing.Size(128, 13);
      this.labelTVPluginNotInstalled.TabIndex = 1;
      this.labelTVPluginNotInstalled.Text = "labelTVPluginNotInstalled";
      // 
      // gbGenreSettings
      // 
      this.gbGenreSettings.Controls.Add(this.cbColoredGuide);
      this.gbGenreSettings.Controls.Add(this.cbBorderHighlight);
      this.gbGenreSettings.Controls.Add(this.cbGenreColorKey);
      this.gbGenreSettings.Controls.Add(this.cbGenreColoring);
      this.gbGenreSettings.Location = new System.Drawing.Point(6, 6);
      this.gbGenreSettings.Name = "gbGenreSettings";
      this.gbGenreSettings.Size = new System.Drawing.Size(432, 117);
      this.gbGenreSettings.TabIndex = 0;
      this.gbGenreSettings.TabStop = false;
      this.gbGenreSettings.Text = "Program color settings";
      // 
      // cbColoredGuide
      // 
      this.cbColoredGuide.AutoSize = true;
      this.cbColoredGuide.Location = new System.Drawing.Point(10, 21);
      this.cbColoredGuide.Name = "cbColoredGuide";
      this.cbColoredGuide.Size = new System.Drawing.Size(247, 17);
      this.cbColoredGuide.TabIndex = 3;
      this.cbColoredGuide.Text = "Enable guide coloring (set colors on Colors tab)";
      this.cbColoredGuide.UseVisualStyleBackColor = true;
      this.cbColoredGuide.CheckedChanged += new System.EventHandler(this.cbColoredGuide_CheckedChanged);
      // 
      // cbBorderHighlight
      // 
      this.cbBorderHighlight.AutoSize = true;
      this.cbBorderHighlight.Location = new System.Drawing.Point(10, 90);
      this.cbBorderHighlight.Name = "cbBorderHighlight";
      this.cbBorderHighlight.Size = new System.Drawing.Size(201, 17);
      this.cbBorderHighlight.TabIndex = 2;
      this.cbBorderHighlight.Text = "Border highlight the selected program";
      this.cbBorderHighlight.UseVisualStyleBackColor = true;
      // 
      // cbGenreColorKey
      // 
      this.cbGenreColorKey.AutoSize = true;
      this.cbGenreColorKey.Location = new System.Drawing.Point(33, 67);
      this.cbGenreColorKey.Name = "cbGenreColorKey";
      this.cbGenreColorKey.Size = new System.Drawing.Size(136, 17);
      this.cbGenreColorKey.TabIndex = 1;
      this.cbGenreColorKey.Text = "Display genre color key";
      this.cbGenreColorKey.UseVisualStyleBackColor = true;
      // 
      // cbGenreColoring
      // 
      this.cbGenreColoring.AutoSize = true;
      this.cbGenreColoring.Location = new System.Drawing.Point(22, 44);
      this.cbGenreColoring.Name = "cbGenreColoring";
      this.cbGenreColoring.Size = new System.Drawing.Size(297, 17);
      this.cbGenreColoring.TabIndex = 0;
      this.cbGenreColoring.Text = "Enable program genre coloring (forces border highlighting)";
      this.cbGenreColoring.UseVisualStyleBackColor = true;
      this.cbGenreColoring.CheckedChanged += new System.EventHandler(this.cbGenreColoring_CheckedChanged);
      // 
      // tabPageTvGuideColors
      // 
      this.tabPageTvGuideColors.Controls.Add(this.groupGenreColors);
      this.tabPageTvGuideColors.Controls.Add(this.groupGroupColor);
      this.tabPageTvGuideColors.Controls.Add(this.groupChannelColors);
      this.tabPageTvGuideColors.Controls.Add(this.groupDefaultColors);
      this.tabPageTvGuideColors.Location = new System.Drawing.Point(4, 22);
      this.tabPageTvGuideColors.Name = "tabPageTvGuideColors";
      this.tabPageTvGuideColors.Size = new System.Drawing.Size(444, 415);
      this.tabPageTvGuideColors.TabIndex = 1;
      this.tabPageTvGuideColors.Text = "Colors";
      this.tabPageTvGuideColors.UseVisualStyleBackColor = true;
      // 
      // groupGenreColors
      // 
      this.groupGenreColors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupGenreColors.Controls.Add(this.listViewGuideGenres);
      this.groupGenreColors.Controls.Add(this.mpButtonOnLaterColor);
      this.groupGenreColors.Controls.Add(this.mpButtonOnNowColor);
      this.groupGenreColors.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupGenreColors.Location = new System.Drawing.Point(6, 3);
      this.groupGenreColors.Name = "groupGenreColors";
      this.groupGenreColors.Size = new System.Drawing.Size(432, 178);
      this.groupGenreColors.TabIndex = 0;
      this.groupGenreColors.TabStop = false;
      this.groupGenreColors.Text = "Program genre colors";
      // 
      // listViewGuideGenres
      // 
      this.listViewGuideGenres.AllowDrop = true;
      this.listViewGuideGenres.AllowRowReorder = true;
      this.listViewGuideGenres.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewGuideGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.columnHeader1,
            this.columnHeader2});
      this.listViewGuideGenres.HideSelection = false;
      this.listViewGuideGenres.Location = new System.Drawing.Point(6, 19);
      this.listViewGuideGenres.Name = "listViewGuideGenres";
      this.listViewGuideGenres.Size = new System.Drawing.Size(420, 123);
      this.listViewGuideGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewGuideGenres.TabIndex = 14;
      this.listViewGuideGenres.UseCompatibleStateImageBehavior = false;
      this.listViewGuideGenres.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "Guide Genre";
      this.columnHeader9.Width = 190;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "On Now Color";
      this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.columnHeader1.Width = 100;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "On Later Color";
      this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.columnHeader2.Width = 100;
      // 
      // mpButtonOnLaterColor
      // 
      this.mpButtonOnLaterColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOnLaterColor.Location = new System.Drawing.Point(102, 150);
      this.mpButtonOnLaterColor.Name = "mpButtonOnLaterColor";
      this.mpButtonOnLaterColor.Size = new System.Drawing.Size(90, 22);
      this.mpButtonOnLaterColor.TabIndex = 11;
      this.mpButtonOnLaterColor.Text = "On Later Color";
      this.mpButtonOnLaterColor.UseVisualStyleBackColor = true;
      this.mpButtonOnLaterColor.Click += new System.EventHandler(this.mpButtonOnLaterColor_Click);
      // 
      // mpButtonOnNowColor
      // 
      this.mpButtonOnNowColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOnNowColor.Location = new System.Drawing.Point(6, 150);
      this.mpButtonOnNowColor.Name = "mpButtonOnNowColor";
      this.mpButtonOnNowColor.Size = new System.Drawing.Size(90, 22);
      this.mpButtonOnNowColor.TabIndex = 10;
      this.mpButtonOnNowColor.Text = "On Now Color";
      this.mpButtonOnNowColor.UseVisualStyleBackColor = true;
      this.mpButtonOnNowColor.Click += new System.EventHandler(this.mpButtonOnNowColor_Click);
      // 
      // groupGroupColor
      // 
      this.groupGroupColor.Controls.Add(this.mpLabel11);
      this.groupGroupColor.Controls.Add(this.colorComboBoxGroupSel);
      this.groupGroupColor.Controls.Add(this.mpLabel12);
      this.groupGroupColor.Controls.Add(this.colorComboBoxGroup);
      this.groupGroupColor.Location = new System.Drawing.Point(6, 359);
      this.groupGroupColor.Name = "groupGroupColor";
      this.groupGroupColor.Size = new System.Drawing.Size(432, 50);
      this.groupGroupColor.TabIndex = 12;
      this.groupGroupColor.TabStop = false;
      this.groupGroupColor.Text = "Channel group select colors";
      // 
      // mpLabel11
      // 
      this.mpLabel11.AutoSize = true;
      this.mpLabel11.Location = new System.Drawing.Point(214, 22);
      this.mpLabel11.Name = "mpLabel11";
      this.mpLabel11.Size = new System.Drawing.Size(52, 13);
      this.mpLabel11.TabIndex = 11;
      this.mpLabel11.Text = "Selected:";
      // 
      // colorComboBoxGroupSel
      // 
      this.colorComboBoxGroupSel.Extended = false;
      this.colorComboBoxGroupSel.Location = new System.Drawing.Point(274, 17);
      this.colorComboBoxGroupSel.Name = "colorComboBoxGroupSel";
      this.colorComboBoxGroupSel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxGroupSel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxGroupSel.TabIndex = 10;
      this.colorComboBoxGroupSel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnGroupSelColorChanged);
      this.colorComboBoxGroupSel.Load += new System.EventHandler(this.colorComboBoxGroupSel_Load);
      // 
      // mpLabel12
      // 
      this.mpLabel12.AutoSize = true;
      this.mpLabel12.Location = new System.Drawing.Point(45, 22);
      this.mpLabel12.Name = "mpLabel12";
      this.mpLabel12.Size = new System.Drawing.Size(43, 13);
      this.mpLabel12.TabIndex = 9;
      this.mpLabel12.Text = "Normal:";
      // 
      // colorComboBoxGroup
      // 
      this.colorComboBoxGroup.Extended = false;
      this.colorComboBoxGroup.Location = new System.Drawing.Point(97, 17);
      this.colorComboBoxGroup.Name = "colorComboBoxGroup";
      this.colorComboBoxGroup.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxGroup.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxGroup.TabIndex = 0;
      this.colorComboBoxGroup.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnGroupColorChanged);
      this.colorComboBoxGroup.Load += new System.EventHandler(this.colorComboBoxGroup_Load);
      // 
      // groupChannelColors
      // 
      this.groupChannelColors.Controls.Add(this.mpLabel9);
      this.groupChannelColors.Controls.Add(this.colorComboBoxChannelSel);
      this.groupChannelColors.Controls.Add(this.mpLabel10);
      this.groupChannelColors.Controls.Add(this.colorComboBoxChannel);
      this.groupChannelColors.Location = new System.Drawing.Point(6, 303);
      this.groupChannelColors.Name = "groupChannelColors";
      this.groupChannelColors.Size = new System.Drawing.Size(432, 50);
      this.groupChannelColors.TabIndex = 12;
      this.groupChannelColors.TabStop = false;
      this.groupChannelColors.Text = "Channel colors";
      // 
      // mpLabel9
      // 
      this.mpLabel9.AutoSize = true;
      this.mpLabel9.Location = new System.Drawing.Point(214, 22);
      this.mpLabel9.Name = "mpLabel9";
      this.mpLabel9.Size = new System.Drawing.Size(52, 13);
      this.mpLabel9.TabIndex = 11;
      this.mpLabel9.Text = "Selected:";
      // 
      // colorComboBoxChannelSel
      // 
      this.colorComboBoxChannelSel.Extended = false;
      this.colorComboBoxChannelSel.Location = new System.Drawing.Point(274, 17);
      this.colorComboBoxChannelSel.Name = "colorComboBoxChannelSel";
      this.colorComboBoxChannelSel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxChannelSel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxChannelSel.TabIndex = 10;
      this.colorComboBoxChannelSel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnChannelSelColorChanged);
      this.colorComboBoxChannelSel.Load += new System.EventHandler(this.colorComboBoxChannelSel_Load);
      // 
      // mpLabel10
      // 
      this.mpLabel10.AutoSize = true;
      this.mpLabel10.Location = new System.Drawing.Point(45, 22);
      this.mpLabel10.Name = "mpLabel10";
      this.mpLabel10.Size = new System.Drawing.Size(43, 13);
      this.mpLabel10.TabIndex = 9;
      this.mpLabel10.Text = "Normal:";
      // 
      // colorComboBoxChannel
      // 
      this.colorComboBoxChannel.Extended = false;
      this.colorComboBoxChannel.Location = new System.Drawing.Point(97, 17);
      this.colorComboBoxChannel.Name = "colorComboBoxChannel";
      this.colorComboBoxChannel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxChannel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxChannel.TabIndex = 0;
      this.colorComboBoxChannel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnChannelColorChanged);
      this.colorComboBoxChannel.Load += new System.EventHandler(this.colorComboBoxChannel_Load);
      // 
      // groupDefaultColors
      // 
      this.groupDefaultColors.Controls.Add(this.mpLabel14);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmOnLater);
      this.groupDefaultColors.Controls.Add(this.mpLabel15);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmOnNow);
      this.groupDefaultColors.Controls.Add(this.mpLabel13);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmBorder);
      this.groupDefaultColors.Controls.Add(this.mpLabel8);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmSel);
      this.groupDefaultColors.Controls.Add(this.mpLabel4);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmEnded);
      this.groupDefaultColors.Location = new System.Drawing.Point(6, 187);
      this.groupDefaultColors.Name = "groupDefaultColors";
      this.groupDefaultColors.Size = new System.Drawing.Size(432, 110);
      this.groupDefaultColors.TabIndex = 0;
      this.groupDefaultColors.TabStop = false;
      this.groupDefaultColors.Text = "Program default colors";
      // 
      // mpLabel14
      // 
      this.mpLabel14.AutoSize = true;
      this.mpLabel14.Location = new System.Drawing.Point(219, 25);
      this.mpLabel14.Name = "mpLabel14";
      this.mpLabel14.Size = new System.Drawing.Size(47, 13);
      this.mpLabel14.TabIndex = 17;
      this.mpLabel14.Text = "On later:";
      // 
      // colorComboBoxPgmOnLater
      // 
      this.colorComboBoxPgmOnLater.Extended = false;
      this.colorComboBoxPgmOnLater.Location = new System.Drawing.Point(274, 20);
      this.colorComboBoxPgmOnLater.Name = "colorComboBoxPgmOnLater";
      this.colorComboBoxPgmOnLater.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmOnLater.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmOnLater.TabIndex = 16;
      this.colorComboBoxPgmOnLater.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmOnLaterColorChanged);
      this.colorComboBoxPgmOnLater.Load += new System.EventHandler(this.colorComboBoxPgmOnLater_Load);
      // 
      // mpLabel15
      // 
      this.mpLabel15.AutoSize = true;
      this.mpLabel15.Location = new System.Drawing.Point(41, 25);
      this.mpLabel15.Name = "mpLabel15";
      this.mpLabel15.Size = new System.Drawing.Size(47, 13);
      this.mpLabel15.TabIndex = 15;
      this.mpLabel15.Text = "On now:";
      // 
      // colorComboBoxPgmOnNow
      // 
      this.colorComboBoxPgmOnNow.Extended = false;
      this.colorComboBoxPgmOnNow.Location = new System.Drawing.Point(97, 20);
      this.colorComboBoxPgmOnNow.Name = "colorComboBoxPgmOnNow";
      this.colorComboBoxPgmOnNow.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmOnNow.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmOnNow.TabIndex = 14;
      this.colorComboBoxPgmOnNow.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmOnNowColorChanged);
      this.colorComboBoxPgmOnNow.Load += new System.EventHandler(this.colorComboBoxPgmOnNow_Load);
      // 
      // mpLabel13
      // 
      this.mpLabel13.AutoSize = true;
      this.mpLabel13.Location = new System.Drawing.Point(5, 83);
      this.mpLabel13.Name = "mpLabel13";
      this.mpLabel13.Size = new System.Drawing.Size(83, 13);
      this.mpLabel13.TabIndex = 13;
      this.mpLabel13.Text = "Border highlight:";
      // 
      // colorComboBoxPgmBorder
      // 
      this.colorComboBoxPgmBorder.Extended = false;
      this.colorComboBoxPgmBorder.Location = new System.Drawing.Point(97, 78);
      this.colorComboBoxPgmBorder.Name = "colorComboBoxPgmBorder";
      this.colorComboBoxPgmBorder.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmBorder.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmBorder.TabIndex = 12;
      this.colorComboBoxPgmBorder.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmBorderColorChanged);
      this.colorComboBoxPgmBorder.Load += new System.EventHandler(this.colorComboBoxPgmBorder_Load);
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(214, 54);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(52, 13);
      this.mpLabel8.TabIndex = 11;
      this.mpLabel8.Text = "Selected:";
      // 
      // colorComboBoxPgmSel
      // 
      this.colorComboBoxPgmSel.Extended = false;
      this.colorComboBoxPgmSel.Location = new System.Drawing.Point(274, 49);
      this.colorComboBoxPgmSel.Name = "colorComboBoxPgmSel";
      this.colorComboBoxPgmSel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmSel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmSel.TabIndex = 10;
      this.colorComboBoxPgmSel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmSelColorChanged);
      this.colorComboBoxPgmSel.Load += new System.EventHandler(this.colorComboBoxPgmSel_Load);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(47, 54);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(41, 13);
      this.mpLabel4.TabIndex = 9;
      this.mpLabel4.Text = "Ended:";
      // 
      // colorComboBoxPgmEnded
      // 
      this.colorComboBoxPgmEnded.Extended = false;
      this.colorComboBoxPgmEnded.Location = new System.Drawing.Point(97, 49);
      this.colorComboBoxPgmEnded.Name = "colorComboBoxPgmEnded";
      this.colorComboBoxPgmEnded.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmEnded.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmEnded.TabIndex = 0;
      this.colorComboBoxPgmEnded.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmEndedColorChanged);
      this.colorComboBoxPgmEnded.Load += new System.EventHandler(this.colorComboBoxPgmEnded_Load);
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(327, 537);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 13;
      this.mpButtonOk.Text = "OK";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(410, 537);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 14;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(9, 527);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(488, 2);
      this.beveledLine1.TabIndex = 18;
      this.beveledLine1.TabStop = false;
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(17, 3);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(472, 24);
      this.headerLabel.TabIndex = 19;
      this.headerLabel.TabStop = false;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // DlgSkinSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(506, 570);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.Controls.Add(this.tabControlSkinSettings);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "DlgSkinSettings";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MediaPortal Skin Settings";
      this.tabControlSkinSettings.ResumeLayout(false);
      this.tabPageGeneral.ResumeLayout(false);
      this.groupBoxTheme.ResumeLayout(false);
      this.panelFitImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
      this.tabPageTVGuideSettings.ResumeLayout(false);
      this.tabControlTvGuideSettings.ResumeLayout(false);
      this.tabPageTvGuideGeneral.ResumeLayout(false);
      this.tabPageTvGuideGeneral.PerformLayout();
      this.gbGenreSettings.ResumeLayout(false);
      this.gbGenreSettings.PerformLayout();
      this.tabPageTvGuideColors.ResumeLayout(false);
      this.groupGenreColors.ResumeLayout(false);
      this.groupGroupColor.ResumeLayout(false);
      this.groupGroupColor.PerformLayout();
      this.groupChannelColors.ResumeLayout(false);
      this.groupChannelColors.PerformLayout();
      this.groupDefaultColors.ResumeLayout(false);
      this.groupDefaultColors.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public static bool UseTvServer
    {
      get { return File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"); }
    }

    private bool LoadGenreList(Settings xmlreader)
    {
      int genreId;
      string genre;
      IDictionary<string, string> allGenres = xmlreader.GetSection<string>("genremap");

      // Each genre map entry is a '{' delimited list of "program" genre names (those that may be compared with the genre from the program listings).
      // It is an error if a single "program" genre is mapped to more than one genre color category; behavior is undefined for this condition.
      foreach (var genreMapEntry in allGenres)
      {
        // The genremap key is an integer value that is added to a base value in order to locate the correct localized genre name string.
        genreId = int.Parse(genreMapEntry.Key);
        genre = GUILocalizeStrings.Get(LOCALIZED_GENRE_STRING_BASE + genreId);
        _genreList.Add(genre);
      }

      return _genreList.Count > 0;
    }

    private bool LoadGuideColors(Settings xmlreader)
    {
      List<string> temp;

      // Load supporting guide colors.
      _guideColorChannelButton = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorchannelbutton", "FF404040"));
      _guideColorChannelButtonSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorchannelbuttonselected", "FF6495ED"));
      _guideColorGroupButton = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorgroupbutton", "FF404040"));
      _guideColorGroupButtonSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorgroupbuttonselected", "FF6495ED"));
      _guideColorProgramSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorprogramselected", "FF6495ED"));
      _guideColorProgramEnded = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorprogramended", "FF202020"));
      _guideColorBorderHighlight = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorborderhighlight", "FF6DF0FF"));

      // Load the default genre colors.
      temp = new List<string>((xmlreader.GetValueAsString("tvguidecolors", "defaultgenre", String.Empty)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
      if (temp.Count == 2)
      {
        _guideColorProgramOnNow = GetColorFromString(temp[0]);
        _guideColorProgramOnLater = GetColorFromString(temp[1]);
      }
      else if (temp.Count == 1)
      {
        _guideColorProgramOnNow = GetColorFromString(temp[0]);
        _guideColorProgramOnLater = _guideColorProgramOnNow;
      }
      else
      {
        _guideColorProgramOnNow = 0xFF404040; // Dark blue
        _guideColorProgramOnLater = 0xFF404040; // Light blue
      }

      // Each genre color entry is a csv list.  The first value is the color for program "on now", the second value is for program "on later".
      // If only one value is provided then that value is used for both.
      long color0;
      for (int i = 0; i < _genreList.Count; i++)
      {
        temp = new List<string>((xmlreader.GetValueAsString("tvguidecolors", i.ToString(), String.Empty)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        if (temp.Count > 0)
        {
          color0 = GetColorFromString(temp[0]);
          if (temp.Count == 2)
          {
            _genreColorsOnNow.Add(_genreList[i], color0);
            _genreColorsOnLater.Add(_genreList[i], GetColorFromString(temp[1]));
          }
          else if (temp.Count == 1)
          {
            _genreColorsOnNow.Add(_genreList[i], color0);
            _genreColorsOnLater.Add(_genreList[i], color0);
          }
        }
      }

      return _genreColorsOnNow.Count > 0;
    }

    private void SaveGuideColors(Settings xmlwriter)
    {
      // Save supporting guide colors.
      xmlwriter.SetValue("tvguidecolors", "guidecolorchannelbutton", String.Format("{0:X8}", (uint)_guideColorChannelButton));
      xmlwriter.SetValue("tvguidecolors", "guidecolorchannelbuttonselected", String.Format("{0:X8}", (uint)_guideColorChannelButtonSelected));
      xmlwriter.SetValue("tvguidecolors", "guidecolorgroupbutton", String.Format("{0:X8}", (uint)_guideColorGroupButton));
      xmlwriter.SetValue("tvguidecolors", "guidecolorgroupbuttonselected", String.Format("{0:X8}", (uint)_guideColorGroupButtonSelected));
      xmlwriter.SetValue("tvguidecolors", "guidecolorprogramselected", String.Format("{0:X8}", (uint)_guideColorProgramSelected));
      xmlwriter.SetValue("tvguidecolors", "guidecolorprogramended", String.Format("{0:X8}", (uint)_guideColorProgramEnded));
      xmlwriter.SetValue("tvguidecolors", "guidecolorborderhighlight", String.Format("{0:X8}", (uint)_guideColorBorderHighlight));
      xmlwriter.SetValue("tvguidecolors", "defaultgenre", String.Format("{0:X8}", (uint)_guideColorProgramOnNow) + "," + String.Format("{0:X8}", (uint)_guideColorProgramOnLater));

      // Each genre color entry is a csv list.  The first value is the color for program "on now", the second value is for program "on later".
      // If only one value is provided then that value is used for both.
      long onNowColor;
      long onLaterColor;

      for (int i = 0; i < _genreList.Count; i++)
      {
        _genreColorsOnNow.TryGetValue(_genreList[i], out onNowColor);
        _genreColorsOnLater.TryGetValue(_genreList[i], out onLaterColor);
        xmlwriter.SetValue("tvguidecolors", i.ToString(), String.Format("{0:X8}", (uint)onNowColor) + "," + String.Format("{0:X8}", (uint)onLaterColor));
      }
    }

    private long GetColorFromString(string strColor)
    {
      long result = 0xFFFFFFFF;

      if (long.TryParse(strColor, System.Globalization.NumberStyles.HexNumber, null, out result))
      {
        // Result set in out param
      }
      else if (Color.FromName(strColor).IsKnownColor)
      {
        result = Color.FromName(strColor).ToArgb();
      }

      return result;
    }

    private void PopulateGuideGenreList()
    {
      // Populate the guide genre list with names and colors.
      listViewGuideGenres.BeginUpdate();
      listViewGuideGenres.Items.Clear();

      foreach (string genre in _genreList)
      {
        long lColorOnNow;
        _genreColorsOnNow.TryGetValue(genre, out lColorOnNow);
        string colorOnNow = String.Format("{0:X8}", (int)lColorOnNow);

        long lColorOnLater;
        _genreColorsOnLater.TryGetValue(genre, out lColorOnLater);
        string colorOnLater = String.Format("{0:X8}", (int)lColorOnLater);

        ListViewItem item = new ListViewItem(new string[] { genre, colorOnNow, colorOnLater });
        item.Name = genre;
        item.UseItemStyleForSubItems = false;
        item.SubItems[1].BackColor = Color.FromArgb((int)lColorOnNow);
        item.SubItems[2].BackColor = Color.FromArgb((int)lColorOnLater);

        listViewGuideGenres.Items.Add(item);
      }
      listViewGuideGenres.EndUpdate();
    }

    private void CreateDefaultGenreColors(Settings settings)
    {
      // Insert a default collection of TV guide genres.
      for (int i = 0; i < LOCALIZED_GENRE_STRING_COUNT; i++)
      {
        settings.SetValue("tvguidecolors", i.ToString(), _defaultGenreColors[i]);
      }
    }

    public void LoadSettings(string selectedSkin)
    {
      // Load the genre map from MP settings.
      // TODO: this needs to be loaded from tv server settings.
      using (Settings xmlreader = new MPSettings())
      {
        if (_genreList.Count == 0)
        {
          LoadGenreList(xmlreader);
        }
      }

      // Load tv guide colors.
      using (Settings xmlreader = new SKSettings())
      {
        string selectedTheme;
        selectedTheme = xmlreader.GetValueAsString("theme", "name", GUIThemeManager.THEME_SKIN_DEFAULT);

        if (!xmlreader.HasSection<string>("tvguidecolors"))
        {
          CreateDefaultGenreColors(xmlreader);
        }

        if (!_guideColorsLoaded)
        {
          _guideColorsLoaded = LoadGuideColors(xmlreader);
        }

        PopulateThemesList(selectedTheme);
        PopulateGuideGenreList();

        // Need to read skin settings as string and parse to boolean to allow skin settings to have true/false values rather than yes/no values.
        cbColoredGuide.Checked = bool.Parse(xmlreader.GetValueAsString("booleansettings", "#skin.tvguide.usecolorsforbuttons", "False"));
        cbGenreColoring.Checked = bool.Parse(xmlreader.GetValueAsString("booleansettings", "#skin.tvguide.usecolorsforgenre", "False"));
        cbGenreColorKey.Checked = bool.Parse(xmlreader.GetValueAsString("booleansettings", "#skin.tvguide.showgenrekey", "False"));
        cbBorderHighlight.Checked = bool.Parse(xmlreader.GetValueAsString("booleansettings", "#skin.tvguide.useborderhighlight", "False"));

        cbGenreColoring.Enabled = cbColoredGuide.Checked;
        cbGenreColorKey.Enabled = cbGenreColoring.Checked;

        if (cbColoredGuide.Checked)
        {
          if (!tabControlTvGuideSettings.Controls.Contains(tabPageTvGuideColors))
          {
            tabControlTvGuideSettings.Controls.Add(tabPageTvGuideColors);
          }
        }
        else
        {
          tabControlTvGuideSettings.Controls.Remove(tabPageTvGuideColors);
        }
      }
    }

    public void PopulateThemesList(string selectedTheme)
    {
      // Get a list of available themes for the selected skin.
      string selectedSkinFolderPath = Config.GetSubFolder(Config.Dir.Skin, Config.SkinName);
      ArrayList listAvailableThemes = GUIThemeManager.GetSkinThemesForSkin(selectedSkinFolderPath);

      // Transfer the list of themes to the drop down combo box and select the proper value.
      listViewAvailableThemes.Items.Clear();
      for (int i = 0; i < listAvailableThemes.Count; i++)
      {
        // Add the theme to the list of selectable themes.
        string theme = listAvailableThemes[i].ToString();
        ListViewItem item = listViewAvailableThemes.Items.Add(theme);

        if (!GUIThemeManager.THEME_SKIN_DEFAULT.Equals(theme))
        {
          // Theme version is in the theme.xml file.
          string filename = selectedSkinFolderPath + @"\Themes\" + theme + @"\theme.xml";

          XmlDocument doc = new XmlDocument();
          doc.Load(filename);
          XmlNode node = doc.SelectSingleNode("/controls/theme/version");
          if (node != null && node.InnerText != null)
          {
            item.SubItems.Add(node.InnerText);
          }
          else
          {
            item.SubItems.Add("?"); // Version number unknown
          }
        }

        // Select the current theme in the list.
        if (listAvailableThemes[i].Equals(selectedTheme))
        {
          if (!listViewAvailableThemes.Items[i].Selected)
          {
            listViewAvailableThemes.Items[i].Selected = true;
          }
        }
      }

      // If no theme is selected then ensure that the skin default theme is selected.
      if (listViewAvailableThemes.SelectedItems.Count == 0)
      {
        listViewAvailableThemes.Items[0].Selected = true;
      }
    }

    public void SaveSettings()
    {
      using (Settings xmlwriter = new SKSettings())
      {
        xmlwriter.SetValue("theme", "name", listViewAvailableThemes.SelectedItems[0].Text);

        xmlwriter.SetValue("booleansettings", "#skin.tvguide.usecolorsforbuttons", cbColoredGuide.Checked);
        xmlwriter.SetValue("booleansettings", "#skin.tvguide.usecolorsforgenre", cbGenreColoring.Checked);
        xmlwriter.SetValue("booleansettings", "#skin.tvguide.useborderhighlight", cbBorderHighlight.Checked);
        xmlwriter.SetValue("booleansettings", "#skin.tvguide.showgenrekey", cbGenreColorKey.Checked);

        SaveGuideColors(xmlwriter);
      }
    }

    private void mpButtonOnNowColor_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        ColorChooser dlg = new ColorChooser();
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.Color = listViewGuideGenres.SelectedItems[0].SubItems[1].BackColor;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          // Update the color map.
          _genreColorsOnNow[listViewGuideGenres.SelectedItems[0].Text] = dlg.Color.ToArgb();

          // Update the control.
          listViewGuideGenres.SelectedItems[0].SubItems[1].BackColor = dlg.Color;
          listViewGuideGenres.SelectedItems[0].SubItems[1].Text = String.Format("{0:X8}", dlg.Color.ToArgb());
        }
      }
    }

    private void mpButtonOnLaterColor_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        ColorChooser dlg = new ColorChooser();
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.Color = listViewGuideGenres.SelectedItems[0].SubItems[2].BackColor;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          // Update the color map.
          _genreColorsOnLater[listViewGuideGenres.SelectedItems[0].Text] = dlg.Color.ToArgb();

          // Update the control.
          listViewGuideGenres.SelectedItems[0].SubItems[2].BackColor = dlg.Color;
          listViewGuideGenres.SelectedItems[0].SubItems[2].Text = String.Format("{0:X8}", dlg.Color.ToArgb());
        }
      }
    }

    private void colorComboBoxPgmEnded_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmEnded.SelectedColor = Color.FromArgb((int)_guideColorProgramEnded);
    }

    protected void OnPgmEndedColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramEnded = e.color.ToArgb();
    }

    private void colorComboBoxPgmSel_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmSel.SelectedColor = Color.FromArgb((int)_guideColorProgramSelected);
    }

    protected void OnPgmSelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramSelected = e.color.ToArgb();
    }

    private void colorComboBoxPgmBorder_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmBorder.SelectedColor = Color.FromArgb((int)_guideColorBorderHighlight);
    }

    protected void OnPgmBorderColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorBorderHighlight = e.color.ToArgb();
    }

    private void colorComboBoxChannel_Load(object sender, EventArgs e)
    {
      colorComboBoxChannel.SelectedColor = Color.FromArgb((int)_guideColorChannelButton);
    }

    protected void OnChannelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorChannelButton = e.color.ToArgb();
    }

    private void colorComboBoxChannelSel_Load(object sender, EventArgs e)
    {
      colorComboBoxChannelSel.SelectedColor = Color.FromArgb((int)_guideColorChannelButtonSelected);
    }

    protected void OnChannelSelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorChannelButtonSelected = e.color.ToArgb();
    }

    private void colorComboBoxGroup_Load(object sender, EventArgs e)
    {
      colorComboBoxGroup.SelectedColor = Color.FromArgb((int)_guideColorGroupButton);
    }

    protected void OnGroupColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorGroupButton = e.color.ToArgb();
    }

    private void colorComboBoxGroupSel_Load(object sender, EventArgs e)
    {
      colorComboBoxGroupSel.SelectedColor = Color.FromArgb((int)_guideColorGroupButtonSelected);
    }

    protected void OnGroupSelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorGroupButtonSelected = e.color.ToArgb();
    }

    private void colorComboBoxPgmOnNow_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmOnNow.SelectedColor = Color.FromArgb((int)_guideColorProgramOnNow);
    }

    protected void OnPgmOnNowColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramOnNow = e.color.ToArgb();
    }

    private void colorComboBoxPgmOnLater_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmOnLater.SelectedColor = Color.FromArgb((int)_guideColorProgramOnLater);
    }

    protected void OnPgmOnLaterColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramOnLater = e.color.ToArgb();
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      SKSettings.ClearCache();
      this.Close();
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
      SKSettings.SaveCache();
      this.Close();
    }

    private void listViewAvailableThemes_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewAvailableThemes.SelectedItems.Count == 0)
      {
        previewPictureBox.Image = null;
        previewPictureBox.Visible = false;
        return;
      }

      string selectedTheme = listViewAvailableThemes.SelectedItems[0].Text;
      string selectedSkinFolderPath = Config.GetSubFolder(Config.Dir.Skin, Config.SkinName);
      string previewFile = selectedSkinFolderPath + @"\Themes\" + selectedTheme + @"\media\preview.png";

      if (!File.Exists(previewFile))
      {
        previewFile = Path.Combine(selectedSkinFolderPath, @"media\preview.png");
      }

      // Clear image
      previewPictureBox.Image = null;

      Image img = Properties.Resources.mplogo;

      if (File.Exists(previewFile))
      {
        using (Stream s = new FileStream(previewFile, FileMode.Open, FileAccess.Read))
        {
          img = Image.FromStream(s);
        }
      }
      previewPictureBox.Width = img.Width;
      previewPictureBox.Height = img.Height;
      previewPictureBox.Image = img;
      previewPictureBox.Visible = true;
    }

    private void cbGenreColoring_CheckedChanged(object sender, EventArgs e)
    {
      // The color key setting is only selectable if genre coloring is enabled.
      cbGenreColorKey.Enabled = cbGenreColoring.Checked;

      // Clear the color key setting if it's not enabled.
      if (!cbGenreColorKey.Enabled)
      {
        cbGenreColorKey.Checked = false;
      }

      // Enforce border highighting when genres are colored.
      if (cbGenreColoring.Checked)
      {
        cbBorderHighlight.Checked = true;
        cbBorderHighlight.Enabled = false;
      }
      else
      {
        cbBorderHighlight.Enabled = true;
      }
    }

    private void cbColoredGuide_CheckedChanged(object sender, EventArgs e)
    {
      if (cbColoredGuide.Checked)
      {
        cbGenreColoring.Enabled = true;
        if (!tabControlTvGuideSettings.Controls.Contains(tabPageTvGuideColors))
        {
          tabControlTvGuideSettings.Controls.Add(tabPageTvGuideColors);
        }
      }
      else
      {
        tabControlTvGuideSettings.Controls.Remove(tabPageTvGuideColors);
        cbGenreColoring.Checked = false;
        cbGenreColoring.Enabled = false;
        cbGenreColorKey.Checked = false;
        cbGenreColorKey.Enabled = false;
      }
    }
  }
}