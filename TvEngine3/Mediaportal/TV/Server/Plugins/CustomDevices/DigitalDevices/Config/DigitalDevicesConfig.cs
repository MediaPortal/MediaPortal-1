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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.Config
{
  internal partial class DigitalDevicesConfig : SectionSettings
  {
    private class CiContext
    {
      public string CamMenuTitle;
      public DigitalDevicesCiSlotConfig Config;
      public NumericUpDown DecryptLimitControl;
      public MPTextBox ProviderListControl;

      public void Debug()
      {
        this.LogDebug("Digital Devices config: slot...");
        this.LogDebug("  name           = {0}", Config.DeviceName);
        this.LogDebug("  device path    = {0}", Config.DevicePath);
        this.LogDebug("  CAM name/title = {0}", CamMenuTitle ?? "[null]");
        this.LogDebug("  decrypt limit  = {0}", Config.DecryptLimit);
        this.LogDebug("  provider list  = {0}", string.Join(", ", Config.Providers));
      }
    }

    private List<CiContext> _ciContexts = null;

    public DigitalDevicesConfig()
      : this("Digital Devices CI")
    {
      ServiceAgents.Instance.AddGenericService<IDigitalDevicesConfigService>();
    }

    public DigitalDevicesConfig(string name)
      : base("Digital Devices CI")
    {
      InitializeComponent();
    }

    private void UpdateUserInterface()
    {
      this.LogDebug("Digital Devices config: update user interface");

      // Get the details for all slots that we've ever seen connected to the system.
      IDigitalDevicesConfigService service = ServiceAgents.Instance.PluginService<IDigitalDevicesConfigService>();
      ICollection<DigitalDevicesCiSlotConfig> settings = service.GetAllSlotConfiguration();
      _ciContexts = new List<CiContext>(settings.Count);
      foreach (DigitalDevicesCiSlotConfig config in settings)
      {
        CiContext context = new CiContext();
        context.Config = config;
        context.CamMenuTitle = service.GetCamNameForSlot(config.DevicePath);
        context.Debug();
        _ciContexts.Add(context);
      }

      _ciContexts.Sort(
        delegate(CiContext context1, CiContext context2)
        {
          return context1.Config.DeviceName.CompareTo(context2.Config.DeviceName);
        }
      );

      SuspendLayout();
      foreach (Control c in Controls)
      {
        c.Dispose();
      }
      Controls.Clear();

      int groupHeight = 73;
      int groupPadding = 10;
      int componentCount = 6;
      for (int i = 0; i < _ciContexts.Count; i++)
      {
        int tabIndexBase = i * componentCount;
        CiContext context = _ciContexts[i];

        // Groupbox wrapper for each CI slot.
        MPGroupBox gb = new MPGroupBox();
        gb.SuspendLayout();
        gb.Location = new Point(3, 3 + (i * (groupHeight + groupPadding)));
        gb.Name = "groupBox" + i;
        gb.Size = new Size(446, groupHeight);
        gb.TabIndex = tabIndexBase + 1;
        gb.TabStop = false;
        gb.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        gb.Text = "CI Slot " + (i + 1) + " - " + context.Config.DeviceName;

        // CAM name.
        MPLabel camNameLabel = new MPLabel();
        camNameLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        camNameLabel.Location = new Point(6, 22);
        camNameLabel.Name = "camName" + i;
        camNameLabel.Size = new Size(412, 20);
        camNameLabel.TabIndex = tabIndexBase + 2;
        string title = context.CamMenuTitle;
        if (title == null)
        {
          camNameLabel.Text = "CAM Name: (CI not connected)";
        }
        else if (title.Equals(string.Empty))
        {
          camNameLabel.Text = "CAM Name: (CAM not present)";
        }
        else
        {
          camNameLabel.Text = string.Format("CAM Name: {0}", title);
        }
        gb.Controls.Add(camNameLabel);

        // Decrypt limit label.
        MPLabel decryptLimitLabel = new MPLabel();
        decryptLimitLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        decryptLimitLabel.Location = new Point(6, 46);
        decryptLimitLabel.Name = "decryptLimitLabel" + i;
        decryptLimitLabel.Size = new Size(71, 13);
        decryptLimitLabel.TabIndex = tabIndexBase + 3;
        decryptLimitLabel.Text = "Decrypt Limit:";
        gb.Controls.Add(decryptLimitLabel);

        // Decrypt limit control.
        NumericUpDown decryptLimitControl = new NumericUpDown();
        ((ISupportInitialize)decryptLimitControl).BeginInit();
        decryptLimitControl.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        decryptLimitControl.Location = new Point(83, 43);
        decryptLimitControl.Name = "decryptLimitControl" + i;
        decryptLimitControl.Size = new Size(44, 20);
        decryptLimitControl.TabIndex = tabIndexBase + 4;
        decryptLimitControl.TextAlign = HorizontalAlignment.Center;
        decryptLimitControl.Value = context.Config.DecryptLimit;
        ((ISupportInitialize)decryptLimitControl).EndInit();
        gb.Controls.Add(decryptLimitControl);
        context.DecryptLimitControl = decryptLimitControl;

        // Provider list label.
        MPLabel providerLabel = new MPLabel();
        providerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        providerLabel.Location = new Point(150, 46);
        providerLabel.Name = "providerListLabel" + i;
        providerLabel.Size = new Size(60, 13);
        providerLabel.TabIndex = tabIndexBase + 5;
        providerLabel.Text = "Provider(s):";
        gb.Controls.Add(providerLabel);

        // Provider list control.
        MPTextBox providerListControl = new MPTextBox();
        providerListControl.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        providerListControl.Location = new Point(216, 43);
        providerListControl.Name = "providerListControl" + i;
        providerListControl.Size = new Size(212, 20);
        providerListControl.TabIndex = tabIndexBase + 6;
        providerListControl.Text = string.Join(", ", context.Config.Providers);
        gb.Controls.Add(providerListControl);
        context.ProviderListControl = providerListControl;

        gb.ResumeLayout(false);
        gb.PerformLayout();
        Controls.Add(gb);
      }

      if (_ciContexts.Count == 0)
      {
        MPLabel noSlotsLabel = new MPLabel();
        noSlotsLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        noSlotsLabel.Location = new Point(6, 5);
        noSlotsLabel.Name = "noSlotsLabel";
        noSlotsLabel.Size = new Size(412, 20);
        noSlotsLabel.TabIndex = 1;
        noSlotsLabel.Text = "No Digital Devices CI slots detected.";
        Controls.Add(noSlotsLabel);
      }

      ResumeLayout(false);
      this.LogDebug("Digital Devices config: updated user interface, slot count = {0}", _ciContexts.Count);
    }

    public override void SaveSettings()
    {
      this.LogDebug("Digital Devices config: saving settings, slot count = {0}", _ciContexts.Count);

      if (_ciContexts.Count > 0)
      {
        ICollection<DigitalDevicesCiSlotConfig> settings = new List<DigitalDevicesCiSlotConfig>();
        foreach (CiContext context in _ciContexts)
        {
          context.Config.DecryptLimit = (int)context.DecryptLimitControl.Value;
          context.Config.Providers = new HashSet<string>(Regex.Split(context.ProviderListControl.Text.Trim(), @"\s*,\s*"));

          context.Debug();
          settings.Add(context.Config);
        }

        ServiceAgents.Instance.PluginService<IDigitalDevicesConfigService>().SaveAllSlotConfiguration(settings);
      }

      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("Digital Devices config: activated");
      UpdateUserInterface();
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("Digital Devices config: deactivated");
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override bool CanActivate
    {
      get
      {
        // The section can always be activated (disabling it might be confusing for people). If there
        // are no CI slots detected then we show a message in a label to say this.
        return true;
      }
    }
  }
}