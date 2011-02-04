#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GuiHomeScreen : SectionSettings
  {
    // IMPORTANT: the enumeration depends on the correct order of items in homeComboBox.
    // The order is chosen to allow compositing SelectedIndex from bitmapped flags.
    [Flags]
    private enum HomeUsageEnum
    {
      PreferClassic = 0,
      PreferBasic = 1,
      UseBoth = 0,
      UseOnlyOne = 2,

      //UseBothPreferClassic = UseBoth | PreferClassic,
      //UseBothPreferBasic = UseBoth | PreferBasic,
      //UseOnlyClassic = UseOnlyOne | PreferClassic,
      //UseOnlyBasic = UseOnlyOne | PreferBasic,
    }

    private MPGroupBox mpGroupBoxHomeScreenSettings;
    private MPComboBox homeComboBox;
    private MPLabel mpLabel1;
    private IContainer components = null;

    public GuiHomeScreen()
      : this("Home Screen") {}

    public GuiHomeScreen(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
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


    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        bool startWithBasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", false);
        bool useOnlyOneHome = xmlreader.GetValueAsBool("gui", "useonlyonehome", false);
        //homeComboBox.SelectedIndex = useOnlyOneHome ? (startWithBasicHome ? 3 : 2) : (startWithBasicHome ? 1 : 0);
        homeComboBox.SelectedIndex = (int)((useOnlyOneHome ? HomeUsageEnum.UseOnlyOne : HomeUsageEnum.UseBoth) |
                                           (startWithBasicHome ? HomeUsageEnum.PreferBasic : HomeUsageEnum.PreferClassic));
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("gui", "useonlyonehome", (homeComboBox.SelectedIndex & (int)HomeUsageEnum.UseOnlyOne) != 0);
        xmlwriter.SetValueAsBool("gui", "startbasichome", (homeComboBox.SelectedIndex & (int)HomeUsageEnum.PreferBasic) != 0);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBoxHomeScreenSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.homeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBoxHomeScreenSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBoxHomeScreenSettings
      // 
      this.mpGroupBoxHomeScreenSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBoxHomeScreenSettings.Controls.Add(this.homeComboBox);
      this.mpGroupBoxHomeScreenSettings.Controls.Add(this.mpLabel1);
      this.mpGroupBoxHomeScreenSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxHomeScreenSettings.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBoxHomeScreenSettings.Name = "mpGroupBoxHomeScreenSettings";
      this.mpGroupBoxHomeScreenSettings.Size = new System.Drawing.Size(460, 50);
      this.mpGroupBoxHomeScreenSettings.TabIndex = 7;
      this.mpGroupBoxHomeScreenSettings.TabStop = false;
      this.mpGroupBoxHomeScreenSettings.Text = "Home Screen Options";
      // 
      // homeComboBox
      // 
      this.homeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.homeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.homeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.homeComboBox.Items.AddRange(new object[] {
            "Classic and Basic, prefer Classic",
            "Classic and Basic, prefer Basic",
            "only Classic Home",
            "only Basic Home"});
      this.homeComboBox.Location = new System.Drawing.Point(118, 21);
      this.homeComboBox.Name = "homeComboBox";
      this.homeComboBox.Size = new System.Drawing.Size(325, 21);
      this.homeComboBox.TabIndex = 11;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 24);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(96, 16);
      this.mpLabel1.TabIndex = 10;
      this.mpLabel1.Text = "Home Screen:";
      // 
      // GuiHomeScreen
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.mpGroupBoxHomeScreenSettings);
      this.Name = "GuiHomeScreen";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBoxHomeScreenSettings.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

  }
}