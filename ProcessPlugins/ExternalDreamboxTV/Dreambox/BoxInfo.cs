#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DreamBox
{
    public class BoxInfo
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";
        private string _Command = "/data";

        public BoxInfo(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
            Request request = new Request(_Url, _UserName, _Password);
            string s = request.PostData(_Command);
            InternalParsing(s);
        }

        public void Refresh()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string s = request.PostData(_Command);
            InternalParsing(s);
        }
        void InternalParsing(string s)
        {
            Regex objAlphaPattern = new Regex("var ?.*\n");
            MatchCollection col = objAlphaPattern.Matches(s);
            for (int i = 0; i < col.Count; i++)
            {
                string val = col[i].ToString().Replace("var ","").Replace("\n","");
                if (val.Contains("=") && val.Contains(";"))
                {
                    val = val.Replace(";", "").Replace(" = ", "=");
                    Console.WriteLine("[" + val + "]");
                    string[] arr = val.Split('=');

                    string name = arr[0];
                    string value = arr[1].Replace("\"", "") ;
                    switch (name)
                    {
                        case "updateCycleTime":
                            this.updateCycleTime = int.Parse(value.ToString());
                            break;
                        case "standby":
                            if (value.ToString() == "0")
                            {
                                this.Standby = false;
                            }
                            else this.Standby = true;
                            break;
                        case "serviceName":
                            this.ServiceName = value.ToString();
                            break;
                        case "nowT":
                            this.NowT = value.ToString();
                            break;
                        case "nowD":
                            this.NowD = value.ToString();
                            break;
                        case "nowSt":
                            this.NowSt = value.ToString();
                            break;
                        case "nextT":
                            this.NextT = value.ToString();
                            break;
                        case "nextD":
                            this.NextD = value.ToString();
                            break;
                        case "nextSt":
                            this.NextSt = value.ToString();
                            break;
                        case "diskGB":
                            this.DiskGB = value.ToString();
                            break;
                        case "diskH":
                            this.DiskH = value.ToString();
                            break;
                        case "apid":
                            this.Apid = value.ToString();
                            break;
                        case "vpid":
                            this.Vpid = value.ToString();
                            break;
                        case "ip":
                            this.IP = value.ToString();
                            break;
                        case "lock":
                            this.Lock = value.ToString();
                            break;
                        case "upTime":
                            this.UpTime = value.ToString();
                            break;
                        case "volume":
                            this.Volume = int.Parse(value.ToString());
                            break;
                        case "mute":
                            if (value.ToString() == "0")
                            {
                                this.Mute = false;
                            }
                            else this.Mute = true;
                            break;
                        case "dolby":
                            if (value.ToString() == "0")
                            {
                                this.Dolby = false;
                            }
                            else this.Dolby = true;
                            break;
                        case "crypt":
                            if (value.ToString() == "0")
                            {
                                this.Crypt = false;
                            }
                            else this.Crypt = true;
                            break;
                        case "format":
                            if (value.ToString() == "0")
                            {
                                this.Format = false;
                            }
                            else this.Format = true;
                            break;
                        case "recording":
                            if (value.ToString() == "0")
                            {
                                this.Recording = false;
                            }
                            else this.Recording = true;
                            break;
                        case "vlcparms":
                            this.VlcParms = value.ToString();
                            break;
                        case "serviceReference":
                            this.ServiceReference = value.ToString();
                            break;
                        case "videoTime":
                            this.VideoTime = value.ToString();
                            break;
                        case "videoPosition":
                            this.VideoPosition = int.Parse(value.ToString());
                            break;
                        case "agc":
                            this.AGC = int.Parse(value.ToString());
                            break;
                        case "snr":
                            this.SNR = int.Parse(value.ToString());
                            break;
                        case "ber":
                            this.BER = int.Parse(value.ToString());
                            break;
                        case "streamingClientStatus":
                            this.StreamingClientStatus = value.ToString();
                            break;
                        default:
                            return;

                    }

                }
                

            }
        }

        #region Properties
        private int _updateCycleTime;

        public int updateCycleTime
        {
            get { return _updateCycleTime; }
            set { _updateCycleTime = value; }
        }
        private bool _Standby;

        public bool Standby
        {
            get { return _Standby; }
            set { _Standby = value; }
        }
        private string _ServiceName;

        public string ServiceName
        {
            get { return _ServiceName; }
            set { _ServiceName = value; }
        }
        private string _NowT;

        public string NowT
        {
            get { return _NowT; }
            set { _NowT = value; }
        }
        private string _NowD;

        public string NowD
        {
            get { return _NowD; }
            set { _NowD = value; }
        }
        private string _NowSt;

        public string NowSt
        {
            get { return _NowSt; }
            set { _NowSt = value; }
        }
        private string _NextT;

        public string NextT
        {
            get { return _NextT; }
            set { _NextT = value; }
        }
        private string _NextD;

        public string NextD
        {
            get { return _NextD; }
            set { _NextD = value; }
        }
        private string _NextSt;

        public string NextSt
        {
            get { return _NextSt; }
            set { _NextSt = value; }
        }
        private string _DiskGB;

        public string DiskGB
        {
            get { return _DiskGB; }
            set { _DiskGB = value; }
        }
        private string _DiskH;

        public string DiskH
        {
            get { return _DiskH; }
            set { _DiskH = value; }
        }
        private string _Apid;

        public string Apid
        {
            get { return _Apid; }
            set { _Apid = value; }
        }
        private string _Vpid;

        public string Vpid
        {
            get { return _Vpid; }
            set { _Vpid = value; }
        }
        private string _IP;

        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }
        private string _Lock;

        public string Lock
        {
            get { return _Lock; }
            set { _Lock = value; }
        }
        private string _UpTime;

        public string UpTime
        {
            get { return _UpTime; }
            set { _UpTime = value; }
        }
        private int _Volume;

        public int Volume
        {
            get { return _Volume; }
            set { _Volume = value; }
        }
        private bool _Mute;

        public bool Mute
        {
            get { return _Mute; }
            set { _Mute = value; }
        }
        private bool _Dolby;

        public bool Dolby
        {
            get { return _Dolby; }
            set { _Dolby = value; }
        }
        private bool _Crypt;

        public bool Crypt
        {
            get { return _Crypt; }
            set { _Crypt = value; }
        }
        private bool _format;

        public bool Format
        {
            get { return _format; }
            set { _format = value; }
        }
        private bool _Recording;

        public bool Recording
        {
            get { return _Recording; }
            set { _Recording = value; }
        }
        private string _VlcParms;

        public string VlcParms
        {
            get { return _VlcParms; }
            set { _VlcParms = value; }
        }
        private string _ServiceReference;

        public string ServiceReference
        {
            get { return _ServiceReference; }
            set { _ServiceReference = value; }
        }
        private string _VideoTime;

        public string VideoTime
        {
            get { return _VideoTime; }
            set { _VideoTime = value; }
        }
        private int _VideoPosition;

        public int VideoPosition
        {
            get { return _VideoPosition; }
            set { _VideoPosition = value; }
        }
        private int _AGC;

        public int AGC
        {
            get { return _AGC; }
            set { _AGC = value; }
        }
        private int _SNR;

        public int SNR
        {
            get { return _SNR; }
            set { _SNR = value; }
        }
        private int _BER;

        public int BER
        {
            get { return _BER; }
            set { _BER = value; }
        }
        private string _StreamingClientStatus;

        public string StreamingClientStatus
        {
            get { return _StreamingClientStatus; }
            set { _StreamingClientStatus = value; }
        }
	
	
	
        #endregion
    }
}
