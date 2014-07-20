#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;

using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.TVEUPnPServer.Server;
using Mediaportal.TV.Server.TVLibrary.TVEUPnPServer.Rtsp;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.ResourceAccess;
using HttpServer;

namespace Mediaportal.TV.Server.TVLibrary.TVEUPnPServer
{
    class MPUPnPServer : System.Object
    {

        #region variables

        public const string CONNECTION_MANAGER_SERVICE_ID = "upnp-org:serviceId:ConnectionManager";
        public const string CONNECTION_MANAGER_SERVICE_TYPE = "schemas-upnp-org:service:ConnectionManager";
        public const int CONNECTION_MANAGER_SERVICE_TYPE_VERSION = 1;
        public const string CONTENT_DIRECTORY_SERVICE_ID = "upnp-org:serviceId:ContentDirectory";
        public const string CONTENT_DIRECTORY_SERVICE_TYPE = "schemas-upnp-org:service:ContentDirectory";
        public const int CONTENT_DIRECTORY_SERVICE_TYPE_VERSION = 1;
        public const string MEDIASERVER_DEVICE_TYPE = "urn:ses-com:device:SatIPServer";
        public const int MEDIASERVER_DEVICE_VERSION = 1;
        public const int RESOURCE_SERVER_PORT = 2014; // if 0 the system will choose a port automatically

        RtspServer _rtsp;
        static UPnPLightServer _server = null;
        static HttpServer.HttpServer _resourceServer = null;
        private Thread UPnPLightServerThread;

        #endregion

        public MPUPnPServer()
        {
            this.LogInfo("SAT>IP: Start Server");
            // starting all the UPnP stuff needs quite some time (~19s!!), so do it in it's own thread
            UPnPLightServerThread = new Thread(UPnPLightServerStartThread);
            UPnPLightServerThread.Start();
            
            _rtsp = new RtspServer();

            _resourceServer = new HttpServer.HttpServer();
            _resourceServer.ServerName = "MP UPnP resource provider";
            _resourceServer.Add(new UPnPStaticResourceAccessModule());
            _resourceServer.Add(new UPnPRecordingResourceAccessModule());
            _resourceServer.Start(IPAddress.Parse(UPnPResourceAccessUtils.LocalIPAddress()), RESOURCE_SERVER_PORT);
        }

        private void UPnPLightServerStartThread()
        {
          Stopwatch stopwatch = new Stopwatch();
          stopwatch.Start();
          _server = new UPnPLightServer();
          _server.Start();
          stopwatch.Stop();
          this.LogDebug("Starting the UPnP server needed {0} [{1} ms]", stopwatch.Elapsed, stopwatch.ElapsedMilliseconds);
        }


        public void stop()
        {
            _rtsp.stop();
            _server.Stop();
            _resourceServer.Stop();
        }
    }
}
