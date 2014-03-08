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
        private static int _rtspStreamingPort = 554;
        static UPnPLightServer _server = null;

        private Channel _channel;
        private IVirtualCard _card;

        #endregion

        public SatIpServer()
        {
            _server = new UPnPLightServer(SERVER_ID.ToString("D")); // vorher B
            _server.Start();

            _rtsp = new RtspServer();
            
            // ToDo: move it to proper places - just temprary here
            IUser user = UserFactory.CreateBasicUser("setuptv");
            TvResult result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(user.Name, _channel.IdChannel, out _card, out user);
            ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_card.Id); // device path
        }

        public void stop()
        {
            _rtsp.stop();
        }
    }
}
