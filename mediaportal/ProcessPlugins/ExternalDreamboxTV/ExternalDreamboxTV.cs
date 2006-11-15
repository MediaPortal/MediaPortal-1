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


namespace ProcessPlugins.ExternalDreamboxTV
{
    public class ExternalDreamboxTV : IPlugin, ISetupForm
    {
        private string _DreamboxIP = "";
        private string _DreamboxUserName = "";
        private string _DreamboxPassword = "";

        public ExternalDreamboxTV()
        {

        }



        public void Start()
        {
            LoadSettings();
            GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_Receivers);
        }

        void GUIWindowManager_Receivers(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
                    bool bIsInteger;
                    double retNum;
                    bIsInteger = Double.TryParse(message.Label, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
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

        private void LoadSettings()
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {

                _DreamboxIP = xmlreader.GetValueAsString("Dreambox", "IP", "dreambox");
                _DreamboxUserName = xmlreader.GetValueAsString("Dreambox", "UserName", "root");
                _DreamboxPassword = xmlreader.GetValueAsString("Dreambox", "Password", "dreambox");
            }
        }

        public void Stop()
        {
            Log.Info("External Dreambox TV: plugin stopping.");
            return;
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