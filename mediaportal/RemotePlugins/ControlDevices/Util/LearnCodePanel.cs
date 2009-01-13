#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Windows.Forms;

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