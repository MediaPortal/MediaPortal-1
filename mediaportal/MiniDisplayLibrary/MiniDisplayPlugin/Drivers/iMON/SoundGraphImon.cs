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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;


namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// Abstract base class for SoundGraph iMON display.
    /// SoundGraph iMON VFD and LCD are implementing this abstraction.
    /// This provides common functionality such as setting persistence.
    /// </summary>
    public abstract class SoundGraphImon
    {
        public SystemStatus MPStatus;
        public Settings iSettings;
        protected SoundGraphDisplay.DSPEQDATA iEqData;
        protected SoundGraphDisplay.DSPEQDATA iEqDataLeft;
        protected SoundGraphDisplay.DSPEQDATA iEqDataRight;
        

        public SoundGraphImon()
        {
            //
            LoadAdvancedSettings();
            Settings.OnSettingsChanged += AdvancedSettings_OnSettingsChanged;
            TextTopLine = string.Empty;
            TextBottomLine = string.Empty;
            NeedTextUpdate = true;
            //Allocate EQ data buffers
            iEqData = new SoundGraphDisplay.DSPEQDATA();
            iEqDataLeft = new SoundGraphDisplay.DSPEQDATA();
            iEqDataRight = new SoundGraphDisplay.DSPEQDATA();

        }

        protected string TextTopLine { get; set; }
        protected string TextBottomLine { get; set; }
        /// <summary>
        /// Specify whether or not our text content was changed
        /// </summary>
        protected bool NeedTextUpdate { get; set; }

        //Display name is notably used during configuration for testing
        public abstract string Name();
        //Launch advanced settings dialog
        public abstract void Configure();
        //Update tick
        public abstract void Update();
        //
        public abstract bool IsLcd();
        public abstract bool IsVfd();

        //Set text for give line index
        public void SetLine(int line, string message)
        {
            //Per our framework each line is updated only once per frame/tick
            if (line == 0 && TextTopLine != message)
            {
                TextTopLine = message;
                NeedTextUpdate = true;
            }
            else if (line == 1 && TextBottomLine != message)
            {
                TextBottomLine = message;
                NeedTextUpdate = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void UpdateEq()
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

            if (IsLcd())
            {
                //Force stereo mode for LCD
                iSettings.iEq.UseStereoEq = true;
            }

            if ((iSettings.iEq.UseStereoEq || iSettings.iEq.UseVUmeter) || iSettings.iEq.UseVUmeter2)
            {
                if (iSettings.iEq.UseStereoEq)
                {
                    //Stereo mode
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
                    //UV Meter or UV Meter: unused
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
                
                //if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
                if (IsLcd())
                {
                    iSettings.iEq.Render_MaxValue = (iSettings.iEq._useEqMode == 2) ? 8 : 0x10;
                    iSettings.iEq.EqArray[0] = (byte)iSettings.iEq._useEqMode;
                }
                /*
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
            
            //if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
            if (IsLcd())
            {
                if (!iSettings.iEq.UseVUmeter && !iSettings.iEq.UseVUmeter2)
                {
                    //SetEQ(EQSettings.EqArray);
                    //We take the last 16 entries cause the first one is static

                    for (int i = 1; i < 17; i++)
                    {
                        iEqDataLeft.BandData[i-1] = iSettings.iEq.EqArray[i];
                        iEqDataRight.BandData[i-1] = iSettings.iEq.EqArray[i];
                    }

                    //Array.Copy(iSettings.iEq.EqArray, 1, iEqDataLeft.BandData, 0, 0x10);
                    //Array.Copy(iSettings.iEq.EqArray, 1, iEqDataRight.BandData, 0, 0x10);
                    SoundGraphDisplay.IDW_SetLcdEqData(iEqDataLeft, iEqDataRight);
                }
                else
                {
                    //DrawVU(EQSettings.EqArray);
                }
            }
            /*
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
            else if (IsVfd())
            {
                if (!iSettings.iEq.UseVUmeter && !iSettings.iEq.UseVUmeter2)
                {
                    //var destinationArray = new int[0x10];
                    //We take the last 16 entries cause the first one is static
                    //Array.Copy(iSettings.iEq.EqArray, 1, iEqData.BandData, 0, 0x10);

                    for (int i = 1; i < 17; i++)
                    {
                        iEqData.BandData[i - 1] = iSettings.iEq.EqArray[i];
                    }

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



        //Here comes settings related stuff
        //Settings stuff
        private void LoadAdvancedSettings()
        {
            iSettings = Settings.Load();
        }

        private void AdvancedSettings_OnSettingsChanged()
        {
            SoundGraphDisplay.LogDebug("SoundGraphImon.AdvancedSettings_OnSettingsChanged(): RELOADING SETTINGS");

            //CleanUp();
            //Thread.Sleep(100);
            LoadAdvancedSettings();
            //Initialize();
        }

 

        [Serializable]
        public class Settings
        {
            public Settings()
            {
                //Init EqData
                MiniDisplayHelper.InitEQ(ref iEq);
            }

            public EQControl iEq;

            //Generic iMON settings
            [XmlAttribute]
            public bool DisableWhenInBackground { get; set; }

            [XmlAttribute]
            public bool DisableWhenIdle { get; set; }

            [XmlAttribute]
            public int DisableWhenIdleDelayInSeconds { get; set; }

            [XmlAttribute]
            public bool ReenableWhenIdleAfter { get; set; }

            [XmlAttribute]
            public int ReenableWhenIdleAfterDelayInSeconds { get; set; }

            [XmlAttribute]
            public bool DisableWhenPlaying { get; set; }

            [XmlAttribute]
            public int DisableWhenPlayingDelayInSeconds { get; set; }

            [XmlAttribute]
            public bool ReenableWhenPlayingAfter { get; set; }

            [XmlAttribute]
            public int ReenableWhenPlayingAfterDelayInSeconds { get; set; }

            
            //LCD properties
            [XmlAttribute]
            public bool PreferFirstLineGeneral { get; set; }

            [XmlAttribute]
            public bool PreferFirstLinePlayback { get; set; }

            //EQ Properties
            [XmlAttribute]
            public bool EqStartDelay { get; set; }

            [XmlAttribute]
            public int DelayEqTime { get; set; }


            [XmlAttribute]
            public int EqRate { get; set; }

            [XmlAttribute]
            public bool EqDisplay { get; set; }

            //Tells whether we periodically disable our EQ during play
            [XmlAttribute]
            public bool EqPeriodic { get; set; }

            //The time our EQ is disabled for
            [XmlAttribute]
            public int EqDisabledTimeInSeconds { get; set; }

            //The time our EQ is enabled for
            [XmlAttribute]
            public int EqEnabledTimeInSeconds { get; set; }

            [XmlAttribute]
            public bool RestrictEQ { get; set; }

            [XmlAttribute]
            public bool SmoothEQ { get; set; }
            //


            #region Delegates

            public delegate void OnSettingsChangedHandler();

            #endregion

            private static Settings m_Instance;
            public const string m_Filename = "MiniDisplay_SoundGraphImon.xml";

            public static Settings Instance
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



            public static event OnSettingsChangedHandler OnSettingsChanged;

            //Load default settings in the given instance
            private static void Default(Settings _settings)
            {
                _settings.EqDisplay = false;
                _settings.RestrictEQ = false;
                _settings.EqRate = 10;
                _settings.EqStartDelay = false;
                _settings.DelayEqTime = 10;
                _settings.SmoothEQ = false;

                //LCD properties
                _settings.PreferFirstLineGeneral = false;
                _settings.PreferFirstLinePlayback = false;

            }

            public static Settings Load()
            {
                Settings settings;
                SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): started");
                if (File.Exists(Config.GetFile(Config.Dir.Config, m_Filename)))
                {
                    SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): Loading settings from XML file");
                    var serializer = new XmlSerializer(typeof(Settings));
                    var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, m_Filename));
                    settings = (Settings)serializer.Deserialize(xmlReader);
                    xmlReader.Close();
                }
                else
                {
                    SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): Loading settings from defaults");
                    settings = new Settings();
                    Default(settings);
                    SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): Loaded settings from defaults");
                }

                //Sync our EQ settings
                settings.iEq.UseEqDisplay = settings.EqDisplay;
                settings.iEq.DelayEQ = settings.EqStartDelay;
                settings.iEq._DelayEQTime = settings.DelayEqTime;
                settings.iEq.EQTitleDisplay = settings.EqPeriodic;
                settings.iEq._EQTitleShowTime = settings.EqDisabledTimeInSeconds;
                settings.iEq._EQTitleDisplayTime = settings.EqEnabledTimeInSeconds;


                SoundGraphDisplay.LogDebug("SoundGraphImon.Settings.Load(): completed");
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

            public static void Save(Settings ToSave)
            {
                var serializer = new XmlSerializer(typeof(Settings));
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