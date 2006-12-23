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

namespace DreamBox
{
    public class Remote
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";

        public Remote(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
        }

        #region RemoteControl
        public void Left()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Left();
        }
        public void Right()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Right();
        }

        public void Mute()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Mute();
        }
        public void Lame()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Lame();
        }

        public void VolumeUp()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.VolumeUp();
        }

        public void VolumeDown()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.VolumeDown();
        }
        public void BouquetUp()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.BouquetUp();
        }
        public void BouquetDown()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.BouquetDown();
        }
        public void Up()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Up();
        }
        public void Down()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Down();
        }
        public void Info()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Info();
        }
        public void Menu()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Menu();
        }
        public void OK()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.OK();
        }
        public void Previous()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Previous();
        }
        public void Next()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.OK();
        }
        public void Audio()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Audio();
        }
        public void Video()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Video();
        }
        public void Red()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Red();
        }
        public void Green()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Green();
        }
        public void Yellow()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Yellow();
        }
        public void Blue()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Blue();
        }
        public void TV()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.TV();
        }
        public void Radio()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Radio();
        }
        public void Text()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Text();
        }
        public void Help()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Help();
        }
        public void Zap(string reference)
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.ZapTo(reference);
        }

        #region Video Keys
        public void Rewind()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Rewind();
        }
        public void Play()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Play();
        }
        public void Pause()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Pause();
        }
        public void Forward()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Forward();
        }
        public void Stop()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Stop();
        }
        public void Record()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Record();
        }

        #endregion

        #region Numbers
        public void Zap0()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap0();
        }
        public void Zap1()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap1();
        }
        public void Zap2()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap2();
        }
        public void Zap3()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap3();
        }
        public void Zap4()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap4();
        }
        public void Zap5()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap5();
        }
        public void Zap6()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap6();
        }
        public void Zap7()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap7();
        }
        public void Zap8()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap8();
        }
        public void Zap9()
        {
            Zap z = new Zap(_Url, _UserName, _Password);
            z.Zap9();
        }

        #endregion
        #endregion
    }
}
