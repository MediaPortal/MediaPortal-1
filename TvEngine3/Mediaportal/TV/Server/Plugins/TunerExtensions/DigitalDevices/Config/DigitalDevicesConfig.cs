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
      public CiSlotConfig Config;
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
      : base("Digital Devices CI")
    {
      ServiceAgents.Instance.AddGenericService<IDigitalDevicesConfigService>();
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("Digital Devices config: activating");

      // Get the details for all slots that we've ever seen connected to the system.
      IDigitalDevicesConfigService service = ServiceAgents.Instance.PluginService<IDigitalDevicesConfigService>();
      ICollection<CiSlotConfig> allConfig = service.GetAllSlotConfiguration();
      _ciContexts = new List<CiContext>(allConfig.Count);
      foreach (CiSlotConfig config in allConfig)
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
        MPGroupBox groupBoxCi = new MPGroupBox();
        groupBoxCi.SuspendLayout();
        groupBoxCi.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        groupBoxCi.Location = new Point(6, 3 + (i * (groupHeight + groupPadding)));
        groupBoxCi.Name = "groupBoxCi" + i;
        groupBoxCi.Size = new Size(473, groupHeight);
        groupBoxCi.TabIndex = tabIndexBase + 1;
        groupBoxCi.TabStop = false;
        groupBoxCi.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        groupBoxCi.Text = "CI Slot " + (i + 1) + " - " + context.Config.DeviceName;

        // CAM name.
        MPLabel labelCamName = new MPLabel();
        labelCamName.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        labelCamName.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        labelCamName.Location = new Point(6, 22);
        labelCamName.Name = "labelCamName" + i;
        labelCamName.Size = new Size(440, 20);
        labelCamName.TabIndex = tabIndexBase + 2;
        string title = context.CamMenuTitle;
        if (title == null)
        {
          labelCamName.Text = "CAM Name: (CI not connected)";
        }
        else if (title.Equals(string.Empty))
        {
          labelCamName.Text = "CAM Name: (CAM not present)";
        }
        else
        {
          labelCamName.Text = string.Format("CAM Name: {0}", title);
        }
        groupBoxCi.Controls.Add(labelCamName);

        // Decrypt limit label.
        MPLabel labelDecryptLimit = new MPLabel();
        labelDecryptLimit.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        labelDecryptLimit.Location = new Point(6, 46);
        labelDecryptLimit.Name = "labelDecryptLimit" + i;
        labelDecryptLimit.Size = new Size(71, 13);
        labelDecryptLimit.TabIndex = tabIndexBase + 3;
        labelDecryptLimit.Text = "Decrypt Limit:";
        groupBoxCi.Controls.Add(labelDecryptLimit);

        // Decrypt limit control.
        NumericUpDown numericUpDownDecryptLimit = new NumericUpDown();
        ((ISupportInitialize)numericUpDownDecryptLimit).BeginInit();
        numericUpDownDecryptLimit.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        numericUpDownDecryptLimit.Location = new Point(83, 43);
        numericUpDownDecryptLimit.Minimum = 1;
        numericUpDownDecryptLimit.Name = "numericUpDownDecryptLimit" + i;
        numericUpDownDecryptLimit.Size = new Size(44, 20);
        numericUpDownDecryptLimit.TabIndex = tabIndexBase + 4;
        numericUpDownDecryptLimit.TextAlign = HorizontalAlignment.Center;
        numericUpDownDecryptLimit.Value = context.Config.DecryptLimit;
        ((ISupportInitialize)numericUpDownDecryptLimit).EndInit();
        groupBoxCi.Controls.Add(numericUpDownDecryptLimit);
        context.DecryptLimitControl = numericUpDownDecryptLimit;

        // Provider list label.
        MPLabel labelProviderList = new MPLabel();
        labelProviderList.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        labelProviderList.Location = new Point(150, 46);
        labelProviderList.Name = "labelProviderList" + i;
        labelProviderList.Size = new Size(60, 13);
        labelProviderList.TabIndex = tabIndexBase + 5;
        labelProviderList.Text = "Provider(s):";
        groupBoxCi.Controls.Add(labelProviderList);

        // Provider list control.
        MPTextBox textBoxProviderList = new MPTextBox();
        textBoxProviderList.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        textBoxProviderList.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        textBoxProviderList.Location = new Point(216, 43);
        textBoxProviderList.Name = "textBoxProviderList" + i;
        textBoxProviderList.Size = new Size(240, 20);
        textBoxProviderList.TabIndex = tabIndexBase + 6;
        textBoxProviderList.Text = string.Join(", ", context.Config.Providers);
        groupBoxCi.Controls.Add(textBoxProviderList);
        context.ProviderListControl = textBoxProviderList;

        groupBoxCi.PerformLayout();
        groupBoxCi.ResumeLayout(false);
        Controls.Add(groupBoxCi);
      }

      if (_ciContexts.Count == 0)
      {
        MPLabel labelNoCiSlots = new MPLabel();
        labelNoCiSlots.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        labelNoCiSlots.Location = new Point(6, 5);
        labelNoCiSlots.Name = "labelNoCiSlots";
        labelNoCiSlots.Size = new Size(412, 20);
        labelNoCiSlots.TabIndex = 1;
        labelNoCiSlots.Text = "No Digital Devices CI slots detected.";
        Controls.Add(labelNoCiSlots);
      }

      PerformLayout();
      ResumeLayout(false);
      this.LogDebug("Digital Devices config: updated user interface, slot count = {0}", _ciContexts.Count);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("Digital Devices config: deactivating, slot count = {0}", _ciContexts.Count);

      ICollection<CiSlotConfig> configToSave = new List<CiSlotConfig>();
      foreach (CiContext context in _ciContexts)
      {
        bool isChanged = false;

        if (context.Config.DecryptLimit != context.DecryptLimitControl.Value)
        {
          this.LogInfo("Digital Devices config: decrypt limit for slot {0} changed from {1} to {2}", context.Config.DevicePath, context.Config.DecryptLimit, context.DecryptLimitControl.Value);
          context.Config.DecryptLimit = (int)context.DecryptLimitControl.Value;
          isChanged = true;
        }

        HashSet<string> providers = new HashSet<string>(Regex.Split(context.ProviderListControl.Text.Trim(), @"\s*,\s*"));
        if (context.Config.Providers.SetEquals(providers))
        {
          this.LogInfo("Digital Devices config: providers for slot {0} changed from [{1}] to [{2}]", context.Config.DevicePath, string.Join(", ", context.Config.Providers), string.Join(", ", providers));
          context.Config.Providers = providers;
          isChanged = true;
        }

        if (isChanged)
        {
          context.Debug();
          configToSave.Add(context.Config);
        }
      }

      if (configToSave.Count > 0)
      {
        ServiceAgents.Instance.PluginService<IDigitalDevicesConfigService>().SaveSlotConfiguration(configToSave);
      }

      base.OnSectionDeActivated();
    }
  }
}