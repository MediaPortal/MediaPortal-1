using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class BlasterSetup : SetupTv.SectionSettings
  {
    public BlasterSetup()
    {
      InitializeComponent();
    }

    public override void OnSectionDeActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("SrvBlasterType");
      setting.Value = comboBox1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("SrvBlasterSpeed");
      setting.Value = comboBox2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("SrvBlaster1Card");
      setting.Value = comboBox3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("SrvBlaster2Card");
      setting.Value = comboBox4.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("SrvBlasterLog");
      setting.Value = Convert.ToString(checkBox1.Checked == true);
      setting.Persist();
      setting = layer.GetSetting("SrvBlasterSendSelect");
      setting.Value = Convert.ToString(checkSendSelect.Checked == true);
      setting.Persist();

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {


      TvBusinessLayer layer = new TvBusinessLayer();
      comboBox1.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlasterType", "0").Value);
      comboBox2.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlasterSpeed", "0").Value.ToString());
      comboBox3.Items.Clear();
      comboBox4.Items.Clear();
      comboBox3.Items.Add("None");
      comboBox4.Items.Add("None");
      for (int i = 0; i < layer.Cards.Count; ++i)
      {
        Card card = (Card)layer.Cards[i];
        comboBox3.Items.Add(card.Name);
        comboBox4.Items.Add(card.Name);
      }
      Log.WriteFile("CB1Size {0}, CB2Size {1}, BT1 {2}, BT2 {3}", comboBox3.Items.Count, comboBox3.Items.Count, Convert.ToInt16(layer.GetSetting("SrvBlaster1Card", "0").Value), Convert.ToInt16(layer.GetSetting("SrvBlaster2Card", "0").Value));
      comboBox3.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlaster1Card", "0").Value);
      comboBox4.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlaster2Card", "0").Value);
      checkBox1.Checked = (layer.GetSetting("SrvBlasterLog").Value == "True");
      checkSendSelect.Checked = (layer.GetSetting("SrvBlasterSendSelect").Value == "True");
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
  }
}