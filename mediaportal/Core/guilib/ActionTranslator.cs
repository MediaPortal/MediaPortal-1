/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Collections;
using System.Xml;
using System.Windows.Forms;

namespace MediaPortal.GUI.Library
{
	// TODO: Change the variable name nodeGamepad to nodekey
	
	/// <summary>
	/// The class that is responsible for translating the keys into actions.
	/// </summary>
  public class ActionTranslator
  {
    static ArrayList mapWindows=new ArrayList ();
		
    /// <summary>
    /// Datastructure containing key/actiontype mapping.
    /// </summary>
    public class button
    {
      public int eKeyChar;
      public int eKeyCode;
      public Action.ActionType eAction;
      public string m_strSoundFile="";  
    };

    /// <summary>
    /// Datastructure containing the list of key/actiontype mappings for a window.
    /// </summary>
    public class WindowMap
    {
      public int iWindow;
      public ArrayList mapButtons=new ArrayList(); 
    };		
		
    // singleton. Dont allow any instance of this class
    private ActionTranslator()
    {
    }

    /// <summary>
    /// Loads the keymap file and creates the mapping.
    /// </summary>
    /// <returns>True if the load was successfull, false if it failed.</returns>
    static public bool Load()
    {
      mapWindows.Clear();
      string strFilename="keymap.xml";
      Log.Write("  Load key mapping from {0}", strFilename);
      try
      {
        // Load the XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(strFilename);
        // Check if it is a keymap
        if (doc.DocumentElement==null) return false;
        string strRoot=doc.DocumentElement.Name;
        if (strRoot!="keymap") return false;
        // Create a new windowmap and fill it with the global actions
        WindowMap map=new WindowMap();
        map.iWindow=-1;
        XmlNodeList list=doc.DocumentElement.SelectNodes("/keymap/global/action");
        foreach (XmlNode node in list)
        {
          XmlNode nodeId=node.SelectSingleNode("id");
          XmlNode nodeGamepad=node.SelectSingleNode("key");
          XmlNode nodeSound=node.SelectSingleNode("sound");
          MapAction(ref map,nodeId,nodeGamepad,nodeSound);
        }
        if (map.mapButtons.Count>0)
        {
          mapWindows.Add(map);
        }
        // For each window
        XmlNodeList listWindows=doc.DocumentElement.SelectNodes("/keymap/window");
        foreach (XmlNode nodeWindow in listWindows)
        {
          XmlNode nodeWindowId=nodeWindow.SelectSingleNode("id");
          if (null!=nodeWindowId)
          {
            map=new WindowMap();
            map.iWindow=System.Int32.Parse(nodeWindowId.InnerText);
            XmlNodeList listNodes=nodeWindow.SelectNodes("action");
            // Create a list of key/actiontype mappings
            foreach (XmlNode node in listNodes)
            {
              XmlNode nodeId=node.SelectSingleNode("id");
              XmlNode nodeGamepad=node.SelectSingleNode("key");
              XmlNode nodeSound=node.SelectSingleNode("sound");
              MapAction(ref map,nodeId,nodeGamepad,nodeSound);
            }
            if (map.mapButtons.Count>0)
            {
              mapWindows.Add(map);
            }
          }
        }
      }
      catch(Exception ex)
      {
        Log.Write("exception loading keymap {0} err:{1} stack:{2}", strFilename, ex.Message,ex.StackTrace);
      }
      return false;
    }

