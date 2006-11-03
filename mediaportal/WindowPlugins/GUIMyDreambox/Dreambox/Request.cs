using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace DreamBox
{
    public class Request
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";

        public Request(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
        }

        public Stream GetStream(string command)
        {
            Uri uri = new Uri(_Url + command);
            WebRequest request = WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(_UserName, _Password);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            return stream;
        }

        public string PostData(string command)
        {
            Uri uri = new Uri(_Url + command);
            WebRequest request = WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(_UserName, _Password);

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            string str = reader.ReadToEnd();
            return str;
        }
    }
}
