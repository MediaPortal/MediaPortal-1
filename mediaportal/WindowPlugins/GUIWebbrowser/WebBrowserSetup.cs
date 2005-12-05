using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.WebBrowser
{
    public partial class WebBrowserSetup : Form, ISetupForm
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WebBrowserSetup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Page Load Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowserSetup_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        /// <summary>
        /// Allows the user to select where internet favorites are stored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickFavoritesFolder_Click(object sender, EventArgs e)
        {
            using (folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the folder where Internet favorites are stored";
                folderBrowserDialog.ShowNewFolderButton = true;
                folderBrowserDialog.SelectedPath = FavoritesFolder.Text;
                DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

                if (dialogResult == DialogResult.OK)
                {
                    FavoritesFolder.Text = folderBrowserDialog.SelectedPath;
                }
            }	
        }

        #region ISetupForm Members

        /// <summary>
        /// Determines whether this instance can enable.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can enable; otherwise, <c>false</c>.
        /// </returns>
        public bool CanEnable()
        {
            return true;
        }

        /// <summary>
        /// Description of this plugin.
        /// </summary>
        /// <returns></returns>
        public string Description()
        {
            return "A web browser plugin based on the Mozilla ActiveX control";
        }

        /// <summary>
        /// Defaults the enabled.
        /// </summary>
        /// <returns></returns>
        public bool DefaultEnabled()
        {
            return true;
        }

        /// <summary>
        /// Gets the window id.
        /// </summary>
        /// <returns></returns>
        public int GetWindowId()
        {
            return 5500;
        }

        /// <summary>
        /// Gets the home.
        /// </summary>
        /// <param name="strButtonText">The STR button text.</param>
        /// <param name="strButtonImage">The STR button image.</param>
        /// <param name="strButtonImageFocus">The STR button image focus.</param>
        /// <param name="strPictureImage">The STR picture image.</param>
        /// <returns></returns>
        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = GUILocalizeStrings.Get(4000);
            strButtonImage = "";
            strButtonImageFocus = "";
            strPictureImage = "hover_web browser.png";
            return true;
        }

        /// <summary>
        /// Authors of this plugin
        /// </summary>
        /// <returns></returns>
        public string Author()
        {
            return "Devo";
        }

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        /// <returns></returns>
        public string PluginName()
        {
            return GUILocalizeStrings.Get(4000);
        }

        /// <summary>
        /// Determines whether this instance has setup.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance has setup; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// Shows the plugin.
        /// </summary>
        public void ShowPlugin()
        {
            ShowDialog();
        }

        #endregion

        /// <summary>
        /// Cancels any changes on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Saves the selected settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Click(object sender, EventArgs e)
        {
            //save settings
            SaveSettings();
            this.Close();
        }

        /// <summary>
        /// Saves my alarm settings to the profile xml.
        /// </summary>
        private void SaveSettings()
        {
            using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
            {
                xmlwriter.SetValue("webbrowser", "favoritesFolder", FavoritesFolder.Text);
                xmlwriter.SetValue("webbrowser", "homePage", HomePage.Text);
            }
        }

        /// <summary>
        /// Loads my alarm settings from the profile xml.
        /// </summary>
        private void LoadSettings()
        {
            using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
            {
                FavoritesFolder.Text = xmlreader.GetValueAsString("webbrowser", "favoritesFolder", string.Empty);
                HomePage.Text = xmlreader.GetValueAsString("webbrowser", "homePage", string.Empty);
            }
        }

    }
}