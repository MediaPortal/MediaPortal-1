using System;
using System.Collections.Generic;
using System.Text;

namespace DreamBox
{
    public class Zap
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";
        private string _Command = "/cgi-bin/rc?";
        private string _Zap = "/cgi-bin/zapTo?mode=zap&path=";

        const string BUTTON_1 = "2";
        const string BUTTON_2 = "3";
        const string BUTTON_3 = "4";
        const string BUTTON_4 = "5";
        const string BUTTON_5 = "6";
        const string BUTTON_6 = "7";
        const string BUTTON_7 = "8";
        const string BUTTON_8 = "9";
        const string BUTTON_9 = "10";
        const string BUTTON_0 = "11";
        const string BUTTON_PREVIOUS = "412";
        const string BUTTON_NEXT = "407";

        const string BUTTON_MUTE = "113";
        const string BUTTON_LAME = "1";

        const string BUTTON_VOLUME_UP = "115";
        const string BUTTON_VOLUME_DOWN = "114";
        const string BUTTON_BOUQUET_UP = "402";
        const string BUTTON_BOUQUET_DOWN = "403";
        const string BUTTON_INFO = "358";
        const string BUTTON_MENU = "141";
        const string BUTTON_AUDIO = "392";
        const string BUTTON_VIDEO = "393";
        const string BUTTON_OK = "352";

        const string BUTTON_UP = "103";
        const string BUTTON_DOWN = "108";
        const string BUTTON_LEFT = "105";
        const string BUTTON_RIGHT = "106";

        const string BUTTON_RED = "398";
        const string BUTTON_GREEN = "399";
        const string BUTTON_YELLOW = "400";
        const string BUTTON_BLUE = "401";

        const string BUTTON_TV = "385";
        const string BUTTON_RADIO = "377";
        const string BUTTON_TEXT = "66";
        const string BUTTON_HELP = "138";

        const string BUTTON_REWIND = "168";
        const string BUTTON_PLAY = "207";
        const string BUTTON_PAUSE = "119";
        const string BUTTON_FORWARD = "208";
        const string BUTTON_STOP = "128";
        const string BUTTON_RECORD = "167";

        public Zap(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
        }

        #region Video Keys
        public void Rewind()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_REWIND);
        }
        public void Play()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_PLAY);
        }
        public void Pause()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_PAUSE);
        }
        public void Forward()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_FORWARD);
        }
        public void Stop()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_STOP);
        }
        public void Record()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_RECORD);
        }
        #endregion

        public void Up()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_UP);
        }
        public void Down()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_DOWN);
        }
        public void Info()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_INFO);
        }
        public void Menu()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_MENU);
        }
        public void Audio()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_AUDIO);
        }
        public void Video()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_VIDEO);
        }
        public void Right()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_RIGHT);
        }
        public void Left()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_LEFT);
        }

        public void Lame()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_LAME);
        }
        public void Mute()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_MUTE);
        }

        public void VolumeUp()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_VOLUME_UP);
        }
        public void VolumeDown()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_VOLUME_DOWN);
        }
        public void BouquetUp()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_BOUQUET_UP);
        }
        public void BouquetDown()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_BOUQUET_DOWN);
        }
        public void OK()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_OK);
        }
        public void Previous()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_PREVIOUS);
        }
        public void Next()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_NEXT);
        }
        public void Red()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_RED);
        }
        public void Green()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_GREEN);
        }
        public void Yellow()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_YELLOW);
        }
        public void Blue()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_BLUE);
        }

        public void TV()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_TV);
        }
        public void Radio()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_RADIO);
        }
        public void Text()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_TEXT);
        }
        public void Help()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_HELP);
        }
        public void ZapTo(string reference)
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Zap + reference);
        }

        #region Numbers
        public void Zap0()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_0);
        }
        public void Zap1()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_1);
        }
        public void Zap2()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_2);
        }
        public void Zap3()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_3);
        }
        public void Zap4()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_4);
        }
        public void Zap5()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_5);
        }
        public void Zap6()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_6);
        }
        public void Zap7()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_7);
        }
        public void Zap8()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_8);
        }
        public void Zap9()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + BUTTON_9);
        }

        #endregion
    }
}
