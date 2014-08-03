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
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Config
{
  public partial class SmarDtvUsbCiConfig : SectionSettings
  {
    private class ProductContext
    {
      public string Name;
      public SmarDtvUsbCiDriverInstallState InstallState;
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
      : this("SmarDTV USB CI")
    {
      ServiceAgents.Instance.AddGenericService<ISmarDtvUsbCiConfigService>();
    }

    public SmarDtvUsbCiConfig(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private void UpdateUserInterface()
    {
      this.LogDebug("SmarDTV USB CI config: update user interface");

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

      this.LogDebug("SmarDTV USB CI config: assemble compatible tuner list");
      IList<Card> allTuners = ServiceAgents.Instance.CardServiceAgent.ListAllCards();
      IList<Card> compatibleTuners = new List<Card>(allTuners.Count);
      foreach (Card tuner in allTuners)
      {
        CardType tunerType = ServiceAgents.Instance.ControllerServiceAgent.Type(tuner.IdCard);
        if (tunerType == CardType.Analog || tunerType == CardType.Unknown)
        {
          continue;
        }
        compatibleTuners.Add(tuner);
      }

      SuspendLayout();
      foreach (Control c in Controls)
      {
        c.Dispose();
      }
      Controls.Clear();

      int groupHeight = 103;
      int groupPadding = 10;
      int componentCount = 5;
      ComponentResourceManager resources = new ComponentResourceManager(typeof(SmarDtvUsbCiConfig));
      for (int i = 0; i < _productContexts.Count; i++)
      {
        int tabIndexBase = i * componentCount;
        ProductContext context = _productContexts[i];

        // Groupbox wrapper for each CI product.
        GroupBox gb = new GroupBox();
        gb.SuspendLayout();
        gb.Location = new Point(3, 3 + (i * (groupHeight + groupPadding)));
        gb.Name = "groupBox" + i;
        gb.Size = new Size(446, groupHeight);
        gb.TabIndex = tabIndexBase + 1;
        gb.TabStop = false;
        gb.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        gb.Text = context.Name;

        // CI product install state label.
        MPLabel installStateLabel = new MPLabel();
        installStateLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
        installStateLabel.Location = new Point(6, 22);
        installStateLabel.Name = "installStateLabel" + i;
        installStateLabel.Size = new Size(412, 20);
        installStateLabel.TabIndex = tabIndexBase + 2;
        if (context.InstallState == SmarDtvUsbCiDriverInstallState.BdaDriver)
        {
          installStateLabel.Text = string.Format("The {0} is installed correctly.", context.Name);
          installStateLabel.ForeColor = Color.ForestGreen;
        }
        else if (context.InstallState == SmarDtvUsbCiDriverInstallState.WdmDriver)
        {
          installStateLabel.Text = string.Format("The {0} is installed without the BDA driver.", context.Name);
          installStateLabel.ForeColor = Color.Orange;
        }
        else
        {
          installStateLabel.Text = string.Format("The {0} is not detected.", context.Name);
          installStateLabel.ForeColor = Color.Red;
        }
        gb.Controls.Add(installStateLabel);

        // Tuner selection label.
        MPLabel tunerSelectionLabel = new MPLabel();
        tunerSelectionLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        tunerSelectionLabel.Location = new Point(6, 45);
        tunerSelectionLabel.Name = "tunerSelectionLabel" + i;
        tunerSelectionLabel.Size = new System.Drawing.Size(412, 18);
        tunerSelectionLabel.TabIndex = tabIndexBase + 3;
        tunerSelectionLabel.Text = "Select a digital tuner to use the CI module with:";
        gb.Controls.Add(tunerSelectionLabel);

        // Tuner icon.
        PictureBox tunerSelectionPicture = new PictureBox();
        ((ISupportInitialize)tunerSelectionPicture).BeginInit();
        tunerSelectionPicture.Image = (Image)resources.GetObject("tunerSelectionPicture.Image");
        tunerSelectionPicture.Location = new Point(24, 68);
        tunerSelectionPicture.Name = "tunerSelectionPicture" + i;
        tunerSelectionPicture.Size = new Size(33, 23);
        tunerSelectionPicture.SizeMode = PictureBoxSizeMode.AutoSize;
        tunerSelectionPicture.TabIndex = tabIndexBase + 4;
        tunerSelectionPicture.TabStop = false;
        ((ISupportInitialize)tunerSelectionPicture).EndInit();
        gb.Controls.Add(tunerSelectionPicture);

        // Tuner selection.
        MPComboBox tunerSelectionCombo = new MPComboBox();
        tunerSelectionCombo.DisplayMember = "Name";
        tunerSelectionCombo.Enabled = context.InstallState == SmarDtvUsbCiDriverInstallState.BdaDriver;
        tunerSelectionCombo.FormattingEnabled = true;
        tunerSelectionCombo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        tunerSelectionCombo.Location = new Point(80, 68);
        tunerSelectionCombo.Name = "tunerSelectionCombo" + i;
        tunerSelectionCombo.Size = new Size(340, 20);
        tunerSelectionCombo.TabIndex = tabIndexBase + 5;
        tunerSelectionCombo.Items.Clear();
        if (context.InstallState == SmarDtvUsbCiDriverInstallState.BdaDriver)
        {
          foreach (Card tuner in compatibleTuners)
          {
            tunerSelectionCombo.Items.Add(tuner);
            if (tuner.DevicePath.Equals(context.LinkedTunerExternalId))
            {
              tunerSelectionCombo.SelectedItem = tuner;
            }
          }
        }
        context.TunerSelectionControl = tunerSelectionCombo;
        gb.Controls.Add(tunerSelectionCombo);

        gb.ResumeLayout(false);
        gb.PerformLayout();
        Controls.Add(gb);
      }

      // "Tips" section heading.
      MPLabel tipHeadingLabel = new MPLabel();
      tipHeadingLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
      tipHeadingLabel.ForeColor = Color.Black;
      tipHeadingLabel.Location = new Point(11, _productContexts.Count * (groupHeight + groupPadding));
      tipHeadingLabel.Name = "tipHeadingLabel";
      tipHeadingLabel.Size = new Size(412, 16);
      tipHeadingLabel.TabIndex = (_productContexts.Count * componentCount) + 1;
      tipHeadingLabel.Text = "Tips:";
      Controls.Add(tipHeadingLabel);

      // Tips.
      MPLabel tipsLabel = new MPLabel();
      tipsLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
      tipsLabel.ForeColor = Color.Black;
      tipsLabel.Location = new Point(11, (_productContexts.Count * (groupHeight + groupPadding)) + 20);
      tipsLabel.Name = "tipsLabel";
      tipsLabel.Size = new Size(438, 105);
      tipsLabel.TabIndex = (_productContexts.Count * componentCount) + 2;
      tipsLabel.Text = resources.GetString("tipsLabel.Text");
      Controls.Add(tipsLabel);

      ResumeLayout(false);
      this.LogDebug("SmarDTV USB CI config: updated user interface, product count = {0}", _productContexts.Count);
    }

    public override void SaveSettings()
    {
      this.LogDebug("SmarDTV USB CI config: saving settings");
      foreach (ProductContext context in _productContexts)
      {
        Card selectedTuner = (Card)context.TunerSelectionControl.SelectedItem;
        if (context.TunerSelectionControl.Enabled && selectedTuner != null)
        {
          context.LinkedTunerExternalId = selectedTuner.DevicePath;
        }
        else
        {
          context.LinkedTunerExternalId = string.Empty;
        }
        context.Debug();
        ServiceAgents.Instance.PluginService<ISmarDtvUsbCiConfigService>().LinkTunerToProduct(context.Name, context.LinkedTunerExternalId);
      }

      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("SmarDTV USB CI config: activated");
      UpdateUserInterface();
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("SmarDTV USB CI config: deactivated");
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override bool CanActivate
    {
      get
      {
        // The section can always be activated (disabling it might be confusing for people), but we don't
        // necessarily enable all of the tuner selection fields.
        return true;
      }
    }
  }
}
