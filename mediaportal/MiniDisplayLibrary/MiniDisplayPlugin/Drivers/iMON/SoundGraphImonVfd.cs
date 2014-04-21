#region Copyright (C) 2014 Team MediaPortal

// Copyright (C) 2014 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
//using System.IO;
using System.Windows.Forms;
//using System.Xml;
//using System.Xml.Serialization;
//using MediaPortal.Configuration;
//using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// SoundGraph iMON VFD.
    /// </summary>
    public class SoundGraphImonVfd : SoundGraphImon
    {
        //private AdvancedSettings AdvSettings;
        

        //Constructor
        public SoundGraphImonVfd()
        {
            //
            /*
            iEqData = new SoundGraphDisplay.DSPEQDATA();
            iEqData.BandData[0] = 100;
            iEqData.BandData[1] = 90;
            iEqData.BandData[2] = 80;
            iEqData.BandData[3] = 70;
            iEqData.BandData[4] = 60;
            iEqData.BandData[5] = 50;
            iEqData.BandData[6] = 40;
            iEqData.BandData[7] = 30;
            iEqData.BandData[8] = 20;
            iEqData.BandData[9] = 10;
            iEqData.BandData[10] = 00;
            iEqData.BandData[11] = 10;
            iEqData.BandData[12] = 20;
            iEqData.BandData[13] = 30;
            iEqData.BandData[14] = 40;
            iEqData.BandData[15] = 50;
             */
        }

        public override bool IsLcd() { return false; }
        public override bool IsVfd() { return true; }
        public override string Name() { return "iMON VFD"; }

        public override void Configure()
        {
            //Our display is initialized, now open the advanced setting dialog
            SoundGraphDisplay.LogDebug("SoundGraphImonVfd.Configure() called");
            SoundGraphImonSettingsForm form = new SoundGraphImonSettingsForm();
            //Hide our LCD tab since we configure a VFD display
            //TODO: make this a method of SoundGraphImonSettingsForm
            form.tabControl.TabPages.Remove(form.tabLcd);
            form.ShowDialog();
            form.Dispose();
            SoundGraphDisplay.LogDebug("(IDisplay) SoundGraphImonVfd.Configure() completed");
        }

        public override void Update()
        {
            //Check if we need to show EQ this is also taking into account our various settings.
            iSettings.iEq._EqDataAvailable = MiniDisplayHelper.GetEQ(ref iSettings.iEq);
            if (iSettings.iEq._EqDataAvailable)
            {
                //SetAndRollEqData();
                UpdateEq();
            }
            else if (NeedTextUpdate)
            {
                //Not show EQ then display our lines
                SoundGraphDisplay.IDW_SetVfdText(TextTopLine, TextBottomLine);
                NeedTextUpdate = false;
            }

        }


        //Testing
        void SetAndRollEqData()
        {
            //SL: The following demonstrates how to pass EqData to our C++ DLL
            SoundGraphDisplay.IDW_SetVfdEqData(iEqData);
            //Move our data for our next pass
            for (int i = 0; i < 15; i++)
            {
                iEqData.BandData[i] = iEqData.BandData[i + 1];
            }

            if (iEqData.BandData[14] == 100)
            {
                //Maxed bounce back down
                iEqData.BandData[15] = 90;
            }
            else if (iEqData.BandData[14] == 0)
            {
                //Mined bounce back up
                iEqData.BandData[15] = 10;
            }
            else if (iEqData.BandData[13] > iEqData.BandData[14])
            {
                //Going down
                iEqData.BandData[15] = iEqData.BandData[14] - 10;
            }
            else if (iEqData.BandData[13] < iEqData.BandData[14])
            {
                //Going up
                iEqData.BandData[15] = iEqData.BandData[14] + 10;
            }
        }

    }

}

