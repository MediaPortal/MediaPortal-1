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
using MediaPortal.TagReader;



namespace MediaPortal.GUI.Music
{
	/// <summary>
	/// 
	/// </summary> 
	public class GUIMusicInfo : GUIWindow
	{
    enum Controls
    {
    	  CONTROL_ALBUM		=20
      ,	CONTROL_ARTIST	=21
      ,	CONTROL_DATE 		=22
      ,	CONTROL_RATING	=23
      ,	CONTROL_GENRE		=24
      ,	CONTROL_TONE 		=25
      ,	CONTROL_STYLES	=26

      , CONTROL_IMAGE		 =3
      , CONTROL_TEXTAREA =4

      , CONTROL_BTN_TRACKS	=5
      , CONTROL_BTN_REFRESH	=6
    }

    #region Base Dialog Variables
    bool m_bRunning=false;
    bool m_bRefresh=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;

    #endregion

    Texture m_pTexture=null;
    bool    m_bViewReview=false;
    MusicAlbumInfo m_pAlbum=null;
    MusicTag  m_tag=null;
    int m_iTextureWidth=0;
    int m_iTextureHeight=0;
    bool m_bOverlay=false;

    public GUIMusicInfo()
    {
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_INFO;
    }
    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\DialogAlbumInfo.xml");
    }
    public override void PreInit()
    {
      AllocResources();
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
    public void RenderDlg()
    {
      // render the parent window
      if (null!=m_pParentWindow) 
        m_pParentWindow.Render();

      // render this dialog box
      base.Render();
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
			    m_pAlbum=null;
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
			    m_bViewReview=true;
			    Refresh();
			    return true;
        }
		
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iControl=message.SenderControlId;
			    if (iControl==(int)Controls.CONTROL_BTN_REFRESH)
			    {
            string strImage=m_pAlbum.ImageURL;
            string strThumb=GUIMusicFiles.GetAlbumThumbName(m_tag.Artist,m_tag.Album);
            Utils.FileDelete(strThumb);
				    m_bRefresh=true;
            Close();
            return true;
			    }

			    if (iControl==(int)Controls.CONTROL_BTN_TRACKS)
			    {
				    m_bViewReview=!m_bViewReview;
				    Update();
			    }
        }
        break;
      }

      return base.OnMessage(message);
    }


    public MusicAlbumInfo Album
    {
      set {m_pAlbum=value; }
    }

    void Update()
    {
	    if (null==m_pAlbum) return;
	    string strTmp;
	    SetLabel((int)Controls.CONTROL_ALBUM, m_pAlbum.Title );
	    SetLabel((int)Controls.CONTROL_ARTIST, m_pAlbum.Artist );
	    SetLabel((int)Controls.CONTROL_DATE, m_pAlbum.DateOfRelease );

	    string strRating="";
	    if (m_pAlbum.Rating > 0)
		    strRating=String.Format("{0}/9", m_pAlbum.Rating);
	    SetLabel((int)Controls.CONTROL_RATING, strRating );

	    SetLabel((int)Controls.CONTROL_GENRE, m_pAlbum.Genre );
	    GUIMessage msg1;
      msg1=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID,0, (int)Controls.CONTROL_TONE,0,0,null); 
      OnMessage(msg1);
	    strTmp=m_pAlbum.Tones; strTmp.Trim();
	    msg1=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID,0, (int)Controls.CONTROL_TONE,0,0,null); 
      msg1.Label= strTmp ;
      OnMessage(msg1);
	    SetLabel((int)Controls.CONTROL_STYLES, m_pAlbum.Styles );

	    if (m_bViewReview)
	    {
			    GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,m_pAlbum.Review);
			    GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_TRACKS,GUILocalizeStrings.Get(182));
	    }
	    else
	    {
		    string strLine="";
		    for (int i=0; i < m_pAlbum.NumberOfSongs;++i)
		    {
			    MusicSong song=m_pAlbum.GetSong(i);
			    strTmp=String.Format("{0}. {1}\n",
							    song.Track, 
							    song.SongName);
			    strLine+=strTmp;
		    };

		    GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,strLine);

		    for (int i=0; i < m_pAlbum.NumberOfSongs;++i)
		    {
			    MusicSong song=m_pAlbum.GetSong(i);
			    strTmp=Utils.SecondsToHMSString(song.Duration);
			    msg1=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL2_SET,GetID,0,(int)Controls.CONTROL_TEXTAREA,i,0,null);
			    msg1.Label=(strTmp);
			    OnMessage(msg1);
		    }

		    GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_TRACKS,GUILocalizeStrings.Get(183));
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

    public override void Render()
    {
      RenderDlg();

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
	    string strImage=m_pAlbum.ImageURL;
      if (m_tag==null)
      {
        m_tag=new MusicTag();
        m_tag.Artist=m_pAlbum.Artist;
        m_tag.Album=m_pAlbum.Title;
      }
      strThumb=GUIMusicFiles.GetAlbumThumbName(m_tag.Artist,m_tag.Album);
	    if (!System.IO.File.Exists(strThumb) )
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


    public MusicTag Tag
    {
      get { return m_tag;}
      set {m_tag=value;}
    }

    public bool NeedsRefresh
    {
      get {return m_bRefresh;}
    }
	}
}
