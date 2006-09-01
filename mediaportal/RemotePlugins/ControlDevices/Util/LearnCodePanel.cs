using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using MediaPortal.ControlDevices;

namespace MediaPortal.ControlDevices
{
  public partial class LearnControlPanel : UserControl
  {

    
    public LearnControlPanel()
    {
      InitializeComponent();
/*
       ArrayList list = new ArrayList();
      list.Add(FactoryButton("CURSOR_LEFT"));
      list.Add(FactoryButton("CURSOR_RIGHT"));
      list.Add(FactoryButton("CURSOR_UP"));
      list.Add(FactoryButton("CURSOR_DOWN"));

      list.Add(FactoryButton("PLAY"));
      list.Add(FactoryButton("PAUSE"));
      list.Add(FactoryButton("STOP"));
      list.Add(FactoryButton("RECORD"));

      list.Add(FactoryButton("NUMBER_0"));
      list.Add(FactoryButton("NUMBER_1"));
      list.Add(FactoryButton("NUMBER_2"));
      list.Add(FactoryButton("NUMBER_3"));
      list.Add(FactoryButton("NUMBER_4"));
      list.Add(FactoryButton("NUMBER_5"));
      list.Add(FactoryButton("NUMBER_6"));
      list.Add(FactoryButton("NUMBER_7"));
      list.Add(FactoryButton("NUMBER_8"));
      list.Add(FactoryButton("NUMBER_9"));

      list.Add(FactoryButton("ASPECT_TOGGLE"));
      list.Add(FactoryButton("ASPECT_16/9"));
      list.Add(FactoryButton("ASPECT_4/3"));


      list.Add(FactoryButton("TELETEXT_SHOW"));
      list.Add(FactoryButton("TELETEXT_TRANSPARENT"));
      list.Add(FactoryButton("TELETEXT_RED"));
      list.Add(FactoryButton("TELETEXT_GREEN"));
      list.Add(FactoryButton("TELETEXT_YELLOW"));
      list.Add(FactoryButton("TELETEXT_BLUE"));

      list.Add(FactoryButton("EXTENDED_HOME"));
      list.Add(FactoryButton("EXTENDED_TV"));
      list.Add(FactoryButton("EXTENDED_VIDEO"));
      list.Add(FactoryButton("EXTENDED_MUSIC"));
      list.Add(FactoryButton("EXTENDED_PICTURES"));
      list.Add(FactoryButton("EXTENDED_RADIO"));
      list.Add(FactoryButton("EXTENDED_WEATHER"));

      list.Add(FactoryButton("CUSTOM_01"));
      list.Add(FactoryButton("CUSTOM_02"));
      list.Add(FactoryButton("CUSTOM_03"));
      list.Add(FactoryButton("CUSTOM_04"));
      list.Add(FactoryButton("CUSTOM_05"));
      list.Add(FactoryButton("CUSTOM_06"));
      list.Add(FactoryButton("CUSTOM_07"));
      list.Add(FactoryButton("CUSTOM_08"));
      list.Add(FactoryButton("CUSTOM_09"));
      list.Add(FactoryButton("CUSTOM_10"));
      list.Add(FactoryButton("CUSTOM_11"));
      list.Add(FactoryButton("CUSTOM_12"));
      list.Add(FactoryButton("CUSTOM_13"));
      list.Add(FactoryButton("CUSTOM_14"));
      list.Add(FactoryButton("CUSTOM_15"));
      list.Add(FactoryButton("CUSTOM_16"));
      list.Add(FactoryButton("CUSTOM_17"));
      list.Add(FactoryButton("CUSTOM_18"));
      list.Add(FactoryButton("CUSTOM_19"));
      list.Add(FactoryButton("CUSTOM_20"));

*/

      //comboBoxFireDTVReceiver.DisplayMember = "FriendlyName";
      //comboBoxFireDTVReceiver.ValueMember = "Name";


//      mpCodeList.Items.Add(FactoryButton("CUSTOM_20"));



    }

 //   public ControlCode FactoryButton(string id)
 //   {
 //     return (ControlCode)new USBUIRTControlCode(id);
 //   }

    private void mpGroupBox1_Enter(object sender, EventArgs e)
    {

    }

    private void USBUIRTLearnControl_Load(object sender, EventArgs e)
    {

    }

    private void mpCheckBox1_CheckedChanged(object sender, EventArgs e)
    {

    }
  }
}
