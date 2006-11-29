#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using DreamBox;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using System.ComponentModel;


namespace ProcessPlugins.ExternalDreamboxTV
{
    public class ExternalDreamboxTV : IPlugin, ISetupForm
    {
        private string _DreamboxIP = "";
        private string _DreamboxUserName = "";
        private string _DreamboxPassword = "";
        private double _SyncHours = 0;
        private System.Timers.Timer _SyncTimer = new System.Timers.Timer();
        private BackgroundWorker _EPGbackgroundWorker;
        private BackgroundWorker _ZapbackgroundWorker;

        public ExternalDreamboxTV()
        {
            Log.Info("DreamboxTV: .ctor");
        }



        public void Start()
        {
            LoadSettings();
            GUIWindowManager.Receivers +=new SendMessageHandler(GUIWindowManager_Receivers);
            _SyncTimer.Elapsed += new System.Timers.ElapsedEventHandler(_SyncTimer_Elapsed);
            this._EPGbackgroundWorker = new BackgroundWorker();
            this._EPGbackgroundWorker.DoWork += new DoWorkEventHandler(_EPGbackgroundWorker_DoWork);
            this._EPGbackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_EPGbackgroundWorker_RunWorkerCompleted);

            this._ZapbackgroundWorker = new BackgroundWorker();
            this._ZapbackgroundWorker.DoWork += new DoWorkEventHandler(_ZapbackgroundWorker_DoWork);

            if (_SyncHours > 0)
            {
                int inval = Convert.ToInt32(_SyncHours);
                TimeSpan ts = new TimeSpan(0, 1, 0);
                _SyncTimer.Interval = ts.TotalMilliseconds;
                _SyncTimer.Start();
            }


            
        }

