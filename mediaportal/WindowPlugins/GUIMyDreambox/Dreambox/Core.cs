using System;
using System.Collections.Generic;
using System.Text;



namespace DreamBox
{
    public class Core
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";

        public DreamBox.Data Data = null;
        public DreamBox.Remote Remote = null;
        private DreamBox.BoxInfo _BoxInfo = null;

        public Core(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;

            this.Data = new Data(url, username, password);
            this.Remote = new Remote(url, username, password);
            this._BoxInfo = new BoxInfo(url, username, password);
        }

        public DreamBox.BoxInfo BoxInfo
        {
            get
            {
                if (_BoxInfo == null)
                {
                    this._BoxInfo = new BoxInfo(_Url, _UserName, _Password);
                }
                else
                {
                    this._BoxInfo.Refresh();
                }
                return this._BoxInfo;
            }
        }


        public string Url
        {
            get { return _Url; }
            set { _Url = value; }
        }

        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }
	
	
        private string _SelectedBouquet;
        public string SelectedBouquet
        {
            get { return _SelectedBouquet; }
            set { _SelectedBouquet = value; }
        }
        private string _SelectedChannel;

        public string SelectedChannel
        {
            get { return _SelectedChannel; }
            set { _SelectedChannel = value; }
        }
	
	
        public ChannelInfo CurrentChannel
        {
            get {
                return new ChannelInfo(_Url, _UserName, _Password); 
            }
        }
        public Remote RemoteControl
        {
            get
            {
                return new Remote(_Url, _UserName, _Password);
            }
        }

	


    }
}