    /// <summary>
    /// Map an action in a windowmap based on the id and key xml nodes. 
    /// </summary>
    /// <param name="map">The windowmap that needs to be filled in.</param>
    /// <param name="nodeId">The id of the action</param>
    /// <param name="nodeGamepad">The key corresponding to the mapping.</param>
    static void MapAction(ref WindowMap map, XmlNode nodeId,XmlNode nodeGamepad,XmlNode nodeSound)
    {
      if (null==nodeId) return;
      button but=new button();
      but.eAction=(Action.ActionType)System.Int32.Parse(nodeId.InnerText);
      but.m_strSoundFile="";
      
      if (nodeSound!=null && nodeSound.InnerText!=null)
      {
        but.m_strSoundFile=nodeSound.InnerText;
      }

      if (nodeGamepad!=null)
      {
        string strButton=nodeGamepad.InnerText.ToLower();
        if (strButton.Length==1)
        {
          but.eKeyChar = (int)strButton[0];
          but.eKeyCode=0;
        }
        else
        {
          but.eKeyChar=0;
          strButton=strButton.ToUpper();
          if (strButton=="F1") but.eKeyCode=  (int)Keys.F1;
          if (strButton=="F2") but.eKeyCode=  (int)Keys.F2;
          if (strButton=="F3") but.eKeyCode=  (int)Keys.F3;
          if (strButton=="F4") but.eKeyCode=  (int)Keys.F4;
          if (strButton=="F5") but.eKeyCode=  (int)Keys.F5;
          if (strButton=="F6") but.eKeyCode=  (int)Keys.F6;
          if (strButton=="F7") but.eKeyCode=  (int)Keys.F7;
          if (strButton=="F8") but.eKeyCode=  (int)Keys.F8;
          if (strButton=="F9") but.eKeyCode=  (int)Keys.F9;
          if (strButton=="F10") but.eKeyCode=  (int)Keys.F10;
          if (strButton=="F11") but.eKeyCode=  (int)Keys.F11;
          if (strButton=="F12") but.eKeyCode=  (int)Keys.F12;
          if (strButton=="BACKSPACE") but.eKeyCode=  (int)Keys.Back;
          if (strButton=="TAB") but.eKeyCode=  (int)Keys.Tab;
          if (strButton=="END") but.eKeyCode=  (int)Keys.End;
          if (strButton=="INSERT") but.eKeyCode=  (int)Keys.Insert;
          if (strButton=="HOME") but.eKeyCode=  (int)Keys.Home;
          if (strButton=="PAGEUP") but.eKeyCode=  (int)Keys.PageUp;
          if (strButton=="PAGEDOWN") but.eKeyCode=  (int)Keys.PageDown;
          if (strButton=="LEFT") but.eKeyCode=  (int)Keys.Left;
          if (strButton=="RIGHT") but.eKeyCode=  (int)Keys.Right;
          if (strButton=="UP") but.eKeyCode=  (int)Keys.Up;
          if (strButton=="DOWN") but.eKeyCode=  (int)Keys.Down;
          if (strButton=="ENTER") but.eKeyCode=  (int)Keys.Enter;
          if (strButton=="DELETE") but.eKeyCode=  (int)Keys.Delete;
          if (strButton=="PAUSE") but.eKeyCode=  (int)Keys.Pause;
          if (strButton=="PRINTSCREEN") but.eKeyCode=  (int)Keys.PrintScreen;
          if (strButton=="ESCAPE") but.eKeyCode=  (int)Keys.Escape;
          if (strButton=="ESC") but.eKeyCode=  (int)Keys.Escape;
          if (strButton=="SPACE") 
          {
            but.eKeyCode=  0;
            but.eKeyChar= 32;
          }
        }
      }

      map.mapButtons.Add(but);
    }

    /// <summary>
    /// Translates a window, key combination to an Action.
    /// </summary>
    /// <param name="iWindow">The window that received the key action.</param>
    /// <param name="key">The key that caused the key action.</param>
    /// <param name="action">The Action that is initialized by this method.</param>
    /// <returns>True if the translation was successful, false if not.</returns>
    static public bool GetAction(int iWindow, Key key, ref Action action)
    {
      // try to get the action from the current window
      if (key==null) return false;
      string strSoundFile;
      int wAction = GetActionCode(iWindow, key, out strSoundFile);
      // if it's invalid, try to get it from the global map
      if (wAction == 0)
        wAction = GetActionCode(-1, key,out strSoundFile);
      if (wAction==0) return false;
      // Now fill our action structure
      action.wID = (Action.ActionType)wAction;
      action.m_key    = key;
      action.m_SoundFileName=strSoundFile;
      return true;
    }

    /// <summary>
    /// Gets the action based on a window key combination
    /// </summary>
    /// <param name="wWindow">The window id.</param>
    /// <param name="key">The key.</param>
    /// <returns>The action if it is found in the map, 0 if not.</returns>
    static int GetActionCode(int wWindow,  Key key,out string strSoundFile)
    {
      strSoundFile="";
      if (key==null) return 0;
      for (int iw=0; iw < mapWindows.Count;++iw)
      {
        WindowMap window =(WindowMap )mapWindows[iw];
        if (window.iWindow == wWindow)
        {
          for (int ib=0; ib < window.mapButtons.Count;ib++)
          {
            button but =(button)window.mapButtons[ib];
          
            if (but.eKeyChar==key.KeyChar && key.KeyChar>0)
            {
              strSoundFile=but.m_strSoundFile;
              return (int)but.eAction;
            }
            if (but.eKeyCode==key.KeyCode&& key.KeyCode>0)
            {
              strSoundFile=but.m_strSoundFile;
              return (int)but.eAction;
            }
          }
          return 0;
        }
      }
      return 0;
    }

    /// <summary>
    /// Update action with soundfilename based on a window id and action id
    /// </summary>
    /// <param name="wWindow">The window id.</param>
    /// <param name="action">The action</param>
    /// <returns>True if it is found in the map, fales if not.</returns>
    static public bool GetActionDetail(int wWindow, Action action)
    {
      if (action.wID == 0) return false;
      for (int iw=0; iw < mapWindows.Count;++iw)
      {
        WindowMap window =(WindowMap )mapWindows[iw];
        if (window.iWindow == wWindow)
        {
          for (int ib=0; ib < window.mapButtons.Count;ib++)
          {
            button but =(button)window.mapButtons[ib];
          
            if (but.eAction == (Action.ActionType)action.wID)
            {
              action.SoundFileName = but.m_strSoundFile;
              return true;
            }
          }
        }
      }
      return false;
    }
    static public bool HasKeyMapped (int iWindow, Key key)
    {
      string strSoundFile;
      int wAction = GetActionCode(iWindow, key, out strSoundFile);
      if (wAction==0) return false;
      return true;
    }
  }
}