        void _ZapbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string reference = (string)e.Argument;
            // Zap channel
            if (!_ZapbackgroundWorker.IsBusy)
            {
                try
                {
                    if (_DreamboxIP.Length > 0)
                    {
                        DreamBox.Core dreambox = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
                        dreambox.Remote.Zap(reference);
                        Log.Info("ExternalDreamboxTV ZAP: {0}\r\n", reference);
                    }
                    else
                        Log.Info("ExternalDreamboxTV Error: {0}\r\n", "Could not zap because IP address of dreambox not set.");

                }
                catch (Exception x)
                {
                    Log.Info("ExternalDreamboxTV Error: {0}\r\n{1}", x.Message, x.StackTrace);
                }
            }
        }

        void _EPGbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.Info("DreamboxTV: Scheduled EPG import has run.");
        }

        void _EPGbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.ImportEPG();
        }

        void _SyncTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Info("DreamboxTV: Timer Tick!");

            // check for last time
            string sLastEPGSync = "";
            System.DateTime lastEPGSync = System.DateTime.Now.AddDays(-1);

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                sLastEPGSync = xmlreader.GetValueAsString("Dreambox", "LastEPGSync", System.DateTime.Now.AddDays(-1).ToString());
                try
                {
                    lastEPGSync = System.DateTime.Parse(sLastEPGSync);
                }
                catch { }

            }

            // do check
            System.DateTime test = lastEPGSync.AddHours(_SyncHours);
            if (test < System.DateTime.Now)
            {
                // Get EPG
                Log.Info("DreamboxTV: Get EPG");
                if (!this._EPGbackgroundWorker.IsBusy)
                    this._EPGbackgroundWorker.RunWorkerAsync();
                //this.ImportEPG();

                // Save new Sync Data
                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
                {
                    xmlwriter.SetValue("Dreambox", "LastEPGSync", System.DateTime.Now.ToString());
                }
            }


        }

        void GUIWindowManager_Receivers(GUIMessage message)
        {
            
            switch (message.Message)
            {
                    
                case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
                    Log.Info("DreamboxTV: Changing channel");
                    bool bIsInteger;
                    double retNum;
                    bIsInteger = Double.TryParse(message.Label, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
                    // is Dreambox recording?
                    if (DreamboxIsRecording)
                    {
                        string recordmessage = "Dreambox is recording.\rAt the moment you cannot zap\rto another channel.";
                        MediaPortal.Dialogs.GUIDialogOK pDlgOK = (MediaPortal.Dialogs.GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                        if (pDlgOK != null)
                        {
                            string[] lines = recordmessage.Split('\r');
                            pDlgOK.SetHeading(605);//my tv
                            pDlgOK.SetLine(1, lines[0]);
                            if (lines.Length > 1)
                                pDlgOK.SetLine(2, lines[1]);
                            else
                                pDlgOK.SetLine(2, "");

                            if (lines.Length > 2)
                                pDlgOK.SetLine(3, lines[2]);
                            else
                                pDlgOK.SetLine(3, "");
                            pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
                        }
                        // future things here like telling GUI that dreambox cannot be switched because it is recording
                        Log.Info("DreamboxTV: Cannot zap because box is recording.");
                        return;
                    }
                    else
                        this.ChangeTunerChannel(message.Label);
                    break;
            }
        }

        public void ChangeTunerChannel(string channel_data)
        {
            Log.Info("ExternalDreamboxTV processing external tuner cmd: {0}", channel_data);
            // ZAP
            Zap(channel_data);
        }

        void Zap(string reference)
        {
            try
            {
                if (_DreamboxIP.Length > 0)
                {
                    DreamBox.Core dreambox = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
                    dreambox.Remote.Zap(reference);
                    Log.Info("ExternalDreamboxTV ZAP: {0}\r\n", reference);
                }
                else
                    Log.Info("ExternalDreamboxTV Error: {0}\r\n", "Could not zap because IP address of dreambox not set.");

            }
            catch (Exception x)
            {
                Log.Info("ExternalDreamboxTV Error: {0}\r\n{1}", x.Message, x.StackTrace);
            }
        }

        bool DreamboxIsRecording
        {
            get
            {
                bool bReturn = false;
                try
                {
                    DreamBox.Core core1 = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
                    int recording = core1.XML.Status.recording;
                    switch (recording)
                    {
                        case 0:
                            {
                                bReturn = false;
                                break;
                            }
                        case 1:
                            {
                                bReturn = true;
                                break;
                            }
                    }
                }
                catch
                {

                }
                return bReturn;
            }
        }

        private void LoadSettings()
        {
            Log.Info("DreamboxTV: Loading settings");
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {

                _DreamboxIP = xmlreader.GetValueAsString("Dreambox", "IP", "dreambox");
                _DreamboxUserName = xmlreader.GetValueAsString("Dreambox", "UserName", "root");
                _DreamboxPassword = xmlreader.GetValueAsString("Dreambox", "Password", "dreambox");
                string syncText = xmlreader.GetValueAsString("Dreambox", "SyncHour", "0");
                try
                {
                    _SyncHours = Convert.ToDouble(syncText);
                }
                catch { }

            }
        }

        public void Stop()
        {
            _SyncTimer.Stop();
            Log.Info("External Dreambox TV: plugin stopping.");
            return;
        }


        void ImportEPG()
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
            DataTable table1 = core1.Data.UserTVBouquets.Tables[0];
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                string text1 = table1.Rows[num1]["Ref"].ToString();
                this.ImportEPGChannels(text1);
            }
            Log.Info("DreamboxTV: EPG imported");
        }

        private void ImportEPGChannels(string reference)
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
            DataTable table1 = core1.Data.Channels(reference).Tables[0];
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                string text1 = table1.Rows[num1]["Ref"].ToString();
                try
                {
                    this.ImportChannelEPG(text1);
                }
                catch { }

            }
        }
        private void ImportChannelEPG(string reference)
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
            ServiceEpgData data1 = core1.XML.EPG(reference);
            foreach (EpgEvent event1 in data1.Events)
            {
                int num1 = Convert.ToInt32(event1.Date.Split(new char[] { '.' })[2].ToString());
                int num2 = Convert.ToInt32(event1.Date.Split(new char[] { '.' })[1].ToString());
                int num3 = Convert.ToInt32(event1.Date.Split(new char[] { '.' })[0].ToString());
                int num4 = Convert.ToInt32(event1.Time.Split(new char[] { ':' })[0].ToString());
                int num5 = Convert.ToInt32(event1.Time.Split(new char[] { ':' })[1].ToString());
                DateTime time1 = new DateTime(num1, num2, num3, num4, num5, 0);
                double num6 = Convert.ToDouble(event1.Duration);
                DateTime time2 = time1.AddSeconds(num6);

                long t1 = Convert.ToInt64(time1.ToString("yyyyMMddHHmmss"));
                long t2 = Convert.ToInt64(time1.ToString("yyyyMMddHHmmss"));
                ArrayList programsInDatabase = new ArrayList();
                TVDatabase.GetProgramsPerChannel(data1.ServiceName, t1 + 1, t2 + 1, ref programsInDatabase);
                if (programsInDatabase.Count == 0)
                {
                    TVProgram program1 = new TVProgram(data1.ServiceName, time1, time2, event1.Description);
                    program1.Description = event1.Details;
                    program1.Genre = event1.Genre;
                    program1.Date = time1.ToString();
                    TVDatabase.AddProgram(program1);
                }



            }
            
        }

        #region IPlugin Members



        #endregion

        #region ISetupForm Members

        public bool CanEnable()
        {
            return true;
        }

        public string Description()
        {
            return "Makes it possible to watch dreambox tv as a TV tuner (you need a tv card for it with analog input connector like s-video or composite)";
        }

        public bool DefaultEnabled()
        {
            return false;
        }

        public int GetWindowId()
        {
            // TODO:  Add CallerIdPlugin.GetWindowId implementation
            return -1;
        }


        public string Author()
        {
            return "Gary Wenneker";
        }

        public string PluginName()
        {
            return "External Dreambox TV";
        }

        public bool HasSetup()
        {
            return true;
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            Form setup = new ExternalDreamBoxTVSettings();
            setup.ShowDialog();
        }

        /// <summary>
        /// If the plugin should have its own button on the main menu of MediaPortal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true  : plugin needs its own button on home
        ///          false : plugin does not need its own button on home</returns>
        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = String.Empty;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return false;
        }

        #endregion



    }
}