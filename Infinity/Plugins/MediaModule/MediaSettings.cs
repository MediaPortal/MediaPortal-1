using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Settings;
using MediaLibrary;

namespace MediaModule
{

    class MediaSettings
    {
        string _Section;

        public MediaSettings()
        {
        }

        [Setting(SettingScope.User, "")]
        public string Section
        {
            get
            {
                return _Section;
            }
            set
            {
                _Section = value;
            }
        }
    }
}

