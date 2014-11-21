#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

#region Usings

using System.Reflection;
using SetupTv;
using TvControl;
using TvLibrary.Interfaces;
using TvEngine.Events;
using TvLibrary.Log;
using TvLibrary.Channels;

#endregion

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    public enum ProviderType
    {
        SqlServer,
        MySql
    }

    public class Plugin : ITvServerPlugin
    {
        #region Variables
        #endregion

        #region Constructor

        public Plugin()
        {
        }

        #endregion

        #region ITvServerPlugin implementation

        public void Start(IController controller)
        {
        }

        public void Stop()
        {
        }


        public string Author
        {
            get { return "georgius"; }
        }

        public bool MasterOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Name of this plugin
        /// </summary>
        public string Name
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyProductAttribute attribute = attributes[0] as AssemblyProductAttribute;
                    if (attribute != null && attribute.Product != "MediaPortal")
                        return attribute.Product;
                }
                return "MediaPortal IPTV filter and url source splitter";
            }
        }

        public SectionSettings Setup
        {
            get { return new Editor(); }
        }

        /// <summary>
        /// Plugin version
        /// </summary>
        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        #endregion

        #region Methods

        #endregion
    }
}