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
using MediaPortal.Playlists;

namespace MediaPortal.MusicVideos.Database
{

    public class MVPlayListItem : PlayListItem
    { 
        private YahooVideo moVideo;
        private YahooUtil moUtil = YahooUtil.getInstance();
        private YahooSettings moSettings = YahooSettings.getInstance();
        private String msUrl = String.Empty;
        private DateTime moTimeOfUrl;
        private bool mbUpdateUrl =true;
        public MVPlayListItem()
        {
        }
        public override string FileName
        {
            get
            {
                if (String.IsNullOrEmpty(msUrl))
                {                    
                    msUrl = String.Concat(moUtil.getVideoMMSUrl(moVideo,moSettings.msDefaultBitRate).Trim(), "&txe=.ymvp");
                    moTimeOfUrl = DateTime.Now;
                    return msUrl;
                }
                DateTime loCurrentTime = DateTime.Now;
                TimeSpan loSpanSince = loCurrentTime - moTimeOfUrl;
                if(loSpanSince.TotalMinutes>5 && mbUpdateUrl){
                     msUrl = String.Concat(moUtil.getVideoMMSUrl(moVideo,moSettings.msDefaultBitRate).Trim(), "&txe=.ymvp");
                    moTimeOfUrl = DateTime.Now;
                }
                return msUrl;                
                
            }
            set
            {
                base.FileName = value;
            }
        }

        public bool UpdateUrl
        {
            get
            {
                return mbUpdateUrl;
            }
            set
            {
                mbUpdateUrl = value;
            }
        }
        public YahooVideo YahooVideo
        {
            get
            {
                return moVideo;
            }
            set
            {
                moVideo = value;
            }
        }
    }
}
