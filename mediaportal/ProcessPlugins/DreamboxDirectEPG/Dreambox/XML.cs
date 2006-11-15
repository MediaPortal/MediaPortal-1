using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace DreamBox
{
    public class XML
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";
        private string _Command = "/xml/";

        public XML(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
        }


        #region Accessible Interface
        public StreamInfoData StreamInfo
        {
            get
            {
                Request request = new Request(_Url, _UserName, _Password);
                Stream sreturn = request.GetStream(_Command + "streaminfo");
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(sreturn);
                StreamInfoData streamInfo = new StreamInfoData();


                XmlNodeList streaminfo = xDoc.GetElementsByTagName("streaminfo");
                streamInfo.Agc = streaminfo.Item(0)["agc"].InnerText.Trim();
                streamInfo.Apid = streaminfo.Item(0)["apid"].InnerText.Trim();
                streamInfo.Ber = streaminfo.Item(0)["ber"].InnerText.Trim();
                streamInfo.Fec = streaminfo.Item(0)["fec"].InnerText.Trim();
                streamInfo.Frequency = streaminfo.Item(0)["frequency"].InnerText.Trim();
                streamInfo.FrontEnd = streaminfo.Item(0)["frontend"].InnerText.Trim();
                streamInfo.Inversion = streaminfo.Item(0)["inversion"].InnerText.Trim();
                streamInfo.Lock = streaminfo.Item(0)["lock"].InnerText.Trim();
                streamInfo.NameSpace = streaminfo.Item(0)["namespace"].InnerText.Trim();
                streamInfo.OnId = streaminfo.Item(0)["onid"].InnerText.Trim();
                streamInfo.PcrPid = streaminfo.Item(0)["pcrpid"].InnerText.Trim();
                streamInfo.Pmt = streaminfo.Item(0)["pmt"].InnerText.Trim();
                streamInfo.Polarisation = streaminfo.Item(0)["polarisation"].InnerText.Trim();
                streamInfo.Provider = streaminfo.Item(0)["provider"].InnerText.Trim();
                streamInfo.Satellite = streaminfo.Item(0)["satellite"].InnerText.Trim();
                streamInfo.ServiceName = streaminfo.Item(0)["service"].ChildNodes[0].InnerText.Trim();
                streamInfo.ServiceReference = streaminfo.Item(0)["service"].ChildNodes[1].InnerText.Trim();
                streamInfo.Sid = streaminfo.Item(0)["sid"].InnerText.Trim();
                streamInfo.Snr = streaminfo.Item(0)["snr"].InnerText.Trim();
                streamInfo.SupportedCryptSystems = streaminfo.Item(0)["supported_crypt_systems"].InnerText.Trim();
                streamInfo.SymbolRate = streaminfo.Item(0)["symbol_rate"].InnerText.Trim();
                streamInfo.Sync = streaminfo.Item(0)["sync"].InnerText.Trim();
                streamInfo.TPid = streaminfo.Item(0)["tpid"].InnerText.Trim();
                streamInfo.TsId = streaminfo.Item(0)["tsid"].InnerText.Trim();
                streamInfo.UsedCryptSystems = streaminfo.Item(0)["used_crypt_systems"].InnerText.Trim();
                streamInfo.VideoFormat = streaminfo.Item(0)["video_format"].InnerText.Trim();
                streamInfo.Vpid = streaminfo.Item(0)["vpid"].InnerText.Trim();
                return streamInfo;

            }
        }
        public CurrentServiceData CurrentService
        {
            get
            {
                Request request = new Request(_Url, _UserName, _Password);
                Stream sreturn = request.GetStream(_Command + "currentservicedata");
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(sreturn);
                XmlNodeList service = xDoc.GetElementsByTagName("service");
                XmlNodeList audiochannels = xDoc.GetElementsByTagName("audio_channels");
                XmlNodeList audiotrack = xDoc.GetElementsByTagName("audio_track");
                XmlNodeList videochannels = xDoc.GetElementsByTagName("video_channels");
                XmlNodeList currentevent = xDoc.GetElementsByTagName("current_event");
                XmlNodeList nextevent = xDoc.GetElementsByTagName("next_event");

                CurrentServiceData currentServiceData = new CurrentServiceData();

                // service
                currentServiceData.ServiceName = service.Item(0)["name"].InnerText.Trim();
                currentServiceData.ServiceReference = service.Item(0)["reference"].InnerText.Trim();
                currentServiceData.AudioTrack = audiotrack.Item(0).InnerText.Trim();

                // audiochannel
                foreach (System.Xml.XmlElement el in audiochannels.Item(0).ChildNodes)
                {
                    string name = el.GetElementsByTagName("name").Item(0).InnerText.Trim();
                    string selected = el.GetElementsByTagName("selected").Item(0).InnerText.Trim();
                    string pid = el.GetElementsByTagName("pid").Item(0).InnerText.Trim();
                    ServiceDataChannel ac = new ServiceDataChannel();
                    ac.Name = name;
                    if (selected == "1") { ac.Selected = true; } else ac.Selected = false;
                    ac.Pid = pid;
                    currentServiceData.AudioChannels.Add(ac);
                }
                // videochannel
                foreach (System.Xml.XmlElement el in videochannels.Item(0).ChildNodes)
                {
                    string name = el.GetElementsByTagName("name").Item(0).InnerText.Trim();
                    string selected = el.GetElementsByTagName("selected").Item(0).InnerText.Trim();
                    string pid = el.GetElementsByTagName("pid").Item(0).InnerText.Trim();
                    ServiceDataChannel ac = new ServiceDataChannel();
                    ac.Name = name;
                    if (selected == "1") { ac.Selected = true; } else ac.Selected = false;
                    ac.Pid = pid;
                    currentServiceData.VideoChannels.Add(ac);
                }

                // current event
                currentServiceData.CurrentEvent.Date = currentevent.Item(0)["date"].InnerText.Trim();
                currentServiceData.CurrentEvent.Time = currentevent.Item(0)["time"].InnerText.Trim();
                currentServiceData.CurrentEvent.Start = currentevent.Item(0)["start"].InnerText.Trim();
                currentServiceData.CurrentEvent.Duration = currentevent.Item(0)["duration"].InnerText.Trim();
                currentServiceData.CurrentEvent.Description = currentevent.Item(0)["description"].InnerText.Trim();
                currentServiceData.CurrentEvent.Details = currentevent.Item(0)["details"].InnerText.Trim();

                // next event
                currentServiceData.NextEvent.Date = nextevent.Item(0)["date"].InnerText.Trim();
                currentServiceData.NextEvent.Time = nextevent.Item(0)["time"].InnerText.Trim();
                currentServiceData.NextEvent.Start = nextevent.Item(0)["start"].InnerText.Trim();
                currentServiceData.NextEvent.Duration = nextevent.Item(0)["duration"].InnerText.Trim();
                currentServiceData.NextEvent.Description = nextevent.Item(0)["description"].InnerText.Trim();
                currentServiceData.NextEvent.Details = nextevent.Item(0)["details"].InnerText.Trim();

                //return
                return currentServiceData;
            }

        }
        public BoxStatus Status
        {
            get
            {
                Request request = new Request(_Url, _UserName, _Password);
                string sreturn = request.PostData(_Command + "boxstatus").Replace("\n", "");

                BoxStatus boxStatus = new BoxStatus();

                // fill values
                boxStatus.current_time = (string)ParseValue(sreturn, "current_time");
                boxStatus.ip = (string)ParseValue(sreturn, "ip");
                boxStatus.mode = int.Parse(ParseValue(sreturn, "mode"));
                boxStatus.recording = int.Parse(ParseValue(sreturn, "recording"));
                boxStatus.standby = int.Parse(ParseValue(sreturn, "standby"));

                return boxStatus;
            }

        }
        public ServicesData Services
        {
            get
            {
                ServicesData data = new ServicesData();

                Request request = new Request(_Url, _UserName, _Password);
                Stream sreturn = request.GetStream(_Command + "services");
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(sreturn);
                XmlNodeList satellites = xDoc.GetElementsByTagName("satellites");
                //parse
                foreach (System.Xml.XmlElement satellite in satellites.Item(0).ChildNodes)
                {
                    // found a sat
                    Satellite sat = new Satellite();
                    sat.ServiceName = satellite.GetElementsByTagName("name").Item(0).InnerText.Trim();
                    sat.ServiceReference = satellite.GetElementsByTagName("reference").Item(0).InnerText.Trim();

                    foreach (System.Xml.XmlElement service in satellite.GetElementsByTagName("service"))
                    {
                        Service srv = new Service();
                        srv.ServiceName = service.GetElementsByTagName("name").Item(0).InnerText.Trim();
                        srv.ServiceReference = service.GetElementsByTagName("reference").Item(0).InnerText.Trim();
                        srv.OrbitalPosition = service.GetElementsByTagName("orbital_position").Item(0).InnerText.Trim();
                        srv.Provider = service.GetElementsByTagName("provider").Item(0).InnerText.Trim();
                        sat.Services.Add(srv);
                    }
                    data.Satellites.Add(sat);
                }

                return data;
            }
        }
        public ServiceEpgData EPG(string reference)
        {
            if (reference.Length == 0) return null;
            ServiceEpgData data = new ServiceEpgData();
            Request request = new Request(_Url, _UserName, _Password);
            Stream sreturn = request.GetStream(_Command + "serviceepg?ref=" + reference);
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(sreturn);
            XmlNodeList epg = xDoc.GetElementsByTagName("event");
            XmlNodeList service = xDoc.GetElementsByTagName("service");
            //parse

            // found service_epg
            data.ServiceReference = service.Item(0)["reference"].InnerText.Trim();
            data.ServiceName = service.Item(0)["name"].InnerText.Trim();

            foreach (System.Xml.XmlElement epgevent in epg)
            {
                EpgEvent evnt = new EpgEvent();
                evnt.Date = epgevent.GetElementsByTagName("date").Item(0).InnerText.Trim();
                evnt.Description = epgevent.GetElementsByTagName("description").Item(0).InnerText.Trim();
                evnt.Details = epgevent.GetElementsByTagName("details").Item(0).InnerText.Trim();
                evnt.Duration = epgevent.GetElementsByTagName("duration").Item(0).InnerText.Trim();
                evnt.Genre = epgevent.GetElementsByTagName("genre").Item(0).InnerText.Trim();
                evnt.GenreCategory = epgevent.GetElementsByTagName("genrecategory").Item(0).InnerText.Trim();
                evnt.Start = epgevent.GetElementsByTagName("start").Item(0).InnerText.Trim();
                evnt.Time = epgevent.GetElementsByTagName("time").Item(0).InnerText.Trim();
                evnt.ID = int.Parse(epgevent.GetAttribute("id").Trim());

                data.Events.Add(evnt);
            }
            return data;
        }
        #endregion


        string ParseValue(string value, string keyname)
        {
            Regex objAlphaPattern = new Regex(@"<" + keyname + ">?.*</" + keyname + ">", RegexOptions.Multiline);
            MatchCollection col = objAlphaPattern.Matches(value);
            string retval = "";
            for (int i = 0; i < col.Count; i++)
            {
                retval = col[i].ToString();
                retval = retval.Replace(@"<" + keyname + ">", "");
                retval = retval.Replace(@"</" + keyname + ">", "");
 
            }
            return retval;
        }
    }


    #region BoxStatus Class
    public class BoxStatus
    {
        private string _current_time;

        public string current_time
        {
            get { return _current_time; }
            set { _current_time = value; }
        }
        private int _standby;

        public int standby
        {
            get { return _standby; }
            set { _standby = value; }
        }
        private int _recording;

        public int recording
        {
            get { return _recording; }
            set { _recording = value; }
        }
        private int _mode;

        public int mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        private string _ip;

        public string ip
        {
            get { return _ip; }
            set { _ip = value; }
        }



    }
    #endregion

    #region CurrentServiceData class
    public class CurrentServiceData
    {
        #region service
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        private string _serviceReference;

        public string ServiceReference
        {
            get { return _serviceReference; }
            set { _serviceReference = value; }
        }
        #endregion

        #region audio_channels
        private List<ServiceDataChannel> _AudioChannels;

        public List<ServiceDataChannel> AudioChannels
        {
            get { if (_AudioChannels == null) { _AudioChannels = new List<ServiceDataChannel>(); } return _AudioChannels; }
            set { _AudioChannels = value; }
        }

        #endregion
        #region video_channels
        private List<ServiceDataChannel> _VideoChannels;

        public List<ServiceDataChannel> VideoChannels
        {
            get { if (_VideoChannels == null) { _VideoChannels = new List<ServiceDataChannel>(); } return _VideoChannels; }
            set { _VideoChannels = value; }
        }

        #endregion
        #region current_event
        private ServiceDataEvent _CurrentEvent;

        public ServiceDataEvent CurrentEvent
        {
            get { if (_CurrentEvent == null) { _CurrentEvent = new ServiceDataEvent(); } return _CurrentEvent; }
            set { _CurrentEvent = value; }
        }

        #endregion
        #region next_event
        private ServiceDataEvent _NextEvent;

        public ServiceDataEvent NextEvent
        {
            get { if (_NextEvent == null) { _NextEvent = new ServiceDataEvent(); } return _NextEvent; }
            set { _NextEvent = value; }
        }
        #endregion

        // Audio track
        private string _audioTrack;

        public string AudioTrack
        {
            get { return _audioTrack; }
            set { _audioTrack = value; }
        }


    }
    public class ServiceDataChannel
    {
        private string _Pid;

        public string Pid
        {
            get { return _Pid; }
            set { _Pid = value; }
        }
        private bool _Selected;

        public bool Selected
        {
            get { return _Selected; }
            set { _Selected = value; }
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }



    }
    public class ServiceDataEvent
    {
        private string _Date;

        public string Date
        {
            get { return _Date; }
            set { _Date = value; }
        }
        private string _Time;

        public string Time
        {
            get { return _Time; }
            set { _Time = value; }
        }
        private string _Start;

        public string Start
        {
            get { return _Start; }
            set { _Start = value; }
        }
        private string _Duration;

        public string Duration
        {
            get { return _Duration; }
            set { _Duration = value; }
        }
        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        private string _Details;

        public string Details
        {
            get { return _Details; }
            set { _Details = value; }
        }


    }
    #endregion

    #region StreamInfoData Class
    public class StreamInfoData
    {
        private string _FrontEnd;

        public string FrontEnd
        {
            get { return _FrontEnd; }
            set { _FrontEnd = value; }
        }

        #region service
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        private string _serviceReference;

        public string ServiceReference
        {
            get { return _serviceReference; }
            set { _serviceReference = value; }
        }
        #endregion

        private string _Provider;

        public string Provider
        {
            get { return _Provider; }
            set { _Provider = value; }
        }

        private string _Vpid;

        public string Vpid
        {
            get { return _Vpid; }
            set { _Vpid = value; }
        }
        private string _Apid;

        public string Apid
        {
            get { return _Apid; }
            set { _Apid = value; }
        }

        private string _PcrPid;

        public string PcrPid
        {
            get { return _PcrPid; }
            set { _PcrPid = value; }
        }

        private string _Tpid;

        public string TPid
        {
            get { return _Tpid; }
            set { _Tpid = value; }
        }

        private string _TsId;

        public string TsId
        {
            get { return _TsId; }
            set { _TsId = value; }
        }

        private string _OnId;

        public string OnId
        {
            get { return _OnId; }
            set { _OnId = value; }
        }

        private string _Sid;

        public string Sid
        {
            get { return _Sid; }
            set { _Sid = value; }
        }

        private string _Pmt;

        public string Pmt
        {
            get { return _Pmt; }
            set { _Pmt = value; }
        }

        private string _VideoFormat;

        public string VideoFormat
        {
            get { return _VideoFormat; }
            set { _VideoFormat = value; }
        }

        private string _NameSpace;

        public string NameSpace
        {
            get { return _NameSpace; }
            set { _NameSpace = value; }
        }

        private string _SupportedCryptSystems;

        public string SupportedCryptSystems
        {
            get { return _SupportedCryptSystems; }
            set { _SupportedCryptSystems = value; }
        }

        private string _UsedCryptSystems;

        public string UsedCryptSystems
        {
            get { return _UsedCryptSystems; }
            set { _UsedCryptSystems = value; }
        }

        private string _Satellite;

        public string Satellite
        {
            get { return _Satellite; }
            set { _Satellite = value; }
        }

        private string _Frequency;

        public string Frequency
        {
            get { return _Frequency; }
            set { _Frequency = value; }
        }

        private string _SymbolRate;

        public string SymbolRate
        {
            get { return _SymbolRate; }
            set { _SymbolRate = value; }
        }

        private string _Polarisation;

        public string Polarisation
        {
            get { return _Polarisation; }
            set { _Polarisation = value; }
        }
        private string _Inversion;

        public string Inversion
        {
            get { return _Inversion; }
            set { _Inversion = value; }
        }

        private string _Fec;

        public string Fec
        {
            get { return _Fec; }
            set { _Fec = value; }
        }

        private string _Snr;

        public string Snr
        {
            get { return _Snr; }
            set { _Snr = value; }
        }

        private string _Agc;

        public string Agc
        {
            get { return _Agc; }
            set { _Agc = value; }
        }

        private string _Ber;

        public string Ber
        {
            get { return _Ber; }
            set { _Ber = value; }
        }

        private string _Lock;

        public string Lock
        {
            get { return _Lock; }
            set { _Lock = value; }
        }

        private string _Sync;

        public string Sync
        {
            get { return _Sync; }
            set { _Sync = value; }
        }


    }
    #endregion

    #region Services Class
    public class Service
    {
        #region service
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        private string _serviceReference;

        public string ServiceReference
        {
            get { return _serviceReference; }
            set { _serviceReference = value; }
        }
        #endregion
        private string _Provider;

        public string Provider
        {
            get { return _Provider; }
            set { _Provider = value; }
        }
        private string _OrbitalPosition;

        public string OrbitalPosition
        {
            get { return _OrbitalPosition; }
            set { _OrbitalPosition = value; }
        }


    }
    public class ServicesData
    {
        private List<Satellite> _Satellites;

        public List<Satellite> Satellites
        {
            get { if (_Satellites == null) { _Satellites = new List<Satellite>(); } return _Satellites; }
            set { _Satellites = value; }
        }
    }
    public class Satellite
    {
        #region service
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        private string _serviceReference;

        public string ServiceReference
        {
            get { return _serviceReference; }
            set { _serviceReference = value; }
        }
        #endregion
        private List<Service> _Services;

        public List<Service> Services
        {
            get { if (_Services == null) { _Services = new List<Service>(); } return _Services; }
            set { _Services = value; }
        }

    }
    #endregion

    #region ServiceEpgData Class
    public class ServiceEpgData
    {
        #region service
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        private string _serviceReference;

        public string ServiceReference
        {
            get { return _serviceReference; }
            set { _serviceReference = value; }
        }
        #endregion
        private List<EpgEvent> _Events;
        public List<EpgEvent> Events
        {
            get { if (_Events == null) { _Events = new List<EpgEvent>(); } return _Events; }
            set { _Events = value; }
        }
    }
    public class EpgEvent
    {
        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private string _Date;

        public string Date
        {
            get { return _Date; }
            set { _Date = value; }
        }
        private string _Time;

        public string Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        private string _Duration;

        public string Duration
        {
            get { return _Duration; }
            set { _Duration = value; }
        }
        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        private string _Genre;

        public string Genre
        {
            get { return _Genre; }
            set { _Genre = value; }
        }
        private string _GenreCategory;

        public string GenreCategory
        {
            get { return _GenreCategory; }
            set { _GenreCategory = value; }
        }
        private string _Start;

        public string Start
        {
            get { return _Start; }
            set { _Start = value; }
        }
        private string _Details;

        public string Details
        {
            get { return _Details; }
            set { _Details = value; }
        }



    }
    #endregion

    #region TimerData Class
    public class TimerData
    {
        private List<Timer> _Timers;
        public List<Timer> Timers
        {
            get { if (_Timers == null) { _Timers = new List<Timer>(); } return _Timers; }
            set { _Timers = value; }
        }
    }
    public class Timer
    {
        private string _Type;

        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        private string _Days;

        public string Days
        {
            get { return _Days; }
            set { _Days = value; }
        }
        private string _Action;

        public string Action
        {
            get { return _Action; }
            set { _Action = value; }
        }
        private string _PostAction;

        public string PostAction
        {
            get { return _PostAction; }
            set { _PostAction = value; }
        }
        private string _Status;

        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        private string _TypeData;

        public string TypeData
        {
            get { return _TypeData; }
            set { _TypeData = value; }
        }

        #region service
        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        private string _serviceReference;

        public string ServiceReference
        {
            get { return _serviceReference; }
            set { _serviceReference = value; }
        }
        #endregion


    }
    #endregion

}
