using System;
using System.Collections;
using System.Reflection;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Player
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayerFactory
	{
    static ArrayList m_externalPlayers=new ArrayList();
    static bool m_loadedExternalPlayers = false;

    static private void LoadExternalPlayers()
    {	
      Log.Write("Loading external players plugins");
      string[] strFiles=System.IO.Directory.GetFiles(@"plugins\ExternalPlayers", "*.dll");
      foreach (string strFile in strFiles)
      {
        try
        {
          Assembly assem = Assembly.LoadFrom(strFile);
          if (assem!=null)
          {
            Type[] types = assem.GetExportedTypes();

            foreach (Type t in types)
            {
              try
              {
                if (t.IsClass)
                {
                  if (t.IsSubclassOf (typeof(IExternalPlayer)))
                  {
                    object newObj=(object)Activator.CreateInstance(t);
                    Log.Write("  found plugin:{0} in {1}",t.ToString(), strFile);
                    IExternalPlayer player=(IExternalPlayer)newObj;
                    Log.Write("  player:{0}.  author: {1}",player.PlayerName, player.AuthorName);
                    m_externalPlayers.Add(player);
                  }
                }
              }
              catch (Exception e)
              {	
                Log.Write("Error loading external player: {0}",t.ToString());
                Log.Write("Error: {0}",e.StackTrace);
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.Write("Error loading external player: {0}",e);
        }
      }
      m_loadedExternalPlayers = true;
    }

    static public IExternalPlayer GetExternalPlayer(string strFile)
    {
      if(!m_loadedExternalPlayers)
      {
        LoadExternalPlayers();
      }

      foreach (IExternalPlayer player in m_externalPlayers)
      {
        using (AMS.Profile.Xml xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          bool enabled = xmlreader.GetValueAsBool("plugins", player.PlayerName, false);
          player.Enabled = enabled;
        }
        if (player.Enabled && player.SupportsFile(strFile))
        {
          return player;
        }
      }
      return null;
    }

		static public IPlayer Create(IRender renderframe,string strFileName)
    {
      IPlayer newPlayer=null;

      string strExt=System.IO.Path.GetExtension(strFileName).ToLower();
			if (  strExt!=".tv" &&   strExt!=".sbe" &&   strExt!=".dvr-ms" )
			{
				newPlayer = GetExternalPlayer(strFileName);
				if(newPlayer != null)
				{
					newPlayer.RenderFrame=renderframe;
					return newPlayer;
				}
			}

			int iUseVMR9inMYTV=0;
			int iUseVMR9inMYMovies=0;
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
				iUseVMR9inMYTV=xmlreader.GetValueAsInt("mytv","vmr9",0);
				iUseVMR9inMYMovies=xmlreader.GetValueAsInt("movieplayer","vmr9",0);
      }

      if (Utils.IsVideo(strFileName))
      {        
        if ( strExt==".tv" ||   strExt==".sbe" || strExt==".dvr-ms" )
        {
          if ( strExt==".sbe" ||strExt==".dvr-ms" )
          {
             GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT,0,0,0,0,0,null);
             GUIGraphicsContext.SendMessage(msg);
          }
          if (iUseVMR9inMYTV==0) newPlayer=new Player.BaseStreamBufferPlayer();
					if (iUseVMR9inMYTV==1) newPlayer=new Player.StreamBufferPlayerVMR9wl();
					if (iUseVMR9inMYTV==2) newPlayer=new Player.StreamBufferPlayer9();
          newPlayer.RenderFrame=renderframe;
          return newPlayer;
        }
        else
        {
          if (iUseVMR9inMYMovies==0) newPlayer=new Player.VideoPlayerVMR7();
					if (iUseVMR9inMYMovies==1) newPlayer=new Player.VideoPlayerVMR9wl();
					if (iUseVMR9inMYMovies==2) newPlayer=new Player.VideoPlayerVMR9();
          newPlayer.RenderFrame=renderframe;
          return newPlayer;
        }
      }


      if ( strExt==".radio" )
      {
        newPlayer=new Player.RadioTuner();
        newPlayer.RenderFrame=renderframe;
        return newPlayer;
      }

      if (Utils.IsCDDA(strFileName))
      {
        newPlayer=new Player.AudioPlayerWMP9();
        newPlayer.RenderFrame=renderframe;
        return newPlayer;
      }

      if (Utils.IsAudio(strFileName))
      {
        using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          string strAudioPlayer=xmlreader.GetValueAsString("audioplayer","player", "Windows Media Player 9");
          if (String.Compare(strAudioPlayer,"Windows Media Player 9",true)==0)
          {
            newPlayer=new Player.AudioPlayerWMP9();
            newPlayer.RenderFrame=renderframe;
            return newPlayer;
          }
          newPlayer=new Player.AudioPlayerVMR7();
          newPlayer.RenderFrame=renderframe;
          return newPlayer;
        }
      }


      newPlayer=new Player.AudioPlayerWMP9();
      newPlayer.RenderFrame=renderframe;
      return newPlayer;

    }
	}
}
