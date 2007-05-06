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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class MusicViews : MediaPortal.Configuration.Sections.Views
  {
    string defaultMusicViews = Config.GetFile(Config.Dir.Base, "defaultMusicViews.xml");
    string customMusicViews = Config.GetFile(Config.Dir.Config, "MusicViews.xml");

    private System.ComponentModel.IContainer components = null;

    private string[] selections = new string[]
      {
        "album",
        "artist",
        "title",
        "genre",
        "year",
        "track",
        "timesplayed",
        "rating",
        "favorites"
      };

    private string[] sqloperators = new string[]
      {
        "",
        "=",
        ">",
        "<",
        ">=",
        "<=",
        "<>",
        "like",
        "group",
      };

    private string[] viewsAs = new string[]
			{
				"List",
				"Icons",
				"Big Icons",
				"Filmstrip",
        "Albums",
		  };

    private string[] sortBy = new string[]
			{
        "Name",
        "Date",
        "Size",
        "Track",
        "Duration",
        "Title",
        "Artist",
        "Album",
        "Filename",
        "Rating"
      };

    public MusicViews()
      : this("Music Views")
    { }

    public MusicViews(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public override void LoadSettings()
    {
      if (!File.Exists(customMusicViews))
      {
        File.Copy(defaultMusicViews, customMusicViews);
      }

      views = new ArrayList();

      try
      {
        using (FileStream fileStream = new FileInfo(customMusicViews).OpenRead())
        {
          SoapFormatter formatter = new SoapFormatter();
          views = (ArrayList)formatter.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception)
      {
      }

      LoadViews();
    }

    public override void SaveSettings()
    {
      if (settingsChanged)
        try
        {
          using (FileStream fileStream = new FileInfo(customMusicViews).OpenWrite())
          {
            SoapFormatter formatter = new SoapFormatter();
            formatter.Serialize(fileStream, views);
            fileStream.Close();
          }
        }
        catch (Exception)
        {
        }
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
    }
    #endregion
  }
}