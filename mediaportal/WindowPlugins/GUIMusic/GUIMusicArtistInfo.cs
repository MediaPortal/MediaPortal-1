using System;
using System.Drawing;
using System.Net;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Music.Database;
using System.Text;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIMusicArtistInfo : GUIWindow
  { 
    enum Controls
    {
        CONTROL_ARTIST		      =20
      ,	CONTROL_ARTIST_NAME_AKA	=21
      ,	CONTROL_BORN 		        =22
      ,	CONTROL_YEARS_ACTIVE    =23
      ,	CONTROL_GENRES	        =24
      ,	CONTROL_TONES		        =25
      ,	CONTROL_STYLES	        =26
      ,	CONTROL_INSTRUMENTS	    =27

      , CONTROL_IMAGE		 =3
      , CONTROL_TEXTAREA =4

      , CONTROL_BTN_BIO	 =5
      , CONTROL_BTN_REFRESH	=6
    }

    #region Base Dialog Variables
    bool m_bRunning=false;
    bool m_bRefresh=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;

    #endregion

    Texture m_pTexture=null;
    bool    m_bViewBio=false;
    MusicArtistInfo m_pArtist=null;
    int m_iTextureWidth=0;
    int m_iTextureHeight=0;
    bool m_bOverlay=false;

    public GUIMusicArtistInfo()
    {
      GetID=(int)GUIWindow.Window.WINDOW_ARTIST_INFO;
    }
    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\DialogArtistInfo.xml");
    }
    public override void PreInit()
    {
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }
    #region Base Dialog Members
    public void RenderDlg(long timePassed)
    {
      // render the parent window
      if (null!=m_pParentWindow) 
        m_pParentWindow.Render(timePassed);

			GUIFontManager.Present();
      // render this dialog box
      base.Render(timePassed);
    }

    void Close()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow=null;
      m_bRunning=false;
    }

    public void DoModal(int dwParentId)
    {
      m_bRefresh=false;
      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      m_bRunning=true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }
    #endregion
	

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          m_pArtist=null;
          if (m_pTexture!=null)
          {
            m_pTexture.Dispose();
            m_pTexture=null;
          }
          GUIGraphicsContext.Overlay=m_bOverlay;
        }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          m_bOverlay=GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          m_pTexture=null;
          m_bViewBio=true;
          Refresh();
          return true;
        }
		
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_BTN_REFRESH)
          {
            string strImage=m_pArtist.ImageURL;
            string strThumb=GUIMusicArtists.GetCoverArt(m_pArtist.Artist);
            if (strThumb!=String.Empty) Utils.FileDelete(strThumb);
            m_bRefresh=true;
            Close();
            return true;
          }

          if (iControl==(int)Controls.CONTROL_BTN_BIO)
          {
            m_bViewBio=!m_bViewBio;
            Update();
          }
        }
          break;
      }

      return base.OnMessage(message);
    }


    public MusicArtistInfo Artist
    {
      set {m_pArtist=value; }
    }

    void Update()
    {
      if (null==m_pArtist) return;
      string strTmp;
      string nameAKA = m_pArtist.Artist;
      if(m_pArtist.Aka != null && m_pArtist.Aka.Length > 0)
        nameAKA += "(" + m_pArtist.Aka + ")";
      SetLabel((int)Controls.CONTROL_ARTIST, m_pArtist.Artist );
      SetLabel((int)Controls.CONTROL_ARTIST_NAME_AKA, nameAKA );
      SetLabel((int)Controls.CONTROL_BORN, m_pArtist.Born );
      SetLabel((int)Controls.CONTROL_YEARS_ACTIVE, m_pArtist.YearsActive );
      SetLabel((int)Controls.CONTROL_GENRES, m_pArtist.Genres );
      SetLabel((int)Controls.CONTROL_INSTRUMENTS, m_pArtist.Instruments );

      // scroll Tones
      GUIMessage msg1;
      msg1=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID,0, (int)Controls.CONTROL_TONES,0,0,null); 
      OnMessage(msg1);
      strTmp=m_pArtist.Tones.Trim();
      msg1=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID,0, (int)Controls.CONTROL_TONES,0,0,null); 
      msg1.Label= strTmp ;
      OnMessage(msg1);

      // scroll Styles
      GUIMessage msg2;
      msg2=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID,0, (int)Controls.CONTROL_STYLES,0,0,null); 
      OnMessage(msg2);
      strTmp=m_pArtist.Styles.Trim();
      msg2=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID,0, (int)Controls.CONTROL_STYLES,0,0,null); 
      msg2.Label= strTmp ;
      OnMessage(msg2);

      if (m_bViewBio)
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,m_pArtist.AMGBiography);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_BIO,GUILocalizeStrings.Get(132));
      }
      else
      {
        // translate the diff. discographys
        string dAlbums = GUILocalizeStrings.Get(690);
        string dCompilations = GUILocalizeStrings.Get(691);
        string dSingles = GUILocalizeStrings.Get(700);
        string dMisc = GUILocalizeStrings.Get(701);

        
        StringBuilder strLine = new StringBuilder(2048);
        ArrayList list = null;
        string discography = null;

        // get the Discography Album
        list = m_pArtist.DiscographyAlbums;
        strLine.Append('\t');
        strLine.Append(dAlbums);
        strLine.Append('\n');

        discography = m_pArtist.Albums;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            strTmp=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(strTmp);
            strLine2.Append(strTmp);
          };
          strLine.Append('\n');
          m_pArtist.Albums = strLine2.ToString();
        }

        // get the Discography Compilations
        list = m_pArtist.DiscographyCompilations;
        strLine.Append('\t');
        strLine.Append(dCompilations);
        strLine.Append('\n');
        discography = m_pArtist.Compilations;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            strTmp=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(strTmp);
            strLine2.Append(strTmp);
          };
          strLine.Append('\n');
          m_pArtist.Compilations = strLine2.ToString();
        }

        // get the Discography Singles
        list = m_pArtist.DiscographySingles;
        strLine.Append('\t');
        strLine.Append(dSingles);
        strLine.Append('\n');
        discography = m_pArtist.Singles;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            strTmp=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(strTmp);
            strLine2.Append(strTmp);
          };
          strLine.Append('\n');
          m_pArtist.Singles = strLine2.ToString();
        }

        // get the Discography Misc
        list = m_pArtist.DiscographyMisc;
        strLine.Append('\t');
        strLine.Append(dMisc);
        strLine.Append('\n');
        discography = m_pArtist.Misc;
        if(discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i=0; i < list.Count;++i)
          {
            string[] listInfo = (string[])list[i];
            strTmp=String.Format("{0} - {1} ({2})\n",
              listInfo[0],  // year 
              listInfo[1],  // title
              listInfo[2]); // label
            strLine.Append(strTmp);
            strLine2.Append(strTmp);
          };
          strLine.Append('\n');
          m_pArtist.Misc = strLine2.ToString();
        }

        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,strLine.ToString());
        
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_BIO,GUILocalizeStrings.Get(689));
      }
    }

    void SetLabel(int iControl,  string strLabel)
    {
      string strLabel1=strLabel;
      if (strLabel1.Length==0)
        strLabel1=GUILocalizeStrings.Get(416);
    	
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET,GetID,0,iControl,0,0,null);
      msg.Label=(strLabel1);
      OnMessage(msg);

    }

    public override void Render(long timePassed)
    {
      RenderDlg(timePassed);

      if (null==m_pTexture) return;

      GUIControl pControl=(GUIControl)GetControl((int)Controls.CONTROL_IMAGE);
      if (null!=pControl)
      {
        float x=(float)pControl.XPosition;
        float y=(float)pControl.YPosition;
        int iwidth;
        int iheight;
        GUIGraphicsContext.Correct(ref x,ref y);

        int iMaxWidth=pControl.Width;
        int iMaxHeight=pControl.Height;
        GUIGraphicsContext.GetOutputRect(m_iTextureWidth, m_iTextureHeight,iMaxWidth,iMaxHeight, out iwidth,out iheight);

				GUIFontManager.Present();
        MediaPortal.Util.Picture.RenderImage(ref m_pTexture,(int)x,(int)y,iwidth,iheight,m_iTextureWidth,m_iTextureHeight,0,0,true);
      }
    }


    void Refresh()
    {
      if (m_pTexture!=null)
      {
        m_pTexture.Dispose();
        m_pTexture=null;
      }

      string strThumb;
      string strImage=m_pArtist.ImageURL;
      strThumb=GUIMusicArtists.GetCoverArtName(m_pArtist.Artist);
      if (strThumb!=String.Empty )
      {
        //	Download image and save as 
        //	permanent thumb
        Utils.DownLoadImage(strImage,strThumb);
      }

      if (System.IO.File.Exists(strThumb) )
      {
        m_pTexture=MediaPortal.Util.Picture.Load(strThumb,0,128,128,true,false,out m_iTextureWidth,out m_iTextureHeight);
      }
      Update();
    }

    
    public bool NeedsRefresh
    {
      get {return m_bRefresh;}
    }
  }
}
