using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.WebBrowser
{
    public sealed partial class WebBrowserControl : UserControl
    {
        static readonly WebBrowserControl instance = new WebBrowserControl();
        private const int TOP = 55;
        private const int LEFT = 4;
        private const int HEIGHT = 493;
        private Common.MenuState _CurrentMenuState;

        /// <summary>
        /// Enumeration for refresh constants
        /// </summary>
        public enum RefreshConstants
        {
            REFRESH_NORMAL = 0,
            REFRESH_IFEXPIRED = 1,
            REFRESH_CONTINUE = 2,
            REFRESH_COMPLETELY = 3
        }

        #region Constructor
            /// <summary>
            /// WebBrowserControl contstructor
            /// </summary>
            private WebBrowserControl()
            {
                try
                {

                    InitializeComponent();
                    this.Top = 55;
                    this.Left = 4;
                    if (Common.HomePage.Length != 0)
                        Browser.Navigate(Common.HomePage);
                }
                catch
                {
                    Log.WriteFile(Log.LogType.Error, true, "Unable to load the web browser plugin, verify that Mozilla ActiveX Control is installed");
                }
            }
        #endregion
        
        #region Properties
        /// <summary>
        /// Gets the Mozilla web browser control
        /// </summary>
        public AxMOZILLACONTROLLib.AxMozillaBrowser Browser
        {
            get { return axMozillaBrowser1; }
        }
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static WebBrowserControl Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        #region Private Methods
            /// <summary>
            /// Rescales & Reszies the browser and control when MediaPortal changes its size
            /// </summary>
            private void ResizeBrowser()
            {
                //rescale & resize control
                int left = LEFT;
                int top = TOP;
                int right = this.Right;
                int bottom = this.Bottom;

                GUIGraphicsContext.ScaleRectToScreenResolution(ref left, ref top, ref right, ref bottom);
                this.Left = left;
                this.Top = top;
                ScaleVertical();
                ScaleHorizontal();
            }

            /// <summary>
            /// Scale y position for current resolution
            /// </summary>
            private void ScaleVertical()
            {
                float fSkinHeight = (float)GUIGraphicsContext.SkinSize.Height; ;
                float fPercentY = ((float)GUIGraphicsContext.Height) / fSkinHeight;
                this.Height = (int)Math.Round(((float)HEIGHT) * fPercentY);
                this.Browser.Height = this.Height;
            }

            /// <summary>
            /// Scale y position for current resolution
            /// </summary>
            private void ScaleHorizontal()
            {
                this.Width = GUIGraphicsContext.Width - 4;
                this.Browser.Width = this.Width;
            }

            /// <summary>
            /// Handles the Layout event of the WebBrowserControl control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.LayoutEventArgs"/> instance containing the event data.</param>
            private void WebBrowserControl_Layout(object sender, LayoutEventArgs e)
            {
                ResizeBrowser();
            }
        #endregion

        #region Public Methods
            /// <summary>
            /// Toggles the menu between the web browser and the web browser topbar.
            /// </summary>
            public void ToggleMenu()
            {
                if (_CurrentMenuState == Common.MenuState.Browser)
                {
                    this.Enabled = false;
                    GUIGraphicsContext.form.Focus();
                    GUIControl.FocusControl(GUIWebBrowser.WINDOW_WEB_BROWSER, (int)GUIWebBrowser.Controls.UrlButton);
                    _CurrentMenuState = Common.MenuState.TopBar;
                }
                else
                {
                    this.Enabled = true;
                    _CurrentMenuState = Common.MenuState.Browser;
                }
            }

            /// <summary>
            /// Refreshes the browser page.
            /// </summary>
            public void RefreshBrowser()
            {
                object refreshType = RefreshConstants.REFRESH_COMPLETELY;
                this.Browser.Refresh2(ref refreshType);
            }

            /// <summary>
            /// Navigates to a url
            /// </summary>
            /// <param name="url">url</param>
            public static void OpenUrl(string url)
            {
                //impliment method to navigate to a url from other controls.
            }
        #endregion

        #region Protected Methods
            /// <summary>
            /// Caputures key presses at a lower level than the windows form keypress event.
            /// </summary>
            /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the window message to process.</param>
            /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
            /// <returns>
            /// 	<see langword="true"/> if the character was processed by
            /// the control; otherwise, <see langword="false"/> .
            /// </returns>
            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                const int WM_KEYDOWN = 0x100;
                const int WM_SYSKEYDOWN = 0x104;
                bool handled = false;

                if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
                {
                    switch (keyData)
                    {
                        case Keys.F9: //Toggles Menu
                            ToggleMenu();
                            handled = true;
                            break;
                        case Keys.F3: //Favorites
                            GUIWindowManager.ActivateWindow(GUIFavorites.WINDOW_FAVORITES); 
                            handled = true;
                            break;
                        case Keys.Escape: //Return to previous screen.
                            GUIWindowManager.ShowPreviousWindow();
                            handled = true;
                            break;
                    }
                }

                if (!handled)
                    handled = base.ProcessCmdKey(ref msg, keyData);

                return handled;
            }
        #endregion
          
        }
}
