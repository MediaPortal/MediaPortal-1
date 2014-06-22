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

using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.SatIp.Server;
using Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp;

namespace Mediaportal.TV.Server.TVLibrary.SatIp
{
    class SatIpServer : System.Object
    {

        #region variables

        public const string CONNECTION_MANAGER_SERVICE_ID = "upnp-org:serviceId:ConnectionManager";
        public const string CONNECTION_MANAGER_SERVICE_TYPE = "schemas-upnp-org:service:ConnectionManager";
        public const int CONNECTION_MANAGER_SERVICE_TYPE_VERSION = 1;
        public const string CONTENT_DIRECTORY_SERVICE_ID = "upnp-org:serviceId:ContentDirectory";
        public const string CONTENT_DIRECTORY_SERVICE_TYPE = "schemas-upnp-org:service:ContentDirectory";
        public const int CONTENT_DIRECTORY_SERVICE_TYPE_VERSION = 1;
        public const string DEVICE_UUID = "db015228-8813-11e3-a86e-d231feb1dc81";
        public const string MEDIASERVER_DEVICE_TYPE = "urn:ses-com:device:SatIPServer";
        public const int MEDIASERVER_DEVICE_VERSION = 1;
        RtspServer _rtsp;
        static readonly Guid SERVER_ID = new Guid("{db015228-8813-11e3-a86e-d231feb1dc81}");
        static UPnPLightServer _server = null;

        #endregion

        public SatIpServer()
        {
            this.LogInfo("SAT>IP: Start Server");
            _server = new UPnPLightServer(SERVER_ID.ToString("D")); // vorher B
            _server.Start();

            _rtsp = new RtspServer();
        }

        public void stop()
        {
            _rtsp.stop();
        }
    }
}
