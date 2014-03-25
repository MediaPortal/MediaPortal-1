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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
//using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// SoundGraph iMON VFD.
    /// </summary>
    public class SoundGraphImonVfd : SoundGraphImon
    {
        SoundGraphDisplay.DSPEQDATA iEqData;
        private AdvancedSettings AdvSettings;
        private EQControl EQSettings;

        //Constructor
        public SoundGraphImonVfd()
        {
            //Settings
            LoadAdvancedSettings();
            AdvancedSettings.OnSettingsChanged += AdvancedSettings_OnSettingsChanged;

            //Init EqData
            MiniDisplayHelper.InitEQ(ref EQSettings);
            EQSettings.UseEqDisplay = true;
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
                EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref EQSettings);
                if (!EQSettings._EqDataAvailable)
                {
                    SoundGraphDisplay.IDW_SetVfdText(Line1, Line2);
                }
                else
                {
                    //SetAndRollEqData();
                    UpdateEq();
                }                
            }
        }

        public override string Name() { return "iMON VFD"; }

        public override void Configure()
        {
            //Our display is initialized, now open the advanced setting dialog
            SoundGraphDisplay.LogDebug("SoundGraphImonVfd.Configure() called");
            Form form = new SoundGraphImonVfdAdvancedSetupForm();
            form.ShowDialog();
            form.Dispose();
            SoundGraphDisplay.LogDebug("(IDisplay) SoundGraphImonVfd.Configure() completed");
        }

        void UpdateEq()
        {

            if (!(EQSettings.UseEqDisplay & EQSettings._EqDataAvailable))
            {
                return;
            }
            if (EQSettings.RestrictEQ &
                ((DateTime.Now.Ticks - EQSettings._LastEQupdate.Ticks) < EQSettings._EqUpdateDelay))
            {
                return;
            }
            //if (DoDebug)
            {
                //SoundGraphDisplay.LogInfo("\niMONLCDg.DisplayEQ(): Retrieved {0} samples of Equalizer data.", EQSettings.EqFftData.Length / 2);
            }
            if ((EQSettings.UseStereoEq || EQSettings.UseVUmeter) || EQSettings.UseVUmeter2)
            {
                if (EQSettings.UseStereoEq)
                {
                    EQSettings.Render_MaxValue = 100;
                    EQSettings.Render_BANDS = 8;
                    EQSettings.EqArray[0] = 0x63;
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
                    MiniDisplayHelper.ProcessEqData(ref EQSettings);
                    for (int i = 0; i < EQSettings.Render_BANDS; i++)
                    {
                        switch (EQSettings.EqArray[0])
                        {
                            case 2:
                                {
                                    var num2 = (byte)(EQSettings.EqArray[1 + i] & 15);
                                    EQSettings.EqArray[1 + i] = (byte)((num2 << 4) | num2);
                                    var num3 = (byte)(EQSettings.EqArray[9 + i] & 15);
                                    EQSettings.EqArray[9 + i] = (byte)((num3 << 4) | num3);
                                    break;
                                }
                        }
                    }
                    for (int j = 15; j > 7; j--)
                    {
                        EQSettings.EqArray[j + 1] = EQSettings.EqArray[j];
                    }
                    EQSettings.EqArray[8] = 0;
                    EQSettings.EqArray[9] = 0;
                }
                else
                {
                    EQSettings.Render_MaxValue = 80;
                    EQSettings.Render_BANDS = 1;
                    if (/*(_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2)*/ false)
                    {
                        EQSettings.Render_MaxValue = 0x60;
                        if (EQSettings._useVUindicators)
                        {
                            EQSettings.Render_MaxValue = 0x60;
                        }
                    }
                    else if (EQSettings._useVUindicators)
                    {
                        EQSettings.Render_MaxValue = 0x4b;
                    }
                    MiniDisplayHelper.ProcessEqData(ref EQSettings);
                }
            }
            else
            {
                EQSettings.Render_MaxValue = 100;
                EQSettings.Render_BANDS = 0x10;
                EQSettings.EqArray[0] = 0x63;
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
                MiniDisplayHelper.ProcessEqData(ref EQSettings);
                for (int k = 0; k < EQSettings.Render_BANDS; k++)
                {
                    switch (EQSettings.EqArray[0])
                    {
                        case 2:
                            {
                                var num6 = (byte)(EQSettings.EqArray[1 + k] & 15);
                                EQSettings.EqArray[1 + k] = (byte)((num6 << 4) | num6);
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
                if (!EQSettings.UseVUmeter && !EQSettings.UseVUmeter2)
                {
                    //var destinationArray = new int[0x10];
                    Array.Copy(EQSettings.EqArray, 1, iEqData.BandData, 0, 0x10);
                    SoundGraphDisplay.IDW_SetVfdEqData(iEqData);
                    goto Label_0613;

                }
                //DrawVU(EQSettings.EqArray);
            }
        Label_0613:
            EQSettings._LastEQupdate = DateTime.Now;
            if ((DateTime.Now.Ticks - EQSettings._EQ_FPS_time.Ticks) < 0x989680L)
            {
                EQSettings._EQ_Framecount++;
            }
            else
            {
                EQSettings._Max_EQ_FPS = Math.Max(EQSettings._Max_EQ_FPS, EQSettings._EQ_Framecount);
                EQSettings._EQ_Framecount = 0;
                EQSettings._EQ_FPS_time = DateTime.Now;
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

        //Settings stuff
        private void LoadAdvancedSettings()
        {
            AdvSettings = AdvancedSettings.Load();
        }

        private void AdvancedSettings_OnSettingsChanged()
        {
            SoundGraphDisplay.LogDebug("SoundGraphImonVfd.AdvancedSettings_OnSettingsChanged(): RELOADING SETTINGS");

            //CleanUp();
            //Thread.Sleep(100);
            LoadAdvancedSettings();
            //Initialize();
        }

        [Serializable]
        public class AdvancedSettings
        {
            #region Delegates

            public delegate void OnSettingsChangedHandler();

            #endregion

            private static AdvancedSettings m_Instance;
            public const string m_Filename = "MiniDisplay_SoundGraphImonVfd.xml";

            public static AdvancedSettings Instance
            {
                get
                {
                    if (m_Instance == null)
                    {
                        m_Instance = Load();
                    }
                    return m_Instance;
                }
                set { m_Instance = value; }
            }

            //Generic iMON settings
            //TODO: Find a way not to duplicate this code for LCD/VFD

            [XmlAttribute]
            public bool DisableWhenInBackground { get; set; }

            [XmlAttribute]
            public bool DisableWhenIdle { get; set; }

            [XmlAttribute]
            public int DisableWhenIdleDelayInSeconds { get; set; }

            [XmlAttribute]
            public bool ReenableAfter { get; set; }

            [XmlAttribute]
            public int ReenableAfterDelayInSeconds { get; set; }

            [XmlAttribute]
            public bool DelayEQ { get; set; }

            [XmlAttribute]
            public int DelayEqTime { get; set; }
 

            [XmlAttribute]
            public int EqMode { get; set; }

            [XmlAttribute]
            public int EqRate { get; set; }

            [XmlAttribute]
            public bool EqDisplay { get; set; }

            [XmlAttribute]
            public bool NormalEQ { get; set; }

            [XmlAttribute]
            public bool StereoEQ { get; set; }

            [XmlAttribute]
            public bool VUmeter { get; set; }

            [XmlAttribute]
            public bool VUmeter2 { get; set; }

            [XmlAttribute]
            public bool VUindicators { get; set; }

            [XmlAttribute]
            public bool RestrictEQ { get; set; }

            [XmlAttribute]
            public bool SmoothEQ { get; set; }


            public static event OnSettingsChangedHandler OnSettingsChanged;

            private static void Default(AdvancedSettings _settings)
            {
                _settings.EqDisplay = false;
                _settings.NormalEQ = true;
                _settings.StereoEQ = false;
                _settings.VUmeter = false;
                _settings.VUmeter2 = false;
                _settings.VUindicators = false;
                _settings.EqMode = 0;
                _settings.RestrictEQ = false;
                _settings.EqRate = 10;
                _settings.DelayEQ = false;
                _settings.DelayEqTime = 10;
                _settings.SmoothEQ = false;
 
            }

            public static AdvancedSettings Load()
            {
                AdvancedSettings settings;
                SoundGraphDisplay.LogDebug("SoundGraphImonVfd.AdvancedSettings.Load(): started");
                if (File.Exists(Config.GetFile(Config.Dir.Config, m_Filename)))
                {
                    SoundGraphDisplay.LogDebug("SoundGraphImonVfd.AdvancedSettings.Load(): Loading settings from XML file");
                    var serializer = new XmlSerializer(typeof(AdvancedSettings));
                    var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, m_Filename));
                    settings = (AdvancedSettings)serializer.Deserialize(xmlReader);
                    xmlReader.Close();
                }
                else
                {
                    SoundGraphDisplay.LogDebug("SoundGraphImonVfd.AdvancedSettings.Load(): Loading settings from defaults");
                    settings = new AdvancedSettings();
                    Default(settings);
                    SoundGraphDisplay.LogDebug("SoundGraphImonVfd.AdvancedSettings.Load(): Loaded settings from defaults");
                }
                SoundGraphDisplay.LogDebug("SoundGraphImonVfd.AdvancedSettings.Load(): completed");
                return settings;
            }

            public static void NotifyDriver()
            {
                if (OnSettingsChanged != null)
                {
                    OnSettingsChanged();
                }
            }

            public static void Save()
            {
                Save(Instance);
            }

            public static void Save(AdvancedSettings ToSave)
            {
                var serializer = new XmlSerializer(typeof(AdvancedSettings));
                var writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, m_Filename),
                                               Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 2 };
                serializer.Serialize(writer, ToSave);
                writer.Close();
            }

            public static void SetDefaults()
            {
                Default(Instance);
            }
        }

    }

}

