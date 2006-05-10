using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.WebBrowser
{
    class Common
    {
        /// <summary>
        /// Menu state enum
        /// </summary>
        public enum MenuState
        {
            Browser = 0,
            TopBar = 1
        }
        /// <summary>
        /// Gets the user configured home page from the mediaportal settings
        /// </summary>
        public static string HomePage
        {
            get
            {
                using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
                {
                    return xmlreader.GetValueAsString("webbrowser", "homePage", string.Empty);
                }
            }
        }
    }
}
