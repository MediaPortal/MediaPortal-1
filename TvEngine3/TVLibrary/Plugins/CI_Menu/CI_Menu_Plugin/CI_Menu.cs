using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

using TvLibrary.Log;
using TvControl;
using SetupTv;
using TvEngine;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvDatabase;

namespace TvEngine
{

        public class CI_Menu : ITvServerPlugin
        {

            #region Constants
            #endregion Constants

            #region Members
            #endregion Members

            #region Properties

            /// <summary>
            /// returns the name of the plugin
            /// </summary>
            public string Name
            {
                get { return "CI Menu"; }
            }
            /// <summary>
            /// returns the version of the plugin
            /// </summary>
            public string Version
            {
                get { return "0.0.0.1"; }
            }
            /// <summary>
            /// returns the author of the plugin
            /// </summary>
            public string Author
            {
                get { return "morpheus_xx"; }
            }
            /// <summary>
            /// returns if the plugin should only run on the master server
            /// or also on slave servers
            /// </summary>
            public bool MasterOnly
            {
                get { return true; }
            }

            #endregion Properties

            #region IPlugin Members

            [CLSCompliant(false)]
            public void Start(IController controller)
            {
                Log.Info("CI Menu: Start");
            }

            public void Stop()
            {
              Log.Info("CI Menu: Stop");
            }

            [CLSCompliant(false)]
            public SetupTv.SectionSettings Setup
            {
                get { return new CI_Menu_Dialog(); }
            }

            #endregion

            #region Implementation

            #endregion Implementation

        }
}
