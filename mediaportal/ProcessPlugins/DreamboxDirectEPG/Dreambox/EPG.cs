using System;
using System.Collections.Generic;
using System.Text;


namespace DreamBox
{
    public class EPG
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";
        private string _Command = "/cgi-bin/";
        private DreamBox.Data Data = null;

        public EPG(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
            this.Data = new Data(url, username, password);
        }

        public System.Data.DataSet CurrentEPG
        {
            get
            {
                return this.Data.CurrentEPG;
            }
        }
    }
}
