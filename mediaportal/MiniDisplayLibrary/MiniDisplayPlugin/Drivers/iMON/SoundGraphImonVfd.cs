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
    public class SoundGraphImonVfd : ISoundGraphImonDisplay
    {
        SoundGraphDisplay.DSPEQDATA iEqData;
        private AdvancedSettings AdvSettings;

        //Constructor
        public SoundGraphImonVfd()
        {
            //Settings
            LoadAdvancedSettings();
            AdvancedSettings.OnSettingsChanged += AdvancedSettings_OnSettingsChanged;

            //Init EqData
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
                SoundGraphDisplay.IDW_SetVfdText(Line1, Line2);
                //SetAndRollEqData();
            }
        }

        public override string Name() { return "iMON VFD"; }

        public override void Configure()
        {
            //No advanced settings for now
            SoundGraphDisplay.LogDebug("SoundGraphImonVfd.Configure() called");
            Form form = new SoundGraphImonVfdAdvancedSetupForm();
            form.ShowDialog();
            form.Dispose();
            SoundGraphDisplay.LogDebug("(IDisplay) SoundGraphImonVfd.Configure() completed");
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
            //_preferLine1General = AdvSettings.PreferFirstLineGeneral;
            //_preferLine1Playback = AdvSettings.PreferFirstLinePlayback;
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

            [XmlAttribute]
            public bool PreferFirstLineGeneral { get; set; }

            [XmlAttribute]
            public bool PreferFirstLinePlayback { get; set; }

            public static event OnSettingsChangedHandler OnSettingsChanged;

            private static void Default(AdvancedSettings _settings)
            {
                _settings.PreferFirstLineGeneral = true;
                _settings.PreferFirstLinePlayback = true;
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

