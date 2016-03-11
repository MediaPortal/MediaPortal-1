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
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;


#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TVDebugOptions : SectionSettings
  {
    private MPGroupBox groupBoxSettings;
    private MPLabel mpWarningLabel;
    public int pluginVersion;


    public TVDebugOptions()
      : this("Debug Options") {}

    public TVDebugOptions(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      SettingsForm.debug_options = true;
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpWarningLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.mpWarningLabel);
      this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSettings.Location = new System.Drawing.Point(0, 0);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(472, 68);
      this.groupBoxSettings.TabIndex = 0;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // mpWarningLabel
      // 
      this.mpWarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpWarningLabel.ForeColor = System.Drawing.Color.Red;
      this.mpWarningLabel.Location = new System.Drawing.Point(6, 16);
      this.mpWarningLabel.Name = "mpWarningLabel";
      this.mpWarningLabel.Size = new System.Drawing.Size(460, 51);
      this.mpWarningLabel.TabIndex = 0;
      this.mpWarningLabel.Text = "This section provides special/debugging settings that are not supported by the Te" +
    "am. Some of these settings are experimental. Do not alter any of the settings be" +
    "low unless you know what you are doing.";
      // 
      // TVDebugOptions
      // 
      this.Controls.Add(this.groupBoxSettings);
      this.Name = "TVDebugOptions";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxSettings.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    public override void LoadSettings()
    {
    }

    public override void SaveSettings()
    {
    }
  }
}