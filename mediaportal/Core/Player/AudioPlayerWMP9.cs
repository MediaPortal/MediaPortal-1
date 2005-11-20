/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics; 
using MediaPortal.TagReader;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using Direct3D = Microsoft.DirectX.Direct3D;



using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.Player 
{
  public class AudioPlayerWMP9 : IPlayer
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }
    string                    m_strCurrentFile="";
    PlayState								  m_state=PlayState.Init;
    bool                      m_bFullScreen=false;
    int                       m_iPositionX=10,m_iPositionY=10,m_iWidth=100,m_iHeight=100;
    static AxWMPLib.AxWindowsMediaPlayer m_player = null;
    bool                      m_bUpdateNeeded=true;
    bool                      m_bNotifyPlaying=true;
    
    public AudioPlayerWMP9()
    {
    }


    static void CreateInstance()
    {
      // disable auto windows mediaplayer auto cd-play
      try
      {
        UInt32 dwValue = (UInt32)0;
        RegistryKey hkcu =Registry.CurrentUser;
        RegistryKey subkey=hkcu.OpenSubKey(@"Software\Microsoft\MediaPlayer\Preferences",true);

        subkey.SetValue("CDAutoPlay", (Int32)dwValue);

        // enable metadata lookup for CD's
        dwValue=(UInt32)Convert.ToInt32(subkey.GetValue("MetadataRetrieval" ));
        dwValue |=1;
        subkey.SetValue("MetadataRetrieval", (Int32)dwValue);
      }
      catch(Exception){}

      if (m_player==null)
      {

                m_player = new AxWMPLib.AxWindowsMediaPlayer();
        
                m_player.BeginInit();
                GUIGraphicsContext.form.SuspendLayout();
                m_player.Enabled = true;
                
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Resource1));
                m_player.Location = new System.Drawing.Point(8, 16);
                m_player.Name = "axWindowsMediaPlayer1";
                m_player.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
                m_player.Size = new System.Drawing.Size(264, 240);
                m_player.TabIndex = 0;
                GUIGraphicsContext.form.Controls.Add(m_player);
                m_player.EndInit();
                m_player.uiMode="none";
                m_player.windowlessVideo=true;

                m_player.enableContextMenu=false;
                m_player.Ctlenabled=false;
                m_player.Visible = false;
                GUIGraphicsContext.form.ResumeLayout(false);
              }
            }
            static public ArrayList GetCDTracks()
            {
              GUIListItem item;
              ArrayList list = new ArrayList();
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label="..";
              item.Label2="";
              item.Path="";
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              list.Add(item);

              CreateInstance();
              if (m_player.cdromCollection.count<=0) return list;
              if (m_player.cdromCollection.count<=0) return list;
      
      
              WMPLib.IWMPCdrom cdrom = m_player.cdromCollection.Item(0);
			
              if (cdrom==null ) return list;
              if (cdrom.Playlist==null ) return list;
			

              for (int iTrack=0; iTrack < cdrom.Playlist.count; iTrack++)
              {
                try
                {
                  MusicTag tag = new MusicTag();
                  WMPLib.IWMPMedia media=cdrom.Playlist.get_Item(iTrack);
                  item = new GUIListItem();
                  item.IsFolder=false;
                  item.Label=media.name;
                  item.Label2="";
                  item.Path=String.Format("cdda:{0}", iTrack);
                  item.FileInfo=null;

                  for (int i=0; i < media.attributeCount; ++i)
                  {
                    string strAttr=media.getAttributeName(i);
                    string strValue=media.getItemInfo(strAttr);
                    if (String.Compare("album", strAttr,true)==0) tag.Album=strValue;
                    if (String.Compare("actor", strAttr,true)==0) tag.Artist=strValue;
                    if (String.Compare("artist", strAttr,true)==0) tag.Artist=strValue;
                    if (String.Compare("style", strAttr,true)==0) tag.Genre=strValue;
                    if (String.Compare("releasedate", strAttr,true)==0) 
                    {
                      try
                      {
                        tag.Year=Convert.ToInt32(strValue.Substring(0,4));
                      }
                      catch(Exception)
                      {
                      }
                    }
                  }
                  tag.Title    = media.name;
                  tag.Duration = (int)media.duration;
                  tag.Track    = iTrack+1;
                  //tag.Comment  =
                  //tag.Year     =
                  //tag.Genre    =
                  item.MusicTag = tag;
                  list.Add(item);
                }
                catch(Exception)
                {
                }
              }
              return list;
            }

            public override bool Play(string strFile)
            {     
              m_state=PlayState.Init;
              m_strCurrentFile=strFile;

              m_bNotifyPlaying=true;
              GC.Collect();
              CreateInstance();

              if (m_player == null) return false;
              if (m_player.cdromCollection == null) return false;

              m_player.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(OnPlayStateChange);

              m_player.enableContextMenu=false;
              m_player.Ctlenabled=false;
              if ( strFile.IndexOf("cdda:")>=0 )
              {
                string strTrack=strFile.Substring(5);
                int iTrack = Convert.ToInt32(strTrack);
                if (m_player.cdromCollection.count<=0) return false;
                if (m_player.cdromCollection.Item(0).Playlist==null) return false;
                if (iTrack > m_player.cdromCollection.Item(0).Playlist.count ) return false;
                m_player.currentMedia =m_player.cdromCollection.Item(0).Playlist.get_Item(iTrack-1);
                if (m_player.currentMedia==null) return false;
				
                Log.Write("Audioplayer:play track:{0}/{1}", iTrack,m_player.cdromCollection.Item(0).Playlist.count);
              }
              else if ( strFile.IndexOf(".cda")>=0 )
              {
                string strTrack="";
                int pos=strFile.IndexOf(".cda");
                if (pos >=0)
                {
                  pos--;
                  while (Char.IsDigit(strFile[pos]) && pos>0) 
                  {
                    strTrack=strFile[pos]+strTrack;
                    pos--;
                  }
                }

                if (m_player.cdromCollection.count<=0) return false;
                string strDrive = strFile.Substring(0,1);
                strDrive += ":";
                int iCdRomDriveNr=0;
                while ((m_player.cdromCollection.Item(iCdRomDriveNr).driveSpecifier != strDrive) && (iCdRomDriveNr<m_player.cdromCollection.count))
                {
                  iCdRomDriveNr++;
                }

                int iTrack = Convert.ToInt32(strTrack);
                if (m_player.cdromCollection.Item(iCdRomDriveNr).Playlist==null) return false;
                int tracks=m_player.cdromCollection.Item(iCdRomDriveNr).Playlist.count ;
                if (iTrack >tracks ) return false;
                m_player.currentMedia =m_player.cdromCollection.Item(iCdRomDriveNr).Playlist.get_Item(iTrack-1);
                if (m_player.currentMedia==null) return false;
                /*
                string strStart=strFile.Substring(0,2)+@"\";
                int ipos=strFile.LastIndexOf("+");
                if (ipos >0) strStart += strFile.Substring(ipos+1);
                strFile=strStart;
                m_strCurrentFile=strFile;
                Log.Write("Audioplayer:play {0}", strFile);*/
        //m_player.URL=strFile;
        m_strCurrentFile=strFile;
      }
      else
      {
        Log.Write("Audioplayer:play {0}", strFile);
        m_player.URL=strFile;
      }
      m_player.Ctlcontrols.play();
      m_player.Visible=false;

      
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED,0,0,0,0,0,null);
      msg.Label=strFile;	
      
      GUIWindowManager.SendThreadMessage(msg);
      m_state=PlayState.Playing;
      GC.Collect();
      m_bUpdateNeeded=true;
      m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
      m_iPositionX=GUIGraphicsContext.VideoWindow.Left;
      m_iPositionY=GUIGraphicsContext.VideoWindow.Top;
      m_iWidth=GUIGraphicsContext.VideoWindow.Width;
      m_iHeight=GUIGraphicsContext.VideoWindow.Height;

      SetVideoWindow();

      return true;
    }

    private void OnPlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
    {
      if (m_player==null) return;
      switch (m_player.playState)
      {
        case WMPLib.WMPPlayState.wmppsStopped:
          SongEnded(false);
        break;
      }
    }

    void SongEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      
      Log.Write("Audioplayer:ended {0} {1}", m_strCurrentFile,bManualStop);
      m_strCurrentFile="";
      if (m_player!=null)
      {
        m_player.Visible=false;
        m_player.PlayStateChange -= new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(OnPlayStateChange);
      }
      //GUIGraphicsContext.IsFullScreenVideo=false;
      GUIGraphicsContext.IsPlaying=false;
      if (!bManualStop)
      {
        m_state=PlayState.Ended;
      }
      else
      {
        m_state=PlayState.Init;
      }
      GC.Collect();
    }


    public override bool Ended
    {
      get { return m_state==PlayState.Ended;}
    }

    public override double Duration
    {
      get 
      {
        if (m_state!=PlayState.Init && m_player!=null) 
        {
          return m_player.currentMedia.duration;
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get 
      {
        return m_player.Ctlcontrols.currentPosition;
      }
    }

    public override void Pause()
    {
      if (m_player==null) return;
      if (m_state==PlayState.Paused) 
      {
        m_state=PlayState.Playing;
        m_player.Ctlcontrols.play();
      }
      else if (m_state==PlayState.Playing) 
      {
        m_player.Ctlcontrols.pause();
        if (m_player.playState ==WMPLib.WMPPlayState.wmppsPaused)
          m_state=PlayState.Paused;
      }
    }

    public override bool Paused
    {
      get 
      {
        return (m_state==PlayState.Paused);
      }
    }

    public override bool Playing
    {
      get 
      { 
        return (m_state==PlayState.Playing||m_state==PlayState.Paused);
      }
    }

    public override bool Stopped
    {
      get 
      { 
        return (m_state==PlayState.Init);
      }
    }

    public override string CurrentFile
    {
      get { return m_strCurrentFile;}
    }

    public override void Stop()
    {
      if (m_player==null) return;
      if (m_state!=PlayState.Init)
      {
        m_player.Ctlcontrols.stop();
        m_player.Visible=false;
        SongEnded(true);       
      }
    }

    public override int Volume
    {
      get {
        
        if (m_player==null) return 100;
        return m_player.settings.volume;
      }
      set 
      {
        
        if (m_player==null) return ;
        if (m_player.settings.volume!=value)
        {
          m_player.settings.volume= value;
        }
      }
    }

    
    public override bool HasVideo
    {
      get { return true;}
    }


    #region IDisposable Members

    public override void Release()
    {
      if (m_player==null) return;
      m_player.Visible=false;
      
      try
      {
        GUIGraphicsContext.form.Controls.Remove(m_player);
      }
      catch(Exception){}
      m_player.Dispose();
      m_player=null;
    }
    #endregion 

    public override bool FullScreen
    {
      get 
      { 
        return GUIGraphicsContext.IsFullScreenVideo;
      }
      set
      {
        if (value != m_bFullScreen )
        {
          m_bFullScreen=value;
          m_bUpdateNeeded=true;          
        }
      }
    }
    public override int PositionX
    {
      get { return m_iPositionX;}
      set 
      { 
        if (value != m_iPositionX)
        {
          m_iPositionX=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override int PositionY
    {
      get { return m_iPositionY;}
      set 
      {
        if (value != m_iPositionY)
        {
          m_iPositionY=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return m_iWidth;}
      set 
      {
        if (value !=m_iWidth)
        {
          m_iWidth=value;
          m_bUpdateNeeded=true;
        }
      }
    }
    public override int RenderHeight
    {
      get { return m_iHeight;}
      set 
      {
        if (value != m_iHeight)
        {
          m_iHeight=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override void Process()
    {
      if ( !Playing) return;
      if (m_player==null) return;
      if (GUIGraphicsContext.BlankScreen||(GUIGraphicsContext.Overlay==false && GUIGraphicsContext.IsFullScreenVideo==false))
      {
        if (m_player.Visible)
        {
          m_player.ClientSize = new Size(0,0);
          m_player.Visible=false;
          m_player.uiMode="invisible";
        }
      }
      else if (!m_player.Visible)
      {
        m_bUpdateNeeded=true;
        SetVideoWindow();
        m_player.uiMode="none";
        m_player.Visible=true;
      }


      if (CurrentPosition>=10.0)
      {
        if (m_bNotifyPlaying)
        {
          m_bNotifyPlaying=false;
          GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC,0,0,0,0,0,null);
          msg.Label=CurrentFile;	
          GUIWindowManager.SendThreadMessage(msg);
        }
      }
    }

    public override void SetVideoWindow()
    {

      if (m_player==null) return;
      if (GUIGraphicsContext.IsFullScreenVideo!= m_bFullScreen)
      {
        m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
        m_bUpdateNeeded=true;
      }
      if (!m_bUpdateNeeded) return;
      m_bUpdateNeeded=false;
      

      if (m_bFullScreen)
      {
        Log.Write("AudioPlayer:Fullscreen");

        m_iPositionX=GUIGraphicsContext.OverScanLeft;
        m_iPositionY=GUIGraphicsContext.OverScanTop;
        m_iWidth=GUIGraphicsContext.OverScanWidth;
        m_iHeight=GUIGraphicsContext.OverScanHeight;

        m_player.Location=new Point(0,0);
        m_player.ClientSize= new System.Drawing.Size(GUIGraphicsContext.Width,GUIGraphicsContext.Height);
        m_player.Size=new System.Drawing.Size(GUIGraphicsContext.Width,GUIGraphicsContext.Height);
        
        m_VideoRect=new Rectangle(0,0,m_player.ClientSize.Width,m_player.ClientSize.Height);
        m_SourceRect=m_VideoRect;
          
        //m_player.fullScreen=true;
		m_player.stretchToFit=true;
        Log.Write("AudioPlayer:done");
        return;
      }
      else
      {

        m_player.ClientSize= new System.Drawing.Size(m_iWidth,m_iHeight);
        m_player.Location=new Point(m_iPositionX,m_iPositionY);
        
        m_VideoRect=new Rectangle(m_iPositionX,m_iPositionY,m_player.ClientSize.Width,m_player.ClientSize.Height);
        m_SourceRect=m_VideoRect;
        //Log.Write("AudioPlayer:set window:({0},{1})-({2},{3})",m_iPositionX,m_iPositionY,m_iPositionX+m_player.ClientSize.Width,m_iPositionY+m_player.ClientSize.Height);
      }
      m_player.uiMode="none";
      m_player.windowlessVideo=true;
      m_player.enableContextMenu=false;
      m_player.Ctlenabled=false;
      GUIGraphicsContext.form.Controls[0].Enabled=false;
    }
/*
    public override int AudioStreams
    {
      get { return m_player.Ctlcontrols.audioLanguageCount;}
    }
    public override int CurrentAudioStream
    {
      get { return m_player.Ctlcontrols.currentAudioLanguage;}
      set { m_player.Ctlcontrols.currentAudioLanguage=value;}
    }
    public override string AudioLanguage(int iStream)
    {
      return m_player.controls.getLanguageName(iStream);
    }
*/
    public override void SeekRelative(double dTime)
    {
      if (m_player==null) return;
      if (m_state!=PlayState.Init)
      {
          
        double dCurTime=CurrentPosition;
        dTime=dCurTime+dTime;
        if (dTime<0.0d) dTime=0.0d;
        if (dTime < Duration)
        {
          m_player.Ctlcontrols.currentPosition=dTime;
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (m_player==null) return;
      if (m_state!=PlayState.Init)
      {
        if (dTime<0.0d) dTime=0.0d;
        if (dTime < Duration)
        {
          m_player.Ctlcontrols.currentPosition=dTime;
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (m_player==null) return;
      if (m_state!=PlayState.Init)
      {
        double dCurrentPos=CurrentPosition;
        double dDuration=Duration;

        double fCurPercent=(dCurrentPos/Duration)*100.0d;
        double fOnePercent=Duration/100.0d;
        fCurPercent=fCurPercent + (double)iPercentage;
        fCurPercent*=fOnePercent;
        if (fCurPercent<0.0d) fCurPercent=0.0d;
        if (fCurPercent<Duration)
        {
          m_player.Ctlcontrols.currentPosition=fCurPercent;
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (m_player==null) return;
      if (m_state!=PlayState.Init)
      {
        if (iPercentage<0) iPercentage=0;
        if (iPercentage>=100) iPercentage=100;
        double fPercent=Duration/100.0f;
        fPercent*=(double)iPercentage;
        m_player.Ctlcontrols.currentPosition=fPercent;
      }
    }
    public override int Speed
    {
      get 
      { 
        if (m_state==PlayState.Init) return 1;
        if (m_player==null) return 1;
        return (int)m_player.settings.rate;
      }
      set 
      {
        if (m_player==null) return ;
        if (m_state!=PlayState.Init)
        {
          try
          {
            m_player.settings.rate=(double)value;
          }
          catch(Exception)
          {
          }
        }
      }
    }

    public override bool IsRadio
    {
      get 
      {
        if (m_strCurrentFile==null) return false;
        //TODO: this has to be changed if we are gonna support video streams in the future
        if (m_strCurrentFile.ToLower().IndexOf("http:")>=0) return true;
        if (m_strCurrentFile.ToLower().IndexOf("https:")>=0) return true;
        if (m_strCurrentFile.ToLower().IndexOf("mms:")>=0) return true;
        if (m_strCurrentFile.ToLower().IndexOf("rtsp:")>=0) return true;
        return false;
      }
    }
  }
}
