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
        }

        public override void SetLine(int line, string message)
        {
            if (line == 0)
            {
                Line1 = message;
            }
            else if (line == 1)
            {
                Line2 = message;

                //Check if we need to show EQ this is also taking into account our various settings.
                iSettings.iEq._EqDataAvailable = MiniDisplayHelper.GetEQ(ref iSettings.iEq);
                if (iSettings.iEq._EqDataAvailable)
                {                    
                    //SetAndRollEqData();
                    UpdateEq();
                }
                else
                {
                    //Not show EQ then display our lines
                    SoundGraphDisplay.IDW_SetVfdText(Line1, Line2);
                }
            }
        }

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

        void UpdateEq()
        {

            if (!(iSettings.iEq.UseEqDisplay & iSettings.iEq._EqDataAvailable))
            {
                return;
            }
            if (iSettings.iEq.RestrictEQ &
                ((DateTime.Now.Ticks - iSettings.iEq._LastEQupdate.Ticks) < iSettings.iEq._EqUpdateDelay))
            {
                return;
            }
            //if (DoDebug)
            {
                //SoundGraphDisplay.LogInfo("\niMONLCDg.DisplayEQ(): Retrieved {0} samples of Equalizer data.", EQSettings.EqFftData.Length / 2);
            }
            if ((iSettings.iEq.UseStereoEq || iSettings.iEq.UseVUmeter) || iSettings.iEq.UseVUmeter2)
            {
                if (iSettings.iEq.UseStereoEq)
                {
                    iSettings.iEq.Render_MaxValue = 100;
                    iSettings.iEq.Render_BANDS = 8;
                    iSettings.iEq.EqArray[0] = 0x63;
                    /*
                    if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
                    {
                        EQSettings.Render_MaxValue = (EQSettings._useEqMode == 2) ? 8 : 0x10;
                        EQSettings.EqArray[0] = (byte)EQSettings._useEqMode;
                    }
                    else if (_DisplayType == DisplayType.ThreeRsystems)
                    {
                        EQSettings.Render_MaxValue = 6;
                        EQSettings.EqArray[0] = 0;
                    }
                     */
                    MiniDisplayHelper.ProcessEqData(ref iSettings.iEq);
                    for (int i = 0; i < iSettings.iEq.Render_BANDS; i++)
                    {
                        switch (iSettings.iEq.EqArray[0])
                        {
                            case 2:
                                {
                                    var num2 = (byte)(iSettings.iEq.EqArray[1 + i] & 15);
                                    iSettings.iEq.EqArray[1 + i] = (byte)((num2 << 4) | num2);
                                    var num3 = (byte)(iSettings.iEq.EqArray[9 + i] & 15);
                                    iSettings.iEq.EqArray[9 + i] = (byte)((num3 << 4) | num3);
                                    break;
                                }
                        }
                    }
                    for (int j = 15; j > 7; j--)
                    {
                        iSettings.iEq.EqArray[j + 1] = iSettings.iEq.EqArray[j];
                    }
                    iSettings.iEq.EqArray[8] = 0;
                    iSettings.iEq.EqArray[9] = 0;
                }
                else
                {
                    iSettings.iEq.Render_MaxValue = 80;
                    iSettings.iEq.Render_BANDS = 1;
                    if (/*(_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2)*/ false)
                    {
                        iSettings.iEq.Render_MaxValue = 0x60;
                        if (iSettings.iEq._useVUindicators)
                        {
                            iSettings.iEq.Render_MaxValue = 0x60;
                        }
                    }
                    else if (iSettings.iEq._useVUindicators)
                    {
                        iSettings.iEq.Render_MaxValue = 0x4b;
                    }
                    MiniDisplayHelper.ProcessEqData(ref iSettings.iEq);
                }
            }
            else
            {
                iSettings.iEq.Render_MaxValue = 100;
                iSettings.iEq.Render_BANDS = 0x10;
                iSettings.iEq.EqArray[0] = 0x63;
                /*
                if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
                {
                    EQSettings.Render_MaxValue = (EQSettings._useEqMode == 2) ? 8 : 0x10;
                    EQSettings.EqArray[0] = (byte)EQSettings._useEqMode;
                }
                else if (_DisplayType == DisplayType.ThreeRsystems)
                {
                    EQSettings.Render_MaxValue = 6;
                    EQSettings.EqArray[0] = 0;
                }*/
                MiniDisplayHelper.ProcessEqData(ref iSettings.iEq);
                for (int k = 0; k < iSettings.iEq.Render_BANDS; k++)
                {
                    switch (iSettings.iEq.EqArray[0])
                    {
                        case 2:
                            {
                                var num6 = (byte)(iSettings.iEq.EqArray[1 + k] & 15);
                                iSettings.iEq.EqArray[1 + k] = (byte)((num6 << 4) | num6);
                                break;
                            }
                    }
                }
            }
            /*
            if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
            {
                if (!EQSettings.UseVUmeter && !EQSettings.UseVUmeter2)
                {
                    SetEQ(EQSettings.EqArray);
                }
                else
                {
                    DrawVU(EQSettings.EqArray);
                }
            }
            else if (_DisplayType == DisplayType.ThreeRsystems)
            {
                for (int m = 0; m < 8; m++)
                {
                    EQSettings.EqArray[1 + m] =
                      (byte)((EQSettings.EqArray[1 + m] << 4) + EQSettings.EqArray[9 + m]);
                }
                ulong data = 0x0901000000000000L;
                ulong num9 = 0x0000000000000002L;
                data = data + EQSettings.EqArray[1] << 40;
                data = data + EQSettings.EqArray[2] << 0x20;
                data = data + EQSettings.EqArray[3] << 0x18;
                data = data + EQSettings.EqArray[4] << 0x10;
                data = data + EQSettings.EqArray[5] << 8;
                num9 = num9 + EQSettings.EqArray[6] << 40;
                num9 = num9 + EQSettings.EqArray[7] << 0x20;
                num9 = num9 + EQSettings.EqArray[8] << 0x18;
                SendData(0x0200020000000000L);
                SendData(0x0000000000000002L);
                SendData(0x0d0f202020202000L);
                SendData(0x2020202020202002L);
                SendData(data);
                SendData(num9);
            }
            */
            
            {
                if (!iSettings.iEq.UseVUmeter && !iSettings.iEq.UseVUmeter2)
                {
                    //var destinationArray = new int[0x10];
                    Array.Copy(iSettings.iEq.EqArray, 1, iEqData.BandData, 0, 0x10);
                    SoundGraphDisplay.IDW_SetVfdEqData(iEqData);
                    goto Label_0613;

                }
                //DrawVU(EQSettings.EqArray);
            }
        Label_0613:
            iSettings.iEq._LastEQupdate = DateTime.Now;
        if ((DateTime.Now.Ticks - iSettings.iEq._EQ_FPS_time.Ticks) < 0x989680L)
            {
                iSettings.iEq._EQ_Framecount++;
            }
            else
            {
                iSettings.iEq._Max_EQ_FPS = Math.Max(iSettings.iEq._Max_EQ_FPS, iSettings.iEq._EQ_Framecount);
                iSettings.iEq._EQ_Framecount = 0;
                iSettings.iEq._EQ_FPS_time = DateTime.Now;
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

