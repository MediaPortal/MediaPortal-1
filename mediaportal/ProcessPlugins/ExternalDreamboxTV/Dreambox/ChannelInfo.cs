using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DreamBox
{
    public class ChannelInfo
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";
        private string _Command = "/cgi-bin/status";


        public ChannelInfo(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;

            Request request = new Request(_Url, _UserName, _Password);
            string s = request.PostData(_Command);
            InternalParsing(s);
        }

        void InternalParsing(string s)
        {
            Regex objAlphaPattern = new Regex("<tr><td>?.*</td></tr>");
            MatchCollection col = objAlphaPattern.Matches(s);
            for (int i = 0; i < col.Count; i++)
            {
                string val = col[i].ToString().Replace("<tr><td>", "").Replace("</td></tr>", "").Replace("</td></td>", "").Replace("</td><td>", "");
                string[] arr = val.Split(':');

                switch (arr[0])
                {
                    case "WebIf-Version":
                        this.WebIfVersion = arr[1];
                        break;
                    case "Standby":
                        if (arr[1] == "OFF")
                        {
                            this.Standby = false;
                        }
                        else
                        {
                            this.Standby = true;
                        }
                        break;
                    case "Recording":
                        if (arr[1] == "OFF")
                        {
                            this.Standby = false;
                        }
                        else
                        {
                            this.Standby = true;
                        }
                        break;
                    case "Mode":
                        this.Mode = Convert.ToInt32(arr[1]);
                        break;
                    case "Current service reference":
                        this.CurrentServiceReference = arr[1];
                        break;
                    case "name":
                        this.Name = arr[1];
                        break;
                    case "provider":
                        this.Provider = arr[1];
                        break;
                    case "vpid":
                        this.Vpid = arr[1].Replace("<td>", "").Substring(0, arr[1].Replace("<td>", "").IndexOf("("));
                        break;
                    case "apid":
                        this.Apid = arr[1].Replace("<td>", "").Substring(0, arr[1].Replace("<td>", "").IndexOf("("));
                        break;
                    case "pcrpid":
                        this.Pcrpid = arr[1].Replace("<td>", "").Substring(0, arr[1].Replace("<td>", "").IndexOf("("));
                        break;
                    case "tpid":
                        this.Tpid = arr[1].Replace("<td>", "").Substring(0, arr[1].Replace("<td>", "").IndexOf("("));
                        break;
                    case "tsid":
                            this.Tsid = arr[1];
                        break;
                    case "onid":
                        this.Onid = arr[1];
                        break;
                    case "sid":
                        this.Sid = arr[1];
                        break;
                    case "pmt":
                        this.Pmt = arr[1];
                        break;
                    case "vidformat":
                        this.VidFormat = arr[1];
                        this.VidFormat = this.VidFormat.Replace("<td>", "");
                        this.VidFormat = this.VidFormat.Replace("</td>", "");
                        break;
                    default:
                        return;

                }

            }
        }

        #region Properties
        private string _CurrentTime;

        public string CurrentTime
        {
            get { return _CurrentTime; }
            set { _CurrentTime = value; }
        }

        private string _WebIfVersion;

        public string WebIfVersion
        {
            get { return _WebIfVersion; }
            set { _WebIfVersion = value; }
        }
        private bool _Standby;

        public bool Standby
        {
            get { return _Standby; }
            set { _Standby = value; }
        }
        private bool _Recording;

        public bool Recording
        {
            get { return _Recording; }
            set { _Recording = value; }
        }
        private int _Mode;

        public int Mode
        {
            get { return _Mode; }
            set { _Mode = value; }
        }
        private string _CurrentServiceReference;

        public string CurrentServiceReference
        {
            get { return _CurrentServiceReference; }
            set { _CurrentServiceReference = value; }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private string _Provider;

        public string Provider
        {
            get { return _Provider; }
            set { _Provider = value; }
        }
        private string _vpid;

        public string Vpid
        {
            get { return _vpid; }
            set { _vpid = value; }
        }
        private string _apid;

        public string Apid
        {
            get { return _apid; }
            set { _apid = value; }
        }
        private string _pcrpid;

        public string Pcrpid
        {
            get { return _pcrpid; }
            set { _pcrpid = value; }
        }
        private string _tpid;

        public string Tpid
        {
            get { return _tpid; }
            set { _tpid = value; }
        }
        private string _tsid;

        public string Tsid
        {
            get { return _tsid; }
            set { _tsid = value; }
        }
        private string _onid;

        public string Onid
        {
            get { return _onid; }
            set { _onid = value; }
        }
        private string _sid;

        public string Sid
        {
            get { return _sid; }
            set { _sid = value; }
        }
        private string _pmt;

        public string Pmt
        {
            get { return _pmt; }
            set { _pmt = value; }
        }
        private string _vidformat;

        public string VidFormat
        {
            get { return _vidformat; }
            set { _vidformat = value; }
        }
        #endregion
       
	
	
    }
}
