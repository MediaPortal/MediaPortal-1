#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Library
{
  // TODO: Change the variable name nodeGamepad to nodekey

  /// <summary>
  /// The class that is responsible for translating the keys into actions.
  /// </summary>
  public class ActionTranslator
  {
    private static ArrayList mapWindows = new ArrayList();

    /// <summary>
    /// Datastructure containing key/actiontype mapping.
    /// </summary>
    public class button
    {
      public int eKeyChar;
      public int eKeyCode;
      public Action.ActionType eAction;
      public string m_strSoundFile = "";
    } ;

    /// <summary>
    /// Datastructure containing the list of key/actiontype mappings for a window.
    /// </summary>
    public class WindowMap
    {
      public int iWindow;
      public ArrayList mapButtons = new ArrayList();
    } ;

    // singleton. Dont allow any instance of this class
    private ActionTranslator()
    {
    }

    /// <summary>
    /// Loads the keymap file and creates the mapping.
    /// </summary>
    /// <returns>True if the load was successfull, false if it failed.</returns>
    public static bool Load()
    {
      mapWindows.Clear();
      string strFilename = Config.GetFile(Config.Dir.Config, "keymap.xml");
      Log.Info("  Load key mapping from {0}", strFilename);
      try
      {
        // Load the XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(strFilename);
        // Check if it is a keymap
        if (doc.DocumentElement == null)
        {
          return false;
        }
        string strRoot = doc.DocumentElement.Name;
        if (strRoot != "keymap")
        {
          return false;
        }
        // Create a new windowmap and fill it with the global actions
        WindowMap map = new WindowMap();
        map.iWindow = -1;
        XmlNodeList list = doc.DocumentElement.SelectNodes("/keymap/global/action");
        foreach (XmlNode node in list)
        {
          XmlNode nodeId = node.SelectSingleNode("id");
          XmlNode nodeGamepad = node.SelectSingleNode("key");
          XmlNode nodeSound = node.SelectSingleNode("sound");
          MapAction(ref map, nodeId, nodeGamepad, nodeSound);
        }
        if (map.mapButtons.Count > 0)
        {
          mapWindows.Add(map);
        }
        // For each window
        XmlNodeList listWindows = doc.DocumentElement.SelectNodes("/keymap/window");
        foreach (XmlNode nodeWindow in listWindows)
        {
          XmlNode nodeWindowId = nodeWindow.SelectSingleNode("id");
          if (null != nodeWindowId)
          {
            map = new WindowMap();
            map.iWindow = Int32.Parse(nodeWindowId.InnerText);
            XmlNodeList listNodes = nodeWindow.SelectNodes("action");
            // Create a list of key/actiontype mappings
            foreach (XmlNode node in listNodes)
            {
              XmlNode nodeId = node.SelectSingleNode("id");
              XmlNode nodeGamepad = node.SelectSingleNode("key");
              XmlNode nodeSound = node.SelectSingleNode("sound");
              MapAction(ref map, nodeId, nodeGamepad, nodeSound);
            }
            if (map.mapButtons.Count > 0)
            {
              mapWindows.Add(map);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Warn("exception loading keymap {0} err:{1} stack:{2}", strFilename, ex.Message, ex.StackTrace);
      }
      return false;
    }

    /// <summary>
    /// Loads a supplementary keymap file and adds it to the map previously built from keymap.xml
    /// </summary>
    /// <returns>True if the load was successfull, false if it failed.</returns>
    public static bool Load(string strFilename)
    {
      //mapWindows.Clear();
      //string strFilename = Config.GetFile(Config.Dir.Config, "keymap.xml");
      Log.Info("  Load supplementary key mapping from {0}", strFilename);
      try
      {
        // Load the XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(strFilename);
        // Check if it is a keymap
        if (doc.DocumentElement == null)
        {
          return false;
        }
        string strRoot = doc.DocumentElement.Name;
        if (strRoot != "keymap")
        {
          return false;
        }

        // Create a new windowmap 
        WindowMap map;

        // For each window
        XmlNodeList listWindows = doc.DocumentElement.SelectNodes("/keymap/window");
        foreach (XmlNode nodeWindow in listWindows)
        {
          XmlNode nodeWindowId = nodeWindow.SelectSingleNode("id");
          if (null != nodeWindowId)
          {
            map = new WindowMap();
            map.iWindow = Int32.Parse(nodeWindowId.InnerText);
            XmlNodeList listNodes = nodeWindow.SelectNodes("action");
            // Create a list of key/actiontype mappings
            foreach (XmlNode node in listNodes)
            {
              XmlNode nodeId = node.SelectSingleNode("id");
              XmlNode nodeGamepad = node.SelectSingleNode("key");
              XmlNode nodeSound = node.SelectSingleNode("sound");
              MapAction(ref map, nodeId, nodeGamepad, nodeSound);
            }
            if (map.mapButtons.Count > 0)
            {
              mapWindows.Add(map);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Warn("exception loading supplementary keymap {0} err:{1} stack:{2}", strFilename, ex.Message, ex.StackTrace);
      }
      return false;
    }

    /// <summary>
    /// Map an action in a windowmap based on the id and key xml nodes. 
    /// </summary>
    /// <param name="map">The windowmap that needs to be filled in.</param>
    /// <param name="nodeId">The id of the action</param>
    /// <param name="nodeGamepad">The key corresponding to the mapping.</param>
    private static void MapAction(ref WindowMap map, XmlNode nodeId, XmlNode nodeGamepad, XmlNode nodeSound)
    {
      if (null == nodeId)
      {
        return;
      }
      button but = new button();
      but.eAction = (Action.ActionType) Int32.Parse(nodeId.InnerText);
      but.m_strSoundFile = "";

      if (nodeSound != null && nodeSound.InnerText != null)
      {
        but.m_strSoundFile = nodeSound.InnerText;
      }

      if (nodeGamepad != null)
      {
        string strButton = nodeGamepad.InnerText.ToLower();
        if (strButton.Length == 1)
        {
          but.eKeyChar = (int) strButton[0];
          but.eKeyCode = 0;
        }
        else
        {
          but.eKeyChar = 0;
          strButton = strButton.ToLower();
          if (strButton == "f1")
          {
            but.eKeyCode = (int) Keys.F1;
          }
          if (strButton == "f2")
          {
            but.eKeyCode = (int) Keys.F2;
          }
          if (strButton == "f3")
          {
            but.eKeyCode = (int) Keys.F3;
          }
          if (strButton == "f4")
          {
            but.eKeyCode = (int) Keys.F4;
          }
          if (strButton == "f5")
          {
            but.eKeyCode = (int) Keys.F5;
          }
          if (strButton == "f6")
          {
            but.eKeyCode = (int) Keys.F6;
          }
          if (strButton == "f7")
          {
            but.eKeyCode = (int) Keys.F7;
          }
          if (strButton == "f8")
          {
            but.eKeyCode = (int) Keys.F8;
          }
          if (strButton == "f9")
          {
            but.eKeyCode = (int) Keys.F9;
          }
          if (strButton == "f10")
          {
            but.eKeyCode = (int) Keys.F10;
          }
          if (strButton == "f11")
          {
            but.eKeyCode = (int) Keys.F11;
          }
          if (strButton == "f12")
          {
            but.eKeyCode = (int) Keys.F12;
          }
          if (strButton == "backspace")
          {
            but.eKeyCode = (int) Keys.Back;
          }
          if (strButton == "tab")
          {
            but.eKeyCode = (int) Keys.Tab;
          }
          if (strButton == "end")
          {
            but.eKeyCode = (int) Keys.End;
          }
          if (strButton == "insert")
          {
            but.eKeyCode = (int) Keys.Insert;
          }
          if (strButton == "home")
          {
            but.eKeyCode = (int) Keys.Home;
          }
          if (strButton == "pageup")
          {
            but.eKeyCode = (int) Keys.PageUp;
          }
          if (strButton == "pagedown")
          {
            but.eKeyCode = (int) Keys.PageDown;
          }
          if (strButton == "left")
          {
            but.eKeyCode = (int) Keys.Left;
          }
          if (strButton == "right")
          {
            but.eKeyCode = (int) Keys.Right;
          }
          if (strButton == "up")
          {
            but.eKeyCode = (int) Keys.Up;
          }
          if (strButton == "down")
          {
            but.eKeyCode = (int) Keys.Down;
          }
          if (strButton == "enter")
          {
            but.eKeyCode = (int) Keys.Enter;
          }
          if (strButton == "delete")
          {
            but.eKeyCode = (int) Keys.Delete;
          }
          if (strButton == "pause")
          {
            but.eKeyCode = (int) Keys.Pause;
          }
          if (strButton == "print")
          {
            but.eKeyCode = (int) Keys.PrintScreen;
          }
          if (strButton == "escape")
          {
            but.eKeyCode = (int) Keys.Escape;
          }
          if (strButton == "esc")
          {
            but.eKeyCode = (int) Keys.Escape;
          }
          if (strButton == "space")
          {
            but.eKeyCode = 0;
            but.eKeyChar = 32;
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
    public static bool GetAction(int iWindow, Key key, ref Action action)
    {
      // try to get the action from the current window
      if (key == null)
      {
        return false;
      }
      string strSoundFile;
      int wAction = GetActionCode(iWindow, key, out strSoundFile);
      // if it's invalid, try to get it from the global map
      if (wAction == 0)
      {
        wAction = GetActionCode(-1, key, out strSoundFile);
      }
      if (wAction == 0)
      {
        return false;
      }
      // Now fill our action structure
      action.wID = (Action.ActionType) wAction;
      action.m_key = key;
      action.m_SoundFileName = strSoundFile;
      return true;
    }

    /// <summary>
    /// Gets the action based on a window key combination
    /// </summary>
    /// <param name="wWindow">The window id.</param>
    /// <param name="key">The key.</param>
    /// <returns>The action if it is found in the map, 0 if not.</returns>
    private static int GetActionCode(int wWindow, Key key, out string strSoundFile)
    {
      strSoundFile = "";
      if (key == null)
      {
        return 0;
      }
      for (int iw = 0; iw < mapWindows.Count; ++iw)
      {
        WindowMap window = (WindowMap) mapWindows[iw];
        if (window.iWindow == wWindow)
        {
          for (int ib = 0; ib < window.mapButtons.Count; ib++)
          {
            button but = (button) window.mapButtons[ib];

            if (but.eKeyChar == key.KeyChar && key.KeyChar > 0)
            {
              strSoundFile = but.m_strSoundFile;
              return (int) but.eAction;
            }
            if (but.eKeyCode == key.KeyCode && key.KeyCode > 0)
            {
              strSoundFile = but.m_strSoundFile;
              return (int) but.eAction;
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
    public static bool GetActionDetail(int wWindow, Action action)
    {
      if (action.wID == 0)
      {
        return false;
      }
      for (int iw = 0; iw < mapWindows.Count; ++iw)
      {
        WindowMap window = (WindowMap) mapWindows[iw];
        if (window.iWindow == wWindow)
        {
          for (int ib = 0; ib < window.mapButtons.Count; ib++)
          {
            button but = (button) window.mapButtons[ib];

            if (but.eAction == (Action.ActionType) action.wID)
            {
              action.SoundFileName = but.m_strSoundFile;
              return true;
            }
          }
        }
      }
      return false;
    }

    public static bool HasKeyMapped(int iWindow, Key key)
    {
      string strSoundFile;
      int wAction = GetActionCode(iWindow, key, out strSoundFile);
      if (wAction == 0)
      {
        return false;
      }
      return true;
    }
  }
}