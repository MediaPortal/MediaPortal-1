using System;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class BlasterSetup : SectionSettings
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
      setting.Value = Convert.ToString(checkBox1.Checked);
      setting.Persist();
      setting = layer.GetSetting("SrvBlasterSendSelect");
      setting.Value = Convert.ToString(checkSendSelect.Checked);
      setting.Persist();

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {


      TvBusinessLayer layer = new TvBusinessLayer();
      comboBox1.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlasterType", "0").Value);
      comboBox2.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlasterSpeed", "0").Value);
      comboBox3.Items.Clear();
      comboBox4.Items.Clear();
      comboBox3.Items.Add("None");
      comboBox4.Items.Add("None");
      for (int i = 0; i < layer.Cards.Count; ++i)
      {
        Card card = layer.Cards[i];
        comboBox3.Items.Add(card.Name);
        comboBox4.Items.Add(card.Name);
      }
      Log.WriteFile("CB1Size {0}, CB2Size {1}, BT1 {2}, BT2 {3}", comboBox3.Items.Count, comboBox3.Items.Count, Convert.ToInt16(layer.GetSetting("SrvBlaster1Card", "0").Value), Convert.ToInt16(layer.GetSetting("SrvBlaster2Card", "0").Value));
      comboBox3.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlaster1Card", "0").Value);
      comboBox4.SelectedIndex = Convert.ToInt16(layer.GetSetting("SrvBlaster2Card", "0").Value);
      checkBox1.Checked = (layer.GetSetting("SrvBlasterLog").Value == "True");
      checkSendSelect.Checked = (layer.GetSetting("SrvBlasterSendSelect").Value == "True");
    }

    void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
    {
    	switch (comboBox1.SelectedIndex)
      	{
        	case 0:
    		case 1:
    			comboBox2.Visible=true;
    			comboBox3.Visible=true;
    			comboBox4.Visible=true;
    			label2.Visible=true;
    			label3.Visible=true;
    			label4.Visible=true;
    			checkSendSelect.Visible=true;
    			mpLabel1.Visible=false;
    			break;
        	
        	case 2: // Hauppauge blasting
    			comboBox2.Visible=false;
    			comboBox3.Visible=false;
    			comboBox4.Visible=false;
    			label2.Visible=false;
    			label3.Visible=false;
    			label4.Visible=false;
    			checkSendSelect.Visible=false;
    			mpLabel1.Visible=true;
    			break;  
        	default: break;
      	}
    }
  }
}