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
