#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.ComponentModel;
using System.Text;
using MediaPortal.UserInterface.Controls;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal
{
  /// <summary>
  /// A form to allow the user to change the current D3D settings.
  /// </summary>
  public class D3DSettingsForm : MPForm
  {
    private D3DEnumeration enumeration;
    public D3DSettings settings; // Potential new D3D settings

    private MPGroupBox adapterDeviceGroupBox;
    private MPLabel displayAdapterLabel;
    private MPComboBox adapterComboBox;
    private MPLabel deviceLabel;
    private MPComboBox deviceComboBox;
    private MPGroupBox modeSettingsGroupBox;
    private MPRadioButton windowedRadioButton;
    private MPRadioButton fullscreenRadioButton;
    private MPLabel adapterFormatLabel;
    private MPComboBox adapterFormatComboBox;
    private MPLabel resolutionLabel;
    private MPComboBox resolutionComboBox;
    private MPLabel refreshRateLabel;
    private MPComboBox refreshRateComboBox;
    private MPGroupBox otherSettingsGroupBox;
    private MPLabel backBufferFormatLabel;
    private MPComboBox backBufferFormatComboBox;
    private MPLabel depthStencilBufferLabel;
    private MPComboBox depthStencilBufferComboBox;
    private MPLabel multisampleLabel;
    private MPComboBox multisampleComboBox;
    private MPLabel vertexProcLabel;
    private MPComboBox vertexProcComboBox;
    private MPLabel presentIntervalLabel;
    private MPComboBox presentIntervalComboBox;
    private MPButton okButton;
    private MPButton cancelButton;
    private MPComboBox multisampleQualityComboBox;
    private MPLabel multisampleQualityLabel;


    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;


    /// <summary>
    /// Constructor.  Pass in an enumeration and the current D3D settings.
    /// </summary>
    public D3DSettingsForm(D3DEnumeration enumerationParam, D3DSettings settingsParam)
    {
      enumeration = enumerationParam;
      settings = settingsParam.Clone();

      // Required for Windows Form Designer support
      InitializeComponent();

      // Fill adapter combo box.  Updating the selected adapter will trigger
      // updates of the rest of the dialog.
      foreach (GraphicsAdapterInfo adapterInfo in enumeration.AdapterInfoList)
      {
        adapterComboBox.Items.Add(adapterInfo);
        if (adapterInfo.AdapterOrdinal == settings.AdapterOrdinal)
        {
          adapterComboBox.SelectedItem = adapterInfo;
        }
      }
      if (adapterComboBox.SelectedItem == null && adapterComboBox.Items.Count > 0)
      {
        adapterComboBox.SelectedIndex = 0;
      }
    }


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool Disposing)
    {
      base.Dispose(Disposing);
      if (components != null)
      {
        components.Dispose();
      }
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.adapterDeviceGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.deviceComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.deviceLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.adapterComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.displayAdapterLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.fullscreenRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.otherSettingsGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.multisampleQualityComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.multisampleQualityLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.multisampleComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.backBufferFormatComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.multisampleLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.depthStencilBufferLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.backBufferFormatLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.depthStencilBufferComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.vertexProcComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.vertexProcLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.presentIntervalComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.presentIntervalLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.resolutionComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.windowedRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.resolutionLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.refreshRateComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.adapterFormatLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.refreshRateLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.modeSettingsGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.adapterFormatComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.adapterDeviceGroupBox.SuspendLayout();
      this.otherSettingsGroupBox.SuspendLayout();
      this.modeSettingsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // adapterDeviceGroupBox
      // 
      this.adapterDeviceGroupBox.Controls.AddRange(new System.Windows.Forms.Control[]
                                                     {
                                                       this.deviceComboBox,
                                                       this.deviceLabel,
                                                       this.adapterComboBox,
                                                       this.displayAdapterLabel
                                                     });
      this.adapterDeviceGroupBox.Location = new System.Drawing.Point(16, 8);
      this.adapterDeviceGroupBox.Name = "adapterDeviceGroupBox";
      this.adapterDeviceGroupBox.Size = new System.Drawing.Size(400, 80);
      this.adapterDeviceGroupBox.TabIndex = 0;
      this.adapterDeviceGroupBox.TabStop = false;
      this.adapterDeviceGroupBox.Text = "Adapter and device";
      // 
      // deviceComboBox
      // 
      this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.deviceComboBox.DropDownWidth = 121;
      this.deviceComboBox.Location = new System.Drawing.Point(160, 48);
      this.deviceComboBox.Name = "deviceComboBox";
      this.deviceComboBox.Size = new System.Drawing.Size(232, 21);
      this.deviceComboBox.TabIndex = 3;
      this.deviceComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceChanged);
      // 
      // deviceLabel
      // 
      this.deviceLabel.Location = new System.Drawing.Point(8, 48);
      this.deviceLabel.Name = "deviceLabel";
      this.deviceLabel.Size = new System.Drawing.Size(152, 23);
      this.deviceLabel.TabIndex = 2;
      this.deviceLabel.Text = "Render &Device:";
      // 
      // adapterComboBox
      // 
      this.adapterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.adapterComboBox.DropDownWidth = 121;
      this.adapterComboBox.Location = new System.Drawing.Point(160, 24);
      this.adapterComboBox.Name = "adapterComboBox";
      this.adapterComboBox.Size = new System.Drawing.Size(232, 21);
      this.adapterComboBox.TabIndex = 1;
      this.adapterComboBox.SelectedIndexChanged += new System.EventHandler(this.AdapterChanged);
      // 
      // displayAdapterLabel
      // 
      this.displayAdapterLabel.Location = new System.Drawing.Point(8, 24);
      this.displayAdapterLabel.Name = "displayAdapterLabel";
      this.displayAdapterLabel.Size = new System.Drawing.Size(152, 23);
      this.displayAdapterLabel.TabIndex = 0;
      this.displayAdapterLabel.Text = "Display &Adapter:";
      // 
      // fullscreenRadioButton
      // 
      this.fullscreenRadioButton.Location = new System.Drawing.Point(9, 38);
      this.fullscreenRadioButton.Name = "fullscreenRadioButton";
      this.fullscreenRadioButton.Size = new System.Drawing.Size(152, 24);
      this.fullscreenRadioButton.TabIndex = 1;
      this.fullscreenRadioButton.Text = "&Fullscreen";
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(248, 464);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "Cancel";
      // 
      // otherSettingsGroupBox
      // 
      this.otherSettingsGroupBox.Controls.AddRange(new System.Windows.Forms.Control[]
                                                     {
                                                       this.multisampleQualityComboBox,
                                                       this.multisampleQualityLabel,
                                                       this.multisampleComboBox,
                                                       this.backBufferFormatComboBox,
                                                       this.multisampleLabel,
                                                       this.depthStencilBufferLabel,
                                                       this.backBufferFormatLabel,
                                                       this.depthStencilBufferComboBox,
                                                       this.vertexProcComboBox,
                                                       this.vertexProcLabel,
                                                       this.presentIntervalComboBox,
                                                       this.presentIntervalLabel
                                                     });
      this.otherSettingsGroupBox.Location = new System.Drawing.Point(16, 264);
      this.otherSettingsGroupBox.Name = "otherSettingsGroupBox";
      this.otherSettingsGroupBox.Size = new System.Drawing.Size(400, 176);
      this.otherSettingsGroupBox.TabIndex = 2;
      this.otherSettingsGroupBox.TabStop = false;
      this.otherSettingsGroupBox.Text = "Device settings";
      // 
      // multisampleQualityComboBox
      // 
      this.multisampleQualityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.multisampleQualityComboBox.DropDownWidth = 144;
      this.multisampleQualityComboBox.Location = new System.Drawing.Point(160, 96);
      this.multisampleQualityComboBox.Name = "multisampleQualityComboBox";
      this.multisampleQualityComboBox.Size = new System.Drawing.Size(232, 21);
      this.multisampleQualityComboBox.TabIndex = 7;
      this.multisampleQualityComboBox.SelectedIndexChanged += new System.EventHandler(this.MultisampleQualityChanged);
      // 
      // multisampleQualityLabel
      // 
      this.multisampleQualityLabel.Location = new System.Drawing.Point(8, 96);
      this.multisampleQualityLabel.Name = "multisampleQualityLabel";
      this.multisampleQualityLabel.Size = new System.Drawing.Size(152, 23);
      this.multisampleQualityLabel.TabIndex = 6;
      this.multisampleQualityLabel.Text = "Multisample &Quality:";
      // 
      // multisampleComboBox
      // 
      this.multisampleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.multisampleComboBox.DropDownWidth = 144;
      this.multisampleComboBox.Location = new System.Drawing.Point(160, 72);
      this.multisampleComboBox.Name = "multisampleComboBox";
      this.multisampleComboBox.Size = new System.Drawing.Size(232, 21);
      this.multisampleComboBox.TabIndex = 5;
      this.multisampleComboBox.SelectedIndexChanged += new System.EventHandler(this.MultisampleTypeChanged);
      // 
      // backBufferFormatComboBox
      // 
      this.backBufferFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.backBufferFormatComboBox.DropDownWidth = 144;
      this.backBufferFormatComboBox.Location = new System.Drawing.Point(160, 24);
      this.backBufferFormatComboBox.Name = "backBufferFormatComboBox";
      this.backBufferFormatComboBox.Size = new System.Drawing.Size(232, 21);
      this.backBufferFormatComboBox.TabIndex = 1;
      this.backBufferFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.BackBufferFormatChanged);
      // 
      // multisampleLabel
      // 
      this.multisampleLabel.Location = new System.Drawing.Point(8, 72);
      this.multisampleLabel.Name = "multisampleLabel";
      this.multisampleLabel.Size = new System.Drawing.Size(152, 23);
      this.multisampleLabel.TabIndex = 4;
      this.multisampleLabel.Text = "&Multisample Type:";
      // 
      // depthStencilBufferLabel
      // 
      this.depthStencilBufferLabel.Location = new System.Drawing.Point(8, 48);
      this.depthStencilBufferLabel.Name = "depthStencilBufferLabel";
      this.depthStencilBufferLabel.Size = new System.Drawing.Size(152, 23);
      this.depthStencilBufferLabel.TabIndex = 2;
      this.depthStencilBufferLabel.Text = "De&pth/Stencil Buffer Format:";
      // 
      // backBufferFormatLabel
      // 
      this.backBufferFormatLabel.Location = new System.Drawing.Point(8, 24);
      this.backBufferFormatLabel.Name = "backBufferFormatLabel";
      this.backBufferFormatLabel.Size = new System.Drawing.Size(152, 23);
      this.backBufferFormatLabel.TabIndex = 0;
      this.backBufferFormatLabel.Text = "&Back Buffer Format:";
      // 
      // depthStencilBufferComboBox
      // 
      this.depthStencilBufferComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.depthStencilBufferComboBox.DropDownWidth = 144;
      this.depthStencilBufferComboBox.Location = new System.Drawing.Point(160, 48);
      this.depthStencilBufferComboBox.Name = "depthStencilBufferComboBox";
      this.depthStencilBufferComboBox.Size = new System.Drawing.Size(232, 21);
      this.depthStencilBufferComboBox.TabIndex = 3;
      this.depthStencilBufferComboBox.SelectedIndexChanged +=
        new System.EventHandler(this.DepthStencilBufferFormatChanged);
      // 
      // vertexProcComboBox
      // 
      this.vertexProcComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.vertexProcComboBox.DropDownWidth = 121;
      this.vertexProcComboBox.Location = new System.Drawing.Point(160, 120);
      this.vertexProcComboBox.Name = "vertexProcComboBox";
      this.vertexProcComboBox.Size = new System.Drawing.Size(232, 21);
      this.vertexProcComboBox.TabIndex = 9;
      this.vertexProcComboBox.SelectedIndexChanged += new System.EventHandler(this.VertexProcessingChanged);
      // 
      // vertexProcLabel
      // 
      this.vertexProcLabel.Location = new System.Drawing.Point(8, 120);
      this.vertexProcLabel.Name = "vertexProcLabel";
      this.vertexProcLabel.Size = new System.Drawing.Size(152, 23);
      this.vertexProcLabel.TabIndex = 8;
      this.vertexProcLabel.Text = "&Vertex Processing:";
      // 
      // presentIntervalComboBox
      // 
      this.presentIntervalComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.presentIntervalComboBox.DropDownWidth = 121;
      this.presentIntervalComboBox.Location = new System.Drawing.Point(160, 144);
      this.presentIntervalComboBox.Name = "presentIntervalComboBox";
      this.presentIntervalComboBox.Size = new System.Drawing.Size(232, 21);
      this.presentIntervalComboBox.TabIndex = 11;
      this.presentIntervalComboBox.SelectedValueChanged += new System.EventHandler(this.PresentIntervalChanged);
      // 
      // presentIntervalLabel
      // 
      this.presentIntervalLabel.Location = new System.Drawing.Point(8, 144);
      this.presentIntervalLabel.Name = "presentIntervalLabel";
      this.presentIntervalLabel.Size = new System.Drawing.Size(152, 23);
      this.presentIntervalLabel.TabIndex = 10;
      this.presentIntervalLabel.Text = "Present &Interval:";
      // 
      // resolutionComboBox
      // 
      this.resolutionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.resolutionComboBox.DropDownWidth = 144;
      this.resolutionComboBox.Location = new System.Drawing.Point(161, 94);
      this.resolutionComboBox.MaxDropDownItems = 14;
      this.resolutionComboBox.Name = "resolutionComboBox";
      this.resolutionComboBox.Size = new System.Drawing.Size(232, 21);
      this.resolutionComboBox.TabIndex = 5;
      this.resolutionComboBox.SelectedIndexChanged += new System.EventHandler(this.ResolutionChanged);
      // 
      // windowedRadioButton
      // 
      this.windowedRadioButton.Location = new System.Drawing.Point(9, 14);
      this.windowedRadioButton.Name = "windowedRadioButton";
      this.windowedRadioButton.Size = new System.Drawing.Size(152, 24);
      this.windowedRadioButton.TabIndex = 0;
      this.windowedRadioButton.Text = "&Windowed";
      this.windowedRadioButton.CheckedChanged += new System.EventHandler(this.WindowedFullscreenChanged);
      // 
      // resolutionLabel
      // 
      this.resolutionLabel.Location = new System.Drawing.Point(8, 94);
      this.resolutionLabel.Name = "resolutionLabel";
      this.resolutionLabel.Size = new System.Drawing.Size(152, 23);
      this.resolutionLabel.TabIndex = 4;
      this.resolutionLabel.Text = "&Resolution:";
      // 
      // refreshRateComboBox
      // 
      this.refreshRateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.refreshRateComboBox.DropDownWidth = 144;
      this.refreshRateComboBox.Location = new System.Drawing.Point(161, 118);
      this.refreshRateComboBox.MaxDropDownItems = 14;
      this.refreshRateComboBox.Name = "refreshRateComboBox";
      this.refreshRateComboBox.Size = new System.Drawing.Size(232, 21);
      this.refreshRateComboBox.TabIndex = 7;
      this.refreshRateComboBox.SelectedIndexChanged += new System.EventHandler(this.RefreshRateChanged);
      // 
      // adapterFormatLabel
      // 
      this.adapterFormatLabel.Location = new System.Drawing.Point(8, 72);
      this.adapterFormatLabel.Name = "adapterFormatLabel";
      this.adapterFormatLabel.Size = new System.Drawing.Size(152, 23);
      this.adapterFormatLabel.TabIndex = 2;
      this.adapterFormatLabel.Text = "Adapter F&ormat:";
      // 
      // refreshRateLabel
      // 
      this.refreshRateLabel.Location = new System.Drawing.Point(8, 118);
      this.refreshRateLabel.Name = "refreshRateLabel";
      this.refreshRateLabel.Size = new System.Drawing.Size(152, 23);
      this.refreshRateLabel.TabIndex = 6;
      this.refreshRateLabel.Text = "R&efresh Rate:";
      // 
      // okButton
      // 
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(112, 464);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      // 
      // modeSettingsGroupBox
      // 
      this.modeSettingsGroupBox.Controls.AddRange(new System.Windows.Forms.Control[]
                                                    {
                                                      this.adapterFormatLabel,
                                                      this.refreshRateLabel,
                                                      this.resolutionComboBox,
                                                      this.adapterFormatComboBox,
                                                      this.resolutionLabel,
                                                      this.refreshRateComboBox,
                                                      this.windowedRadioButton,
                                                      this.fullscreenRadioButton
                                                    });
      this.modeSettingsGroupBox.Location = new System.Drawing.Point(16, 96);
      this.modeSettingsGroupBox.Name = "modeSettingsGroupBox";
      this.modeSettingsGroupBox.Size = new System.Drawing.Size(400, 160);
      this.modeSettingsGroupBox.TabIndex = 1;
      this.modeSettingsGroupBox.TabStop = false;
      this.modeSettingsGroupBox.Text = "Display mode settings";
      // 
      // adapterFormatComboBox
      // 
      this.adapterFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.adapterFormatComboBox.DropDownWidth = 121;
      this.adapterFormatComboBox.Location = new System.Drawing.Point(161, 70);
      this.adapterFormatComboBox.MaxDropDownItems = 14;
      this.adapterFormatComboBox.Name = "adapterFormatComboBox";
      this.adapterFormatComboBox.Size = new System.Drawing.Size(232, 21);
      this.adapterFormatComboBox.TabIndex = 3;
      this.adapterFormatComboBox.SelectedValueChanged += new System.EventHandler(this.AdapterFormatChanged);
      // 
      // D3DSettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(438, 512);
      this.Controls.AddRange(new System.Windows.Forms.Control[]
                               {
                                 this.cancelButton,
                                 this.okButton,
                                 this.adapterDeviceGroupBox,
                                 this.modeSettingsGroupBox,
                                 this.otherSettingsGroupBox
                               });
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.Name = "D3DSettingsForm";
      this.Text = "Direct3D Settings";
      this.adapterDeviceGroupBox.ResumeLayout(false);
      this.otherSettingsGroupBox.ResumeLayout(false);
      this.modeSettingsGroupBox.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// Respond to a change of selected adapter by rebuilding the device 
    /// list.  Updating the selected device will trigger updates of the 
    /// rest of the dialog.
    /// </summary>
    private void AdapterChanged(object sender, EventArgs e)
    {
      GraphicsAdapterInfo adapterInfo = (GraphicsAdapterInfo) adapterComboBox.SelectedItem;
      settings.AdapterInfo = adapterInfo;

      // Update device combo box
      deviceComboBox.Items.Clear();
      foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfoList)
      {
        deviceComboBox.Items.Add(deviceInfo);
        if (deviceInfo.DevType == settings.DevType)
        {
          deviceComboBox.SelectedItem = deviceInfo;
        }
      }
      if (deviceComboBox.SelectedItem == null && deviceComboBox.Items.Count > 0)
      {
        deviceComboBox.SelectedIndex = 0;
      }
    }


    /// <summary>
    /// Respond to a change of selected device by resetting the 
    /// fullscreen/windowed radio buttons.  Updating these buttons
    /// will trigger updates of the rest of the dialog.
    /// </summary>
    private void DeviceChanged(object sender, EventArgs e)
    {
      GraphicsAdapterInfo adapterInfo = (GraphicsAdapterInfo) adapterComboBox.SelectedItem;
      GraphicsDeviceInfo deviceInfo = (GraphicsDeviceInfo) deviceComboBox.SelectedItem;

      settings.DeviceInfo = deviceInfo;

      // Update fullscreen/windowed radio buttons
      bool HasWindowedDeviceCombo = false;
      bool HasFullscreenDeviceCombo = false;
      foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
      {
        if (deviceCombo.IsWindowed)
        {
          HasWindowedDeviceCombo = true;
        }
        else
        {
          HasFullscreenDeviceCombo = true;
        }
      }
      windowedRadioButton.Enabled = HasWindowedDeviceCombo;
      fullscreenRadioButton.Enabled = HasFullscreenDeviceCombo;
      if (settings.IsWindowed && HasWindowedDeviceCombo)
      {
        windowedRadioButton.Checked = true;
      }
      else
      {
        fullscreenRadioButton.Checked = true;
      }
      WindowedFullscreenChanged(null, null);
    }


    /// <summary>
    /// Respond to a change of windowed/fullscreen state by rebuilding the
    /// adapter format list, resolution list, and refresh rate list.
    /// Updating the selected adapter format will trigger updates of the 
    /// rest of the dialog.
    /// </summary>
    private void WindowedFullscreenChanged(object sender, EventArgs e)
    {
      GraphicsAdapterInfo adapterInfo = (GraphicsAdapterInfo) adapterComboBox.SelectedItem;
      GraphicsDeviceInfo deviceInfo = (GraphicsDeviceInfo) deviceComboBox.SelectedItem;

      if (windowedRadioButton.Checked)
      {
        settings.IsWindowed = true;
        settings.WindowedAdapterInfo = adapterInfo;
        settings.WindowedDeviceInfo = deviceInfo;

        // Update adapter format combo box
        adapterFormatComboBox.Items.Clear();
        adapterFormatComboBox.Items.Add(settings.WindowedDisplayMode.Format);
        adapterFormatComboBox.SelectedIndex = 0;
        adapterFormatComboBox.Enabled = false;

        // Update resolution combo box
        resolutionComboBox.Items.Clear();
        resolutionComboBox.Items.Add(FormatResolution(settings.WindowedDisplayMode.Width,
                                                      settings.WindowedDisplayMode.Height));
        resolutionComboBox.SelectedIndex = 0;
        resolutionComboBox.Enabled = false;

        // Update refresh rate combo box
        refreshRateComboBox.Items.Clear();
        refreshRateComboBox.Items.Add(FormatRefreshRate(settings.WindowedDisplayMode.RefreshRate));
        refreshRateComboBox.SelectedIndex = 0;
        refreshRateComboBox.Enabled = false;
      }
      else
      {
        settings.IsWindowed = false;
        settings.FullscreenAdapterInfo = adapterInfo;
        settings.FullscreenDeviceInfo = deviceInfo;

        // Update adapter format combo box
        adapterFormatComboBox.Items.Clear();
        foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
        {
          if (deviceCombo.IsWindowed)
          {
            continue;
          }
          if (!adapterFormatComboBox.Items.Contains(deviceCombo.AdapterFormat))
          {
            adapterFormatComboBox.Items.Add(deviceCombo.AdapterFormat);
            if (deviceCombo.AdapterFormat == (settings.IsWindowed
                                                ?
                                                  settings.WindowedDisplayMode.Format
                                                : settings.FullscreenDisplayMode.Format))
            {
              adapterFormatComboBox.SelectedItem = deviceCombo.AdapterFormat;
            }
          }
        }
        if (adapterFormatComboBox.SelectedItem == null && adapterFormatComboBox.Items.Count > 0)
        {
          adapterFormatComboBox.SelectedIndex = 0;
        }
        adapterFormatComboBox.Enabled = true;

        // Update resolution combo box
        resolutionComboBox.Enabled = true;

        // Update refresh rate combo box
        refreshRateComboBox.Enabled = true;
      }
    }


    /// <summary>
    /// Converts the given width and height into a formatted string
    /// </summary>
    private string FormatResolution(int width, int height)
    {
      StringBuilder sb = new StringBuilder(20);
      sb.AppendFormat("{0} by {1}", width, height);
      return sb.ToString();
    }


    /// <summary>
    /// Converts the given refresh rate into a formatted string
    /// </summary>
    private string FormatRefreshRate(int refreshRate)
    {
      if (refreshRate == 0)
      {
        return "Default Rate";
      }
      else
      {
        StringBuilder sb = new StringBuilder(20);
        sb.AppendFormat("{0} Hz", refreshRate);
        return sb.ToString();
      }
    }


    /// <summary>
    /// Respond to a change of selected adapter format by rebuilding the
    /// resolution list and back buffer format list.  Updating the selected 
    /// resolution and back buffer format will trigger updates of the rest 
    /// of the dialog.
    /// </summary>
    private void AdapterFormatChanged(object sender, EventArgs e)
    {
      if (!windowedRadioButton.Checked)
      {
        GraphicsAdapterInfo adapterInfo = (GraphicsAdapterInfo) adapterComboBox.SelectedItem;
        Format adapterFormat = (Format) adapterFormatComboBox.SelectedItem;
        settings.FullscreenDisplayMode.Format = adapterFormat;
        StringBuilder sb = new StringBuilder(20);

        resolutionComboBox.Items.Clear();
        foreach (DisplayMode displayMode in adapterInfo.DisplayModeList)
        {
          if (displayMode.Format == adapterFormat)
          {
            string resolutionString = FormatResolution(displayMode.Width, displayMode.Height);
            if (!resolutionComboBox.Items.Contains(resolutionString))
            {
              resolutionComboBox.Items.Add(resolutionString);
              if (settings.FullscreenDisplayMode.Width == displayMode.Width &&
                  settings.FullscreenDisplayMode.Height == displayMode.Height)
              {
                resolutionComboBox.SelectedItem = resolutionString;
              }
            }
          }
        }
        if (resolutionComboBox.SelectedItem == null && resolutionComboBox.Items.Count > 0)
        {
          resolutionComboBox.SelectedIndex = 0;
        }
      }

      // Update backbuffer format combo box
      GraphicsDeviceInfo deviceInfo = (GraphicsDeviceInfo) deviceComboBox.SelectedItem;
      backBufferFormatComboBox.Items.Clear();
      foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
      {
        if (deviceCombo.IsWindowed == settings.IsWindowed &&
            deviceCombo.AdapterFormat == settings.DisplayMode.Format)
        {
          if (!backBufferFormatComboBox.Items.Contains(deviceCombo.BackBufferFormat))
          {
            backBufferFormatComboBox.Items.Add(deviceCombo.BackBufferFormat);
            if (deviceCombo.BackBufferFormat == settings.BackBufferFormat)
            {
              backBufferFormatComboBox.SelectedItem = deviceCombo.BackBufferFormat;
            }
          }
        }
      }
      if (backBufferFormatComboBox.SelectedItem == null && backBufferFormatComboBox.Items.Count > 0)
      {
        backBufferFormatComboBox.SelectedIndex = 0;
      }
    }


    /// <summary>
    /// Respond to a change of selected resolution by rebuilding the
    /// refresh rate list.
    /// </summary>
    private void ResolutionChanged(object sender, EventArgs e)
    {
      if (settings.IsWindowed)
      {
        return;
      }

      GraphicsAdapterInfo adapterInfo = (GraphicsAdapterInfo) adapterComboBox.SelectedItem;

      // Update settings with new resolution
      string resolution = (string) resolutionComboBox.SelectedItem;
      string[] resolutionSplitStringArray = resolution.Split();
      int width = int.Parse(resolutionSplitStringArray[0]);
      int height = int.Parse(resolutionSplitStringArray[2]);
      settings.FullscreenDisplayMode.Width = width;
      settings.FullscreenDisplayMode.Height = height;

      // Update refresh rate list based on new resolution
      Format adapterFormat = (Format) adapterFormatComboBox.SelectedItem;
      refreshRateComboBox.Items.Clear();
      foreach (DisplayMode displayMode in adapterInfo.DisplayModeList)
      {
        if (displayMode.Format == adapterFormat &&
            displayMode.Width == width &&
            displayMode.Height == height)
        {
          string refreshRateString = FormatRefreshRate(displayMode.RefreshRate);
          if (!refreshRateComboBox.Items.Contains(refreshRateString))
          {
            refreshRateComboBox.Items.Add(refreshRateString);
            if (settings.FullscreenDisplayMode.RefreshRate == displayMode.RefreshRate)
            {
              refreshRateComboBox.SelectedItem = refreshRateString;
            }
          }
        }
      }
      if (refreshRateComboBox.SelectedItem == null && refreshRateComboBox.Items.Count > 0)
      {
        refreshRateComboBox.SelectedIndex = refreshRateComboBox.Items.Count - 1;
      }
    }


    /// <summary>
    /// Respond to a change of selected refresh rate.
    /// </summary>
    private void RefreshRateChanged(object sender, EventArgs e)
    {
      if (settings.IsWindowed)
      {
        return;
      }

      // Update settings with new refresh rate
      string refreshRateString = (string) refreshRateComboBox.SelectedItem;
      int refreshRate = 0;
      if (refreshRateString != "Default Rate")
      {
        string[] refreshRateSplitStringArray = refreshRateString.Split();
        refreshRate = int.Parse(refreshRateSplitStringArray[0]);
      }
      settings.FullscreenDisplayMode.RefreshRate = refreshRate;
    }


    /// <summary>
    /// Respond to a change of selected back buffer format by rebuilding
    /// the depth/stencil format list, multisample type list, and vertex
    /// processing type list.
    /// </summary>
    private void BackBufferFormatChanged(object sender, EventArgs e)
    {
      GraphicsDeviceInfo deviceInfo = (GraphicsDeviceInfo) deviceComboBox.SelectedItem;
      Format adapterFormat = (Format) adapterFormatComboBox.SelectedItem;
      Format backBufferFormat = (Format) backBufferFormatComboBox.SelectedItem;

      foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
      {
        if (deviceCombo.IsWindowed == settings.IsWindowed &&
            deviceCombo.AdapterFormat == adapterFormat &&
            deviceCombo.BackBufferFormat == backBufferFormat)
        {
          deviceCombo.BackBufferFormat = backBufferFormat;
          settings.DeviceCombo = deviceCombo;

          depthStencilBufferComboBox.Items.Clear();
          if (enumeration.AppUsesDepthBuffer)
          {
            foreach (DepthFormat format in deviceCombo.DepthStencilFormatList)
            {
              depthStencilBufferComboBox.Items.Add(format);
              if (format == settings.DepthStencilBufferFormat)
              {
                depthStencilBufferComboBox.SelectedItem = format;
              }
            }
            if (depthStencilBufferComboBox.SelectedItem == null && depthStencilBufferComboBox.Items.Count > 0)
            {
              depthStencilBufferComboBox.SelectedIndex = 0;
            }
          }
          else
          {
            depthStencilBufferComboBox.Enabled = false;
            depthStencilBufferComboBox.Items.Add("(not used)");
            depthStencilBufferComboBox.SelectedIndex = 0;
          }

          vertexProcComboBox.Items.Clear();
          foreach (VertexProcessingType vpt in deviceCombo.VertexProcessingTypeList)
          {
            vertexProcComboBox.Items.Add(vpt);
            if (vpt == settings.VertexProcessingType)
            {
              vertexProcComboBox.SelectedItem = vpt;
            }
          }
          if (vertexProcComboBox.SelectedItem == null && vertexProcComboBox.Items.Count > 0)
          {
            vertexProcComboBox.SelectedIndex = 0;
          }

          presentIntervalComboBox.Items.Clear();
          foreach (PresentInterval pi in deviceCombo.PresentIntervalList)
          {
            presentIntervalComboBox.Items.Add(pi);
            if (pi == settings.PresentInterval)
            {
              presentIntervalComboBox.SelectedItem = pi;
            }
          }
          if (presentIntervalComboBox.SelectedItem == null && presentIntervalComboBox.Items.Count > 0)
          {
            presentIntervalComboBox.SelectedIndex = 0;
          }

          break;
        }
      }
    }


    /// <summary>
    /// Respond to a change of selected depth/stencil buffer format.
    /// </summary>
    private void DepthStencilBufferFormatChanged(object sender, EventArgs e)
    {
      if (enumeration.AppUsesDepthBuffer)
      {
        settings.DepthStencilBufferFormat = (DepthFormat) depthStencilBufferComboBox.SelectedItem;
      }

      multisampleComboBox.Items.Clear();
      foreach (MultiSampleType msType in settings.DeviceCombo.MultiSampleTypeList)
      {
        // Check for DS/MS conflict
        bool conflictFound = false;
        foreach (
          DepthStencilMultiSampleConflict DSMSConflict in settings.DeviceCombo.DepthStencilMultiSampleConflictList)
        {
          if (DSMSConflict.DepthStencilFormat == settings.DepthStencilBufferFormat &&
              DSMSConflict.MultiSampleType == msType)
          {
            conflictFound = true;
            break;
          }
        }
        if (!conflictFound)
        {
          multisampleComboBox.Items.Add(msType);
          if (msType == settings.MultisampleType)
          {
            multisampleComboBox.SelectedItem = msType;
          }
        }
      }
      if (multisampleComboBox.SelectedItem == null && multisampleComboBox.Items.Count > 0)
      {
        multisampleComboBox.SelectedIndex = 0;
      }
    }


    /// <summary>
    /// Respond to a change of selected multisample type.
    /// </summary>
    private void MultisampleTypeChanged(object sender, EventArgs e)
    {
      settings.MultisampleType = (MultiSampleType) multisampleComboBox.SelectedItem;

      // Find current max multisample quality
      int maxQuality = 0;
      DeviceCombo deviceCombo = settings.DeviceCombo;
      for (int i = 0; i < deviceCombo.MultiSampleQualityList.Count; i++)
      {
        if ((MultiSampleType) deviceCombo.MultiSampleTypeList[i] == settings.MultisampleType)
        {
          maxQuality = (int) deviceCombo.MultiSampleQualityList[i];
          break;
        }
      }

      // Update multisample quality list based on new type
      multisampleQualityComboBox.Items.Clear();
      for (int iLevel = 0; iLevel < maxQuality; iLevel++)
      {
        multisampleQualityComboBox.Items.Add(iLevel);
        if (settings.MultisampleQuality == iLevel)
        {
          multisampleQualityComboBox.SelectedItem = iLevel;
        }
      }
      if (multisampleQualityComboBox.SelectedItem == null && multisampleQualityComboBox.Items.Count > 0)
      {
        multisampleQualityComboBox.SelectedIndex = 0;
      }
    }


    /// <summary>
    /// Respond to a change of selected multisample quality.
    /// </summary>
    private void MultisampleQualityChanged(object sender, EventArgs e)
    {
      settings.MultisampleQuality = (int) multisampleQualityComboBox.SelectedItem;
    }


    /// <summary>
    /// Respond to a change of selected vertex processing type.
    /// </summary>
    private void VertexProcessingChanged(object sender, EventArgs e)
    {
      settings.VertexProcessingType = (VertexProcessingType) vertexProcComboBox.SelectedItem;
    }


    /// <summary>
    /// Respond to a change of selected vertex processing type.
    /// </summary>
    private void PresentIntervalChanged(object sender, EventArgs e)
    {
      settings.PresentInterval = (PresentInterval) presentIntervalComboBox.SelectedItem;
    }
  }


  /// <summary>
  /// Current D3D settings: adapter, device, mode, formats, etc.
  /// </summary>
  public class D3DSettings
  {
    public bool IsWindowed;

    public GraphicsAdapterInfo WindowedAdapterInfo;
    public GraphicsDeviceInfo WindowedDeviceInfo;
    public DeviceCombo WindowedDeviceCombo;
    public DisplayMode WindowedDisplayMode; // not changable by the user
    public DepthFormat WindowedDepthStencilBufferFormat;
    public MultiSampleType WindowedMultisampleType;
    public int WindowedMultisampleQuality;
    public VertexProcessingType WindowedVertexProcessingType;
    public PresentInterval WindowedPresentInterval;
    public int WindowedWidth;
    public int WindowedHeight;

    public GraphicsAdapterInfo FullscreenAdapterInfo;
    public GraphicsDeviceInfo FullscreenDeviceInfo;
    public DeviceCombo FullscreenDeviceCombo;
    public DisplayMode FullscreenDisplayMode; // changable by the user
    public DepthFormat FullscreenDepthStencilBufferFormat;
    public MultiSampleType FullscreenMultisampleType;
    public int FullscreenMultisampleQuality;
    public VertexProcessingType FullscreenVertexProcessingType;
    public PresentInterval FullscreenPresentInterval;


    /// <summary>
    /// The adapter information
    /// </summary>
    public GraphicsAdapterInfo AdapterInfo
    {
      get { return IsWindowed ? WindowedAdapterInfo : FullscreenAdapterInfo; }
      set
      {
        if (IsWindowed)
        {
          WindowedAdapterInfo = value;
        }
        else
        {
          FullscreenAdapterInfo = value;
        }
      }
    }


    /// <summary>
    /// The device information
    /// </summary>
    public GraphicsDeviceInfo DeviceInfo
    {
      get { return IsWindowed ? WindowedDeviceInfo : FullscreenDeviceInfo; }
      set
      {
        if (IsWindowed)
        {
          WindowedDeviceInfo = value;
        }
        else
        {
          FullscreenDeviceInfo = value;
        }
      }
    }


    /// <summary>
    /// The device combo
    /// </summary>
    public DeviceCombo DeviceCombo
    {
      get { return IsWindowed ? WindowedDeviceCombo : FullscreenDeviceCombo; }
      set
      {
        if (IsWindowed)
        {
          WindowedDeviceCombo = value;
        }
        else
        {
          FullscreenDeviceCombo = value;
        }
      }
    }


    /// <summary>
    /// The adapters ordinal
    /// </summary>
    public int AdapterOrdinal
    {
      get { return DeviceCombo.AdapterOrdinal; }
    }


    /// <summary>
    /// The type of device this is
    /// </summary>
    public DeviceType DevType
    {
      get { return DeviceCombo.DevType; }
    }


    /// <summary>
    /// The back buffers format
    /// </summary>
    public Format BackBufferFormat
    {
      get { return DeviceCombo.BackBufferFormat; }
    }


    /// <summary>
    /// The display mode
    /// </summary>
    public DisplayMode DisplayMode
    {
      get { return IsWindowed ? WindowedDisplayMode : FullscreenDisplayMode; }
      set
      {
        if (IsWindowed)
        {
          WindowedDisplayMode = value;
        }
        else
        {
          FullscreenDisplayMode = value;
        }
      }
    }


    /// <summary>
    /// The Depth stencils format
    /// </summary>
    public DepthFormat DepthStencilBufferFormat
    {
      get { return IsWindowed ? WindowedDepthStencilBufferFormat : FullscreenDepthStencilBufferFormat; }
      set
      {
        if (IsWindowed)
        {
          WindowedDepthStencilBufferFormat = value;
        }
        else
        {
          FullscreenDepthStencilBufferFormat = value;
        }
      }
    }


    /// <summary>
    /// The multisample type
    /// </summary>
    public MultiSampleType MultisampleType
    {
      get { return IsWindowed ? WindowedMultisampleType : FullscreenMultisampleType; }
      set
      {
        if (IsWindowed)
        {
          WindowedMultisampleType = value;
        }
        else
        {
          FullscreenMultisampleType = value;
        }
      }
    }


    /// <summary>
    /// The multisample quality
    /// </summary>
    public int MultisampleQuality
    {
      get { return IsWindowed ? WindowedMultisampleQuality : FullscreenMultisampleQuality; }
      set
      {
        if (IsWindowed)
        {
          WindowedMultisampleQuality = value;
        }
        else
        {
          FullscreenMultisampleQuality = value;
        }
      }
    }


    /// <summary>
    /// The vertex processing type
    /// </summary>
    public VertexProcessingType VertexProcessingType
    {
      get { return IsWindowed ? WindowedVertexProcessingType : FullscreenVertexProcessingType; }
      set
      {
        if (IsWindowed)
        {
          WindowedVertexProcessingType = value;
        }
        else
        {
          FullscreenVertexProcessingType = value;
        }
      }
    }


    /// <summary>
    /// The presentation interval
    /// </summary>
    public PresentInterval PresentInterval
    {
      get { return IsWindowed ? WindowedPresentInterval : FullscreenPresentInterval; }
      set
      {
        if (IsWindowed)
        {
          WindowedPresentInterval = value;
        }
        else
        {
          FullscreenPresentInterval = value;
        }
      }
    }


    /// <summary>
    /// Clone the d3d settings class
    /// </summary>
    public D3DSettings Clone()
    {
      return (D3DSettings) MemberwiseClone();
    }
  }
}