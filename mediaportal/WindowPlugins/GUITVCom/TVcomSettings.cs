/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.Video
{
  class TVcomSettings
  {
    static TVcomSettings()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();

      loadSettings();
    }

    public static bool lookupIfNoSEinFilename = false;
    public static bool renameFiles = false;
    public static bool renameOnlyIfNoSEinFilename = true;
    public static string renameFormat = "[SHOWNAME] - [SEASONNO]x[EPISODENO] - [EPISODETITLE]";
    public static char replaceSpacesWith = ' ';
    public static bool lookupActors = false;
    public static string titleFormat = "[SHOWNAME] - [SEASONNO]x[EPISODENO] - [EPISODETITLE]";
    public static string genreFormat = "[SHOWNAME] ([GENRE])";
    private const string settingsFilePath = "Episode Guides/settings";
    static ILog _log;


    private static bool loadSettings()
    {
      _log.Info("Loading Settings...");
      try
      {
        System.IO.StreamReader r = new System.IO.StreamReader(settingsFilePath);
        lookupIfNoSEinFilename = Convert.ToBoolean(r.ReadLine());
        renameFiles = Convert.ToBoolean(r.ReadLine());
        renameOnlyIfNoSEinFilename = Convert.ToBoolean(r.ReadLine());
        renameFormat = r.ReadLine();
        replaceSpacesWith = Convert.ToChar(r.ReadLine());
        titleFormat = r.ReadLine();
        genreFormat = r.ReadLine();
        lookupActors = Convert.ToBoolean(r.ReadLine());
        r.Close();

        _log.Info("Settings loaded Succesfully");
        _log.Info(lookupIfNoSEinFilename.ToString());
        _log.Info(renameFiles.ToString());
        _log.Info(renameOnlyIfNoSEinFilename.ToString());
        _log.Info(renameFormat);
        _log.Info(replaceSpacesWith.ToString());
        _log.Info(titleFormat);
        _log.Info(genreFormat);
        _log.Info(lookupActors.ToString());

        return true;
      }
      catch
      {
        _log.Info("There was an error loading the Settings!");

        return false;
      }

    }


    public static void writeSettings()
    {
      if (!System.IO.Directory.Exists("Episode Guides"))
        System.IO.Directory.CreateDirectory("Episode Guides");

      System.IO.StreamWriter w = new System.IO.StreamWriter(settingsFilePath, false);
      w.WriteLine(lookupIfNoSEinFilename.ToString());
      w.WriteLine(renameFiles.ToString());
      w.WriteLine(renameOnlyIfNoSEinFilename.ToString());
      w.WriteLine(renameFormat);
      w.WriteLine(replaceSpacesWith.ToString());
      w.WriteLine(titleFormat);
      w.WriteLine(genreFormat);
      w.WriteLine(lookupActors.ToString());
      w.Close();

      _log.Info("Settings updated:");
      _log.Info(lookupIfNoSEinFilename.ToString());
      _log.Info(renameFiles.ToString());
      _log.Info(renameOnlyIfNoSEinFilename.ToString());
      _log.Info(renameFormat);
      _log.Info(replaceSpacesWith.ToString());
      _log.Info(titleFormat);
      _log.Info(genreFormat);
      _log.Info(lookupActors.ToString());

    }
  }



}
