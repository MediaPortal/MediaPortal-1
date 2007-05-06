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
  public class MovieViews : MediaPortal.Configuration.Sections.Views
  {
    string defaultVideoViews = Config.GetFile(Config.Dir.Base, "defaultVideoViews.xml");
    string customVideoViews = Config.GetFile(Config.Dir.Config, "VideoViews.xml");

    private System.ComponentModel.IContainer components = null;

    private string[] selections = new string[]
      {
        "actor",
        "title",
        "genre",
        "year",
        "rating",
        "watched",
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
      };

    private string[] viewsAs = new string[]
			{
				"List",
				"Icons",
				"Big Icons",
				"Filmstrip",
		  };

    private string[] sortBy = new string[]
			{
        "Name",
        "Date",
        "Size",
        "Year",
        "Rating",
        "Label",
      };

    public MovieViews()
      : this("Movie Views")
    { }

    public MovieViews(string name)
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
      if (!File.Exists(customVideoViews))
      {
        File.Copy(defaultVideoViews, customVideoViews);
      }

      views = new ArrayList();

      try
      {
        using (FileStream fileStream = new FileInfo(customVideoViews).OpenRead())
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
          using (FileStream fileStream = new FileInfo(customVideoViews).OpenWrite())
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