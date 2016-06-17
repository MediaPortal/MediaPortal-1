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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Config
{
  public partial class SmarDtvUsbCiConfig : SectionSettings
  {
    private class ProductContext
    {
      public string Name;
      public DriverInstallState InstallState;
      public string LinkedTunerExternalId;
      public MPComboBox TunerSelectionControl;

      public void Debug()
      {
        this.LogDebug("SmarDTV USB CI config: product...");
        this.LogDebug("  name          = {0}", Name);
        this.LogDebug("  install state = {0}", InstallState);
        this.LogDebug("  linked tuner  = {0}", LinkedTunerExternalId ?? "[null]");
      }
    }

    private List<ProductContext> _productContexts = null;

    public SmarDtvUsbCiConfig()
      : base("SmarDTV USB CI")
    {
      ServiceAgents.Instance.AddGenericService<ISmarDtvUsbCiConfigService>();
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("SmarDTV USB CI config: activating");

      ISmarDtvUsbCiConfigService service = ServiceAgents.Instance.PluginService<ISmarDtvUsbCiConfigService>();
      ICollection<string> products = service.GetProductNames();
      _productContexts = new List<ProductContext>(products.Count);
      foreach (string product in products)
      {
        ProductContext context = new ProductContext();
        context.Name = product;
        context.InstallState = service.GetProductInstallState(product);
        context.LinkedTunerExternalId = service.GetLinkedTunerForProduct(product);
        context.Debug();
        _productContexts.Add(context);
      }

      _productContexts.Sort(
        delegate(ProductContext context1, ProductContext context2)
        {
          return context1.Name.CompareTo(context2.Name);
        }
      );

      IList<Tuner> allTuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TVDatabase.Entities.Enums.TunerRelation.None);
      this.LogDebug("SmarDTV USB CI config: assemble compatible tuner list, tuner count = {0}", allTuners.Count);
      ICollection<Tuner> compatibleTuners = new List<Tuner>(allTuners.Count);
      foreach (Tuner tuner in allTuners)
      {
        if ((tuner.SupportedBroadcastStandards & (int)BroadcastStandard.MaskDvb) == 0)
        {
          continue;
        }
        compatibleTuners.Add(tuner);
      }

      SuspendLayout();
      pictureBoxTuner.Visible = false;
      foreach (Control c in Controls)
      {
        if (!c.Name.Equals("pictureBoxTuner"))
        {
          c.Dispose();
        }
      }
      Controls.Clear();

      int groupHeight = 103;
      int groupPadding = 10;
      int componentCount = 5;
      for (int i = 0; i < _productContexts.Count; i++)
      {
        int tabIndexBase = i * componentCount;
        ProductContext context = _productContexts[i];

        // Groupbox wrapper for each CI product.
        MPGroupBox groupBoxCiProduct = new MPGroupBox();
        groupBoxCiProduct.SuspendLayout();
        groupBoxCiProduct.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        groupBoxCiProduct.Location = new Point(6, 6 + (i * (groupHeight + groupPadding)));
        groupBoxCiProduct.Name = "groupBoxCiProduct" + i;
        groupBoxCiProduct.Size = new Size(473, groupHeight);
        groupBoxCiProduct.TabIndex = tabIndexBase + 1;
        groupBoxCiProduct.TabStop = false;
        groupBoxCiProduct.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBoxCiProduct.Text = context.Name;

        // CI product install state label.
        MPLabel labelInstallState = new MPLabel();
        labelInstallState.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelInstallState.Location = new Point(6, 22);
        labelInstallState.Name = "labelInstallState" + i;
        labelInstallState.Size = new Size(412, 20);
        labelInstallState.TabIndex = tabIndexBase + 2;
        if (context.InstallState == DriverInstallState.BdaDriver)
        {
          labelInstallState.Text = string.Format("The {0} is installed correctly.", context.Name);
          labelInstallState.ForeColor = Color.ForestGreen;
        }
        else if (context.InstallState == DriverInstallState.WdmDriver)
        {
          labelInstallState.Text = string.Format("The {0} is installed without the BDA driver.", context.Name);
          labelInstallState.ForeColor = Color.Orange;
        }
        else
        {
          labelInstallState.Text = string.Format("The {0} is not detected.", context.Name);
          labelInstallState.ForeColor = Color.Red;
        }
        groupBoxCiProduct.Controls.Add(labelInstallState);

        // Tuner selection label.
        MPLabel labelTunerSelection = new MPLabel();
        labelTunerSelection.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        labelTunerSelection.Location = new Point(6, 45);
        labelTunerSelection.Name = "labelTunerSelection" + i;
        labelTunerSelection.Size = new System.Drawing.Size(412, 18);
        labelTunerSelection.TabIndex = tabIndexBase + 3;
        labelTunerSelection.Text = "Select a digital tuner to use the CI device with:";
        groupBoxCiProduct.Controls.Add(labelTunerSelection);

        // Tuner icon.
        PictureBox pictureBoxTunerSelection = new PictureBox();
        ((ISupportInitialize)pictureBoxTunerSelection).BeginInit();
        pictureBoxTunerSelection.Image = pictureBoxTuner.Image;
        pictureBoxTunerSelection.Location = new Point(24, 68);
        pictureBoxTunerSelection.Name = "pictureBoxTunerSelection" + i;
        pictureBoxTunerSelection.Size = new Size(33, 23);
        pictureBoxTunerSelection.SizeMode = PictureBoxSizeMode.AutoSize;
        pictureBoxTunerSelection.TabIndex = tabIndexBase + 4;
        pictureBoxTunerSelection.TabStop = false;
        ((ISupportInitialize)pictureBoxTunerSelection).EndInit();
        groupBoxCiProduct.Controls.Add(pictureBoxTunerSelection);

        // Tuner selection.
        MPComboBox comboBoxTunerSelection = new MPComboBox();
        comboBoxTunerSelection.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        comboBoxTunerSelection.DisplayMember = "Name";
        comboBoxTunerSelection.Enabled = context.InstallState == DriverInstallState.BdaDriver;
        comboBoxTunerSelection.FormattingEnabled = true;
        comboBoxTunerSelection.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        comboBoxTunerSelection.Location = new Point(80, 68);
        comboBoxTunerSelection.Name = "comboBoxTunerSelection" + i;
        comboBoxTunerSelection.Size = new Size(368, 20);
        comboBoxTunerSelection.TabIndex = tabIndexBase + 5;
        comboBoxTunerSelection.Items.Clear();
        if (context.InstallState == DriverInstallState.BdaDriver)
        {
          comboBoxTunerSelection.Items.Add(new Tuner() { Name = string.Empty, ExternalId = string.Empty });
          comboBoxTunerSelection.SelectedIndex = 0;
          foreach (Tuner tuner in compatibleTuners)
          {
            comboBoxTunerSelection.Items.Add(tuner);
            if (tuner.ExternalId.Equals(context.LinkedTunerExternalId))
            {
              comboBoxTunerSelection.SelectedItem = tuner;
            }
          }
        }
        context.TunerSelectionControl = comboBoxTunerSelection;
        groupBoxCiProduct.Controls.Add(comboBoxTunerSelection);

        groupBoxCiProduct.PerformLayout();
        groupBoxCiProduct.ResumeLayout(false);
        Controls.Add(groupBoxCiProduct);
      }

      PerformLayout();
      ResumeLayout(false);
      this.LogDebug("SmarDTV USB CI config: updated user interface, product count = {0}", _productContexts.Count);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("SmarDTV USB CI config: deactivating");

      foreach (ProductContext context in _productContexts)
      {
        if (!context.TunerSelectionControl.Enabled)
        {
          continue;
        }
        string selectedTunerExternalId = string.Empty;
        Tuner selectedTuner = context.TunerSelectionControl.SelectedItem as Tuner;
        if (selectedTuner != null)
        {
          selectedTunerExternalId = selectedTuner.ExternalId;
        }
        if (!string.Equals(selectedTunerExternalId, context.LinkedTunerExternalId))
        {
          this.LogInfo("SmarDTV USB CI config: linked tuner for product {0} changed from {1} to {2}", context.Name, context.LinkedTunerExternalId, selectedTunerExternalId);
          ServiceAgents.Instance.PluginService<ISmarDtvUsbCiConfigService>().LinkTunerToProduct(context.Name, selectedTunerExternalId);
          context.LinkedTunerExternalId = selectedTunerExternalId;
        }
      }

      base.OnSectionDeActivated();
    }
  }
}