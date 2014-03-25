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
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// Abstract base class for SoundGraph iMON display.
    /// SoundGraph iMON VFD and LCD are implementing this abstractiong.
    /// </summary>
    public abstract class SoundGraphImon
    {       
        public SoundGraphImon()
        {
            Line1 = string.Empty;
            Line2 = string.Empty;
        }

        protected string Line1 { get; set; }
        protected string Line2 { get; set; }
        
        //Set text for give line index
        public abstract void SetLine(int line, string message);
        //Display name is notably used during configuration for testing
        public abstract string Name();
        //Launch advanced settings dialog
        public abstract void Configure();

        //Here comes settings related stuff


        [Serializable]
        public abstract class Settings
        {
            //Generic iMON settings
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
        }
    }

}